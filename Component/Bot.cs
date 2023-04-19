using System;
using System.Collections;
using System.Reflection;

using EFT;
using EFT.Game.Spawning;

using UnityEngine;

namespace Gaylatea
{
    namespace Spawn
    {
        public class Bot : MonoBehaviour
        {
            public BotOwner bot;

            public void Awake()
            {
                bot = GetComponent<BotOwner>();
                StartCoroutine(Activate());
            }

            public IEnumerator Activate()
            {
                // We need to wait for the bot's weapon to ready up, which seems
                // to be delayed from its initialization.
                while (!bot.WeaponManager.IsReady)
                {
                    yield return new WaitForEndOfFrame();
                }

                bot.Transform.position = bot.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;

                var actualActivate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);
                actualActivate.Invoke(bot, null);

                StartCoroutine(Run());
                StartCoroutine(CalculateNewGoals());

                yield return new WaitForEndOfFrame();
            }

            public IEnumerator CalculateNewGoals()
            {
                while (true)
                {
                    try
                    {
                        bot.CalcGoal();
                    }
                    catch (Exception ex)
                    {
                        Plugin.logger.LogError($"On frame {Time.frameCount}, {bot.ProfileId} had a goals issue: {ex}");
                    }
                    yield return new WaitForSeconds(3.0f);
                }
            }

            public IEnumerator Run()
            {
                // This is the main runloop of the bot. Compatibility with existing
                // AI client-side mods is not currently a priority.
                while (true)
                {
                    try
                    {
                        BrainUpdate();
                        BotUpdate();
                    }
                    catch (Exception ex)
                    {
                        Plugin.logger.LogError($"On frame {Time.frameCount}, {bot.ProfileId} had an update issue: {ex}");
                    }

                    yield return new WaitForEndOfFrame();
                }
            }

            public void BrainUpdate()
            {
                bot?.Brain?.Agent?.Update();
            }

            public void BotUpdate()
            {
                // Do not update bots that aren't alive.
                if (bot.BotState != EBotState.Active || !bot.GetPlayer.HealthController.IsAlive) { return; }

                bot.StandBy.Update();
                bot.LookSensor.UpdateLook();

                // Don't update bots that are paused, for whatever reason.
                if (bot.StandBy.StandByType == BotStandByType.paused) { return; }

                bot.ShootData.ManualUpdate();
                bot.DogFight.ManualUpdate();
                bot.RecoilData.LosingRecoil();
                bot.AimingData.PermanentUpdate();

                if (bot.WeaponManager != null)
                {
                    bot.WeaponManager.ManualUpdate();
                }

                bot.HeadData.ManualUpdate();
                bot.Tilt.ManualUpdate();
                bot.NightVision.ManualUpdate();
                bot.CellData.Update();
                bot.FriendChecker.ManualUpdate();
                bot.TrianglePosition.ManualUpdate();
                bot.Medecine.ManualUpdate();
                bot.Boss.ManualUpdate();
                bot.BotTalk.ManualUpdate();
                bot.BotRequestController.Update();
                bot.Tactic.UpdateChangeTactics();
                bot.Memory.ManualUpdate(Time.deltaTime);
                bot.Settings.UpdateManual();
                bot.BotRequestController.TryToFind();

                bot.Mover.ManualUpdate();
                bot.Mover.ManualFixedUpdate();
                bot.Steering.ManualFixedUpdate();
            }
        }
    }
}
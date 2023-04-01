using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using BepInEx;
using BepInEx.Logging;

using UnityEngine;

using EFT;
using EFT.Game.Spawning;

using Aki.Reflection.Patching;

namespace Framesaver
{
    // ActivatePatch implements a better way of "activating" and running the
    // run loop of an AI bot, which avoids some of the per-frame computational
    // issues that the game normally has.
    public class ActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsClass)?.GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotsClass __instance, BotOwner bot, ref HashSet<BotOwner> ___hashSet_0, ref GClass284 ___gclass284_0, Action<BotOwner> ___action_0)
        {
            if (Plugin.Enabled.Value)
            {
                ___hashSet_0.Add(bot);
                ___gclass284_0.AddPerson(bot);

                if (___action_0 != null)
                {
                    ___action_0(bot);
                }

                Activate(bot);
                return false;
            }

            return true;
        }

        public static bool isOnOddFrame()
        {
            return ((Time.frameCount % 2) == 1);
        }

        // Activate more efficiently activates a bot by avoiding the more
        // complicated pathfinding calculations that are done in vanilla,
        // as well as spreading them out throughout the frames.
        public static async void Activate(BotOwner bot)
        {
            var actualActivate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);
            var spawnedOnOddFrame = isOnOddFrame();

            Logger.LogInfo(String.Format("Framesaver is taking control of bot {0}, spawned on odd frame: {1}", bot.ProfileId, spawnedOnOddFrame));

            while (bot.WeaponManager == null || !bot.WeaponManager.IsReady)
            {
                await Task.Yield();
            }

            bot.Transform.position = bot.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
            actualActivate.Invoke(bot, null);

            await RunLoop(bot, spawnedOnOddFrame);
        }

        // RunLoop is a more efficient way of running the Bot code so that it
        // doesn't take up all the time in a frame.
        public static async Task RunLoop(BotOwner bot, bool spawnedOnOddFrame)
        {
            var nextGoalCalcTime = Time.time;

            for (; ; )
            {
                // Do not update bots that aren't alive.
                if (bot.BotState != EBotState.Active) { await Task.Yield(); continue; }

                // If a bot is dead, then just stop processing entirely.
                if (!bot.GetPlayer.HealthController.IsAlive)
                {
                    await Task.Yield(); break;
                }

                bot.StandBy.Update();
                bot.LookSensor.UpdateLook();

                // Don't update bots that are paused, for whatever reason.
                if (bot.StandBy.StandByType == BotStandByType.paused) { await Task.Yield(); continue; }

                // Has enough time passed for the bot to try and figure out what
                // its new goal should be?
                if (nextGoalCalcTime < Time.time)
                {
                    await Task.Run(() => { bot.CalcGoal(); });
                    nextGoalCalcTime = Time.time + 3.0f;
                }

                // Bots spawned on odd frames will only update on odd frames,
                // and similarly with bots spawned on even frames.
                if (spawnedOnOddFrame ^ isOnOddFrame())
                {
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

                    bot.ShootData.ManualUpdate();
                    bot.DogFight.ManualUpdate();
                    bot.RecoilData.LosingRecoil();
                    bot.AimingData.PermanentUpdate();

                    if (bot.WeaponManager != null)
                    {
                        bot.WeaponManager.ManualUpdate();
                    }

                    bot.Mover.ManualUpdate();
                    if (bot.GetPlayer.UpdateQueue == EUpdateQueue.Update)
                    {
                        bot.Mover.ManualFixedUpdate();
                        bot.Steering.ManualFixedUpdate();
                    }
                }

                await Task.Yield();
            }
        }
    }

    // UpdatePatch removes the original handling for updating bots, which causes
    // quite a lot of performance issues.
    public class UpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsClass)?.GetMethod("UpdateByUnity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return !Plugin.Enabled.Value;
        }
    }

    // MoverPatch removes an expensive debug calculation in the AI updates that
    // doesn't actually need to run.
    public class MoverPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GClass406)?.GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return !Plugin.Enabled.Value;
        }
    }
}
using System.Collections;
using System.Reflection;
using UnityEngine;
using System.Threading.Tasks;

using EFT;
using EFT.Game.Spawning;

namespace Framesaver
{
    // Class AI implements new functions for running AI calculations in a way
    // that doesn't kill the framerate.
    public class AI : MonoBehaviour
    {
        // Activate more efficiently activates a bot by avoiding the more
        // complicated pathfinding calculations that are done in vanilla,
        // as well as spreading them out throughout the frames.
        public async void Activate(BotOwner bot)
        {
            var actualActivate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.LogFormat("Bot %s is awaiting activation.", bot.ProfileId);

            for (; ; )
            {
                // Don't do anything until the weapon is ready to use.
                if (bot.WeaponManager == null || !bot.WeaponManager.IsReady) { await Task.Yield(); continue; }

                Debug.LogFormat("Bot %s is ready and is spawning in.", bot.ProfileId);
                bot.Transform.position = bot.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
                actualActivate.Invoke(bot, null);
                break;
            }

            await this.RunLoop(bot);
        }

        // RunLoop is a more efficient way of running the Bot code so that it
        // doesn't take up all the time in a frame.
        public async Task RunLoop(BotOwner bot)
        {
            var nextGoalCalcTime = Time.time;

            for (; ; )
            {
                // Do not update bots that aren't alive.
                if (bot.BotState != EBotState.Active) { await Task.Yield(); continue; }

                // If a bot is dead, then just stop processing entirely.
                if (!bot.GetPlayer.HealthController.IsAlive) { await Task.Yield(); break; }

                bot.StandBy.Update();
                bot.LookSensor.UpdateLook();

                // Don't update bots that are paused, for whatever reason.
                if (bot.StandBy.StandByType == BotStandByType.paused) { await Task.Yield(); continue; }

                // Has enough time passed for the bot to try and figure out what
                // its new goal should be?
                if (nextGoalCalcTime < Time.time)
                {
                    bot.CalcGoal();
                    nextGoalCalcTime = Time.time + 3.0f;
                }

                bot.Mover.ManualUpdate();
                if (bot.GetPlayer.UpdateQueue == EUpdateQueue.Update)
                {
                    bot.Mover.ManualFixedUpdate();
                    bot.Steering.ManualFixedUpdate();
                }

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


                await Task.Yield();
            }
        }
    }
}
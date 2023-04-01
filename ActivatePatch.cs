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

                var actualActivate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);
                var spawnedOnOddFrame = Plugin.isOnOddFrame();

                // This cursed line is required in order to properly set the odd frame flag on an extant unused boolean
                // left on the BotOwner class.
                typeof(BotOwner)?.GetField("bool_1", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bot, spawnedOnOddFrame);

                Logger.LogInfo(String.Format("Framesaver is taking control of bot {0}, spawned on odd frame: {1}", bot.ProfileId, spawnedOnOddFrame));

                bot.Transform.position = bot.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
                actualActivate.Invoke(bot, null);
                return false;
            }

            return true;
        }
    }

    public class UpdatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("UpdateManual", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance, float ___float_0, float ___float_3, bool ___bool_1)
        {
            // Do not update bots that aren't alive.
            if (__instance.BotState != EBotState.Active) { return false; }

            // If a bot is dead, then just stop processing entirely.
            if (!__instance.GetPlayer.HealthController.IsAlive)
            {
                return false;
            }

            __instance.StandBy.Update();
            __instance.LookSensor.UpdateLook();

            // Don't update bots that are paused, for whatever reason.
            if (__instance.StandBy.StandByType == BotStandByType.paused) { return false; }

            // Has enough time passed for the bot to try and figure out what
            // its new goal should be?
            if (___float_0 < Time.time)
            {
                __instance.CalcGoal();
            }

            if (___bool_1 ^ Plugin.isOnOddFrame())
            {
                __instance.ShootData.ManualUpdate();
                __instance.DogFight.ManualUpdate();
                __instance.RecoilData.LosingRecoil();
                __instance.AimingData.PermanentUpdate();

                if(__instance.WeaponManager != null) {
                    __instance.WeaponManager.ManualUpdate();
                }

                __instance.HeadData.ManualUpdate();
                __instance.Tilt.ManualUpdate();
                __instance.NightVision.ManualUpdate();
                __instance.CellData.Update();
                __instance.FriendChecker.ManualUpdate();
                __instance.TrianglePosition.ManualUpdate();
                __instance.Medecine.ManualUpdate();
                __instance.Boss.ManualUpdate();
                __instance.BotTalk.ManualUpdate();
                __instance.BotRequestController.Update();
                __instance.Tactic.UpdateChangeTactics();
                __instance.Memory.ManualUpdate(Time.deltaTime);
                __instance.Settings.UpdateManual();
                __instance.BotRequestController.TryToFind();

                __instance.Mover.ManualUpdate();
                if (__instance.GetPlayer.UpdateQueue == EUpdateQueue.Update)
                {
                    __instance.Mover.ManualFixedUpdate();
                    __instance.Steering.ManualFixedUpdate();
                }
            }


            // This isn't needed for our use cases, I'm not even sure why it
            // would be included in the production code.
            //__instance.UnityEditorRunChecker.ManualLateUpdate();

            // Always skip the original bot code.
            return false;
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
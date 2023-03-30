using System.Reflection;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI;

using EFT;
using EFT.Game.Spawning;

using Aki.Reflection.Patching;


namespace Framesaver
{
    public class Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("UpdateManual", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance, float ___float_0, float ___float_3)
        {
            // TODO: investigate making this a coroutine instead
            // This pre-work needs to be done in order to activate the bot properly at first spawn.
            // TODO: wonder if this is what causes a bunch of the stutters on spawn waves.
            if (__instance.BotState == EBotState.PreActive && __instance.WeaponManager.IsReady)
            {
                var activate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);

                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(__instance.GetPlayer.Position, out navMeshHit, 0.2f, -1))
                {
                    activate.Invoke(__instance, null);
                    return false;
                }
                if (___float_3 < Time.time)
                {
                    ___float_3 = Time.time + 1f;
                    __instance.Transform.position = __instance.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
                    activate.Invoke(__instance, null);
                }

                return false;
            }

            // Do not update bots that aren't alive.
            if (__instance.BotState != EBotState.Active || !__instance.GetPlayer.HealthController.IsAlive) { return false; }

            __instance.StandBy.Update();
            __instance.LookSensor.UpdateLook();

            // Don't update bots that are paused, for whatever reason.
            if (__instance.StandBy.StandByType == BotStandByType.paused) { return false; }

            // Has enough time passed for the bot to try and figure out what
            // its new goal should be?
            // TODO: check with Solarint, is this the bug he was mentioning?
            if (___float_0 < Time.time)
            {
                __instance.CalcGoal();
            }

            // TODO: update shooting every other frame
            if ((Time.frameCount % 2) == 1)
            {
                __instance.ShootData.ManualUpdate();
                __instance.DogFight.ManualUpdate();
                __instance.RecoilData.LosingRecoil();
                __instance.AimingData.PermanentUpdate();
                __instance.WeaponManager.ManualUpdate();
            }
            // TODO: update everything else every quarter frame
            if ((Time.frameCount % 4) == 1)
            {
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
            }

            __instance.Mover.ManualUpdate();
            if (__instance.GetPlayer.UpdateQueue == EUpdateQueue.Update)
            {
                __instance.Mover.ManualFixedUpdate();
                __instance.Steering.ManualFixedUpdate();
            }


            // This isn't needed for our use cases, I'm not even sure why it
            // would be included in the production code.
            //__instance.UnityEditorRunChecker.ManualLateUpdate();

            // Always skip the original bot code.
            return false;
        }
    }
}
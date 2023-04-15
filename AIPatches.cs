using Aki.Reflection.Patching;
using EFT;
using EFT.Game.Spawning;

using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

namespace Framesaver
{
    class DisableBotBrainUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AICoreControllerClass)?.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    class DisableBotUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsClass)?.GetMethod("UpdateByUnity", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    // OptimizeBotStateMachineTransitionsPatch removes a lot of the extra
    // processing and heap allocations that the built-in transitions do, that
    // cause GC churn.
    // class OptimizeBotStateMachineTransitionsPatch : ModulePatch
    // {
    //     private static GClass102 currentAction;
    //     private static GStruct8<BotLogicDecision>? nextState;

    //     protected override MethodBase GetTargetMethod()
    //     {
    //         return typeof(GClass25<BotLogicDecision>)?.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    //     }

    //     [PatchPrefix]
    //     public static bool Prefix(GClass215<BotLogicDecision> ___gclass215_0, ref GStruct8<BotLogicDecision> ___gstruct8_0, Dictionary<BotLogicDecision, GClass102> ___dictionary_0)
    //     {
    //         ___gclass215_0.ManualUpdate();

    //         nextState = ___gclass215_0.Update(___gstruct8_0);
    //         if (nextState == null) { return false; }

    //         if (___dictionary_0.TryGetValue(nextState.Value.Action, out currentAction))
    //         {
    //             currentAction.Update();
    //         }
    //         ___gstruct8_0 = nextState.Value;
    //         return false;
    //     }
    // }

    // ActivatePatch implements a better way of "activating" and running the
    // run loop of an AI bot, which avoids some of the per-frame computational
    // issues that the game normally has.
    public class ActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotsClass)?.GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPostfix]
        public static void PatchPostfix(BotOwner bot)
        {
            var actualActivate = typeof(BotOwner)?.GetMethod("method_10", BindingFlags.Instance | BindingFlags.NonPublic);

            Logger.LogWarning($"Framesaver is taking control of bot {bot.ProfileId}");

            bot.gameObject.AddComponent<Component.Bot>();

            bot.Transform.position = bot.BotsGroup.BotZone.SpawnPoints.RandomElement<ISpawnPoint>().Position + Vector3.up * 0.5f;
            actualActivate.Invoke(bot, null);
        }
    }
}
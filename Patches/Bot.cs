using System.Collections.Generic;
using System.Reflection;

using EFT;

using Aki.Reflection.Patching;

namespace Gaylatea
{
    namespace Spawn
    {
        // DisableBotBrainUpdatesPatch stops the game from running the normal AI
        // state machine updates during LocalGame::Update(). Instead, each bot
        // will update itself during its own runloop.
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

        // DisableBotUpdatesPatch disables the normal bot update loop, and
        // instead allows it to be run in each bot's runloop.
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
        class OptimizeBotStateMachineTransitionsPatch : ModulePatch
        {
            private static GClass103 currentAction;
            private static GStruct8<BotLogicDecision>? nextState;

            protected override MethodBase GetTargetMethod()
            {
                return typeof(GClass26<BotLogicDecision>)?.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            [PatchPrefix]
            public static bool Prefix(GClass216<BotLogicDecision> ___gclass216_0, ref GStruct8<BotLogicDecision> ___gstruct8_0, Dictionary<BotLogicDecision, GClass103> ___dictionary_0)
            {
                ___gclass216_0.ManualUpdate();

                nextState = ___gclass216_0.Update(___gstruct8_0);
                if (nextState == null) { return false; }

                if (___dictionary_0.TryGetValue(nextState.Value.Action, out currentAction))
                {
                    currentAction.Update();
                }
                ___gstruct8_0 = nextState.Value;
                return false;
            }
        }

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
                bot.gameObject.AddComponent<Bot>();
            }
        }

        // MoverPatch removes an expensive debug calculation in the AI updates that
        // doesn't actually need to run.
        public class MoverPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return typeof(GClass408)?.GetMethod("method_4", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            [PatchPrefix]
            public static bool PatchPrefix()
            {
                return false;
            }
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using EFT;

using HarmonyLib;

using Aki.Common.Http;
using Aki.Reflection.Patching;

namespace Gaylatea
{
    namespace Spawn
    {
        class HttpClient
        {
            public static Profile[] GetBots(List<WaveInfo> conditions)
            {
                var s = new ConditionsWrapper();
                s.conditions = conditions;

                var p = RequestHandler.PostJson("/client/game/bot/generate", s.ToJson());
                return p.ParseJsonTo<BotsResponseWrapper>().data;
            }

            struct ConditionsWrapper
            {
                public List<WaveInfo> conditions;
            }

            struct BotsResponseWrapper
            {
                public Profile[] data;
            }
        }

        // UseAKIHTTPForBotLoadingPatch overrides the normal Diz.Jobs-based
        // loading, which creates a ton of garbage due to how that background
        // job manager is implemented.
        class UseAKIHTTPForBotLoadingPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.TypeByName("Class224").GetMethod("LoadBots", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            [PatchPrefix]
            public static bool Prefix(ref Task<Profile[]> __result, List<WaveInfo> conditions)
            {
                TaskCompletionSource<Profile[]> tcs = new TaskCompletionSource<Profile[]>();
                __result = tcs.Task;

                Task.Factory.StartNew(() => {
                    Plugin.logger.LogWarning($"Loading a new bot from the server: {conditions.ToJson()}");
                    var p = HttpClient.GetBots(conditions);
                    // TODO: error handling.
                    tcs.SetResult(p);
                });

                return false;
            }
        }
    }
}
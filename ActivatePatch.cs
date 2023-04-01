using System;
using System.Collections.Generic;
using System.Reflection;

using EFT;

using Aki.Reflection.Patching;
using UnityEngine;

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
            ___hashSet_0.Add(bot);
            ___gclass284_0.AddPerson(bot);

            if (___action_0 != null)
            {
                ___action_0(bot);
            }

            Plugin.ai.Activate(bot);
            return false;
        }
    }
}
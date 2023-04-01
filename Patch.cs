using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

using EFT;
using EFT.Game.Spawning;

using Aki.Reflection.Patching;


namespace Framesaver
{
    // UpdatePatch completely skips the synchronous updates that EFT usually
    // uses for AI, allowing the new Activate() to create the RunLoop instead.
    public class UpdatePatch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("UpdateManual", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner __instance)
        {
            // Always just use the new AI code instead of the vanilla one.
            return false;
        }
    }
}
using System.Reflection;
using Aki.Reflection.Patching;
using EFT;
using UnityEngine;

namespace Framesaver
{
    public class Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetMethod("UpdateManual", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            return (Time.frameCount % 2 == 1);
        }
    }
}
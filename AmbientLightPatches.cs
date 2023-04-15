using Aki.Reflection.Patching;
using EFT;

using System.Reflection;

namespace Framesaver
{
    class AmbientLightOptimizeRenderingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight)?.GetMethod("method_8", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }

    class AmbientLightDisableFrequentUpdatesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(AmbientLight)?.GetMethod("method_1", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
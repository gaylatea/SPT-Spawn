using BepInEx;
using BepInEx.Configuration;

using UnityEngine;


namespace Framesaver
{
    [BepInPlugin("com.gaylatea.framesaver", "SPT-Framesaver", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool isOnOddFrame()
        {
            return ((Time.frameCount % 2) == 1);
        }

        internal static ConfigEntry<bool> Enabled;

        void Awake()
        {
            Enabled = Config.Bind("Status", "Enabled", true, "Use the async bot updates instead");

            new UpdatePatch().Enable();
            new ActivatePatch().Enable();
            new MoverPatch().Enable();
        }
    }
}
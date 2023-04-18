using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using UnityEngine;

namespace Gaylatea
{
    namespace Spawn
    {
        [BepInPlugin("com.gaylatea.spawn", "SPT-Spawn", "1.0.0")]
        public class Plugin : BaseUnityPlugin
        {
            private GameObject Hook;
            private const string KeybindSectionName = "Keybinds";
            internal static ManualLogSource logger;
            internal static ConfigEntry<KeyboardShortcut> Spawn;

            public Plugin()
            {
                logger = Logger;
                Spawn = Config.Bind(KeybindSectionName, "Spawn a Bot", new KeyboardShortcut(KeyCode.Equals), "Spawns a random bot in front of the player.");

                Hook = new GameObject("Gaylatea.Spawn");
                Hook.AddComponent<Controller>();
                DontDestroyOnLoad(Hook);
                Logger.LogInfo($"S.P.A.W.N Loaded");
            }
        }
    }
}
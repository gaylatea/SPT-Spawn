using BepInEx;

using UnityEngine;


namespace Framesaver
{
    [BepInPlugin("com.gaylatea.framesaver", "SPT-Framesaver", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static AI ai;
        public static GameObject hook;

        void Awake()
        {
            hook = new GameObject();
            ai = hook.AddComponent<AI>();
            DontDestroyOnLoad(hook);

            new UpdatePatch().Enable();
            new ActivatePatch().Enable();
        }
    }
}
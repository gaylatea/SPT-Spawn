using BepInEx;
using EFT;

using Config;

using Aki.Reflection.Utils;

namespace Framesaver
{
    [BepInPlugin("com.gaylatea.framesaver", "SPT-Framesaver", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public Plugin()
        {
            Profiles.Init(Config);

            // Here we experiment with just straight disabling shit to save FPS.

            // This seems to disable a bunch of expensive rigid body physics
            // calculations that don't have much of an effect on gameplay.
            GClass670.GClass672.Enabled = false;

            new DontSpawnShellsFiringPatch().Enable();
            new DontSpawnShellsJamPatch().Enable();
            new DontSpawnShellsAtAllReallyPatch().Enable();

            new AmbientLightOptimizeRenderingPatch().Enable();
            new AmbientLightDisableFrequentUpdatesPatch().Enable();

            //new OptimizeBotStateMachineTransitionsPatch().Enable();
            new ActivatePatch().Enable();

            new DisableBotBrainUpdatesPatch().Enable();
            new DisableBotUpdatesPatch().Enable();

            //var p = HookObject.AddOrGetComponent<Profiling>();

            // Something in these ticks seems to be a hotspot. Let's profile
            // to figure out which they are.
            // p.EnableOn(typeof(LocalPlayer), "LateUpdate");
            // p.EnableOn(typeof(AmbientLight), "LateUpdate");
            // p.EnableOn(typeof(BotControllerClass), "method_0");
            // p.EnableOn(typeof(AICoreControllerClass), "Update");
            // p.EnableOn(typeof(AiTaskManagerClass), "Update");
            // p.EnableOn(typeof(BotsClass), "UpdateByUnity");
            // p.EnableOn(typeof(GClass25<BotLogicDecision>), "Update");

            // These are a part of the TickListener- something in these spikes.
            // p.EnableOn(typeof(GameWorld), "PlayerTick");
            // p.EnableOn(typeof(GameWorld), "BallisticsTick");
            // p.EnableOn(typeof(GameWorld), "AfterPlayerTick");
            // p.EnableOn(typeof(GameWorld), "OtherElseWorldTick");
        }
    }
}
using BepInEx;

namespace Framesaver
{
    [BepInPlugin("com.gaylatea.framesaver", "SPT-Framesaver", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            new Patch().Enable();
        }
    }
}
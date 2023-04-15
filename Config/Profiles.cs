using BepInEx.Configuration;

namespace Config
{
    internal class Profiles
    {
        public static ConfigEntry<bool> Enabled { get; private set; }

        public static void Init(ConfigFile Config)
        {
            string section = "1. Profiling";

            Enabled = Config.Bind(section, "Enabled", false, new ConfigDescription("", null, null));
        }
    }
}
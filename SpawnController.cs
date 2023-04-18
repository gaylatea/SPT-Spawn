using Comfort.Common;

using EFT;

using UnityEngine;

namespace Gaylatea
{
    namespace Spawn
    {
        public class Controller : MonoBehaviour
        {
            Player player
            { get => gameWorld.AllPlayers[0]; }

            GameWorld gameWorld
            { get => Singleton<GameWorld>.Instance; }

            IBotGame game { get => (IBotGame)Singleton<AbstractGame>.Instance; }

            void Update()
            {
                if (Input.GetKeyDown(Plugin.Spawn.Value.MainKey))
                {
                    if(gameWorld == null) {
                        Plugin.logger.LogWarning("Not spawning because there's no game world to spawn into.");
                        return;
                    }

                    // TODO: choose a random WildSpawnType and EPlayerSide.
                    var botData = new GClass622(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, 0f, null);
                    game.BotsController.BotSpawner.ActivateBotsWithoutWave(1, botData);
                    Plugin.logger.LogWarning("Spawned in a new bot at a random spot in the map.");
                }
            }
        }
    }
}
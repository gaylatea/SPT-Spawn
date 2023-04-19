using Comfort.Common;

using UnityEngine;

using EFT;
using EFT.Communications;

namespace Gaylatea
{
    namespace Spawn
    {
        public class Controller : MonoBehaviour
        {
            IBotGame game { get => (IBotGame)Singleton<AbstractGame>.Instance; }

            void Update()
            {
                if (Input.GetKeyDown(Plugin.Spawn.Value.MainKey))
                {
                    if(game == null) {
                        NotificationManagerClass.DisplayMessageNotification("Not spawning because there's no game to spawn into.", ENotificationDurationType.Default, ENotificationIconType.Alert);
                        return;
                    }

                    // Note that the AKI server might rewrite this bot to be a PMC.
                    var botData = new GClass624(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, 0f, null);
                    game.BotsController.BotSpawner.ActivateBotsWithoutWave(1, botData);
                    NotificationManagerClass.DisplayMessageNotification("Spawned in a new bot at a random spot in the map.", ENotificationDurationType.Default, ENotificationIconType.Alert);
                }
            }
        }
    }
}
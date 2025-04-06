using SonsSdk.Attributes;
using SonsSdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LootRespawnControl.LootManager.LootRespawnManager;
using static LootRespawnControl.LootManager;
using RedLoader;

namespace LootRespawnControl.Managers
{
    internal class DebugManager
    {
        public static void ConsoleLog(string message)
        {
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg("[LRC] " + message);
            }
        }

        public static void ConsoleLogWarning(string message)
        {
            if (Config.ConsoleLogging.Value)
            {
                RLog.Warning("[LRC] " + message);
            }
        }

        public static void ConsoleLogError(string message)
        {
            if (Config.ConsoleLogging.Value)
            {
                RLog.Error("[LRC] " + message);
            }
        }

        /// <summary>
        /// Debug commands for testing purposes
        /// </summary>

        [DebugCommand("lrcresetcollected")]
        private void LRCResetCollected()
        {
            LootRespawnManager.collectedLootIds = new HashSet<LootData>();
            SonsTools.ShowMessage("Loot Respawn Control: All picked up loot has been reset. Save your game and reload");
        }


        [DebugCommand("lrcforcereloadconfig")]
        private void LRCForceReloadConfig()
        {
            if (BoltNetwork.isRunning && BoltNetwork.isServer)
            {
                if (Config.ConsoleLogging.Value)
                {
                    SonsTools.ShowMessage("Loot Respawn Control: WARNING! You are in multiplayer! Without a reload desyncs can occur!");
                    DebugManager.ConsoleLog($"Force reloaded Config While user is in Multiplayer!!!");
                }
                ConfigManager.SetLocalConfigValues();
            }
            else if (!BoltNetwork.isRunning)
            {
                DebugManager.ConsoleLog($"User is in singleplayer... setting local config values");
                ConfigManager.SetLocalConfigValues();
                ConfigManager.SetMultiplayerConfigValue(false);
            }

            SonsTools.ShowMessage("Loot Respawn Control: Force reloaded local config values...");
        }
    }
}

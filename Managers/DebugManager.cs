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
             DebugManager.ConsoleLog("Running debug command  LRCResetCollected...");
            if (BoltNetwork.isRunning && BoltNetwork.isServer && ConfigManager.enableMultiplayer) //Bolt running and is host and multiplayer enabled
            {
                SonsTools.ShowMessage("Loot Respawn Control: Collected loot has been reset... WARNING!!! You are in multiplayer!!! This session is now desynced until all clients exit and rejoin!");
                DebugManager.ConsoleLog($"Collected loot reset while user is in Multiplayer!!!");
                
                LootRespawnManager.collectedLootIds = new HashSet<LootData>();
                return;
            }
            else if (BoltNetwork.isRunning && ConfigManager.enableMultiplayer){
                SonsTools.ShowMessage("Loot Respawn Control: You are NOT the host, loot reset has been denied!");
                DebugManager.ConsoleLog("$User attempted to reset collected loot as client! Action was denied");
                return;
            }

            SonsTools.ShowMessage("Loot Respawn Control: Collected loot has been reset...");
            DebugManager.ConsoleLog($"User is in singleplayer... resetting collected loot");
            LootRespawnManager.collectedLootIds = new HashSet<LootData>();
        }


        [DebugCommand("lrcforcereloadconfig")]
        private void LRCForceReloadConfig()
        {
            DebugManager.ConsoleLog("Running debug command LRCForceReloadConfig...");
            if (BoltNetwork.isRunning && BoltNetwork.isServer) //Bolt running and is host
            {
                SonsTools.ShowMessage("Loot Respawn Control: WARNING!!! You are in multiplayer!!! This session is now desynced until all clients exit and rejoin!");
                DebugManager.ConsoleLog($"Force reloaded config while user is in Multiplayer!!!");
                
                ConfigManager.SetLocalConfigValues();
                return;
            }
            else if (BoltNetwork.isRunning && ConfigManager.enableMultiplayer){
                SonsTools.ShowMessage("Loot Respawn Control: You are NOT the host, the config was NOT reloaded!");
                DebugManager.ConsoleLog("$User attempted to force reload as client! Reload denied");
                return;
            }

            SonsTools.ShowMessage("Loot Respawn Control: Force reloaded local config values...");
            DebugManager.ConsoleLog($"User is in singleplayer... setting local config values");
            ConfigManager.SetLocalConfigValues();
            ConfigManager.SetMultiplayerConfigValue(false);
        }
    }
}

﻿using SonsSdk;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using Alt.Json;
using HarmonyLib;
using RedLoader;
using Sons.Save;
using static RedLoader.RLog;
using SonsSdk.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Common;
using Sons.Gameplay;
using static LootRespawnControl.LootRespawnSaveManager;
using SUI;
using Sons.Environment;
using static LootRespawnControl.LootManager;
using UnityEngine.SceneManagement;
using Sons.Utilities;
using Endnight.Utilities;
using UnityEngine.Windows;
using SonsSdk.Networking;
using static SonsSdk.ItemTools;
using System.Security.AccessControl;
using static LootRespawnControl.LootManager.LootRespawnManager;
using System.Linq;

namespace LootRespawnControl;

public class LootRespawnControl : SonsMod
{
    public static List<int> ItemIdsMeleeWeapons = new List<int>()
{
    340, // guitar
    356, // modern axe
    359, // machete
    367, // katana
    379, // battle axe
    394, // chainsaw
    396, // electric club
    431, // fire axe
    474, // spear
    477, // manufactured baton
    503, // torch
    525, // club
    663, // pickaxe
    485  // Shovel 
};

    public static List<int> ItemIdsRangedWeapons = new List<int>()
{
    358, // shotgun
    360, // block bow
    361, // rifle
    386, // revolver
    443, // homemade bow
    459, // slingshot
    353, // taser
    365, // crossbow
    355  // pistol
};

    public static List<int> ItemIdsWeaponMods = new List<int>()
{
    346, // shotgun mount
    374, // silencer
    375, // laser sight
    376, // pistol mount
    378  // gun flashlight
};

    public static List<int> ItemIdsMaterials = new List<int>()
{
    634, // solar panel
    635, // light bulb
    403, // rope
    405, // bone
    410, // wristwatch
    414, // bottle of vodka
    415, // cloth
    416, // circuit board
    418, // wire
    419, // duct tape
    420, // c4 brick
    430, // skull
    479, // feather
    496, // money
    502, // coins
    527, // batteries
    661, // battery
    517, // saucepan
    504, // tarp
    590  // radio
};

    public static List<int> ItemIdsFood = new List<int>()
{
    421, // instant noodles
    425, // breakfast cereal
    433, // meat
    434, // canned food
    436, // fish
    438, // dry rations
    464, // cat food
    569, // piece of brain
    570, // piece of steak
    571  // piece of bacon
};

    public static List<int> ItemIdsMedicineAndEnergy = new List<int>()
{
    437, // pills
    441, // candy bar
    439  //energy drink
};

    public static List<int> ItemIdsPlants = new List<int>()
{
    397, // shiitake
    398, // oyster mushrooms
    399, // yellow hedgehog
    400, // fly agaric
    450, // horsetail
    451, // aloe vera
    452, // yarrow
    453, // cypress
    454, // balsamorrhiza
    465  // chicory
};

    public static List<int> ItemIdsAmmunition = new List<int>()
{
    373, // carbon fiber arrows
    368, // crossbow bolt
    362, // pistol cartridge
    363, // heavy bullets
    364, // buckshot
    369, // ammo for tasers
    387, // ammo for rifle
    370, // 1-5 pistol cartridges
    371, // 1-5 cartridges
    372, // 1-5 heavy bullets
    457, // 1-2 ammo for taser
    523  // zipline rope
};

    public static List<int> ItemIdsThrowables = new List<int>()
{
    524, // golf ball 
    381, // grenade
    417, // sticky bomb
    440, // signal fire
};


    public static List<int> ItemIdsExpendables = new List<int>()
{
    390, // printer resin
    469, // air tank
    508  // hide bag
};

    public static List<int> ItemIdsTransport = new List<int>()
{
    626, // paraglider
    630  // knight v

};

    public static List<int> ItemIdsBlacklistBreakable = new List<int>()
{
    392  // stick
};

    public static List<int> CustomWhitelist = new List<int>();

    public static List<int> CustomWhitelistTimed = new List<int>();

    public static List<int> CustomBlacklist = new List<int>();

    public static List<int> CustomNetworkingList = new List<int>();

    public static string _modVersion;

    public static int _breakableId = 9999;

    public static List<KeyValuePair<string, bool?>> pickupsAwaitingReply = new List<KeyValuePair<string, bool?>>();

    public static bool DoubleCheckedCollectedLoot = false;
    public static bool recievedConfigData = false;

    public static ESonsScene _currentScene;

    public LootRespawnControl()
    {
        HarmonyPatchAll = true;
        OnUpdateCallback = OnUpdate;
    }

    protected override void OnInitializeMod()
    {
        Config.Init();
        CustomWhitelist = ExtractIds(Config.allowList.Value);
        CustomWhitelistTimed = ExtractIds(Config.allowListTimed.Value);
        CustomBlacklist = ExtractIds(Config.removeList.Value);
        CustomNetworkingList = ExtractIds(Config.networkList.Value);

        NetworkManager.RegisterPackets();
        _modVersion = Manifest.Version;

        SdkEvents.OnWorldExited.Subscribe(OnWorldExitedCallback);
    }

    protected override void OnSdkInitialized()
    {
        SettingsRegistry.CreateSettings(this, null, typeof(Config));
        var saveManager = new LootRespawnSaveManager();
        SonsSaveTools.Register(saveManager);
    }

    protected override void OnGameStart()
    {
        // This is called once the player spawns in the world and gains control.
        if (BoltNetwork.isServerOrNotRunning)
        {
            //Initialize event handler
            EventHandler.Create();
            if (Config.ConsoleLogging.Value)
            {
                RLog.Msg("User in Singleplayer or Hosting!");
            }
            return;
        }

        if (!DoubleCheckedCollectedLoot)
        {
            HandleStartupLootData(null);
        }
    }

    protected override void OnSonsSceneInitialized(ESonsScene sonsScene)
    {
        _currentScene = sonsScene;
    }

    private void OnUpdate()
    {
        NetworkManager.Update();
    }

    private void OnWorldExitedCallback()
    {
        LootRespawnManager.collectedLootIds = new HashSet<LootData>();
        DoubleCheckedCollectedLoot = false;
        recievedConfigData = false;
        if (Config.ConsoleLogging.Value)
        {
            RLog.Msg("Exited World, Reset Values");
        }
    }

    public static void HandlePickupDataRecieved(string objectName, string identifier, int pickupId, long timestamp)
    {
        if(objectName != null && pickupId != _breakableId)
        {
            GameObject pickupObject = GameObject.Find(objectName);
            if (pickupObject != null)
            {
                LootIdentifier lootIdentifier = pickupObject.GetComponent<LootIdentifier>();
                if (lootIdentifier != null && lootIdentifier.Identifier == identifier)
                {
                    if (Config.ConsoleLogging.Value) { RLog.Msg($"Found {objectName} destroying..."); }
                    GameObject.Destroy(pickupObject);
                }
                else
                {
                    if (Config.ConsoleLogging.Value) { RLog.Msg($"Unable to find {objectName} with correct identifier!"); }
                }
            }
        }
        if (Config.ConsoleLogging.Value) { RLog.Msg($"Added loot from recieved packet: Name: {objectName} Identifier: {identifier} Timestamp: {timestamp}"); }
        MarkLootAsCollected(identifier, null, pickupId, false, timestamp);
    }

    public static long GetTimestampFromGameTime(string gameTimeString)
    {
        string[] parts = gameTimeString.Split(' ');
        int day = int.Parse(parts[1]);
        string timeString = parts[2];

        TimeSpan time = TimeSpan.Parse(timeString);

        long timestamp = day * 24 * 60 * 60 + (long)time.TotalSeconds;

        return timestamp;
    }

    public static bool HasEnoughTimePassed(string identifier, long currentTimestamp)
    {
        long? originalTimestamp = GetLootTimestamp(identifier);
        if (originalTimestamp == null) { return false; }

        long timePassed = currentTimestamp - originalTimestamp.Value;
        long respawnTime = ConfigManager.timeInDays * 24 * 60 * 60;
        return timePassed >= respawnTime;
    }

    public static List<int> ExtractIds(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<int>();
        }

        var ids = input
            .Split(';')
            .Select(id => id.Trim())
            .Where(id => !string.IsNullOrEmpty(id))
            .Select(int.Parse)
            .ToList();

        return ids;
    }

    [DebugCommand("fullresetlootrespawncontrol")]
    private void FullResetLootRespawnControl()
    {
        LootRespawnManager.collectedLootIds = new HashSet<LootData>();
        SonsTools.ShowMessage("Loot Respawn Control: All picked up loot has been reset. Save your game and reload");
    }
}


[RegisterTypeInIl2Cpp]
public class LootIdentifier : MonoBehaviour
{
    public string Identifier { get; private set; }
    public void GenerateIdentifier()
    {
        Identifier = LootRespawnManager.GenerateLootID(transform.position, transform.rotation, transform.name);
    }
    private void Awake()
    {
        GenerateIdentifier();
    }

    /*void OnEnable()
    {
        GenerateIdentifier();
    }   */

    public string ReturnIdentifier()
    {
        return Identifier;
    }

    public bool HasBeenCollected()
    {
        return LootRespawnManager.IsLootCollected(Identifier);
    }
}

public class LootRespawnSaveManager : ICustomSaveable<LootRespawnSaveManager.LootSaveData>
{
    public string Name => "LootRespawnSaveManager";

    // Used to determine if the data should also be saved in multiplayer saves
    public bool IncludeInPlayerSave => true;

    public LootSaveData Save()
    {
        // Serialize your data from game state here
        var LootSaveData = new LootSaveData();
        LootSaveData.collectedLootJson = LootRespawnManager.SaveCollectedLoot();

        return LootSaveData;
    }

    public void Load(LootSaveData obj)
    {
        LootRespawnManager.LoadCollectedLoot(obj.collectedLootJson);

    }

    public class LootSaveData
    {
        public string collectedLootJson;
    }
}
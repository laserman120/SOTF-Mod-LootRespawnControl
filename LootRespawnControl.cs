using SonsSdk;
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
using static LootRespawnControl.LootRespawnManager;

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
    504, //tarp
    590  //radio
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
    457  // 1-2 ammo for taser
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

    public LootRespawnControl()
    {
        HarmonyPatchAll = true;
    }

    protected override void OnInitializeMod()
    {
        Config.Init();
    }

    protected override void OnSdkInitialized()
    {
        SettingsRegistry.CreateSettings(this, null, typeof(Config));
        var saveManager = new LootRespawnSaveManager();
        SonsSaveTools.Register(saveManager);
    }

    //Patch the spawning of pickups
    [HarmonyPatch(typeof(Sons.Gameplay.PickUp), "Awake")]
    private static class PickUpAwakePatch
    {
        private static void Postfix(Sons.Gameplay.PickUp __instance)
        {
            LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
            if(identifierComponent == null) {
                // Add the LootIdentifier component to the parent
                identifierComponent = __instance.transform.gameObject.AddComponent<LootIdentifier>();
            }
            string identifier = identifierComponent.Identifier;

            if (LootRespawnManager.IsLootCollected(identifier) && ShouldIdBeRemoved(__instance._itemId))
            {
                //Global Timer Check
                if (Config.UseTimerGlobal.Value && HasEnoughTimePassed(identifier, GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    return;
                }

                //If the specific category times is enabled it will check this here
                if (ShouldIdBeRemovedTimed(__instance._itemId) && HasEnoughTimePassed(identifier, GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    return;
                }
                UnityEngine.Object.Destroy(__instance._destroyTarget);
            }
        }
    }

    //Patch the spawning of breakables
    [HarmonyPatch(typeof(Sons.Gameplay.BreakableObject), "Awake")]
    private static class BreakableObjectAwakePatch
    {
        private static void Postfix(Sons.Gameplay.BreakableObject __instance)
        {
            LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
            if (identifierComponent == null)
            {
                // Add the LootIdentifier component to the parent
                identifierComponent = __instance.transform.gameObject.AddComponent<LootIdentifier>();
            }
            string identifier = identifierComponent.Identifier;

            if (LootRespawnManager.IsLootCollected(identifier) && !Config.AllowBreakables.Value)
            {
                if (Config.UseTimerGlobal.Value && HasEnoughTimePassed(identifier, GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    return;
                }

                //if breakable category is allowe timed, remove it
                if(Config.AllowBreakablesTimed.Value && HasEnoughTimePassed(identifier, GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
                {
                    LootRespawnManager.RemoveLootFromCollected(identifier);
                    return;
                }

                UnityEngine.Object.Destroy(__instance.transform.gameObject);
            }
        }
    }

    //runs whenever a pickup is collected
    [HarmonyPatch(typeof(Sons.Gameplay.PickUp), "Collect")]
    private static class CollectPatch
    {
        private static void Postfix(Sons.Gameplay.PickUp __instance)
        {
            LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
            string identifier = identifierComponent.Identifier;

            if (__instance.name.Contains("Clone")) { return; }
            
            if(ShouldIdBeRemoved(__instance._itemId))
            {
                LootRespawnManager.MarkLootAsCollected(identifier);
                return;
            }
            
            if (LootRespawnManager.IsLootCollected(identifier))
            {
                LootRespawnManager.RemoveLootFromCollected(identifier);
            }
        }
    }

    //runs whenever a breakable object is broken
    [HarmonyPatch(typeof(Sons.Gameplay.BreakableObject), "OnBreak")]
    private static class OnBreakPatch
    {
        private static void Postfix(Sons.Gameplay.BreakableObject __instance)
        {
            LootIdentifier identifierComponent = __instance.transform.GetComponent<LootIdentifier>();
            if(identifierComponent == null) { return; }
            string identifier = identifierComponent.Identifier;

            if (__instance.name.Contains("Clone")) { return; }

            //Pickup is a simple item spawner on broken
            PickUp PickUp = __instance._brokenPrefab?.transform.GetComponent<PickUp>() ?? null;
            int PickUpArrayLength = __instance._spawnDefinitions.Count;
            if (PickUp != null && !Config.AllowBreakables.Value)
            {
                //return out if blacklisted item would be dropped
                if (ItemIdsBlacklistBreakable.Contains(PickUp._itemId)) { return; }
                LootRespawnManager.MarkLootAsCollected(identifier);
                return;
            }
            if (PickUpArrayLength > 0 && !Config.AllowBreakables.Value)
            {
                bool HasBlacklisted = false;
                bool NoPickupComponent = false;
                Il2CppSystem.Collections.Generic.List<BreakableObject.SpawnDefinition> SpawnDefinitions = __instance._spawnDefinitions;
                //Iterate over the pick up array and check if any of the items are blacklisted
                for (int i = 0; i < PickUpArrayLength; i++)
                {
                    PickUp PickUpComponent = SpawnDefinitions[i]._prefab?.transform.GetComponent<PickUp>() ?? null;
                    if (PickUpComponent != null && ItemIdsBlacklistBreakable.Contains(PickUpComponent._itemId))
                    {
                        //if any is blacklisted set true and break out of loop
                        HasBlacklisted = true;
                        break;
                    }
                    if(PickUpComponent == null)
                    {
                        //if any do not have a pickup component we treat it as invalid. ( Fix for huts )
                        NoPickupComponent = true;
                        break;
                    }
                }
                if (!HasBlacklisted && !NoPickupComponent)
                {
                    //if not blacklisted and only includes pickup components store the hash
                    LootRespawnManager.MarkLootAsCollected(identifier);
                }
                return;
            }
            if (LootRespawnManager.IsLootCollected(identifier))
            {
                LootRespawnManager.RemoveLootFromCollected(identifier);
            }
        }
    }

    private static bool ShouldIdBeRemoved(int ItemId)
    {
        bool result = false;
        if (ItemIdsMeleeWeapons.Contains(ItemId) && !Config.AllowMelee.Value){ result = true; }
        if (ItemIdsRangedWeapons.Contains(ItemId) && !Config.AllowRanged.Value) { result = true; }
        if (ItemIdsMaterials.Contains(ItemId) && !Config.AllowMaterials.Value) { result = true; }
        if (ItemIdsAmmunition.Contains(ItemId) && !Config.AllowAmmunition.Value) { result = true; }
        if (ItemIdsExpendables.Contains(ItemId) && !Config.AllowExpendables.Value) { result = true; }
        if (ItemIdsFood.Contains(ItemId) && !Config.AllowFood.Value) { result = true; }
        if (ItemIdsThrowables.Contains(ItemId) && !Config.AllowThrowables.Value) { result = true; }
        if (ItemIdsMedicineAndEnergy.Contains(ItemId) && !Config.AllowMeds.Value) { result = true; }
        if (ItemIdsPlants.Contains(ItemId) && !Config.AllowPlants.Value) { result = true; }

        return result;
    }

    private static bool ShouldIdBeRemovedTimed(int ItemId)
    {
        bool result = false;
        if (ItemIdsMeleeWeapons.Contains(ItemId) && Config.AllowMeleeTimed.Value) { result = true; }
        if (ItemIdsRangedWeapons.Contains(ItemId) && Config.AllowRangedTimed.Value) { result = true; }
        if (ItemIdsMaterials.Contains(ItemId) && Config.AllowMaterialsTimed.Value) { result = true; }
        if (ItemIdsAmmunition.Contains(ItemId) && Config.AllowAmmunitionTimed.Value) { result = true; }
        if (ItemIdsExpendables.Contains(ItemId) && Config.AllowExpendablesTimed.Value) { result = true; }
        if (ItemIdsFood.Contains(ItemId) && Config.AllowFoodTimed.Value) { result = true; }
        if (ItemIdsThrowables.Contains(ItemId) && Config.AllowThrowablesTimed.Value) { result = true; }
        if (ItemIdsMedicineAndEnergy.Contains(ItemId) && Config.AllowMedsTimed.Value) { result = true; }
        if (ItemIdsPlants.Contains(ItemId) && Config.AllowPlantsTimed.Value) { result = true; }

        return result;
    }

    public static long GetTimestampFromGameTime(string gameTimeString)
    {
        // Parse the game time string
        string[] parts = gameTimeString.Split(' ');
        int day = int.Parse(parts[1]);
        string timeString = parts[2];

        // Parse the time string
        TimeSpan time = TimeSpan.Parse(timeString);

        // Calculate total seconds
        long timestamp = day * 24 * 60 * 60 + (long)time.TotalSeconds;

        return timestamp;
    }

    public static bool HasEnoughTimePassed(string identifier, long currentTimestamp)
    {
        long? originalTimestamp = GetLootTimestamp(identifier);
        if (originalTimestamp == null)
            return false; // No Timestamp found

        long timePassed = currentTimestamp - originalTimestamp.Value;
        long respawnTime = Config.TimeInDays.Value * 24 * 60 * 60;
        return timePassed >= respawnTime;
    }

    [DebugCommand("fullresetlootrespawncontrol")]
    private void FullResetLootRespawnControl()
    {
        LootRespawnManager.collectedLootIds = new HashSet<LootData>();
        SonsTools.ShowMessage("Loot Respawn Control: All picked up loot has been reset. Save your game and reload");
    }
}
public class LootRespawnManager
{
    public static HashSet<LootData> collectedLootIds = new HashSet<LootData>();

    public static string GenerateLootID(Vector3 position, Quaternion rotation)
    {
        string combinedString = $"{position.x:F6}-{position.y:F6}-{position.z:F6}-{rotation.x:F6}-{rotation.y:F6}-{rotation.z:F6}-{rotation.w:F6}";
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
            // Convert the byte array to hexadecimal string
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            
            return builder.ToString();
        }
    }

    public static string SaveCollectedLoot()
    {
        var serializableSet = new SerializableHashSet<LootData>(LootRespawnManager.collectedLootIds);
        return JsonConvert.SerializeObject(serializableSet);
    }

    public static void LoadCollectedLoot(string jsonData)
    {
        if(jsonData == null) {
            LootRespawnManager.collectedLootIds = new HashSet<LootData>();
            return; }
        var serializableSet = JsonConvert.DeserializeObject<SerializableHashSet<LootData>>(jsonData);
        
        LootRespawnManager.collectedLootIds = serializableSet.ToHashSet();
    }


    public static bool IsLootCollected(string identifier)
    {
        return collectedLootIds.Any(lootData => lootData.Hash == identifier);
    }

    public static void MarkLootAsCollected(string identifier)
    {
        long timestamp = LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString());
        collectedLootIds.Add(new LootData(identifier, timestamp));
    }

    public static void RemoveLootFromCollected(string identifier)
    {
        collectedLootIds.RemoveWhere(lootData => lootData.Hash == identifier);
    }

    public static long? GetLootTimestamp(string identifier)
    {
        LootData lootData = collectedLootIds.FirstOrDefault(ld => ld.Hash == identifier);
        return lootData != null ? lootData.Timestamp : null;
    }

    public class LootData
    {
        public string Hash { get; set; }
        public long Timestamp { get; set; }

        public LootData(string hash, long timestamp)
        {
            Hash = hash;
            Timestamp = timestamp;
        }
    }

    [Serializable]
    public class SerializableHashSet<T>
    {
        public List<T> list;
        public SerializableHashSet() { }
        public SerializableHashSet(HashSet<T> hashSet)
        {
            list = new List<T>(hashSet);
        }
        public HashSet<T> ToHashSet()
        {
            return new HashSet<T>(list);
        }
    }
}
[RegisterTypeInIl2Cpp]
public class LootIdentifier : MonoBehaviour
{
    public string Identifier { get; private set; }
    public void GenerateIdentifier()
    {
        Identifier = LootRespawnManager.GenerateLootID(transform.position, transform.rotation);
    }
    private void Awake()
    {
        GenerateIdentifier();
    }
    public string ReturnIdentifier()
    {
        return Identifier;
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
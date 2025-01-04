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
    private static class AwakePatch
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
                UnityEngine.Object.Destroy(__instance._destroyTarget);
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

    private static bool ShouldIdBeRemoved(int ItemId)
    {
        bool result = false;
        if (ItemIdsMeleeWeapons.Contains(ItemId) && !Config.AllowMelee.Value){ result = true; }
        if (ItemIdsRangedWeapons.Contains(ItemId) && !Config.AllowRanged.Value) { result = true; }
        if (ItemIdsMaterials.Contains(ItemId) && !Config.AllowMaterials.Value) { result = true; }
        if (ItemIdsAmmunition.Contains(ItemId) && !Config.AllowAmmunition.Value) { result = true; }
        if (ItemIdsExpendables.Contains(ItemId) && !Config.AllowExpandables.Value) { result = true; }
        if (ItemIdsFood.Contains(ItemId) && !Config.AllowFood.Value) { result = true; }
        if (ItemIdsThrowables.Contains(ItemId) && !Config.AllowThrowables.Value) { result = true; }
        if (ItemIdsMedicineAndEnergy.Contains(ItemId) && !Config.AllowMeds.Value) { result = true; }
        return result;
    }
}
public class LootRespawnManager
{
    public static HashSet<string> collectedLootIds = new HashSet<string>();

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
        var serializableSet = new SerializableHashSet<string>(LootRespawnManager.collectedLootIds);
        RLog.Msg(serializableSet.ToString());
        return JsonConvert.SerializeObject(serializableSet);
    }

    public static void LoadCollectedLoot(string jsonData)
    {
        if(jsonData == null) {
            LootRespawnManager.collectedLootIds = new HashSet<string>();
            return; }
        var serializableSet = JsonConvert.DeserializeObject<SerializableHashSet<string>>(jsonData);
        
        RLog.Msg(serializableSet.ToString());
        LootRespawnManager.collectedLootIds = serializableSet.ToHashSet();
    }


    public static bool IsLootCollected(string identifier)
    {
        return collectedLootIds.Contains(identifier);
    }

    public static void MarkLootAsCollected(string identifier)
    {
        collectedLootIds.Add(identifier);
    }

    public static void RemoveLootFromCollected(string identifier)
    {
        collectedLootIds.Remove(identifier);
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
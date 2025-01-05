using RedLoader;
using static RedLoader.RLog;

namespace LootRespawnControl;

public static class Config
{
    public static ConfigCategory Category { get; private set; }
    public static ConfigEntry<bool> UseTimerGlobal { get; private set; }
    public static ConfigEntry<int> TimeInDays { get; private set; }
    public static ConfigEntry<bool> AllowMelee { get; private set; }
    public static ConfigEntry<bool> AllowMeleeTimed { get; private set; }
    public static ConfigEntry<bool> AllowRanged { get; private set; }
    public static ConfigEntry<bool> AllowRangedTimed { get; private set; }
    public static ConfigEntry<bool> AllowWeaponMods { get; private set; }
    public static ConfigEntry<bool> AllowWeaponModsTimed { get; private set; }
    public static ConfigEntry<bool> AllowMaterials { get; private set; }
    public static ConfigEntry<bool> AllowMaterialsTimed { get; private set; }
    public static ConfigEntry<bool> AllowFood { get; private set; }
    public static ConfigEntry<bool> AllowFoodTimed { get; private set; }
    public static ConfigEntry<bool> AllowMeds { get; private set; }
    public static ConfigEntry<bool> AllowMedsTimed { get; private set; }
    public static ConfigEntry<bool> AllowPlants { get; private set; }
    public static ConfigEntry<bool> AllowPlantsTimed { get; private set; }
    public static ConfigEntry<bool> AllowAmmunition { get; private set; }
    public static ConfigEntry<bool> AllowAmmunitionTimed { get; private set; }
    public static ConfigEntry<bool> AllowThrowables { get; private set; }
    public static ConfigEntry<bool> AllowThrowablesTimed { get; private set; }
    public static ConfigEntry<bool> AllowExpendables { get; private set; }
    public static ConfigEntry<bool> AllowExpendablesTimed { get; private set; }
    public static ConfigEntry<bool> AllowBreakables { get; private set; }
    public static ConfigEntry<bool> AllowBreakablesTimed { get; private set; }
    //public static ConfigEntry<bool> SomeEntry { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("Timer Settings", "Timer Settings", "LootRespawnControl.cfg");

        UseTimerGlobal = Category.CreateEntry(
        "UseTimerGlobal",
        false,
        "Should all loot be allowed to respawn after X days?",
        "Enabling this will enable timed respawn for all categories");

        TimeInDays = Category.CreateEntry(
        "TimeInDays",
        7,
        "Time in Days",
        "How many ingame days need to pass for loot to once again respawn");
        TimeInDays.SetRange(1, 50); // Days of ingame time for respawn

        Category = ConfigSystem.CreateFileCategory("Weapons", "Weapons", "LootRespawnControl.cfg");

        //melee
        AllowMelee = Category.CreateEntry(
            "AllowMelee",
            false,
            "Allow Melee Weapons",
            "Allows melee weapons to respawn (Modern Axe, Fire Axe, Stun Baton...");

        AllowMeleeTimed = Category.CreateEntry(
            "AllowMeleeTimed",
            false,
            "Allow Melee Weapons to respawn with the Timer",
            "Allows melee weapons to respawn (Modern Axe, Fire Axe, Stun Baton...)");

        //ranged
        AllowRanged = Category.CreateEntry(
            "AllowRanged",
            false,
            "Allow Ranged Weapons",
            "Allows ranged weapons to respawn (Pistol, Shotgun, Crossbow...)");

        AllowRangedTimed = Category.CreateEntry(
            "AllowRangedTimed",
            false,
            "Allow Ranged Weapons to respawn with the Timer",
            "Allows ranged weapons to respawn (Pistol, Shotgun, Crossbow...)");

        //mods
        AllowWeaponMods = Category.CreateEntry(
            "AllowWeaponMods",
            false,
            "Allow Weapon Mods",
            "Allows weapon mods to respawn (Pistol Rail, Gun Flashlight...)");

        AllowWeaponModsTimed = Category.CreateEntry( // Added Timed entry
            "AllowWeaponModsTimed",
            false,
            "Allow Weapon Mods to respawn with the Timer",
            "Allows weapon mods to respawn (Pistol Rail, Gun Flashlight...)");

        //ammo
        AllowAmmunition = Category.CreateEntry(
            "AllowAmmunition",
            false,
            "Allow Ammunition",
            "Allows ammunition to respawn (Pistol ammo, Arrows, Bolts...)");

        AllowAmmunitionTimed = Category.CreateEntry( // Added Timed entry
            "AllowAmmunitionTimed",
            false,
            "Allow Ammunition to respawn with the Timer",
            "Allows ammunition to respawn (Pistol ammo, Arrows, Bolts...)");


        //throwables
        AllowThrowables = Category.CreateEntry(
            "AllowThrowables",
            false,
            "Allow Throwables",
            "Allows throwables to respawn (Grenades, Sticky Bombs, Golf Balls...)");

        AllowThrowablesTimed = Category.CreateEntry( // Added Timed entry
            "AllowThrowablesTimed",
            false,
            "Allow Throwables to respawn with the Timer",
            "Allows throwables to respawn (Grenades, Sticky Bombs, Golf Balls...)");

        Category = ConfigSystem.CreateFileCategory("Pickups", "Pickups", "LootRespawnControl.cfg");

        //materials
        AllowMaterials = Category.CreateEntry(
            "AllowMaterials",
            false,
            "Allow Materials",
            "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");

        AllowMaterialsTimed = Category.CreateEntry( // Added Timed entry
            "AllowMaterialsTimed",
            false,
            "Allow Materials to respawn with the Timer",
            "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");


        //food
        AllowFood = Category.CreateEntry(
            "AllowFood",
            false,
            "Allow Food",
            "This allows food to respawn (Cat Food, Cereal, MRE packs...)");

        AllowFoodTimed = Category.CreateEntry( // Added Timed entry
            "AllowFoodTimed",
            false,
            "Allow Food to respawn with the Timer",
            "This allows food to respawn (Cat Food, Cereal, MRE packs...)");

        //meds
        AllowMeds = Category.CreateEntry(
            "AllowMeds",
            false,
            "Allow Medicine & Energy",
            "Allows medicine and energy drinks to respawn");

        AllowMedsTimed = Category.CreateEntry( // Added Timed entry
            "AllowMedsTimed",
            false,
            "Allow Medicine & Energy to respawn with the Timer",
            "Allows medicine and energy drinks to respawn");


        //plants
        AllowPlants = Category.CreateEntry(
            "AllowPlants",
            false,
            "Allow plants to spawn",
            "Allows plants to respawn(Aloe Vera, Mushrooms, Chicory...)");

        AllowPlantsTimed = Category.CreateEntry( // Added Timed entry
            "AllowPlantsTimed",
            false,
            "Allow plants to spawn with the Timer",
            "Allows plants to respawn(Aloe Vera, Mushrooms, Chicory...)");

        Category = ConfigSystem.CreateFileCategory("Miscellaneous", "Miscellaneous", "LootRespawnControl.cfg");

        //expendables
        AllowExpendables = Category.CreateEntry(
            "AllowExpendables",
            false,
            "Allow Expendables",
            "Allows expendables to respawn (Air Canisters, Printer Ink, Hide Bags)");

        AllowExpendablesTimed = Category.CreateEntry( // Added Timed entry
            "AllowExpendablesTimed",
            false,
            "Allow Expendables to respawn with the Timer",
            "Allows expendables to respawn (Air Canisters, Printer Ink, Hide Bags)");


        //breakables
        AllowBreakables = Category.CreateEntry(
            "AllowBreakables",
            false,
            "Allow Breakables",
            "Allows breakable objects to respawn (Laptops, Gore Vases...)");

        AllowBreakablesTimed = Category.CreateEntry( // Added Timed entry
            "AllowBreakablesTimed",
            false,
            "Allow Breakables to respawn with the Timer",
            "Allows breakable objects to respawn (Laptops, Gore Vases...)");
    }

    // Same as the callback in "CreateSettings". Called when the settings ui is closed.
    public static void OnSettingsUiClosed()
    {
    }
}
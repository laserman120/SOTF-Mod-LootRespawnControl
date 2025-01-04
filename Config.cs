using RedLoader;

namespace LootRespawnControl;

public static class Config
{
    public static ConfigCategory Category { get; private set; }
    public static ConfigEntry<bool> AllowMelee { get; private set; }
    public static ConfigEntry<bool> AllowRanged { get; private set; }
    public static ConfigEntry<bool> AllowWeaponMods { get; private set; }
    public static ConfigEntry<bool> AllowMaterials { get; private set; }
    public static ConfigEntry<bool> AllowFood { get; private set; }
    public static ConfigEntry<bool> AllowMeds { get; private set; }
    public static ConfigEntry<bool> AllowPlants { get; private set; }
    public static ConfigEntry<bool> AllowAmmunition { get; private set; }
    public static ConfigEntry<bool> AllowThrowables { get; private set; }
    public static ConfigEntry<bool> AllowExpendables { get; private set; }
    public static ConfigEntry<bool> AllowBreakables { get; private set; }
    //public static ConfigEntry<bool> SomeEntry { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("LootRespawnControl", "LootRespawnControl", "LootRespawnControl.cfg");

        AllowMelee = Category.CreateEntry(
        "AllowMelee",
        false,
        "Allow Melee Weapons",
        "Allows melee weapons to respawn (Modern Axe, Fire Axe, Stun Baton...");

        AllowRanged = Category.CreateEntry(
        "AllowRanged",
        false,
        "Allow Ranged Weapons",
        "Allows ranged weapons to respawn (Pistol, Shotgun, Crossbow...)");

        AllowWeaponMods = Category.CreateEntry(
        "AllowWeaponMods ",
        false,
        "Allow Weapon Mods",
        "Allows weapon mods to respawn (Pistol Rail, Gun Flashlight...)");

        AllowMaterials = Category.CreateEntry(
        "AllowMaterials",
        false,
        "Allow Materials",
        "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");

        AllowFood = Category.CreateEntry(
        "AllowFood",
        false,
        "Allow Food",
        "This allows food to respawn (Cat Food, Cereal, MRE packs...)");

        AllowMeds = Category.CreateEntry(
        "AllowMeds",
        false,
        "Allow Medicine & Energy",
        "Allows medicine and energy drinks to respawn");

        AllowPlants = Category.CreateEntry(
        "AllowPlants",
        false,
        "Allow plants to spawn",
        "Allows plants to respawn(Aloe Vera, Mushrooms, Chicory...)");

        AllowAmmunition = Category.CreateEntry(
        "AllowAmmunition",
        false,
        "Allow Ammunition",
        "Allows ammunition to respawn (Pistol ammo, Arrows, Bolts...)");

        AllowThrowables = Category.CreateEntry(
        "AllowThrowables",
        false,
        "Allow Throwables",
        "Allows throwables to respawn (Grenades, Sticky Bombs, Golf Balls...)");

        AllowExpendables = Category.CreateEntry(
        "AllowExpendables",
        false,
        "Allow Expendables",
        "Allows expendables to respawn (Air Canisters, Printer Ink, Hide Bags)");

        AllowBreakables = Category.CreateEntry(
        "AllowBreakables",
        false,
        "Allow Breakables",
        "Allows breakable objects to respawn (Laptops, Gore Vases...)");
    }

    // Same as the callback in "CreateSettings". Called when the settings ui is closed.
    public static void OnSettingsUiClosed()
    {
    }
}
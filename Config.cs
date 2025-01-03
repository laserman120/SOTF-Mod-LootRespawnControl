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
    public static ConfigEntry<bool> AllowAmmunition { get; private set; }
    public static ConfigEntry<bool> AllowThrowables { get; private set; }
    public static ConfigEntry<bool> AllowExpandables { get; private set; }

    //public static ConfigEntry<bool> SomeEntry { get; private set; }

    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("LootRespawnControl", "LootRespawnControl", "LootRespawnControl.cfg");

        AllowMelee = Category.CreateEntry(
        "AllowMelee",
        false,
        "Allow Melee Weapons",
        "Allows melee weapons to respawn");

        AllowRanged = Category.CreateEntry(
        "AllowRanged",
        false,
        "Allow Ranged Weapons",
        "Allows ranged weapons to respawn");

        AllowWeaponMods = Category.CreateEntry(
        "AllowWeaponMods ",
        false,
        "Allow Weapon Mods",
        "Allows weapon mMods to respawn");

        AllowMaterials = Category.CreateEntry(
        "AllowMaterials",
        false,
        "Allow Materials",
        "Allows crafting materials to respawn (Duct Tape, Rope, Coins...)");

        AllowFood = Category.CreateEntry(
        "AllowFood",
        false,
        "Allow Food",
        "Allows food to respawn (Cat Food, Meat Cubes, Cereal...)");

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

        AllowExpandables = Category.CreateEntry(
        "AllowExpandables",
        false,
        "Allow Expandables",
        "Allows expandables to respawn (Air Canisters, Printer Ink, Hide Bags)");
    }

    // Same as the callback in "CreateSettings". Called when the settings ui is closed.
    public static void OnSettingsUiClosed()
    {
    }
}
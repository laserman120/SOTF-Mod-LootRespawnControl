using RedLoader;
using static RedLoader.RLog;

namespace LootRespawnControl;


public static class ConfigWeapons
{
    public static ConfigCategory Category { get; private set; }
    public static ConfigEntry<bool> AllowMelee { get; private set; }
    public static ConfigEntry<bool> AllowMeleeTimed { get; private set; }
    public static ConfigEntry<bool> AllowRanged { get; private set; }
    public static ConfigEntry<bool> AllowRangedTimed { get; private set; }
    public static ConfigEntry<bool> AllowWeaponMods { get; private set; }
    public static ConfigEntry<bool> AllowWeaponModsTimed { get; private set; }


    public static void Init()
    {
        Category = ConfigSystem.CreateFileCategory("Timer Settings", "Timer Settings", "LootRespawnControl.cfg");

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
}
}
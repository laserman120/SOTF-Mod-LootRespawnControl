# Loot Respawn Control - Sons of the Forest Mod

This mod allows you to control the respawning of loot in Sons of the Forest. You can prevent collected loot from respawning, configure respawn timers, and manage loot synchronization in multiplayer.

## Important Configuration Notes

* **In-Game Changes:** Configuration changes made while in-game will only take effect after reloading the save or restarting the game.
* **Location of Config File:** `Sons Of The Forest\UserData\LootRespawnControl.cfg`

## Base Configuration

* **"Should pickups be synced across players?" (Default: False)**
    * Enables multiplayer synchronization of collected loot. See the "Networking" section for more details.
* **"Should all loot be allowed to respawn after X days?" (Default: False)**
    * If enabled, overrides individual "use timer" options and allows all loot to respawn after a set number of in-game days.
* **"Time in Days" (Default: 7)**
    * Sets the number of in-game days required for loot to respawn when the "Should all loot be allowed to respawn after X days?" or individual group timers are enabled.

## Loot Groups

Each loot group has the following configuration options:

* **"Allow [Group]" (Default: False)**
    * Allows the specified loot group to respawn normally when the save is reloaded.
* **"Allow [Group] to respawn with the Timer" (Default: False)**
    * Allows the specified loot group to respawn after the "Time in Days" has passed.
* **"Block [Group]" (Default: False)**
    * Completely prevents the specified loot group from spawning.
* **"Sync [Group]" (Default: True)**
    * Synchronizes the specified loot group between players when "Should pickups be synced across players?" is enabled.

## Default Loot Group Item IDs

Here are the item IDs for the default loot groups. You can use these as a reference for your custom lists.

* **Melee Weapons:**
    ```
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
    ```
* **Ranged Weapons:**
    ```
    358, // shotgun
    360, // block bow
    361, // rifle
    386, // revolver
    443, // homemade bow
    459, // slingshot
    353, // taser
    365, // crossbow
    355  // pistol
    ```
* **Weapon Mods:**
    ```
    346, // shotgun mount
    374, // silencer
    375, // laser sight
    376, // pistol mount
    378  // gun flashlight
    ```
* **Materials:**
    ```
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
    ```
* **Food:**
    ```
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
    ```
* **Medicine and Energy:**
    ```
    437, // pills
    441, // candy bar
    439  // energy drink
    ```
* **Plants:**
    ```
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
    ```
* **Ammunition:**
    ```
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
    ```
* **Throwables:**
    ```
    524, // golf ball
    381, // grenade
    417, // sticky bomb
    440, // signal fire
    ```
* **Expendables:**
    ```
    390, // printer resin
    469, // air tank
    508  // hide bag
    ```

## Custom Categories

Customize your experience further by adding specific item IDs to these lists:

* **"Custom Whitelist"**
    * Items in this list will always respawn.
* **"Custom Whitelist with Timer"**
    * Items in this list will respawn after the "Time in Days" has passed.
* **"Blacklist"**
    * Items in this list will never spawn.
* **"Custom Networking"**
    * Items in this list will be synchronized between players when "Should pickups be synced across players?" is enabled.

**Adding Item IDs:**

* Separate item IDs with a semicolon (;).
    * Example: `437; 634`

**Extending Lists:**

* If you need to add more entries than the in-game UI allows, you can directly edit the `LootRespawnControl.cfg` file.

**Example Configuration:**

```ini
[Custom Options]
Whitelist = "356; 367; 437; 415; 503"  // Modern Axe, Katana, Pills, Cloth, Torch
WhitelistTimed = "433; 436"  // Meat, Fish
Blacklist = "394; 361; 420; 524"  // Chainsaw, Rifle, C4 Brick, Golf Ball
networkList = "358; 365; 634"  // Shotgun, Crossbow, Solar Panel
```

## Networking

* **Enabling Synchronization:** To synchronize loot across players, enable the "Should pickups be synced across players?" option. When any player collects loot, it will be removed for everyone else as well.
* **Host Configuration:** When a client connects to a server with networking enabled, the client will use the host's configuration to ensure consistency. The client's local configuration will be ignored.
* **Mod Compatibility:** All players must have the mod installed and use the same version to join a networked game. Players without the mod or with outdated versions will not be able to connect.

## Good to Know / Important Interactions

* **Enforcing Configuration in Multiplayer:**
    * You can enforce your configuration on other players in multiplayer by enabling "Should pickups be synced across players?" while disabling all individual "Sync [Group]" options. This will force clients to use the host's configuration without synchronizing individual loot pickups.
* **Respawning with Timers in Multiplayer:**
    * When both global or group-specific "Allow [Group] to respawn with the Timer" options and "Should pickups be synced across players?" are enabled, loot will only respawn after the host restarts the game. Clients restarting their game will not trigger loot respawns based on timers.
* **Whitelist and Blacklist Priority:**
    * If an item is present in both a whitelist and a blacklist, the blacklist will take priority, and the item will not spawn.
* **Client config is ignored:**
    * When connecting to a host with networking enabled the clients config file will be ignored, and the clients game will use the hosts config.


## Console Logging (Debugging)

* **Purpose:** This option is for debugging and troubleshooting. It logs various events, including pickups, deletions, networking, conditions, and error handling.
* **Performance:** Enabling logging can cause performance issues when using tools like Unity Explorer with the console enabled. It is recommended to keep logging disabled during normal gameplay.
* **Bug Reporting:** If you encounter a bug or unexpected issue, enable logging, reproduce the issue, and include the log file with your bug report. For networking issues like desyncs, both the host and client should provide log files.
* **Log File Location:** `Sons Of The Forest\_Redloader\Latest.log`

## Special Thanks

* **TheSuperSnooper:** For helping me test and debug the mod as well as tons of feedback
* **Spidy:** For testing and providing feedback
* **ToniMacaroni:** For redloader and all his help during its creation

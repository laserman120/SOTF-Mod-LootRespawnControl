using Bolt;
using Construction;
using Endnight.Utilities;
using LootRespawnControl;
using LootRespawnControl.Managers;
using RedLoader;
using Sons.Environment;
using Sons.Gameplay;
using Sons.Items.Core;
using Sons.Prefabs;
using SonsSdk;
using UnityEngine;

namespace LootRespawnControl.Managers
{
    internal class TimedLootRespawnManager
    {
        public static GameObject LootRespawnManager;
        public static RespawnDataHolderManager RespawnDataHolderManager;
        public static List<RespawnDataHolder> RespawnDataHoldersAwaitingRespawn = new List<RespawnDataHolder>();
        public static float RespawnCheckRange = 50f;

        public static void IntitializeManager()
        {
            // Initialize the manager
            LootRespawnManager = new GameObject("LootRespawnManager");
            RespawnDataHolderManager = LootRespawnManager.AddComponent<RespawnDataHolderManager>();

            DebugManager.ConsoleLog("LootRespawnManager Initialized");
        }

        public static void CreateRespawnDataHolder(GameObject destroyTarget, LootIdentifier lootIdentifier, int id)
        {
            //Check if it already exists
            if(RespawnDataHolderManager.DoesLootAlreadyExist("DataHolder-" + lootIdentifier.Identifier)) { return; }

            GameObject lootRespawnDataHolder = new GameObject("DataHolder-" + lootIdentifier.Identifier);
            lootRespawnDataHolder.transform.parent = LootRespawnManager.transform; // Set the parent to the LootRespawnManager
            //Set the location to the location of the current target, ignore original position due to endnight being wunky funky in their spawning logic, ffs
            lootRespawnDataHolder.transform.position = destroyTarget.transform.position;
            lootRespawnDataHolder.transform.rotation = destroyTarget.transform.rotation;
            lootRespawnDataHolder.layer = LayerMask.NameToLayer("Player"); // Set the layer to Player to avoid collision with other objects

            RespawnDataHolder dataHolder = lootRespawnDataHolder.AddComponent<RespawnDataHolder>();
            dataHolder.identifier = lootIdentifier.Identifier;
            dataHolder.position = destroyTarget.transform.position;
            dataHolder.rotation = destroyTarget.transform.rotation;
            dataHolder.lootName = lootIdentifier.LootName;
            dataHolder.id = id;
            dataHolder.isBreakable = id == LootRespawnControl._breakableId;
            dataHolder.timestamp = LootManager.LootRespawnManager.GetLootTimestamp(lootIdentifier.Identifier);

            GameObject respawnTarget = GameObject.Instantiate(destroyTarget);
            respawnTarget.active = false;
            respawnTarget.transform.parent = lootRespawnDataHolder.transform; // Set the parent to the LootRespawnManager
            respawnTarget.transform.localPosition = Vector3.zero;
            dataHolder._respawnTarget = respawnTarget;

            //if we have a breakable we keep a reference to the original object
            if(id == LootRespawnControl._breakableId)
            {
                dataHolder._originalTarget = destroyTarget;
            }
            
            RespawnDataHolderManager.AddLootToRespawnManagerList(lootRespawnDataHolder);
        }

        public static void RespawnItem(RespawnDataHolder dataHolder)
        {
            //setup loot
            GameObject respawnedTarget = GameObject.Instantiate(dataHolder._respawnTarget);
            respawnedTarget.transform.position = dataHolder.position;
            respawnedTarget.transform.rotation = dataHolder.rotation;
            respawnedTarget.name = dataHolder.lootName;

            //enforce identifier
            LootIdentifier lootIdentifier = respawnedTarget.GetComponentInChildren<LootIdentifier>();
            lootIdentifier.Identifier = dataHolder.identifier;
            lootIdentifier.enforceIdentifier = true;
            respawnedTarget.SetActive(true);
            DebugManager.ConsoleLog($"Respawned loot: {dataHolder.lootName}...");
        }

        public static void RespawnBreakable(RespawnDataHolder dataHolder)
        {
            //Remove original object if it still exists
            if(dataHolder._originalTarget != null)
            {
                GameObject.Destroy(dataHolder._originalTarget);
            }

            //setup loot
            GameObject respawnedTarget = GameObject.Instantiate(dataHolder._respawnTarget);
            respawnedTarget.transform.position = dataHolder.position;
            respawnedTarget.transform.rotation = dataHolder.rotation;
            respawnedTarget.name = dataHolder.lootName;

            BreakableObject breakableObjectComponent = respawnedTarget.GetComponent<BreakableObject>();

            if(breakableObjectComponent == null)
            {
                DebugManager.ConsoleLog("Failed to find breakable object component!");
                return;
            }

            //Verify the original object is not destroyed
            breakableObjectComponent._originalObject?.SetActive(true);
            breakableObjectComponent._brokenShowObject?.SetActive(false);

            Sons.Weapon.DamageNode damageNode = respawnedTarget.GetComponentInChildren<Sons.Weapon.DamageNode>();
            Sons.Ai.VailCollisionTags vailCollisionTags = respawnedTarget.GetComponentInChildren<Sons.Ai.VailCollisionTags>();

            if(damageNode != null) damageNode.enabled = true;
            if(vailCollisionTags != null) vailCollisionTags.enabled = true;

            //enforce identifier
            LootIdentifier lootIdentifier = respawnedTarget.GetComponentInChildren<LootIdentifier>();
            lootIdentifier.Identifier = dataHolder.identifier;
            lootIdentifier.enforceIdentifier = true;
            respawnedTarget.SetActive(true);
            DebugManager.ConsoleLog($"Respawned breakable object: {dataHolder.lootName}...");
        }


        public static void CheckForRespawn()
        {
            List<RespawnDataHolder> respawnCandidates = RespawnDataHoldersAwaitingRespawn.ToList();

            foreach (var dataHolder in respawnCandidates)
            {
                dataHolder.CheckForRespawn();
            }
        }

        public static void ForceRespawnAll()
        {
            RespawnDataHolderManager.ForceRespawnAll();
        }

        public static void SetLootRespawnDistance(float distance)
        {
            RespawnDataHolderManager.SetLootRespawnDistance(distance);
        }
    }
}

[RegisterTypeInIl2Cpp]
public class RespawnDataHolder : MonoBehaviour  
{
    public string identifier;
    public Vector3 position;
    public Quaternion rotation;
    public int id;
    public string lootName;
    public long? timestamp;
    public bool isBreakable;
    public GameObject _respawnTarget;
    public GameObject _originalTarget;
    SphereCollider triggerCollider;

    private float triggerRadius = TimedLootRespawnManager.RespawnCheckRange;

    private void Start()
    {
        triggerCollider = gameObject.AddComponent<SphereCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = triggerRadius;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name != "LocalPlayer") return;

        CheckForRespawn();
    }

    public void CheckForRespawn()
    {
        if(!CanBeRespawned()) return;

        if (BoltNetwork.isRunning)
        {
            //We are on a server
            if (BoltNetwork.isServer)
            {
                //We are the host
                //Directly send message to all clients to remove from collected
                TryRespawn();
                NetworkManager.SendRespawnEvent(identifier);
                return;
            }
            else if (BoltNetwork.isClient)
            {
                //We are a client
                if (!LootManager.LootRespawnManager.IsLootCollected(identifier))
                {
                    //Directly attempt respawning when the hash is no longer in the list
                    TryRespawn();
                    if (TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Contains(this))
                    {
                        TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Remove(this);
                    }
                    return;
                }
                //if it is not yet included in the awaiting list
                //Send network request to verify if we should respawn
                //Add us to the list of respawn data holders awaiting respawn
                if (!TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Contains(this))
                {
                    TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Add(this);
                    NetworkManager.SendRespawnRequest(lootName, identifier, id, isBreakable);
                    DebugManager.ConsoleLog($"Requesting loot respawn for {lootName} {identifier}");
                }
                
                return;
            }
        }

        //We are not on a server
        TryRespawn();
    }

    private bool CanBeRespawned()
    {
        if (!LootManager.LootRespawnManager.IsLootCollected(identifier))
        {
            return true;
        }
        if (isBreakable)
        {
            if (ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())) || ConfigManager.allowBreakablesTimed && LootRespawnControl.LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
            {
                return true;
            }
        }
        else
        {
            if (ConfigManager.IsGlobalTimerEnabled() && LootRespawnControl.LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())) || ConfigManager.ShouldIdBeRemovedTimed(id) && LootRespawnControl.LootRespawnControl.HasEnoughTimePassed(identifier, LootRespawnControl.LootRespawnControl.GetTimestampFromGameTime(TimeOfDayHolder.GetTimeOfDay().ToString())))
            {
                return true;
            }
        }
        return false;
    }

    private void TryRespawn()
    {
        if (!CanBeRespawned()) return;
        if (TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Contains(this))
        {
            TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Remove(this);
        }
        if (isBreakable)
        {
            RespawnBreakable();
        }
        else
        {

            RespawnPickup();
        }
    }

    public void ForceRespawn()
    {
        if (TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Contains(this))
        {
            TimedLootRespawnManager.RespawnDataHoldersAwaitingRespawn.Remove(this);
        }
        if (isBreakable)
        {
            RespawnBreakable();
        }
        else
        {
            RespawnPickup();
        }
    }

    private void RespawnPickup()
    {
        //respawn the item
        TimedLootRespawnManager.RespawnItem(this);
        //remove the loot from the respawn manager
        LootManager.LootRespawnManager.RemoveLootFromCollected(identifier);
        //remove the loot from the respawn data holder manager
        RespawnDataHolderManager.RemoveLootFromRespawnManagerList(gameObject);
        //destroy the data holder

        DebugManager.ConsoleLog($"Respawned: {lootName}...");
        Destroy(gameObject);
    }

    private void RespawnBreakable()
    {
        //respawn the item
        TimedLootRespawnManager.RespawnBreakable(this);
        //remove the loot from the respawn manager
        LootManager.LootRespawnManager.RemoveLootFromCollected(identifier);
        //remove the loot from the respawn data holder manager
        RespawnDataHolderManager.RemoveLootFromRespawnManagerList(gameObject);
        //destroy the data holder

        DebugManager.ConsoleLog($"Respawned: {lootName}...");
        Destroy(gameObject);
    }

    public void setTriggerRadius(float radius)
    {
        triggerRadius = radius;
        triggerCollider.radius = triggerRadius;
    }
}

[RegisterTypeInIl2Cpp]
public class RespawnDataHolderManager : MonoBehaviour
{
    public static List<GameObject> LootRespawnManagerList = new List<GameObject>();
    public static List<RespawnDataHolder> ActiveRespawnDataHolders = new List<RespawnDataHolder>();
    public static List<string> LootRespawnControlList = new List<string>();

    public static void AddLootToRespawnManagerList(GameObject loot)
    {
        LootRespawnManagerList.Add(loot);
        LootRespawnControlList.Add(loot.name);
        if (loot.GetComponent<RespawnDataHolder>() != null)
        {
            ActiveRespawnDataHolders.Add(loot.GetComponent<RespawnDataHolder>());
        }

    }

    public static void RemoveLootFromRespawnManagerList(GameObject loot)
    {
        if (LootRespawnControlList.Contains(loot.name))
        {
            LootRespawnManagerList.Remove(loot);
            LootRespawnControlList.Remove(loot.name);
            ActiveRespawnDataHolders.Remove(loot.GetComponent<RespawnDataHolder>());
        }
    }

    public static bool DoesLootAlreadyExist(string lootName)
    {
        return LootRespawnControlList.Contains(lootName);
    }

    public static void SetLootRespawnDistance(float distance)
    {
        foreach(var loot in ActiveRespawnDataHolders)
        {
            loot.setTriggerRadius(distance);
        }
    }

    public static void ForceRespawnAll()
    {
        foreach(var loot in ActiveRespawnDataHolders)
        {
            loot.ForceRespawn();
        }
    }
}

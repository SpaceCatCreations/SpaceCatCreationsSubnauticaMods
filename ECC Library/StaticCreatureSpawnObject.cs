using ECCLibrary;
using ECCLibrary.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UWE;

namespace ECCLibrary
{
    /// <summary>
    /// Static class related to static spawns, such as those of Leviathans in the base game.
    /// </summary>
    public static class StaticCreatureSpawns
    {
        /// <summary>
        /// Allow a new creature to spawn in a specific location. Call this in your patch method.
        /// </summary>
        /// <param name="spawnData">Information related to this specific StaticSpawn.</param>
        public static void RegisterStaticSpawn(StaticSpawn spawnData)
        {
            foreach(var spawn in staticSpawns)
            {
                if(spawn.uniqueIdentifier == spawnData.uniqueIdentifier)
                {
                    ECCLog.AddMessage("Multiple StaticSpawns that share the Unique Identifier '{0}'.", spawnData.uniqueIdentifier);
                }
            }
            staticSpawns.Add(spawnData);
        }
        internal static readonly List<StaticSpawn> staticSpawns = new List<StaticSpawn>();

        internal static bool HasSpawnedInCurrentSave(string id)
        {
            using (StreamReader sr = new StreamReader(ECCPatch.staticCreaturesPath))
            {
                string text = sr.ReadToEnd();
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                string[] alreadySpawned = text.Split(',');
                foreach (string str in alreadySpawned)
                {
                    if (str == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void InstantiateAllPrefabSpawners()
        {
            foreach(StaticSpawn spawnData in staticSpawns)
            {
                CreatePrefabSpawner(spawnData);
            }
        }
        private static void CreatePrefabSpawner(StaticSpawn spawnData)
        {
            if (HasSpawnedInCurrentSave(spawnData.uniqueIdentifier))
            {
                return;
            }
            GameObject spawnObject = new GameObject(string.Format("spawner-{0}", spawnData.uniqueIdentifier));
            spawnObject.AddComponent<StaticCreatureSpawnObject>().InitializeInstance(spawnData);
        }
    }
    internal class StaticCreatureSpawnObject : MonoBehaviour
    {
        public StaticSpawn mySpawnData;
        const float checkDelay = 1f;
        const float doubleCheckDelay = 1f;

        public void InitializeInstance(StaticSpawn spawnData)
        {
            mySpawnData = spawnData;
            StartCoroutine(SpawnSchedule(Random.value));
        }

        private IEnumerator SpawnSchedule(float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);
            for (; ; )
            {
                yield return new WaitForSeconds(checkDelay);
                if (InSpawningDistance())
                {
                    if (StaticCreatureSpawns.HasSpawnedInCurrentSave(mySpawnData.uniqueIdentifier))
                    {
                        Destroy(gameObject);
                        yield break;
                    }
                    yield return new WaitForSeconds(doubleCheckDelay);
                    if (InSpawningDistance())
                    {
                        Spawn();
                    }
                }
            }
        }

        private bool InSpawningDistance()
        {
            if (Vector3.Distance(MainCamera.camera.transform.position, mySpawnData.position) > mySpawnData.maxDistance)
            {
                return false;
            }
            return true;
        }

        GameObject GetPrefab()
        {
            GameObject prefab = null;
            if(mySpawnData.spawnType == StaticSpawn.SpawnType.TechType)
            {
                prefab = CraftData.GetPrefabForTechType(mySpawnData.prefab);
            }
            else if(mySpawnData.spawnType == StaticSpawn.SpawnType.ClassID)
            {
                if (!PrefabDatabase.TryGetPrefab(mySpawnData.classId, out prefab))
                {
                    ECCLog.AddMessage("No prefab found by classId {0}.", mySpawnData.classId);
                }
            }
            if (prefab == null)
            {
                ECCLog.AddMessage("Warning: StaticCreatureSpawnObject failed for Unique Spawn {0}.", mySpawnData.uniqueIdentifier);
            }
            return prefab;
        }
        void Spawn()
        {
            GameObject obj = UWE.Utils.InstantiateDeactivated(GetPrefab(), mySpawnData.position, Quaternion.identity);
            LargeWorldEntity lwe = obj.GetComponent<LargeWorldEntity>();
            bool active = LargeWorld.main.streamer.cellManager.RegisterEntity(lwe);
            if (active)
            {
                obj.SetActive(true);
                Destroy(gameObject);
                using (StreamWriter sw = new StreamWriter(ECCPatch.staticCreaturesPath, true))
                {
                    sw.Write(mySpawnData.uniqueIdentifier + ",");
                }
            }
            else //Cells take a little while to load in, this is for in a case where that happens
            {
                Destroy(obj);
            }
        }
    }
    /// <summary>
    /// Settings based around a creature that will always spawn in the same position(s), and is separate from the LootDistributionData.
    /// </summary>
    public struct StaticSpawn
    {
        internal SpawnType spawnType;
        public TechType prefab;
        public string classId;
        public Vector3 position;
        public string uniqueIdentifier;
        public float maxDistance;

        /// <summary>
        /// Constructor for this struct.
        /// </summary>
        /// <param name="prefab">The Creature to be spawned.</param>
        /// <param name="position">World position of the object's spawn.</param>
        /// <param name="uniqueIdentifier">An ID that must be unique to all other static spawns.</param>
        /// <param name="maxDistance">The creature will attempt to spawn when wihin this distance. Note: Non-global creatures cannot spawn in areas that have not fully loaded.</param>
        public StaticSpawn(CreatureAsset prefab, Vector3 position, string uniqueIdentifier, float maxDistance)
        {
            this.prefab = prefab.TechType;
            this.position = position;
            this.uniqueIdentifier = uniqueIdentifier;
            this.maxDistance = maxDistance;
            spawnType = SpawnType.TechType;
            classId = string.Empty;
        }
        /// <summary>
        /// Constructor for this struct.
        /// </summary>
        /// <param name="prefab">The TechType to be spawned.</param>
        /// <param name="position">World position of the object's spawn.</param>
        /// <param name="uniqueIdentifier">An ID that must be unique to all other static spawns.</param>
        /// <param name="maxDistance">The creature will attempt to spawn when wihin this distance. Note: Non-global creatures cannot spawn in areas that have not fully loaded.</param>
        public StaticSpawn(TechType prefab, Vector3 position, string uniqueIdentifier, float maxDistance)
        {
            this.prefab = prefab;
            this.position = position;
            this.uniqueIdentifier = uniqueIdentifier;
            this.maxDistance = maxDistance;
            spawnType = SpawnType.TechType;
            classId = string.Empty;
        }
        /// <summary>
        /// Constructor for this struct.
        /// </summary>
        /// <param name="classId">The ClassId of the prefab to be spawned.</param>
        /// <param name="position">World position of the object's spawn.</param>
        /// <param name="uniqueIdentifier">An ID that must be unique to all other static spawns.</param>
        /// <param name="maxDistance">The creature will attempt to spawn when wihin this distance. Note: Non-global creatures cannot spawn in areas that have not fully loaded.</param>
        public StaticSpawn(string classId, Vector3 position, string uniqueIdentifier, float maxDistance)
        {
            this.classId = classId;
            this.prefab = TechType.None;
            this.position = position;
            this.uniqueIdentifier = uniqueIdentifier;
            this.maxDistance = maxDistance;
            spawnType = SpawnType.ClassID;
        }

        public enum SpawnType
        {
            TechType,
            ClassID
        }
    }
}

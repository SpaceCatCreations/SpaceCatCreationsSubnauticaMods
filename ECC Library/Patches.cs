using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using SMLHelper.V2.Utility;
using System.IO;
using ECCLibrary;

namespace ECCLibrary
{
    internal class Patches
    {
        [HarmonyPatch(typeof(LargeWorldStreamer), "Initialize")]
        public static class LWSInitPatch
        {
            [HarmonyPostfix()]
            public static void Postfix()
            {
                string directory = SaveUtils.GetCurrentSaveDataDir();
                ECCPatch.staticCreaturesPath = Path.Combine(directory, "StaticCreatures.data");
                if (!File.Exists(ECCPatch.staticCreaturesPath))
                {
                    FileStream stream = File.Create(ECCPatch.staticCreaturesPath, 4096);
                    stream.Close();
                }
                StaticCreatureSpawns.InstantiateAllPrefabSpawners();
            }
        }
    }
}

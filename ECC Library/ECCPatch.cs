using QModManager.API.ModLoading;
using UnityEngine;
using SMLHelper.V2.Handlers;
using HarmonyLib;
using System.Reflection;

namespace ECCLibrary
{
    [QModCore]
    public class ECCPatch
    {
        public static string staticCreaturesPath = "uninitialized";
        public static ECCConfig config = OptionsPanelHandler.Main.RegisterModOptions<ECCConfig>();

        [QModPatch]
        public static void Patch()
        {
            LanguageHandler.SetLanguageLine("EncyPath_Lifeforms/Fauna/Eggs", "Creature Eggs");
            Harmony harmony = new Harmony("Lee23.ECC");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Internal.ECCLog.AddMessage("Eel's Creature Creator loaded.");
        }

        private static void MaterialDebug_Unused(TechType techType)
        {
            GameObject peeper = CraftData.GetPrefabForTechType(techType);
            foreach (Renderer renderer in peeper.GetComponentsInChildren<Renderer>())
            {
                Debug.Log("ECC Test----");
                Debug.Log(renderer.gameObject.name);
                foreach (Material mat in renderer.materials)
                {
                    Debug.Log("Looking over material " + mat.name);
                    Debug.Log("Shader name: " + mat.shader.name);
                    Debug.Log("Keywords:");
                    foreach (string keyw in mat.shaderKeywords)
                    {
                        Debug.Log("Keyword: " + keyw);
                        Debug.Log("Keyword enabled: " + mat.IsKeywordEnabled(keyw));
                    }
                    Debug.Log("Properties of material " + mat.name);
                    Debug.Log("_SpecInt: " + mat.GetFloat("_SpecInt"));
                    Debug.Log("_Shininess: " + mat.GetFloat("_Shininess"));
                }
            }
        }
    }
}

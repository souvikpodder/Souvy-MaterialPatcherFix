using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Bobingabout.MaterialPatcher;
using Timberborn.BaseComponentSystem;
using Timberborn.BlueprintSystem;
using Timberborn.Debugging;
using Timberborn.GameFactionSystem;
using Timberborn.Timbermesh;
using UnityEngine;

namespace Souvy.MaterialPatcherFix
{
    [HarmonyPatch(typeof(BuildingPrefabMaterialPatcher), nameof(BuildingPrefabMaterialPatcher.InitializeEntity))]
    public static class BuildingPrefabMaterialPatcherPatch
    {
        public static bool Prefix(
            BuildingPrefabMaterialPatcher __instance,
            IMaterialRepository ____materialRepository,
            FactionService ____factionService,
            DevModeManager ____devModeManager,
            ISpecService ____specService)
        {
            bool enabled = ____devModeManager != null && ____devModeManager.Enabled;
            string name = __instance.Name.Replace("(Clone)", "").Replace("Preview", "").Trim();

            foreach (BuildingPrefabMaterialPatchSpec item in ____specService.GetSpecs<BuildingPrefabMaterialPatchSpec>()
                .Where(spec => ____factionService.Current.Id == spec.FactionID && spec.PrefabName == name))
            {
                if (enabled)
                {
                    Debug.Log(__instance.Name + "_PatchSpecification");
                }

                // CRITICAL FIX: Pass includeInactive: true so ALL variants (connected tubeways, vertical tubeways, etc.) are included!
                Renderer[] componentsInChildren = __instance.GameObject.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer val in componentsInChildren)
                {
                    if (val == null) continue;
                    if (enabled)
                    {
                        Debug.Log(val.name);
                    }

                    List<Material> materials = val.sharedMaterials.ToList();
                    for (int i = 0; i < materials.Count; i++)
                    {
                        if (materials[i] == null) continue;
                        if (enabled)
                        {
                            Debug.Log(materials[i].name);
                        }

                        foreach (BuildingPrefabMaterialEntry item2 in item.MaterialEntries
                            .Where(entry => materials[i].name.Contains(entry.MaterialName)))
                        {
                            if (enabled)
                            {
                                Debug.Log("Replacing " + materials[i].name + " on mesh " + val.name + " with " + item2.NewMaterialName);
                            }
                            materials[i] = new Material(____materialRepository.GetMaterial(item2.NewMaterialName))
                            {
                                shader = materials[i].shader,
                                color = Color.white
                            };
                        }
                    }
                    val.SetSharedMaterials(materials);
                }
            }
            return false; // Skip the original method
        }
    }
}


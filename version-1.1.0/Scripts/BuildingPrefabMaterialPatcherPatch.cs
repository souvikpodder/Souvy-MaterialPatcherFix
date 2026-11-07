using System;
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
            if (__instance == null || ____specService == null || ____factionService?.Current == null)
            {
                return true;
            }

            bool devMode = ____devModeManager != null && ____devModeManager.Enabled;
            string rawName = __instance.Name;
            string name = rawName.Replace("(Clone)", "").Replace("Preview", "").Trim();
            string factionId = ____factionService.Current.Id;

            List<BuildingPrefabMaterialPatchSpec> matchingSpecs = ____specService.GetSpecs<BuildingPrefabMaterialPatchSpec>()
                .Where(spec => spec.FactionID == factionId && MatchesPrefab(spec.PrefabName, name))
                .ToList();

            if (matchingSpecs.Count == 0)
            {
                if (devMode || name.Contains("Tubeway") || name.Contains("Levee") || name.Contains("Tunnel"))
                {
                    Debug.LogWarning($"[MaterialPatcherFix] No material patch found for '{rawName}' (clean: '{name}') in faction '{factionId}'");
                }
                return false;
            }

            Renderer[] componentsInChildren = __instance.GameObject.GetComponentsInChildren<Renderer>(true);
            int totalReplaced = 0;

            foreach (BuildingPrefabMaterialPatchSpec item in matchingSpecs)
            {
                foreach (Renderer val in componentsInChildren)
                {
                    if (val == null) continue;

                    List<Material> materials = val.sharedMaterials.ToList();
                    bool rendererModified = false;

                    for (int i = 0; i < materials.Count; i++)
                    {
                        if (materials[i] == null) continue;
                        string matName = materials[i].name;

                        foreach (BuildingPrefabMaterialEntry entry in item.MaterialEntries)
                        {
                            if (string.IsNullOrEmpty(entry.MaterialName) || string.IsNullOrEmpty(entry.NewMaterialName))
                                continue;

                            if (matName.IndexOf(entry.MaterialName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Material repMat = ____materialRepository.GetMaterial(entry.NewMaterialName);
                                if (repMat != null)
                                {
                                    materials[i] = new Material(repMat)
                                    {
                                        shader = materials[i].shader != null ? materials[i].shader : repMat.shader,
                                        color = Color.white
                                    };
                                    rendererModified = true;
                                    totalReplaced++;

                                    if (devMode)
                                    {
                                        Debug.Log($"[MaterialPatcherFix] Replaced '{matName}' on '{val.name}' of '{name}' with '{entry.NewMaterialName}'");
                                    }
                                }
                                break;
                            }
                        }
                    }

                    if (rendererModified)
                    {
                        val.SetSharedMaterials(materials);
                    }
                }
            }

            if (totalReplaced > 0)
            {
                Debug.LogWarning($"[MaterialPatcherFix] Successfully patched '{name}': {totalReplaced} material(s) overridden for faction '{factionId}'.");
            }

            return false;
        }

        private static bool MatchesPrefab(string specPrefabName, string entityName)
        {
            if (string.IsNullOrEmpty(specPrefabName) || string.IsNullOrEmpty(entityName))
                return false;

            if (string.Equals(specPrefabName, entityName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Handle path prefixes like "Buildings/Paths/Tubeway/TubewayLevee.IronTeeth" vs "TubewayLevee.IronTeeth"
            if (entityName.EndsWith("/" + specPrefabName, StringComparison.OrdinalIgnoreCase) ||
                entityName.EndsWith("." + specPrefabName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (specPrefabName.EndsWith("/" + entityName, StringComparison.OrdinalIgnoreCase) ||
                specPrefabName.EndsWith("." + entityName, StringComparison.OrdinalIgnoreCase))
                return true;

            // Handle naming alias differences (e.g. TubewayLevee vs LeveeTubeway)
            string normSpec = specPrefabName.Replace("LeveeTubeway", "TubewayLevee");
            string normEntity = entityName.Replace("LeveeTubeway", "TubewayLevee");
            if (string.Equals(normSpec, normEntity, StringComparison.OrdinalIgnoreCase))
                return true;

            if (normEntity.EndsWith("/" + normSpec, StringComparison.OrdinalIgnoreCase) ||
                normSpec.EndsWith("/" + normEntity, StringComparison.OrdinalIgnoreCase))
                return true;

            // Handle tunnel faction suffix fallback (e.g. TubewayLeveeTunnel.Whitepaws vs TubewayLeveeTunnel.IronTeeth)
            if (normSpec.StartsWith("TubewayLeveeTunnel", StringComparison.OrdinalIgnoreCase) &&
                normEntity.StartsWith("TubewayLeveeTunnel", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }
    }
}

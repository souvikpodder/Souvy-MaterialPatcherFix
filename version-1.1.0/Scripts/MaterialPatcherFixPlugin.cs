using System.Security.Permissions;
using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Souvy.MaterialPatcherFix
{
    public class MaterialPatcherFixPlugin : IModStarter
    {
        public const string Id = "Souvy.MaterialPatcherFix";

        public void StartMod(IModEnvironment modEnvironment)
        {
            try
            {
                new Harmony(Id).PatchAll();
                Debug.Log("[MaterialPatcherFix] Successfully applied BuildingPrefabMaterialPatcher inactive renderer patch.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[MaterialPatcherFix] Failed to apply patch: " + ex);
            }
        }
    }
}


using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Utilities;
using System;
using System.Linq;
using ThunderKit.Core.Actions;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.ThunderKitUtils
{
    public class ModManifestUtils
    {
        /// <summary>
        /// Create a raw specialized Manifest used in a mod build.
        /// This should not be used in scripts, this is mainly there for Menu Item creation, use CreateModPipeline instead
        /// </summary>
        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Manifest", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        public static void CreateManifestWithDatums()
        {
            var manifest = ScriptableObject.CreateInstance<Manifest>();

            string path = ProjectWindowPathHelper.GetCurrentProjectWindowPath();

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/MyNewManifest.asset");

            Action<int, string, string> endAction = (instanceId, pathname, resourceFile) =>
            {
                CreateModManifest(manifest, instanceId, pathname, resourceFile);
                Selection.activeObject = manifest;
            };

            var tempAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            tempAction.action = endAction;

            var findTexture = typeof(EditorGUIUtility).GetMethod("FindTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Texture2D icon = (Texture2D)findTexture.Invoke(null, new object[] { typeof(Pipeline) });

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(manifest.GetInstanceID(), tempAction, assetPathAndName, icon, null);
        }

        /// <summary>
        /// Create a specialized Manifest for the mod build
        /// </summary>
        public static void CreateModManifest(Manifest manifest, int instanceId, string pathname, string resourceFile = "")
        {
            AssetDatabase.CreateAsset(manifest, pathname);

            manifest.Identity = ScriptableObject.CreateInstance<ManifestIdentity>();
            manifest.Identity.name = nameof(ManifestIdentity);
            manifest.InsertElement(manifest.Identity, 0);

            AddDatum(typeof(AssemblyDefinitions), manifest, nameof(AssemblyDefinitions));
            AddDatum(typeof(AssetBundleDefinitions), manifest, nameof(AssetBundleDefinitions));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static ManifestDatum AddDatum(Type datumType, Manifest manifest, string name)
        {
            if (!typeof(ManifestDatum).IsAssignableFrom(datumType))
            {
                Debug.LogError($"Type {datumType} is not a PipelineJob.");
                return null;
            }

            var datum = (ManifestDatum)ScriptableObject.CreateInstance(datumType);
            datum.name = name;
            manifest.InsertElement(datum, manifest.Data != null ? manifest.Data.Length : 0);

            EditorUtility.SetDirty(manifest);

            return datum;
        }

        public static T GetManifestDatum<T>(Manifest manifest) where T : ManifestDatum
        {
            if (manifest == null || manifest.Data == null)
                return null;

            return manifest.Data.OfType<T>().FirstOrDefault();
        }
    }
}
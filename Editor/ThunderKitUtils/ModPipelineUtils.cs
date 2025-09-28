using ADOFAIModdingHelper.Common;
using ADOFAIModdingHelper.Utilities;
using System;
using System.Linq;
using ThunderKit.Core.Actions;
using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Pipelines.Jobs;
using ThunderKit.Pipelines.Jobs;
using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.ThunderKitUtils
{
    public class ModPipelineUtils
    {
        /// <summary>
        /// Create a raw specialized Pipeline used in a mod build.
        /// This should not be used in scripts, this is mainly there for Menu Item creation, use CreateModPipeline instead
        /// </summary>
        [MenuItem(Constants.ADOFAIModdingHelperMenuRoot + "Create Mod Pipeline", false, priority: Constants.ADOFAIModdingHelperMenuPriority)]
        public static void CreatePipelineWithJobs()
        {
            var pipeline = ScriptableObject.CreateInstance<Pipeline>();

            string path = ProjectWindowPathHelper.GetCurrentProjectWindowPath();

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/MyNewPipeline.asset");

            Action<int, string, string> endAction = (instanceId, pathname, resourceFile) =>
            {
                CreateModPipeline(pipeline, instanceId, pathname, resourceFile);
                Selection.activeObject = pipeline;
            };

            var tempAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            tempAction.action = endAction;

            var findTexture = typeof(EditorGUIUtility).GetMethod("FindTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Texture2D icon = (Texture2D)findTexture.Invoke(null, new object[] { typeof(Pipeline) });

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(pipeline.GetInstanceID(), tempAction, assetPathAndName, icon, null);
        }

        /// <summary>
        /// Create a specialized Pipeline for the mod build
        /// </summary>
        public static void CreateModPipeline(Pipeline pipeline, int instanceId, string pathname, string resourceFile = "")
        {
            AssetDatabase.CreateAsset(pipeline, pathname);

            (AddJob(typeof(Delete), pipeline, nameof(Delete)) as Delete).Recursive = true;
            (AddJob(typeof(Delete), pipeline, nameof(Delete)) as Delete).Recursive = true;
            AddJob(typeof(StageManifestFiles), pipeline, nameof(StageManifestFiles));
            AddJob(typeof(StageAssemblies), pipeline, nameof(StageAssemblies));
            StageAssetBundles assetBundle = AddJob(typeof(StageAssetBundles), pipeline, nameof(StageAssetBundles)) as StageAssetBundles;

            assetBundle.AssetBundleBuildOptions = BuildAssetBundleOptions.None;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static PipelineJob AddJob(Type jobType, Pipeline pipeline, string name)
        {
            if (!typeof(PipelineJob).IsAssignableFrom(jobType))
            {
                Debug.LogError($"Type {jobType} is not a PipelineJob.");
                return null;
            }

            var job = (PipelineJob)ScriptableObject.CreateInstance(jobType);
            job.name = name;
            pipeline.InsertElement(job, pipeline.Data != null ? pipeline.Data.Length : 0);

            EditorUtility.SetDirty(pipeline);

            return job;
        }

        public static T GetPipelineJob<T>(Pipeline pipeline, int index) where T : PipelineJob
        {
            if (pipeline == null || pipeline.Data == null)
                return null;

            var jobsOfType = pipeline.Data.OfType<T>().ToList();

            if (index < 0 || index >= jobsOfType.Count)
                return null;

            return jobsOfType[index];
        }
    }
}
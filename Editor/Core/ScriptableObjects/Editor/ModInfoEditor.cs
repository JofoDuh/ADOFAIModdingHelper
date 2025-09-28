using UnityEditor;
using UnityEngine;

namespace ADOFAIModdingHelper.Core.ScriptableObjects.Editor
{
    [CustomEditor(typeof(ModInfo))]
    public class ModInfoEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty modInfoUMM = serializedObject.FindProperty("modInfoUMM");
            SerializedProperty modInfoBIE = serializedObject.FindProperty("modInfoBIE");

            // UMM
            EditorGUILayout.PropertyField(modInfoUMM, true);

            EditorGUILayout.Space();

            // BIE
            EditorGUILayout.PropertyField(modInfoBIE, true);

            if (modInfoBIE != null)
            {
                SerializedProperty csPathProp = modInfoBIE.FindPropertyRelative("BIPModInfoCSPath");

                EditorGUILayout.BeginHorizontal();

                // Drag & drop area
                Rect dropArea = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
                GUI.Box(dropArea, "Drag & Drop .cs file here", EditorStyles.helpBox);

                Event evt = Event.current;
                if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
                {
                    if (dropArea.Contains(evt.mousePosition))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (evt.type == EventType.DragPerform)
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (var obj in DragAndDrop.objectReferences)
                            {
                                string path = AssetDatabase.GetAssetPath(obj);
                                if (path.EndsWith(".cs"))
                                {
                                    csPathProp.stringValue = path;
                                    break;
                                }
                            }
                        }
                        evt.Use();
                    }
                }

                // Browse button
                if (GUILayout.Button("Browse .cs", GUILayout.Width(100)))
                {
                    string selectedPath = EditorUtility.OpenFilePanel("Select BIPModInfo.cs", Application.dataPath, "cs");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        // Convert absolute path to relative project path
                        if (selectedPath.StartsWith(Application.dataPath))
                        {
                            selectedPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        }
                        csPathProp.stringValue = selectedPath;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}

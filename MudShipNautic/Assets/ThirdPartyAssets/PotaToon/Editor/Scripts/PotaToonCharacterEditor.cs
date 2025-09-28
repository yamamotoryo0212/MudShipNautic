using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PotaToon.Editor
{
    [CustomEditor(typeof(PotaToonCharacter))]
    public class PotaToonCharacterEditor : UnityEditor.Editor
    {
        private static bool s_FoldoutMaterials = true;
        private static bool s_FoldoutController = true;
        private double m_LastUpdateTime = 0;
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var character = target as PotaToonCharacter;
            if (character == null)
                return;
            
            GUIStyle headerStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 25,
                normal = { textColor = Color.white }
            };
            
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(5, 5, 5, 5)
            };
            
            GUIStyle borderStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(5, 5, 5, 5),
                normal = { background = Texture2D.grayTexture }
            };

            // Update materials periodically.
            var currentTime = EditorApplication.timeSinceStartup;
            if (currentTime - m_LastUpdateTime >= 10.0)
            {
                character.UpdateMaterials();
                m_LastUpdateTime = currentTime;
            }

            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            character.head = (Transform)EditorGUILayout.ObjectField("Head", character.head, typeof(Transform), true);

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            if (GUILayout.Button((s_FoldoutMaterials ? "▼ " : "► ") + "[Read Only] All Materials", headerStyle))
            {
                s_FoldoutMaterials = !s_FoldoutMaterials;
            }
            if (s_FoldoutMaterials)
            {
                if (GUILayout.Button( "Refresh Materials"))
                {
                    character.UpdateMaterials();
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allMaterials"), true);
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            
            if (GUILayout.Button((s_FoldoutController ? "▼ " : "► ") + "[Editor Only] All Materials Control", headerStyle))
            {
                s_FoldoutController = !s_FoldoutController;
            }
            if (s_FoldoutController)
            {
                EditorGUILayout.HelpBox("Note that this changes all materials directly. If you share materials for other characters, please duplicate materials first.", MessageType.Info);
                if (GUILayout.Button( "Duplicate Materials"))
                {
                    DuplicateMaterials(character);
                }
                
                EditorGUILayout.BeginVertical(borderStyle);
                EditorGUILayout.BeginVertical(boxStyle);

                EditorGUI.BeginChangeCheck();
                character.baseColor = EditorGUILayout.ColorField("Base Color", character.baseColor);
                character.shadeColor = EditorGUILayout.ColorField("Shade Color", character.shadeColor);
                character.baseStep = EditorGUILayout.Slider("Base Step", character.baseStep, 0f, 1f);
                character.stepSmoothness = EditorGUILayout.Slider("Step Smoothness", character.stepSmoothness, 0f, 0.1f);

                character.receiveLightShadow = EditorGUILayout.Toggle("Receive Light Shadow", character.receiveLightShadow);
                character.useMidTone = EditorGUILayout.Toggle("Use Mid Tone", character.useMidTone);
                character.midTone = EditorGUILayout.ColorField("Mid Tone", character.midTone);
                character.midThickness = EditorGUILayout.Slider("Mid Thickness", character.midThickness, 0f, 1f);
                character.indirectDimmer = EditorGUILayout.Slider("Indirect Dimmer", character.indirectDimmer, 0f, 10f);

                character.rimLightColor = EditorGUILayout.ColorField("Rim Light Color", character.rimLightColor);
                character.rimPower = EditorGUILayout.Slider("Rim Power", character.rimPower, 0f, 1f);
                character.rimSmoothness = EditorGUILayout.Slider("Rim Smoothness", character.rimSmoothness, 0f, 0.5f);

                character.outlineWidth = EditorGUILayout.Slider("Outline Width", character.outlineWidth, 0f, 10f);
                character.outlineColor = EditorGUILayout.ColorField("Outline Color", character.outlineColor);

                character.hiLightColor = EditorGUILayout.ColorField("Hi-Light Color", character.hiLightColor);
                character.emissionColor = EditorGUILayout.ColorField("Emission Color", character.emissionColor);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }

            if (EditorGUI.EndChangeCheck())
            {
                foreach (var mat in character.allMaterials)
                {
                    if (mat != null)
                        Undo.RecordObject(mat, "Update PotaToon Material Properties");
                }
                character.UpdateMaterialProperties();
                foreach (var mat in character.allMaterials)
                {
                    if (mat != null)
                        EditorUtility.SetDirty(mat);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(character);
        }
        
        private void DuplicateMaterials(PotaToonCharacter target)
        {
            // Choose folder to save duplicated materials
            string folderPath = EditorUtility.OpenFolderPanel(
                "Select Folder to Save Materials",
                Application.dataPath,
                ""
            );
            if (string.IsNullOrEmpty(folderPath))
                return;

            if (!folderPath.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Please select a folder inside the project's Assets directory.",
                    "OK"
                );
                return;
            }

            string assetFolder = "Assets" + folderPath.Substring(Application.dataPath.Length);
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);

            var materialMap = new Dictionary<Material, Material>();

            // Duplicate each unique material and create it as an asset
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null || materialMap.ContainsKey(mat))
                        continue;

                    Material duplicated = new Material(mat)
                    {
                        name = mat.name
                    };

                    string newPath = AssetDatabase.GenerateUniqueAssetPath(
                        $"{assetFolder}/{duplicated.name}.mat"
                    );

                    AssetDatabase.CreateAsset(duplicated, newPath);
                    materialMap.Add(mat, duplicated);
                }
            }

            int undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Duplicate Materials");
            
            // Replace each renderer’s materials with the duplicated versions
            foreach (var renderer in renderers)
            {
                var mats = renderer.sharedMaterials;
                bool replaced = false;

                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && materialMap.ContainsKey(mats[i]))
                    {
                        replaced = true;
                        break;
                    }
                }
                
                if (!replaced)
                    continue;
                
                Undo.RecordObject(renderer, "Duplicate Materials");
                
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && materialMap.TryGetValue(mats[i], out var newMat))
                    {
                        mats[i] = newMat;
                    }
                }
                
                renderer.sharedMaterials = mats;
                EditorUtility.SetDirty(renderer);
            }
            
            Undo.CollapseUndoOperations(undoGroup);

            // Save assets, refresh database, mark scene dirty
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(target.gameObject.scene);

            EditorUtility.DisplayDialog(
                "Done",
                "Materials have been duplicated and applied.",
                "OK"
            );
        }
    }
}

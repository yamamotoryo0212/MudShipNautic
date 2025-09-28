using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PotaToon.Editor
{
    public class PotaToonEyeShaderGUI : PotaToonShaderGUIBase
    {
        private static bool s_FoldoutMain = true;
        private static bool s_FoldoutRefraction = false;
        private static bool s_FoldoutHighLight = false;
        private static bool s_FoldoutStencil = false;
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!m_PrestIconInitialized)
            {
                m_PrestIconInitialized = true;
                InitializePresetsAndIcons();
            }

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 5, 5),
                margin = new RectOffset(5, 5, 5, 5)
            };

            GUIStyle headerStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 25,
                normal = { textColor = Color.white }
            };

            GUIStyle borderStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(1, 1, 1, 1),
                margin = new RectOffset(5, 5, 5, 5),
                normal = { background = Texture2D.grayTexture }
            };
            
            GUIStyle advancedSettingsStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };
            
            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 0f;
            
            MaterialProperty _CullMode = FindProperty("_CullMode", properties);
            MaterialProperty _MainTex = FindProperty("_MainTex", properties);
            MaterialProperty _ClippingMask = FindProperty("_ClippingMask", properties);
            MaterialProperty _BaseColor = FindProperty("_BaseColor", properties);
            MaterialProperty _Exposure = FindProperty("_Exposure", properties);
            MaterialProperty _IndirectDimmer = FindProperty("_IndirectDimmer", properties);
            MaterialProperty _MinIntensity = FindProperty("_MinIntensity", properties);
            MaterialProperty _UseRefraction = FindProperty("_UseRefraction", properties);
            MaterialProperty _RefractionWeight = FindProperty("_RefractionWeight", properties);
            MaterialProperty _UseHiLight = FindProperty("_UseHiLight", properties);
            MaterialProperty _UseHiLightJitter = FindProperty("_UseHiLightJitter", properties);
            MaterialProperty _HiLightTex = FindProperty("_HiLightTex", properties);
            MaterialProperty _HiLightColor = FindProperty("_HiLightColor", properties);
            MaterialProperty _HiLightPowerR = FindProperty("_HiLightPowerR", properties);
            MaterialProperty _HiLightPowerG = FindProperty("_HiLightPowerG", properties);
            MaterialProperty _HiLightPowerB = FindProperty("_HiLightPowerB", properties);
            MaterialProperty _HiLightIntensityR = FindProperty("_HiLightIntensityR", properties);
            MaterialProperty _HiLightIntensityG = FindProperty("_HiLightIntensityG", properties);
            MaterialProperty _HiLightIntensityB = FindProperty("_HiLightIntensityB", properties);
            MaterialProperty _ClippingMaskCH = FindProperty("_ClippingMaskCH", properties);
            MaterialProperty _StencilComp = FindProperty("_StencilComp", properties);
            MaterialProperty _StencilRef = FindProperty("_StencilRef", properties);
            MaterialProperty _StencilPass = FindProperty("_StencilPass", properties);
            MaterialProperty _StencilFail = FindProperty("_StencilFail", properties);
            MaterialProperty _StencilZFail = FindProperty("_StencilZFail", properties);
            Material material = materialEditor.target as Material;

            m_ShaderType = 2;
            DrawTitle(m_ShaderType, true, material);
            
            // Base Settings
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            m_ShaderType = GUILayout.Toolbar(m_ShaderType, GetToonTypeContents(), GUILayout.Width(EditorGUIUtility.currentViewWidth - 80f), GUILayout.Height(20f));
            if (GUILayout.Button(EditorGUIUtility.IconContent("_Help"), GUILayout.Width(25f)))
            {
                s_ShowMaininfo = !s_ShowMaininfo;
            }
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
            {
                if (PotaToonGUIUtility.ChangeShader(material, m_ShaderType))
                    return;
            }
            
            DrawPresetField(material);
            
            if (s_ShowMaininfo)
            {
                DrawInfoBox("1. General: The default type, used for most parts of the character such as the body, clothing, and hair.\n2. Face: Recommended for facial surfaces and eyeballs.\n3. Eye: Designed specifically for pupil-only meshes. If the eyeball and pupil are not separated into different submeshes or materials, use the Face type instead.");
            }
            
            EditorGUILayout.BeginVertical(borderStyle);
            EditorGUILayout.BeginVertical(boxStyle);
            materialEditor.ShaderProperty(_CullMode, new GUIContent("Cull Mode"));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            
            // Main
            if (GUILayout.Button((s_FoldoutMain ? "▼ " : "► ") + "Main Settings", headerStyle))
            {
                s_FoldoutMain = !s_FoldoutMain;
            }
            if (s_FoldoutMain)
            {
                EditorGUILayout.BeginVertical(borderStyle);
                EditorGUILayout.BeginVertical(boxStyle);
                materialEditor.TexturePropertySingleLine(new GUIContent("Main Tex"), _MainTex, _BaseColor);
                materialEditor.TexturePropertySingleLine(new GUIContent("Clipping Mask"), _ClippingMask, _ClippingMaskCH);
                materialEditor.RangeProperty(_Exposure, "Exposure");
                materialEditor.RangeProperty(_MinIntensity, "Min Intensity");
                materialEditor.RangeProperty(_IndirectDimmer, "Indirect Dimmer");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            
            // Refraction
            if (GUILayout.Button((s_FoldoutRefraction ? "▼ " : "► ") + "Refraction", headerStyle))
            {
                s_FoldoutRefraction = !s_FoldoutRefraction;
            }
            if (s_FoldoutRefraction)
            {
                EditorGUILayout.BeginVertical(borderStyle);
                EditorGUILayout.BeginVertical(boxStyle);
                materialEditor.ShaderProperty(_UseRefraction, "Use Refraction");
                EditorGUI.BeginDisabledGroup(material.GetInt("_UseRefraction") == 0);
                EditorGUI.indentLevel++;
                materialEditor.RangeProperty(_RefractionWeight, "Weight");
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            
            // Advanced Settings
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Advanced Settings", advancedSettingsStyle);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUI.BeginDisabledGroup(!PotaToonGUIUtility.advancedSettingsUnlocked);
            
            // High Light
            if (GUILayout.Button((s_FoldoutHighLight ? "▼ " : "► ") + "High Light", headerStyle))
            {
                s_FoldoutHighLight = !s_FoldoutHighLight;
            }
            if (s_FoldoutHighLight)
            {
                EditorGUILayout.BeginVertical(borderStyle);
                EditorGUILayout.BeginVertical(boxStyle);
                materialEditor.ShaderProperty(_UseHiLight, "Use High Light");
                EditorGUI.BeginDisabledGroup(material.GetInt("_UseHiLight") == 0);
                EditorGUI.indentLevel++;
                materialEditor.ShaderProperty(_UseHiLightJitter, "Jitter");
                materialEditor.TexturePropertySingleLine(new GUIContent("Hi Tex"), _HiLightTex, _HiLightColor);
                materialEditor.RangeProperty(_HiLightPowerR, "Power R");
                materialEditor.RangeProperty(_HiLightPowerG, "Power G");
                materialEditor.RangeProperty(_HiLightPowerB, "Power B");
                materialEditor.RangeProperty(_HiLightIntensityR, "Intensity R");
                materialEditor.RangeProperty(_HiLightIntensityG, "Intensity G");
                materialEditor.RangeProperty(_HiLightIntensityB, "Intensity B");
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            material.SetKeyword(new LocalKeyword(material.shader, "_USE_EYE_HI_LIGHT"), material.GetInt("_UseHiLight") > 0);
            
            // Stencil
            if (GUILayout.Button((s_FoldoutStencil ? "▼ " : "► ") + "Stencil", headerStyle))
            {
                s_FoldoutStencil = !s_FoldoutStencil;
            }
            if (s_FoldoutStencil)
            {
                EditorGUILayout.BeginVertical(borderStyle);
                EditorGUILayout.BeginVertical(boxStyle);
                materialEditor.ShaderProperty(_StencilComp, "Comp");
                materialEditor.RangeProperty(_StencilRef, "Ref");
                materialEditor.ShaderProperty(_StencilPass, "Pass");
                materialEditor.ShaderProperty(_StencilFail, "Fail");
                materialEditor.ShaderProperty(_StencilZFail, "ZFail");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
            }
            
            EditorGUI.EndDisabledGroup();
        }
    }
}

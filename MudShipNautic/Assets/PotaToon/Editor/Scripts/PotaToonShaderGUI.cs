using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static PotaToon.Editor.PotaToonShaderGUISearchHelper;

namespace PotaToon.Editor
{
    public class PotaToonShaderGUI : PotaToonShaderGUIBase
    {
        private enum SurfaceType
        {
            Opaque,
            Cutout,
            Refraction,
            Transparent
        }
        
        private bool m_AutoRenderQueue = true;
        private int  m_RenderQueue = 2000;
        private static bool[] s_FoldoutMatcaps = new bool[8];
        private static int[]  s_AutoRenderQueues = new int[] { 2000, 2450, 2900, 3000 };

        private static class GUIContents
        {
            public static readonly GUIContent matCapMode = new GUIContent("Mode");
            public static readonly GUIContent matCapMap = new GUIContent("MatCap Map");
            public static readonly GUIContent matCapMask = new GUIContent("MatCap Mask");
            public static readonly GUIContent matCapUV = new GUIContent("UV");
            public static readonly GUIContent matCapWeight = new GUIContent("Weight", "Controls the weight of the MatCap.");
            public static readonly GUIContent matCapLightingDimmer = new GUIContent("Lighting Dimmer", "Controls the lighting contribution to the MatCap if 'Add' mode. If this value is 0, the MatCap result will always be constant because it ignores lighting.");
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            if (!m_PrestIconInitialized)
            {
                m_PrestIconInitialized = true;
                InitializePresetsAndIcons();
            }

            GUIStyle advancedSettingsStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12
            };

            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 0f;

            MaterialProperty _SurfaceType = FindProperty("_SurfaceType", properties);
            MaterialProperty _Cull = FindProperty("_Cull", properties);
            MaterialProperty _Cutoff = FindProperty("_Cutoff", properties);
            MaterialProperty _ClippingMask = FindProperty("_ClippingMask", properties);
            MaterialProperty _MainTex = FindProperty("_MainTex", properties);
            MaterialProperty _ShadeMap = FindProperty("_ShadeMap", properties);
            MaterialProperty _ShadowBorderMask = FindProperty("_ShadowBorderMask", properties);
            MaterialProperty _BaseColor = FindProperty("_BaseColor", properties);
            MaterialProperty _ShadeColor = FindProperty("_ShadeColor", properties);
            MaterialProperty _BaseStep = FindProperty("_BaseStep", properties);
            MaterialProperty _StepSmoothness = FindProperty("_StepSmoothness", properties);
            MaterialProperty _ReceiveLightShadow = FindProperty("_ReceiveLightShadow", properties);
            MaterialProperty _UseVertexColor = FindProperty("_UseVertexColor", properties);
            MaterialProperty _UseMidTone = FindProperty("_UseMidTone", properties);
            MaterialProperty _MidColor = FindProperty("_MidColor", properties);
            MaterialProperty _MidWidth = FindProperty("_MidWidth", properties);
            MaterialProperty _UseDarknessMode = FindProperty("_UseDarknessMode", properties);
            MaterialProperty _IndirectDimmer = FindProperty("_IndirectDimmer", properties);
            MaterialProperty _NormalMap = FindProperty("_NormalMap", properties);
            MaterialProperty _BumpScale = FindProperty("_BumpScale", properties);
            MaterialProperty _UseNormalMap = FindProperty("_UseNormalMap", properties);
            MaterialProperty _SpecularColor = FindProperty("_SpecularColor", properties);
            MaterialProperty _SpecularMap = FindProperty("_SpecularMap", properties);
            MaterialProperty _SpecularMask = FindProperty("_SpecularMask", properties);
            MaterialProperty _SpecularPower = FindProperty("_SpecularPower", properties);
            MaterialProperty _SpecularSmoothness = FindProperty("_SpecularSmoothness", properties);
            MaterialProperty _RimColor = FindProperty("_RimColor", properties);
            MaterialProperty _RimMask = FindProperty("_RimMask", properties);
            MaterialProperty _RimPower = FindProperty("_RimPower", properties);
            MaterialProperty _RimSmoothness = FindProperty("_RimSmoothness", properties);
            MaterialProperty _EmissionColor = FindProperty("_EmissionColor", properties);
            MaterialProperty _EmissionMap = FindProperty("_EmissionMap", properties);
            MaterialProperty _EmissionMask = FindProperty("_EmissionMask", properties);
            MaterialProperty _UseGlitter = FindProperty("_UseGlitter", properties);
            MaterialProperty _GlitterMainStrength = FindProperty("_GlitterMainStrength", properties);
            MaterialProperty _GlitterEnableLighting = FindProperty("_GlitterEnableLighting", properties);
            MaterialProperty _GlitterBackfaceMask = FindProperty("_GlitterBackfaceMask", properties);
            MaterialProperty _GlitterApplyTransparency = FindProperty("_GlitterApplyTransparency", properties);
            MaterialProperty _GlitterShadowMask = FindProperty("_GlitterShadowMask", properties);
            MaterialProperty _GlitterParticleSize = FindProperty("_GlitterParticleSize", properties);
            MaterialProperty _GlitterScaleRandomize = FindProperty("_GlitterScaleRandomize", properties);
            MaterialProperty _GlitterContrast = FindProperty("_GlitterContrast", properties);
            MaterialProperty _GlitterSensitivity = FindProperty("_GlitterSensitivity", properties);
            MaterialProperty _GlitterBlinkSpeed = FindProperty("_GlitterBlinkSpeed", properties);
            MaterialProperty _GlitterAngleLimit = FindProperty("_GlitterAngleLimit", properties);
            MaterialProperty _GlitterLightDirection = FindProperty("_GlitterLightDirection", properties);
            MaterialProperty _GlitterColorRandomness = FindProperty("_GlitterColorRandomness", properties);
            MaterialProperty _GlitterNormalStrength = FindProperty("_GlitterNormalStrength", properties);
            MaterialProperty _GlitterPostContrast = FindProperty("_GlitterPostContrast", properties);
            MaterialProperty _GlitterColor = FindProperty("_GlitterColor", properties);
            MaterialProperty _GlitterColorTex = FindProperty("_GlitterColorTex", properties);
            MaterialProperty _BlendOutlineMainTex = FindProperty("_BlendOutlineMainTex", properties);
            MaterialProperty _OutlineMode = FindProperty("_OutlineMode", properties);
            MaterialProperty _UseOutlineNormalMap = FindProperty("_UseOutlineNormalMap", properties);
            MaterialProperty _OutlineNormalMap = FindProperty("_OutlineNormalMap", properties);
            MaterialProperty _OutlineColor = FindProperty("_OutlineColor", properties);
            MaterialProperty _OutlineWidthMask = FindProperty("_OutlineWidthMask", properties);
            MaterialProperty _OutlineWidth = FindProperty("_OutlineWidth", properties);
            MaterialProperty _OutlineOffsetZ = FindProperty("_OutlineOffsetZ", properties);
            MaterialProperty _RefractionWeight = FindProperty("_RefractionWeight", properties);
            MaterialProperty _RefractionBlurStep = FindProperty("_RefractionBlurStep", properties);
            MaterialProperty _DisableCharShadow = FindProperty("_DisableCharShadow", properties);
            MaterialProperty _CharShadowType = FindProperty("_CharShadowType", properties);
            MaterialProperty _DepthBias = FindProperty("_DepthBias", properties);
            MaterialProperty _NormalBias = FindProperty("_NormalBias", properties);
            MaterialProperty _CharShadowSmoothnessOffset = FindProperty("_CharShadowSmoothnessOffset", properties);
            MaterialProperty _2DFaceShadowWidth = FindProperty("_2DFaceShadowWidth", properties);
            MaterialProperty _UseFaceSDFShadow = FindProperty("_UseFaceSDFShadow", properties);
            MaterialProperty _FaceSDFTex = FindProperty("_FaceSDFTex", properties);
            MaterialProperty _SDFReverse = FindProperty("_SDFReverse", properties);
            MaterialProperty _SDFOffset = FindProperty("_SDFOffset", properties);
            MaterialProperty _SDFBlur = FindProperty("_SDFBlur", properties);
            MaterialProperty _DebugFaceSDF = FindProperty("_DebugFaceSDF", properties);
            MaterialProperty _UseHairHighLight = FindProperty("_UseHairHighLight", properties);
            MaterialProperty _HairHighLightTex = FindProperty("_HairHighLightTex", properties);
            MaterialProperty _ReverseHairHighLightTex = FindProperty("_ReverseHairHighLightTex", properties);
            MaterialProperty _HairHiStrength = FindProperty("_HairHiStrength", properties);
            MaterialProperty _HairHiUVOffset = FindProperty("_HairHiUVOffset", properties);

            // Stencil
            MaterialProperty _StencilComp = FindProperty("_StencilComp", properties);
            MaterialProperty _StencilRef = FindProperty("_StencilRef", properties);
            MaterialProperty _StencilPass = FindProperty("_StencilPass", properties);
            MaterialProperty _StencilFail = FindProperty("_StencilFail", properties);
            MaterialProperty _StencilZFail = FindProperty("_StencilZFail", properties);

            // UV Channels
            MaterialProperty _BaseMapUV = FindProperty("_BaseMapUV", properties);
            MaterialProperty _NormalMapUV = FindProperty("_NormalMapUV", properties);
            MaterialProperty _ClippingMaskUV = FindProperty("_ClippingMaskUV", properties);
            MaterialProperty _FaceSDFUV = FindProperty("_FaceSDFUV", properties);
            MaterialProperty _SpecularMapUV = FindProperty("_SpecularMapUV", properties);
            MaterialProperty _RimMaskUV = FindProperty("_RimMaskUV", properties);
            MaterialProperty _HairHiMapUV = FindProperty("_HairHiMapUV", properties);
            MaterialProperty _GlitterMapUV = FindProperty("_GlitterMapUV", properties);
            MaterialProperty _EmissionMapUV = FindProperty("_EmissionMapUV", properties);
            MaterialProperty _OutlineMaskUV = FindProperty("_OutlineMaskUV", properties);

            // MatCaps
            MaterialProperty _MatCapMode = FindProperty("_MatCapMode", properties);
            MaterialProperty _MatCapColor = FindProperty("_MatCapColor", properties);
            MaterialProperty _MatCapTex = FindProperty("_MatCapTex", properties);
            MaterialProperty _MatCapMask = FindProperty("_MatCapMask", properties);
            MaterialProperty _MatCapWeight = FindProperty("_MatCapWeight", properties);
            MaterialProperty _MatCapLightingDimmer = FindProperty("_MatCapLightingDimmer", properties);
            MaterialProperty _MatCapMode2 = FindProperty("_MatCapMode2", properties);
            MaterialProperty _MatCapColor2 = FindProperty("_MatCapColor2", properties);
            MaterialProperty _MatCapTex2 = FindProperty("_MatCapTex2", properties);
            MaterialProperty _MatCapMask2 = FindProperty("_MatCapMask2", properties);
            MaterialProperty _MatCapWeight2 = FindProperty("_MatCapWeight2", properties);
            MaterialProperty _MatCapLightingDimmer2 = FindProperty("_MatCapLightingDimmer2", properties);
            MaterialProperty _MatCapMode3 = FindProperty("_MatCapMode3", properties);
            MaterialProperty _MatCapColor3 = FindProperty("_MatCapColor3", properties);
            MaterialProperty _MatCapTex3 = FindProperty("_MatCapTex3", properties);
            MaterialProperty _MatCapMask3 = FindProperty("_MatCapMask3", properties);
            MaterialProperty _MatCapWeight3 = FindProperty("_MatCapWeight3", properties);
            MaterialProperty _MatCapLightingDimmer3 = FindProperty("_MatCapLightingDimmer3", properties);
            MaterialProperty _MatCapMode4 = FindProperty("_MatCapMode4", properties);
            MaterialProperty _MatCapColor4 = FindProperty("_MatCapColor4", properties);
            MaterialProperty _MatCapTex4 = FindProperty("_MatCapTex4", properties);
            MaterialProperty _MatCapMask4 = FindProperty("_MatCapMask4", properties);
            MaterialProperty _MatCapWeight4 = FindProperty("_MatCapWeight4", properties);
            MaterialProperty _MatCapLightingDimmer4 = FindProperty("_MatCapLightingDimmer4", properties);
            MaterialProperty _MatCapMode5 = FindProperty("_MatCapMode5", properties);
            MaterialProperty _MatCapColor5 = FindProperty("_MatCapColor5", properties);
            MaterialProperty _MatCapTex5 = FindProperty("_MatCapTex5", properties);
            MaterialProperty _MatCapMask5 = FindProperty("_MatCapMask5", properties);
            MaterialProperty _MatCapWeight5 = FindProperty("_MatCapWeight5", properties);
            MaterialProperty _MatCapLightingDimmer5 = FindProperty("_MatCapLightingDimmer5", properties);
            MaterialProperty _MatCapMode6 = FindProperty("_MatCapMode6", properties);
            MaterialProperty _MatCapColor6 = FindProperty("_MatCapColor6", properties);
            MaterialProperty _MatCapTex6 = FindProperty("_MatCapTex6", properties);
            MaterialProperty _MatCapMask6 = FindProperty("_MatCapMask6", properties);
            MaterialProperty _MatCapWeight6 = FindProperty("_MatCapWeight6", properties);
            MaterialProperty _MatCapLightingDimmer6 = FindProperty("_MatCapLightingDimmer6", properties);
            MaterialProperty _MatCapMode7 = FindProperty("_MatCapMode7", properties);
            MaterialProperty _MatCapColor7 = FindProperty("_MatCapColor7", properties);
            MaterialProperty _MatCapTex7 = FindProperty("_MatCapTex7", properties);
            MaterialProperty _MatCapMask7 = FindProperty("_MatCapMask7", properties);
            MaterialProperty _MatCapWeight7 = FindProperty("_MatCapWeight7", properties);
            MaterialProperty _MatCapLightingDimmer7 = FindProperty("_MatCapLightingDimmer7", properties);
            MaterialProperty _MatCapMode8 = FindProperty("_MatCapMode8", properties);
            MaterialProperty _MatCapColor8 = FindProperty("_MatCapColor8", properties);
            MaterialProperty _MatCapTex8 = FindProperty("_MatCapTex8", properties);
            MaterialProperty _MatCapMask8 = FindProperty("_MatCapMask8", properties);
            MaterialProperty _MatCapWeight8 = FindProperty("_MatCapWeight8", properties);
            MaterialProperty _MatCapLightingDimmer8 = FindProperty("_MatCapLightingDimmer8", properties);
            MaterialProperty _MatCapUV1 = FindProperty("_MatCapUV1", properties);
            MaterialProperty _MatCapUV2 = FindProperty("_MatCapUV2", properties);
            MaterialProperty _MatCapUV3 = FindProperty("_MatCapUV3", properties);
            MaterialProperty _MatCapUV4 = FindProperty("_MatCapUV4", properties);
            MaterialProperty _MatCapUV5 = FindProperty("_MatCapUV5", properties);
            MaterialProperty _MatCapUV6 = FindProperty("_MatCapUV6", properties);
            MaterialProperty _MatCapUV7 = FindProperty("_MatCapUV7", properties);
            MaterialProperty _MatCapUV8 = FindProperty("_MatCapUV8", properties);

            MaterialProperty _ClippingMaskCH = FindProperty("_ClippingMaskCH", properties);
            MaterialProperty _SpecularMaskCH = FindProperty("_SpecularMaskCH", properties);
            MaterialProperty _RimMaskCH = FindProperty("_RimMaskCH", properties);
            MaterialProperty _EmissionMaskCH = FindProperty("_EmissionMaskCH", properties);
            MaterialProperty _OutlineMaskCH = FindProperty("_OutlineMaskCH", properties);
            MaterialProperty _MatCapMaskCH1 = FindProperty("_MatCapMaskCH1", properties);
            MaterialProperty _MatCapMaskCH2 = FindProperty("_MatCapMaskCH2", properties);
            MaterialProperty _MatCapMaskCH3 = FindProperty("_MatCapMaskCH3", properties);
            MaterialProperty _MatCapMaskCH4 = FindProperty("_MatCapMaskCH4", properties);
            MaterialProperty _MatCapMaskCH5 = FindProperty("_MatCapMaskCH5", properties);
            MaterialProperty _MatCapMaskCH6 = FindProperty("_MatCapMaskCH6", properties);
            MaterialProperty _MatCapMaskCH7 = FindProperty("_MatCapMaskCH7", properties);
            MaterialProperty _MatCapMaskCH8 = FindProperty("_MatCapMaskCH8", properties);
            MaterialProperty _AOMapCH = FindProperty("_AOMapCH", properties);
            MaterialProperty _FaceSDFTexCH = FindProperty("_FaceSDFTexCH", properties);
            
            Material material = materialEditor.target as Material;

            // Title
            DrawTitle(m_ShaderType, false, material);

            // Toon type selection
            m_ShaderType = material.GetInt("_ToonType");
            
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

            var surfaceType = material.GetInt("_SurfaceType");

            // Base Settings
            PropertyGroup("Global Settings", (Property, shouldRender) =>
            {
                Property("Cull Mode", (s) => materialEditor.ShaderProperty(_Cull, new GUIContent(s)));
                Property("Surface Type", (s) => materialEditor.ShaderProperty(_SurfaceType, new GUIContent(s)));

                if (shouldRender) EditorGUI.BeginDisabledGroup(surfaceType != (int)SurfaceType.Cutout);
                Property("Cut Off", (s) => materialEditor.RangeProperty(_Cutoff, s));
                if (shouldRender) EditorGUI.EndDisabledGroup();

                m_AutoRenderQueue = material.GetInt("_AutoRenderQueue") > 0;

                Property("Auto Render Queue", (s) =>
                {
                    m_AutoRenderQueue = EditorGUILayout.Toggle("Auto Render Queue", m_AutoRenderQueue);
                });

                if (shouldRender)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(m_AutoRenderQueue);
                }

                m_RenderQueue = material.renderQueue;

                Property("Render Queue", (s) =>
                {
                    m_RenderQueue = EditorGUILayout.IntField(s, m_RenderQueue);
                });

                if (shouldRender)
                {
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }

                SetRenderQueueAndKeywords(surfaceType, material);
            });

            PropertyGroupBox("Main Settings", (Property) =>
            {
                Property("Main Tex", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _MainTex, _BaseColor, _BaseMapUV));
                Property("Shade Tex", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _ShadeMap, _ShadeColor));
                Property("AO Map", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _ShadowBorderMask, _AOMapCH));
                Property("Clipping Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _ClippingMask, _ClippingMaskUV, _ClippingMaskCH));
                Property("Base Step", (s) => materialEditor.RangeProperty(_BaseStep, s));
                Property("Step Smoothness", (s) => materialEditor.RangeProperty(_StepSmoothness, s));
                Property("Receive Light Shadow", (s) => materialEditor.ShaderProperty(_ReceiveLightShadow, new GUIContent(s, "If enabled, the material receives the default light shadow from the brightest light in the scene(MainLight or SpotLight). This does not affect the other shadows(base step + self character shadow).")));
                Property("$_MainSettings_HSlider", (s) => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider));
                Property("MidTone", (s) => materialEditor.ShaderProperty(_UseMidTone, new GUIContent(s, "MidTone is computed by main directional light. (Disappears if lit by additional lights.)")));
                EditorGUI.BeginDisabledGroup(material.GetInt("_UseMidTone") == 0);
                EditorGUI.indentLevel++;
                Property("MidTone Color", (s) => materialEditor.ColorProperty(_MidColor, s));
                Property("MidTone Thickness", (s) => materialEditor.RangeProperty(_MidWidth, s));
                EditorGUI.indentLevel--;
                EditorGUI.EndDisabledGroup();
                Property("Indirect Dimmer", (s) => materialEditor.ShaderProperty(_IndirectDimmer, new GUIContent(s, "Controls the intensity of indirect lighting. (LightMap, Light Probe, Adaptive Probe Volume, Reflection Probe, Skybox)")));
                Property("Vertex Color", (s) => materialEditor.ShaderProperty(_UseVertexColor, new GUIContent(s, "If enabled, the vertex color will be used for both base/shade colors.")));
                Property("Backlight Mode", (s) => materialEditor.ShaderProperty(_UseDarknessMode, new GUIContent(s, "If enabled, force to make shade color black.")));
            });

            material.SetInt("_UseShadeMap", _ShadeMap.textureValue != null ? 1 : 0);

            PropertyGroupBox("Normal Map", (Property) =>
            {
                Property("Use Normal Map", (s) => materialEditor.ShaderProperty(_UseNormalMap, new GUIContent(s)));
                Property("Normal Map", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _NormalMap, _NormalMapUV));
                Property("Bump Scale", (s) => materialEditor.RangeProperty(_BumpScale, s));
            });

            PropertyGroupBox("Rim Light", (Property) =>
            {
                Property("Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _RimMask, _RimMaskUV, _RimMaskCH));
                Property("Color", (s) => materialEditor.ColorProperty(_RimColor, s));
                Property("Power", (s) => materialEditor.RangeProperty(_RimPower, s));
                Property("Smoothness", (s) => materialEditor.RangeProperty(_RimSmoothness, s));
            });

            PropertyGroupBox("MatCap", (Property) =>
            {
                Property("MatCap 1", (s) => CustomFoldout(ref s_FoldoutMatcaps[0], "MatCap 1", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV1, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex, _MatCapColor);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask, _MatCapMaskCH1);
                }));

                Property("MatCap 2", (s) => CustomFoldout(ref s_FoldoutMatcaps[1], "MatCap 2", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode2, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV2, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight2, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer2, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex2, _MatCapColor2);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask2, _MatCapMaskCH2);
                }));

                Property("MatCap 3", (s) => CustomFoldout(ref s_FoldoutMatcaps[2], "MatCap 3", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode3, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV3, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight3, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer3, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex3, _MatCapColor3);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask3, _MatCapMaskCH3);
                }));

                Property("MatCap 4", (s) => CustomFoldout(ref s_FoldoutMatcaps[3], "MatCap 4", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode4, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV4, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight4, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer4, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex4, _MatCapColor4);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask4, _MatCapMaskCH4);
                }));

                Property("MatCap 5", (s) => CustomFoldout(ref s_FoldoutMatcaps[4], "MatCap 5", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode5, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV5, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight5, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer5, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex5, _MatCapColor5);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask5, _MatCapMaskCH5);
                }));

                Property("MatCap 6", (s) => CustomFoldout(ref s_FoldoutMatcaps[5], "MatCap 6", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode6, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV6, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight6, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer6, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex6, _MatCapColor6);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask6, _MatCapMaskCH6);
                }));

                Property("MatCap 7", (s) => CustomFoldout(ref s_FoldoutMatcaps[6], "MatCap 7", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode7, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV7, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight7, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer7, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex7, _MatCapColor7);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask7, _MatCapMaskCH7);
                }));

                Property("MatCap 8", (s) => CustomFoldout(ref s_FoldoutMatcaps[7], "MatCap 8", () =>
                {
                    materialEditor.ShaderProperty(_MatCapMode8, GUIContents.matCapMode);
                    materialEditor.ShaderProperty(_MatCapUV8, GUIContents.matCapUV);
                    materialEditor.ShaderProperty(_MatCapWeight8, GUIContents.matCapWeight);
                    EditorGUI.BeginDisabledGroup(material.GetInt("_MatCapMode") != 1);
                    materialEditor.ShaderProperty(_MatCapLightingDimmer8, GUIContents.matCapLightingDimmer);
                    EditorGUI.EndDisabledGroup();
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMap, _MatCapTex8, _MatCapColor8);
                    materialEditor.TexturePropertySingleLine(GUIContents.matCapMask, _MatCapMask8, _MatCapMaskCH8);
                }));
            });

            PropertyGroupBox("Outline", (Property, shouldRender) =>
            {
                if (shouldRender) DrawInfoBox("Note that the outline is not rendered for the Refraction/Transparent type unless OIT is enabled.");
                Property("Mode", (s) => materialEditor.ShaderProperty(_OutlineMode, new GUIContent(s, "Select how the outline is calculated by normal or by object position.")));

                if (shouldRender)
                {
                    var outlineMode = material.GetInt("_OutlineMode");
                    EditorGUI.BeginDisabledGroup(outlineMode > 0);
                    EditorGUI.indentLevel++;
                }

                Property("Use Normal Map", (s) => materialEditor.ShaderProperty(_UseOutlineNormalMap, new GUIContent(s)));
                Property("Normal Map", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s, "Normal Map for Outline"), _OutlineNormalMap));

                if (shouldRender)
                {
                    EditorGUI.indentLevel--;
                    EditorGUI.EndDisabledGroup();
                }

                Property("Width Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _OutlineWidthMask, _OutlineMaskUV, _OutlineMaskCH));
                Property("Blend Main Tex", (s) => materialEditor.ShaderProperty(_BlendOutlineMainTex, new GUIContent(s)));
                Property("Outline Color", (s) => materialEditor.ColorProperty(_OutlineColor, s));
                Property("Outline Width", (s) => materialEditor.RangeProperty(_OutlineWidth, s));
                Property("Depth Offset", (s) => materialEditor.FloatProperty(_OutlineOffsetZ, s));
            });

            // Refraction
            if (surfaceType == (int)SurfaceType.Refraction)
            {
                PropertyGroupBox("Refraction", (Property) =>
                {
                    Property("Refraction Weight", (s) => materialEditor.RangeProperty(_RefractionWeight, s));
                    Property("Blur Step", (s) => materialEditor.RangeProperty(_RefractionBlurStep, s));
                });
            }

            // Advanced Settings
            var originalAdvancedSettingsUnlocked = PotaToonGUIUtility.advancedSettingsUnlocked;
            if (IsAdvancedSettingsMatched())
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Advanced Settings", advancedSettingsStyle);
                PotaToonGUIUtility.DrawAdvancedSettingsButton();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            }
            
            EditorGUI.BeginDisabledGroup(!PotaToonGUIUtility.advancedSettingsUnlocked);

            // Character Shadow
            PropertyGroupBox("Character Shadow", (Property, shouldRender) =>
            {
                Property("Disable Char Shadow", (s) => materialEditor.ShaderProperty(_DisableCharShadow, new GUIContent(s, "Toggles character self-shadowing. In some cases (e.g., bangs), disabling self-shadow can create a cleaner look.")));
                if (shouldRender) EditorGUI.BeginDisabledGroup(material.GetInt("_DisableCharShadow") == 1);
                Property("Shadow Type", (s) => materialEditor.ShaderProperty(_CharShadowType, new GUIContent(s, "We recommend using the 2D face shadow mode if face type material. However, if you prefer a more realistic shadow (i.e. physically correct), 3D shadow mode is also available.")));
                if (material.GetInt("_CharShadowType") == 0)
                {
                    Property("Depth Bias", (s) => materialEditor.RangeProperty(_DepthBias, s));
                    Property("Normal Bias", (s) => materialEditor.RangeProperty(_NormalBias, s));
                    Property("Smoothness", (s) => materialEditor.RangeProperty(_CharShadowSmoothnessOffset, s));
                }
                else
                {
                    Property("2D Shadow Width", (s) => materialEditor.RangeProperty(_2DFaceShadowWidth, s));
                }
                if (shouldRender) EditorGUI.EndDisabledGroup();
            });
            
            material.SetKeyword(new LocalKeyword(material.shader, "_USE_2D_FACE_SHADOW"), material.GetInt("_CharShadowType") > 0);

            // Face SDF
            PropertyGroupBox("Face SDF", (Property, shouldRender) =>
            {
                Property("Use Face SDF", (s) => materialEditor.ShaderProperty(_UseFaceSDFShadow, new GUIContent(s)));

                if (shouldRender)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(material.GetInt("_UseFaceSDFShadow") == 0);
                }

                Property("Debug Face SDF", (s) => materialEditor.ShaderProperty(_DebugFaceSDF, new GUIContent(s)));
                Property("Reverse", (s) => materialEditor.ShaderProperty(_SDFReverse, new GUIContent(s)));
                Property("Face SDF Tex", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _FaceSDFTex, _FaceSDFUV, _FaceSDFTexCH));
                EditorGUI.indentLevel++;
                Property("Face SDF Tex", (s) => materialEditor.TextureScaleOffsetProperty(_FaceSDFTex));
                EditorGUI.indentLevel--;
                Property("Post Offset", (s) => materialEditor.RangeProperty(_SDFOffset, s));
                Property("Blur Step", (s) => materialEditor.RangeProperty(_SDFBlur, s));

                if (shouldRender)
                {
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
            });

            material.SetKeyword(new LocalKeyword(material.shader, "_USE_FACE_SDF"), material.GetInt("_UseFaceSDFShadow") > 0);

            // Specular
            PropertyGroupBox("High Light", (Property, shouldRender) =>
            {
                Property("HighLight Tex UV", (s) => materialEditor.ShaderProperty(_SpecularMapUV, new GUIContent(s)));
                Property("HighLight Map", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _SpecularMap, _SpecularColor));
                Property("HighLight Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _SpecularMask, _SpecularMaskCH));
                Property("Power", (s) => materialEditor.RangeProperty(_SpecularPower, s));
                Property("Smoothness", (s) => materialEditor.RangeProperty(_SpecularSmoothness, s));
            });

            PropertyGroupBox("Emission", (Property) =>
            {
                Property("Emission Tex UV", (s) => materialEditor.ShaderProperty(_EmissionMapUV, new GUIContent(s)));
                Property("Emission Map", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _EmissionMap, _EmissionColor));
                Property("Emission Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _EmissionMask, _EmissionMaskCH));
            });

            PropertyGroupBox("Glitter", (Property, shouldRender) =>
            {
                Property("Use Glitter", (s) => materialEditor.ShaderProperty(_UseGlitter, new GUIContent(s)));

                if (shouldRender)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(material.GetInt("_UseGlitter") == 0);
                }

                Property("Color / Mask", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _GlitterColorTex, _GlitterColor, _GlitterMapUV));
                Property("Main Color Power", (s) => materialEditor.RangeProperty(_GlitterMainStrength, s));
                Property("Enable Lighting", (s) => materialEditor.RangeProperty(_GlitterEnableLighting, s));
                Property("Backface Mask", (s) => materialEditor.ShaderProperty(_GlitterBackfaceMask, new GUIContent(s)));
                Property("Apply Transparency", (s) => materialEditor.ShaderProperty(_GlitterApplyTransparency, new GUIContent(s)));
                Property("Shadow Mask", (s) => materialEditor.RangeProperty(_GlitterShadowMask, s));
                Property("Particle Size", (s) => materialEditor.FloatProperty(_GlitterParticleSize, s));
                Property("Scale Randomize", (s) => materialEditor.RangeProperty(_GlitterScaleRandomize, s));
                Property("Contrast", (s) => materialEditor.FloatProperty(_GlitterContrast, s));
                Property("Sensitivity", (s) => materialEditor.FloatProperty(_GlitterSensitivity, s));
                Property("Blink Speed", (s) => materialEditor.FloatProperty(_GlitterBlinkSpeed, s));
                Property("Angle Limit", (s) => materialEditor.FloatProperty(_GlitterAngleLimit, s));
                Property("Light Direction Strength", (s) => materialEditor.FloatProperty(_GlitterLightDirection, s));
                Property("Color Randomness", (s) => materialEditor.RangeProperty(_GlitterColorRandomness, s));
                Property("Normal Strength", (s) => materialEditor.RangeProperty(_GlitterNormalStrength, s));
                Property("Post Contrast", (s) => materialEditor.FloatProperty(_GlitterPostContrast, s));

                if (shouldRender)
                {
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
            });

            material.SetKeyword(new LocalKeyword(material.shader, "_USE_GLITTER"), material.GetInt("_UseGlitter") > 0);

            PropertyGroupBox("Hair High Light", (Property, shouldRender) =>
            {
                Property("Use Hair High Light", (s) => materialEditor.ShaderProperty(_UseHairHighLight, new GUIContent(s)));

                if (shouldRender)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginDisabledGroup(material.GetInt("_UseHairHighLight") == 0);
                }

                Property("High Light Tex", (s) => materialEditor.TexturePropertySingleLine(new GUIContent(s), _HairHighLightTex, _HairHiMapUV));
                Property("Reverse Tex", (s) => materialEditor.ShaderProperty(_ReverseHairHighLightTex, new GUIContent(s)));
                Property("Strength", (s) => materialEditor.RangeProperty(_HairHiStrength, s));
                Property("UV Offset", (s) => materialEditor.RangeProperty(_HairHiUVOffset, s));

                if (shouldRender)
                {
                    EditorGUI.EndDisabledGroup();
                    EditorGUI.indentLevel--;
                }
            });

            PropertyGroupBox("Stencil", (Property) =>
            {
                Property("Comp", (s) => materialEditor.ShaderProperty(_StencilComp, s));
                Property("Ref", (s) => materialEditor.RangeProperty(_StencilRef, s));
                Property("Pass", (s) => materialEditor.ShaderProperty(_StencilPass, s));
                Property("Fail", (s) => materialEditor.ShaderProperty(_StencilFail, s));
                Property("ZFail", (s) => materialEditor.ShaderProperty(_StencilZFail, s));
            });

            EditorGUI.EndDisabledGroup();
            
            if (PotaToonGUIUtility.advancedSettingsUnlocked != originalAdvancedSettingsUnlocked)
                PotaToonGUIUtility.SaveAdvancedSettingUnlocked();
        }

        private void SetRenderQueueAndKeywords(int surfaceType, Material material)
        {
            if (m_AutoRenderQueue)
            {
                m_RenderQueue = s_AutoRenderQueues[surfaceType];
            }
            else
            {
                switch ((SurfaceType)surfaceType)
                {
                    case SurfaceType.Opaque:
                        if (m_RenderQueue > 2100)
                            m_RenderQueue = 2100;
                        break;
                    case SurfaceType.Cutout:
                        m_RenderQueue = Mathf.Clamp(m_RenderQueue, 2000, 2500);
                        break;
                    case SurfaceType.Refraction:
                        m_RenderQueue = Mathf.Clamp(m_RenderQueue, 2501, 2900);
                        break;
                    case SurfaceType.Transparent:
                        m_RenderQueue = Mathf.Clamp(m_RenderQueue, 2901, 5000);
                        break;
                }
            }

            material.SetInt("_ZWriteMode", surfaceType < (int)SurfaceType.Refraction ? 1 : 0);
            material.SetInt("_AutoRenderQueue", m_AutoRenderQueue ? 1 : 0);
            material.renderQueue = m_RenderQueue;
            material.SetKeyword(new LocalKeyword(material.shader, "_ALPHATEST_ON"), material.renderQueue >= 2450);
            material.SetKeyword(new LocalKeyword(material.shader, "_SURFACE_TYPE_TRANSPARENT"), material.renderQueue > 2500);
        }
    }
}

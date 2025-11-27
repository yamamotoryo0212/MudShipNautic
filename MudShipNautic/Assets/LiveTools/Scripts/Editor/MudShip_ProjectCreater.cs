using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Switch;
using UnityEngine.Rendering.Universal;

public class MudShip_ProjectCreater : EditorWindow
{
	private DisplayMode currentMode = DisplayMode.CreateProject;
	private string textValue = "デフォルトの文字列";
	private int intValue = 100;
	private Color colorValue = Color.white;
	private float floatSliderValue = 0.5f;

	[MenuItem("MudShip/ProjectWindow")]
	public static void ShowWindow()
	{
		// エディタウィンドウを表示またはフォーカスする
		GetWindow<MudShip_ProjectCreater>("ProjectWindow");
	}

	private void OnGUI()
	{
		EditorGUILayout.Space(10);

		// 1. Enumのドロップダウンメニューを表示
		// この値によって、次に描画するUIを切り替えます
		currentMode = (DisplayMode)EditorGUILayout.EnumPopup("表示モードの選択", currentMode);

		EditorGUILayout.Space(20);

		// 2. Enumの値に応じてUIを条件分岐して描画
		switch (currentMode)
		{
			case DisplayMode.None:
				GUILayout.Label("モードが選択されていません。", EditorStyles.boldLabel);
				break;
			case DisplayMode.CreateProject:
				CreateProject();
				break;

		}

		// エディタ上での変更を検知し、即座に再描画（特にColorの変更などに有効）
		if (GUI.changed)
		{
			Repaint();
		}


	}

	// === 各モードごとのUI描画メソッド ===

	private void CreateProject()
	{
		
		GUILayout.Label("作成するプロジェクトリスト", EditorStyles.boldLabel);
		EditorGUILayout.Space(10);

		int oldSize = listSize;

		listSize = Mathf.Max(0, EditorGUILayout.IntField(listSize, GUILayout.Width(30)));

		if (listSize != oldSize)
		{
			ResizeList(listSize);
		}

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Mathf.Min(300, 40 + inputValues.Count * 25)));

		for (int i = 0; i < inputValues.Count; i++)
		{
			inputValues[i] = EditorGUILayout.TextField($"プロジェクト [{i}]", inputValues[i]);
		}

		EditorGUILayout.EndScrollView();
		EditorGUILayout.Space(10);
		if (GUI.changed)
		{
			Repaint();
		}
		if (GUILayout.Button("Create"))
		{
			for (int i = 0; i < inputValues.Count; i++)
			{
				var parentDirectry = $"Assets/Art/{inputValues[i]}";
				Directory.CreateDirectory(parentDirectry);
				Directory.CreateDirectory(parentDirectry + "/AudioClip");
				Directory.CreateDirectory(parentDirectry + "/Lip");
				Directory.CreateDirectory(parentDirectry + "/Motion");
				Directory.CreateDirectory(parentDirectry + "/Item");
				Directory.CreateDirectory(parentDirectry + "/RenderTexture");


				var scene =EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
				new GameObject("==========================================");
				new GameObject("================= Timeline ==================");
				new GameObject("==========================================");


				var timelineAsset = new TimelineAsset();
				AssetDatabase.CreateAsset(timelineAsset, parentDirectry + $"/{inputValues[i]}.playable");
				GameObject timelineObj = new GameObject(inputValues[i] + "_timeline");
				PlayableDirector director = timelineObj.AddComponent<PlayableDirector>();

				SerializedObject so = new SerializedObject(director);
				so.FindProperty("m_PlayableAsset").objectReferenceValue = timelineAsset;
				so.ApplyModifiedProperties();

				Undo.RegisterCreatedObjectUndo(timelineObj, "Create Timeline GameObject");
				new GameObject("==========================================");
				new GameObject("================= System ==================");
				new GameObject("==========================================");

				var systemObj = new GameObject("System");
				RenderTexture mainRGB = new RenderTexture(3840, 2160, 0, RenderTextureFormat.ARGBFloat);
				mainRGB.antiAliasing = 1;
				mainRGB.useMipMap = true;
				mainRGB.autoGenerateMips = true;
				mainRGB.useDynamicScale = true;
				mainRGB.wrapMode = TextureWrapMode.Repeat;

				RenderTexture mainA = new RenderTexture(3840, 2160, 0, RenderTextureFormat.ARGBFloat);
				mainA.antiAliasing = 1;
				mainA.useMipMap = true;
				mainA.autoGenerateMips = true;
				mainA.useDynamicScale = true;
				mainA.wrapMode = TextureWrapMode.Repeat;

				RenderTexture Output = new RenderTexture(3840, 2160, 0, RenderTextureFormat.ARGBFloat);
				Output.antiAliasing = 1;
				Output.useMipMap = true;
				Output.autoGenerateMips = true;
				Output.useDynamicScale = true;
				Output.wrapMode = TextureWrapMode.Repeat;

				Material outputMat = new Material(Shader.Find("MudShip/Channel Merger"));
				outputMat.SetTexture("_TexA", mainRGB);
				outputMat.SetTexture("_TexB", mainA);

				AssetDatabase.CreateAsset(mainRGB, $"{parentDirectry + "/RenderTexture/"}Mainout-RGB.renderTexture");
				AssetDatabase.CreateAsset(mainA, $"{parentDirectry + "/RenderTexture/"}Mainout-A.renderTexture");
				AssetDatabase.CreateAsset(Output, $"{parentDirectry + "/RenderTexture/"}Output.renderTexture");
				AssetDatabase.CreateAsset(outputMat, $"{parentDirectry + "/Item/"}Output.mat");

				var renderTextureCombiner = systemObj.AddComponent<RenderTextureCombiner>();
				renderTextureCombiner.textureA = mainRGB;
				renderTextureCombiner.textureB = mainA;
				renderTextureCombiner.resultTexture = Output;
				renderTextureCombiner.channelMergeMaterial = outputMat;

				Volume volume = new GameObject("Volume").AddComponent<Volume>();

				string assetPath = $"{parentDirectry}/Item/volume.asset";
				AssetDatabase.CopyAsset(
					AssetDatabase.GetAssetPath(Mudship_Core.Settings.OriginalVolume),
					assetPath
				);
				AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

				var vol = AssetDatabase.LoadAssetAtPath<VolumeProfile>(assetPath);

				// SerializedObjectを使用
				SerializedObject volumeso = new SerializedObject(volume);
				volumeso.FindProperty("sharedProfile").objectReferenceValue = vol;
				volumeso.ApplyModifiedProperties();

				Undo.RegisterCreatedObjectUndo(volume.gameObject, "Create New GameObject");

				new GameObject("==========================================");
				new GameObject("================ Character =================");
				new GameObject("==========================================");
				new GameObject("==========================================");
				new GameObject("================== Stage ===================");
				new GameObject("==========================================");
				new GameObject("==========================================");
				new GameObject("================= Camera ==================");
				new GameObject("==========================================");

				var cameraM = new GameObject("CameraGroup");
				var cameraParent = new GameObject("CameraParent").AddComponent<CameraShake>();
				cameraParent.enabled = false;
				cameraParent.transform.parent = cameraM.transform;

				var swichCamera = new GameObject("SwitchCamera").AddComponent<Camera>();
				var swUAC = swichCamera.AddComponent<UniversalAdditionalCameraData>();
				swichCamera.transform.parent = cameraParent.transform;
				swichCamera.AddComponent<Animator>();
				swichCamera.fieldOfView = 30;
				swUAC.renderPostProcessing = true;
				swUAC.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				swUAC .antialiasingQuality = AntialiasingQuality.High;
				swichCamera.clearFlags = CameraClearFlags.SolidColor;
				swichCamera.backgroundColor = new Color(0,0,0,0);

				var fixCamera = new GameObject("FixedCamera").AddComponent<Camera>();
				var fixUAC = fixCamera.AddComponent<UniversalAdditionalCameraData>();
				fixCamera.transform.parent = cameraParent.transform;
				fixCamera.AddComponent<Animator>();
				fixCamera.fieldOfView = 30;
				fixUAC.renderPostProcessing = true;
				fixUAC.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				fixUAC.antialiasingQuality = AntialiasingQuality.High;
				fixCamera.clearFlags = CameraClearFlags.SolidColor;
				fixCamera.backgroundColor = new Color(0, 0, 0, 0);

				var outputCamera = new GameObject("OutputCamera").AddComponent<Camera>();
				outputCamera.transform.parent = cameraParent.transform;
				outputCamera.AddComponent<Animator>();
				outputCamera.fieldOfView = 30;
				var outputUAC = outputCamera.AddComponent<UniversalAdditionalCameraData>();
				outputUAC.renderPostProcessing = true;
				outputUAC.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				outputUAC.antialiasingQuality = AntialiasingQuality.High;
				outputCamera.clearFlags = CameraClearFlags.SolidColor;
				outputCamera.backgroundColor = new Color(0, 0, 0, 0);
				outputCamera.targetTexture = mainRGB;

				var outputAlphaCamera = new GameObject("OutputCamera_Alpha").AddComponent<Camera>();
				outputAlphaCamera.transform.parent = outputCamera.transform;
				outputAlphaCamera.AddComponent<Animator>();
				outputAlphaCamera.fieldOfView = 30;
				var outputAlphaUAC = outputAlphaCamera.AddComponent<UniversalAdditionalCameraData>();
				outputAlphaUAC.renderPostProcessing = true;
				outputAlphaUAC.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				outputAlphaUAC.antialiasingQuality = AntialiasingQuality.High;
				outputAlphaCamera.cullingMask = LayerMask.GetMask("Character");
				outputAlphaCamera.clearFlags = CameraClearFlags.SolidColor;
				outputAlphaCamera.backgroundColor = new Color(0, 0, 0, 0);
				outputAlphaCamera.targetTexture = mainA;

				outputCamera.gameObject.active = false;
				fixCamera.gameObject.active = false;

				new GameObject("==========================================");
				new GameObject("================== Light ===================");
				new GameObject("==========================================");

				var ms_DL = new GameObject("MS_DirectionalLight").AddComponent<MS_DirectionalLight>();
				ms_DL.AddComponent<Animator>();

				var characterLight = new GameObject("CharacterLight").AddComponent<Light>();
				var characterUALD = characterLight.gameObject.AddComponent<UniversalAdditionalLightData>();
				characterLight.type = LightType.Directional;
				characterLight.gameObject.transform.parent = ms_DL.transform;
				characterLight.color = Color.white;
				characterLight.intensity = 0.3f;
				characterLight.cullingMask = LayerMask.GetMask("Character");
				characterLight.shadows = LightShadows.Soft;
				characterUALD.renderingLayers =2;

				var stageLight = new GameObject("StageLight").AddComponent<Light>();
				var stageUALD = stageLight.gameObject.AddComponent<UniversalAdditionalLightData>();
				stageLight.type = LightType.Directional;
				stageLight.gameObject.transform.parent = ms_DL.transform;
				stageLight.color = Color.white;
				stageLight.intensity = 2.58f;
				stageLight.cullingMask = LayerMask.GetMask("Default");
				stageUALD.renderingLayers = 1;

				ms_DL.postProcessingVolume = volume;
				ms_DL.characterLight = characterLight;
				ms_DL._stageLight = stageLight;

				RenderSettings.skybox = null;
				RenderSettings.ambientMode = AmbientMode.Flat;
				RenderSettings.ambientLight = new Color(0, 0, 0, 0);
				EditorSceneManager.SaveScene(scene, $"{parentDirectry}/{inputValues[i]}.unity");
			}
		}
	}

	private int listSize = 1;
	private List<string> inputValues = new List<string> {"" };
	private Vector2 scrollPosition;
	private void ResizeList(int newSize)
	{
		try
		{
			if (newSize > inputValues.Count)
			{
				// サイズを大きくする場合: 新しい要素を空の文字列 ("") で埋める
				int itemsToAdd = newSize - inputValues.Count;
				for (int i = 0; i < itemsToAdd; i++)
				{
					inputValues.Add("");
				}
			}
			else if (newSize < inputValues.Count)
			{
				// サイズを小さくする場合: 末尾から要素を削除する
				inputValues.RemoveRange(newSize, inputValues.Count - newSize);
			}
		}
		catch (System.Exception e)
		{
			Debug.LogError($"リストのリサイズ中にエラーが発生しました: {e.Message}");
		}
	}
}
public enum DisplayMode
{
	None,
	CreateProject,
}
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 選択したオブジェクトの子SkinnedMeshRendererのマテリアルを、指定フォルダ内のマテリアルで置き換えるエディタ拡張
/// </summary>
public class SkinnedMeshMaterialReplacer : EditorWindow
{
	// === プロパティ ===

	// マテリアルを検索するフォルダのパス
	private string materialFolderPath = "Assets/Materials/Replacement";

	// 処理対象のルートオブジェクト
	private GameObject targetRootObject;

	// 置換処理の結果を格納するメッセージ
	private string statusMessage = "";

	// スクロールビューの状態
	private Vector2 scrollPosition;

	// === 初期設定 ===

	[MenuItem("Tools/MudShip/Skinned Mesh Material Replacer")]
	public static void ShowWindow()
	{
		// エディタウィンドウを表示またはフォーカスする
		GetWindow<SkinnedMeshMaterialReplacer>("Material Replacer");
	}

	// === GUI描画 ===

	private void OnGUI()
	{
		// ヘッダー
		GUILayout.Label("Skinned Mesh マテリアル置換ツール", EditorStyles.boldLabel);
		EditorGUILayout.Space(10);

		// 1. ターゲットオブジェクトの指定
		EditorGUILayout.LabelField("1. 処理対象のルートオブジェクト (親)", EditorStyles.miniLabel);
		targetRootObject = (GameObject)EditorGUILayout.ObjectField(
			"Root Object",
			targetRootObject,
			typeof(GameObject),
			true
		);

		EditorGUILayout.Space(5);

		// 2. 検索フォルダパスの指定
		EditorGUILayout.LabelField("2. 置き換えマテリアル検索フォルダ (Assets/ からの相対パス)", EditorStyles.miniLabel);
		materialFolderPath = EditorGUILayout.TextField("Material Folder Path", materialFolderPath);

		// フォルダ選択ボタン
		if (GUILayout.Button("Select Folder"))
		{
			SelectMaterialFolder();
		}

		EditorGUILayout.Space(20);

		// 3. 実行ボタン
		GUI.enabled = targetRootObject != null && !string.IsNullOrEmpty(materialFolderPath);
		if (GUILayout.Button("▶ マテリアルを置換して Missing を確認"))
		{
			ReplaceMaterials();
		}
		GUI.enabled = true;

		EditorGUILayout.Space(20);

		// 4. ステータスメッセージ表示
		GUILayout.Label("処理結果:", EditorStyles.boldLabel);
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
		EditorGUILayout.TextArea(statusMessage, GUILayout.ExpandHeight(true));
		EditorGUILayout.EndScrollView();

		if (GUILayout.Button("Clear Status"))
		{
			statusMessage = "";
			Debug.ClearDeveloperConsole();
		}
	}

	/// <summary>
	/// フォルダパスを選択するダイアログを表示
	/// </summary>
	private void SelectMaterialFolder()
	{
		string selectedPath = EditorUtility.OpenFolderPanel("Select Material Folder", "Assets", "");

		if (!string.IsNullOrEmpty(selectedPath))
		{
			// パスを "Assets/" から始まる相対パスに変換
			if (selectedPath.Contains("Assets"))
			{
				materialFolderPath = selectedPath.Substring(selectedPath.IndexOf("Assets"));
			}
			else
			{
				// Assetsフォルダ外の場合は警告
				Debug.LogError("選択されたフォルダは Assets/ フォルダ内にありません。");
			}
		}
	}

	/// <summary>
	/// マテリアル置換のメイン処理
	/// </summary>
	private void ReplaceMaterials()
	{
		if (targetRootObject == null)
		{
			statusMessage = "エラー: Root Objectが指定されていません。";
			Debug.LogError(statusMessage);
			return;
		}

		// ターゲットフォルダ内の全マテリアルをロード
		Material[] replacementMaterials = LoadMaterialsFromFolder(materialFolderPath);
		if (replacementMaterials == null || replacementMaterials.Length == 0)
		{
			statusMessage = $"エラー: 指定されたフォルダ '{materialFolderPath}' にマテリアルが見つかりません。";
			Debug.LogError(statusMessage);
			return;
		}

		// マテリアル名とマテリアルオブジェクトを紐づける辞書を作成（高速検索のため）
		Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();
		foreach (var mat in replacementMaterials)
		{
			if (mat != null && !materialLookup.ContainsKey(mat.name))
			{
				materialLookup.Add(mat.name, mat);
			}
		}

		if (materialLookup.Count == 0)
		{
			statusMessage = $"エラー: 有効なマテリアルがフォルダ '{materialFolderPath}' に見つかりません。";
			Debug.LogError(statusMessage);
			return;
		}


		// ルートオブジェクト以下のすべてのSkinnedMeshRendererを取得
		SkinnedMeshRenderer[] smrs = targetRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);

		if (smrs.Length == 0)
		{
			statusMessage = $"警告: オブジェクト '{targetRootObject.name}' の子に SkinnedMeshRenderer が見つかりませんでした。";
			Debug.LogWarning(statusMessage);
			return;
		}

		// ログメッセージを初期化
		statusMessage = $"--- マテリアル置換処理開始: {targetRootObject.name} ---\n";
		int replacedCount = 0;
		int missingCount = 0;

		// SkinnedMeshRendererごとに処理
		foreach (var smr in smrs)
		{
			Material[] currentMaterials = smr.sharedMaterials;
			Material[] newMaterials = new Material[currentMaterials.Length];
			bool changed = false;

			for (int i = 0; i < currentMaterials.Length; i++)
			{
				Material originalMat = currentMaterials[i];

				// 元マテリアルが存在しない場合はそのままにする
				if (originalMat == null)
				{
					newMaterials[i] = null;
					continue;
				}

				string matName = originalMat.name;
				Material replacementMat = null;

				// 辞書から名前が一致するマテリアルを検索
				if (materialLookup.TryGetValue(matName, out replacementMat))
				{
					// 置き換え成功
					newMaterials[i] = replacementMat;
					changed = true;
					statusMessage += $"[成功] {smr.name} - Slot {i}: '{matName}' -> '{replacementMat.name}'\n";
					replacedCount++;
				}
				else
				{
					// 置き換えマテリアルが見つからない場合
					newMaterials[i] = null; // Missingに設定
					changed = true;
					statusMessage += $"[MISSING警告] {smr.name} - Slot {i}: '{matName}' の置換マテリアルが '{materialFolderPath}' に見つかりませんでした。Missingに設定しました。\n";
					Debug.LogWarning($"[Missing Material] {smr.name} のマテリアル '{matName}' の置換が見つかりませんでした。Missingに設定されました。");
					missingCount++;
				}
			}

			// マテリアル配列が変更された場合のみ代入する
			if (changed)
			{
				smr.sharedMaterials = newMaterials;
				// 変更を記録（Undo可能にするため）
				EditorUtility.SetDirty(smr);
			}
		}

		// 処理完了メッセージ
		statusMessage += "\n--- 処理完了 ---\n";
		statusMessage += $"成功置換数: {replacedCount} 件\n";
		statusMessage += $"Missing設定数: {missingCount} 件\n";
		Debug.Log(statusMessage);
	}

	/// <summary>
	/// 指定されたフォルダからすべてのマテリアルアセットをロード
	/// </summary>
	/// <param name="path">Assets/ からの相対パス</param>
	/// <returns>ロードされたマテリアルの配列</returns>
	private Material[] LoadMaterialsFromFolder(string path)
	{
		// フォルダが存在しないか、アセットフォルダ外であれば処理を中止
		if (!AssetDatabase.IsValidFolder(path))
		{
			return null;
		}

		// フォルダ内のすべてのアセットパスを取得
		string[] guids = AssetDatabase.FindAssets("t:Material", new[] { path });
		List<Material> materials = new List<Material>();

		foreach (string guid in guids)
		{
			string assetPath = AssetDatabase.GUIDToAssetPath(guid);
			Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
			if (mat != null)
			{
				materials.Add(mat);
			}
		}
		return materials.ToArray();
	}
}
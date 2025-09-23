using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BlendShapeAnimationCreator : EditorWindow
{
	// メニュー項目を追加
	[MenuItem("GameObject/MudShip/Create Facial Clips", false, 100)]
	public static void CreateAnimClips()
	{
		// 選択中のGameObjectを取得
		GameObject selectedObject = Selection.activeGameObject;
		if (selectedObject == null)
		{
			EditorUtility.DisplayDialog("エラー", "Skinned Mesh Rendererを持つGameObjectを選択してください。", "OK");
			return;
		}

		// SkinnedMeshRendererコンポーネントを取得
		SkinnedMeshRenderer skinnedMeshRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();
		if (skinnedMeshRenderer == null)
		{
			EditorUtility.DisplayDialog("エラー", "選択されたGameObjectにSkinned Mesh Rendererがアタッチされていません。", "OK");
			return;
		}

		// 保存先フォルダをユーザーに選択させる
		string savePath = EditorUtility.OpenFolderPanel("アニメーションクリップの保存先を選択", "Assets", "");
		if (string.IsNullOrEmpty(savePath))
		{
			return;
		}

		// Assetsフォルダからの相対パスに変換
		if (!savePath.Contains(Application.dataPath))
		{
			EditorUtility.DisplayDialog("エラー", "プロジェクトのAssetsフォルダ内の場所を選択してください。", "OK");
			return;
		}
		string relativePath = "Assets" + savePath.Substring(Application.dataPath.Length);

		// ブレンドシェイプの数を取得
		int blendShapeCount = skinnedMeshRenderer.sharedMesh.blendShapeCount;
		if (blendShapeCount == 0)
		{
			EditorUtility.DisplayDialog("情報", "選択されたメッシュにブレンドシェイプがありません。", "OK");
			return;
		}

		string defaultblendShape = null;
		AnimationClip defaultClip = new AnimationClip();
		defaultClip.name = "default";
		for (int i = 0; i < blendShapeCount; i++)
		{
			defaultblendShape = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
			AnimationCurve curve = new AnimationCurve();

			Keyframe keyframe = new Keyframe(0f, 0f);
			curve.AddKey(keyframe);
			string bindingPath = ""; // ルートからパスを取得
			Transform currentTransform = skinnedMeshRenderer.transform;
			while (currentTransform.parent != null)
			{
				bindingPath = skinnedMeshRenderer.transform.parent.name + "/" + skinnedMeshRenderer.name;
				currentTransform = currentTransform.parent;
			}
			bindingPath = bindingPath.TrimEnd('/');

			EditorCurveBinding binding = new EditorCurveBinding
			{
				path = bindingPath,
				type = typeof(SkinnedMeshRenderer),
				propertyName = "blendShape." + defaultblendShape
			};

			AnimationUtility.SetEditorCurve(defaultClip, binding, curve);
			Debug.Log("def");
		}

		// 生成したクリップをアセットとして保存
		AssetDatabase.CreateAsset(defaultClip, Path.Combine(relativePath, "def" + ".anim"));

		// 各ブレンドシェイプに対してアニメーションクリップを作成
		for (int i = 0; i < blendShapeCount; i++)
		{
			string blendShapeName = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);

			// アニメーションクリップを新規作成
			AnimationClip clip = new AnimationClip();
			clip.name = blendShapeName;

			// アニメーションカーブを作成
			AnimationCurve curve = new AnimationCurve();

			// 0フレームに値を100でキーを追加
			Keyframe keyframe = new Keyframe(0f, 100f);
			curve.AddKey(keyframe);

			// アニメーションカーブをクリップに設定
			string bindingPath = ""; // ルートからパスを取得
			Transform currentTransform = skinnedMeshRenderer.transform;
			while (currentTransform.parent != null)
			{
				bindingPath = skinnedMeshRenderer.transform.parent.name + "/" + skinnedMeshRenderer.name;
				currentTransform = currentTransform.parent;
			}
			bindingPath = bindingPath.TrimEnd('/');

			EditorCurveBinding binding = new EditorCurveBinding
			{
				path = bindingPath,
				type = typeof(SkinnedMeshRenderer),
				propertyName = "blendShape." + blendShapeName
			};
			AnimationUtility.SetEditorCurve(clip, binding, curve);

			// 生成したクリップをアセットとして保存
			AssetDatabase.CreateAsset(clip, Path.Combine(relativePath, blendShapeName + ".anim"));
		}

		EditorUtility.DisplayDialog("完了", $"{blendShapeCount}個のアニメーションクリップを生成しました。", "OK");

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
}

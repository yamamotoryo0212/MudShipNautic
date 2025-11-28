using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FBXAnimExtractor
{
	static string exportAnimFolder = "Animations";

	[MenuItem("Assets/Extract FBX Animations")]
	static void AssetCopy()
	{
		Object[] selectedAssets = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
		foreach (var asset in selectedAssets)
		{
			ExtractFunc(asset);
		}

		AssetDatabase.Refresh();
		Debug.Log("FBXAnimExtractor Done.");
	}

	private static void ExtractFunc(Object obj)
	{
		string path = AssetDatabase.GetAssetPath(obj);
		string exportFolder = Path.Combine(Path.GetDirectoryName(path), exportAnimFolder);

		if (!Directory.Exists(exportFolder))
		{
			Directory.CreateDirectory(exportFolder);
		}

		var fbx = AssetDatabase.LoadAllAssetsAtPath(path);
		var originalClips = System.Array.FindAll<Object>(fbx, item =>
			  item is AnimationClip
		);

		foreach (var clip in originalClips)
		{
			copyClip(clip, exportFolder);
		}
	}

	private static void copyClip(Object clip, string exportFolder)
	{
		if (clip.name.StartsWith("__preview__"))
			return;

		var instance = Object.Instantiate(clip);
		AnimationClip newAnim = instance as AnimationClip;

		string clip_name = clip.name.Replace("|", "_") + ".anim"; // replace illegal character
		string exportPath = Path.Combine(exportFolder, clip_name);

		if (File.Exists(exportPath))
		{
			var actions = new List<System.Action>();
			var bindings = AnimationUtility.GetCurveBindings(newAnim);
			AnimationClip existingAnim = (AnimationClip)AssetDatabase.LoadAssetAtPath(
					exportPath, typeof(AnimationClip));

			foreach (var binding in bindings)
			{
				var curve = AnimationUtility.GetEditorCurve(newAnim, binding);
				var cb = binding;

				actions.Add(() =>
				{
					AnimationUtility.SetEditorCurve(existingAnim, cb, curve);
				});
			}

			existingAnim.ClearCurves();
			foreach (var action in actions)
				action.Invoke();

			AssetDatabase.SaveAssets();
		}
		else
		{
			AssetDatabase.CreateAsset(newAnim, exportPath);
		}
	}
}
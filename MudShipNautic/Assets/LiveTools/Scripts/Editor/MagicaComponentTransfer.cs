using UnityEngine;
using MagicaCloth2;
using System.ComponentModel;


#if UNITY_EDITOR
using UnityEditor;
public class MagicaComponentTransfer : EditorWindow
{
	[MenuItem("Tools/MudShip/MagicaComponentTransfer")]
	public static void ShowWindow()
	{
		GetWindow<MagicaComponentTransfer>("MagicaComponentTransfer");
	}
	private GameObject sourceObject;
	private GameObject targetObject;
	private void OnGUI()
	{
		GUILayout.Label("Magica Component Transfer", EditorStyles.boldLabel);
		sourceObject = (GameObject)EditorGUILayout.ObjectField("Source RootBone", sourceObject, typeof(GameObject), true);
		targetObject = (GameObject)EditorGUILayout.ObjectField("Target RootBone", targetObject, typeof(GameObject), true);
		if (GUILayout.Button("Transfer Components"))
		{
			TransferComponents();
		}
	}
	private void TransferComponents()
	{
		if (sourceObject == null || targetObject == null)
		{
			Debug.LogError("Source and Target objects must be assigned.");
			return;
		}
		var sourceTransforms = sourceObject.GetComponentsInChildren<Transform>();
		var targetTransforms = targetObject.GetComponentsInChildren<Transform>();

		for (int i = 0; i < sourceTransforms.Length; i++)
		{
			if (sourceTransforms[i].TryGetComponent<MagicaCapsuleCollider>(out MagicaCapsuleCollider magicaCapsuleCollider))
			{
				if (sourceTransforms[i].name == targetTransforms[i].name)
				{
					MagicaCapsuleCollider copyCC = targetTransforms[i].gameObject.AddComponent<MagicaCapsuleCollider>();
					UnityEditorInternal.ComponentUtility.CopyComponent(magicaCapsuleCollider);
					UnityEditorInternal.ComponentUtility.PasteComponentAsNew(copyCC.gameObject);
				}
			}
		}



		return;
		//var components = sourceObject.GetComponents<MonoBehaviour>();
		//foreach (var component in components)
		//{
		//	var type = component.GetType();
		//	if (type.Namespace != null && type.Namespace.StartsWith("MagicaCloth"))
		//	{
		//		var existingComponent = targetObject.GetComponent(type);
		//		if (existingComponent == null)
		//		{
		//			UnityEditorInternal.ComponentUtility.CopyComponent(component);
		//			UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetObject);
		//		}
		//		else
		//		{
		//			Debug.LogWarning($"Target object already has a component of type {type.Name}. Skipping.");
		//		}
		//	}
		//}
		//Debug.Log("Component transfer complete.");
	}
}
#endif
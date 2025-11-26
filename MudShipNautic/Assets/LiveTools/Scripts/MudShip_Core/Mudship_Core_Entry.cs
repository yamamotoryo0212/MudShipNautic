using UnityEngine;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
public class Mudship_Core_Entry : Editor
{
	private void OnValidate()
	{
		Debug.Log("Mudship_Core_Entry OnValidate");
		string[] a = AssetDatabase.FindAssets("HapPlayerEditor");
		Debug.Log(a.Length);



	}
}
#endif
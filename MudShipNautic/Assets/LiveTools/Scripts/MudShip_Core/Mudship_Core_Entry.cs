using UnityEngine;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public class Mudship_Core_Entry
{
	static Mudship_Core_Entry()
	{
		string[] MS_CPS = AssetDatabase.FindAssets("MudShip-CreateProjectSettings");
		var path = AssetDatabase.GUIDToAssetPath(MS_CPS[0]);
		Mudship_Core.Settings = AssetDatabase.LoadAssetAtPath<CreateProjectSettings>(path);		
	}
}
#endif
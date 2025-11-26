using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateProjectSettings : ScriptableObject
{
	[Header("ƒŒƒ“ƒ_ƒŠƒ“ƒO•`‰ææ‚ÌRTİ’è")]
	public Vector2 Resolution = new Vector2(1920, 1080);
	public RenderTextureFormat RenderTextureFormat = RenderTextureFormat.ARGBFloat;
	public RTAntiAliasing AntiAliasing = RTAntiAliasing.Samples2;
	public bool EnableMipMap = true;
	public bool AutoGenerateMips = true;
	public bool useDynamicScale = true;
	public TextureWrapMode WrapMode = TextureWrapMode.Repeat;

	[Header("•¡»Œ³‚ÌVolume")]
	public VolumeProfile OriginalVolume;

	private void OnValidate()
	{
		Mudship_Core.Settings = this;
		Debug.Log("Mudship_Core Settings Updated" );
	}
}

public enum RTAntiAliasing
{
	None =0,
	Samples2 = 1,
	Samples4 = 2,
	Samples8 = 3,
}
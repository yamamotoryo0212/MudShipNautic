using UnityEngine;

public class RippleController : MonoBehaviour
{
	[Header("Ripple Settings")]
	[SerializeField] private Material rippleMaterial;
	[SerializeField] private float rippleSpeed = 2f;
	[SerializeField] private float rippleWidth = 0.5f;
	[SerializeField] private float rippleStrength = 0.3f;

	private Vector4[] rippleData = new Vector4[10];
	private int currentRippleIndex = 0;

	void Start()
	{
		if (rippleMaterial == null)
		{
			Debug.LogError("Ripple Material is not assigned!");
			return;
		}

		// 波紋データを初期化
		for (int i = 0; i < rippleData.Length; i++)
		{
			rippleData[i] = Vector4.zero;
		}

		UpdateShaderProperties();
	}

	void Update()
	{
		UpdateShaderProperties();
	}

	/// <summary>
	/// 指定位置に波紋を発生させる
	/// </summary>
	public void TriggerRipple(Vector3 position)
	{
		if (rippleMaterial == null) return;

		// 現在のインデックスに新しい波紋データを設定
		rippleData[currentRippleIndex] = new Vector4(
			position.x,
			position.y,
			position.z,
			Time.time
		);

		// 次のインデックスへ（循環）
		currentRippleIndex = (currentRippleIndex + 1) % rippleData.Length;
	}

	private void UpdateShaderProperties()
	{
		if (rippleMaterial == null) return;

		rippleMaterial.SetVectorArray("_RippleData", rippleData);
		rippleMaterial.SetFloat("_RippleSpeed", rippleSpeed);
		rippleMaterial.SetFloat("_RippleWidth", rippleWidth);
		rippleMaterial.SetFloat("_RippleStrength", rippleStrength);
	}
}
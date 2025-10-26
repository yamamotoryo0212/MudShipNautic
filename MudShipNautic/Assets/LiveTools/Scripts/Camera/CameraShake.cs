using UnityEngine;

/// <summary>
/// カメラに手振れ効果を追加するコンポーネント
/// </summary>
public class CameraShake : MonoBehaviour
{
	[Header("手振れ設定")]
	[Tooltip("手振れの強さ")]
	[SerializeField] private float shakeAmount = 0.1f;

	[Tooltip("手振れの速度")]
	[SerializeField] private float shakeSpeed = 1.0f;

	[Tooltip("手振れを有効にするか")]
	[SerializeField] private bool enableShake = true;

	[Header("詳細設定")]
	[Tooltip("位置の手振れ強度")]
	[SerializeField] private Vector3 positionShakeMultiplier = new Vector3(1f, 1f, 0.3f);

	[Tooltip("回転の手振れ強度")]
	[SerializeField] private Vector3 rotationShakeMultiplier = new Vector3(0.5f, 0.5f, 0.2f);

	[Tooltip("パーリンノイズのオフセット")]
	[SerializeField] private Vector3 noiseOffset = Vector3.zero;

	private Vector3 originalPosition;
	private Quaternion originalRotation;
	private float time;

	private void Start()
	{
		originalPosition = transform.localPosition;
		originalRotation = transform.localRotation;

		// ランダムなオフセットを生成（異なる手振れパターンにする）
		if (noiseOffset == Vector3.zero)
		{
			noiseOffset = new Vector3(
				Random.Range(0f, 100f),
				Random.Range(0f, 100f),
				Random.Range(0f, 100f)
			);
		}
	}

	private void Update()
	{
		if (!enableShake)
		{
			// 手振れが無効の場合は元の位置に戻す
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
			return;
		}

		time += Time.deltaTime * shakeSpeed;

		// パーリンノイズを使って滑らかな手振れを生成
		Vector3 positionShake = new Vector3(
			(Mathf.PerlinNoise(time + noiseOffset.x, 0f) - 0.5f) * 2f * positionShakeMultiplier.x,
			(Mathf.PerlinNoise(0f, time + noiseOffset.y) - 0.5f) * 2f * positionShakeMultiplier.y,
			(Mathf.PerlinNoise(time + noiseOffset.z, time + noiseOffset.z) - 0.5f) * 2f * positionShakeMultiplier.z
		) * shakeAmount;

		Vector3 rotationShake = new Vector3(
			(Mathf.PerlinNoise(time * 0.5f + noiseOffset.x + 50f, 0f) - 0.5f) * 2f * rotationShakeMultiplier.x,
			(Mathf.PerlinNoise(0f, time * 0.5f + noiseOffset.y + 50f) - 0.5f) * 2f * rotationShakeMultiplier.y,
			(Mathf.PerlinNoise(time * 0.5f + noiseOffset.z + 50f, time * 0.5f + noiseOffset.z + 50f) - 0.5f) * 2f * rotationShakeMultiplier.z
		) * shakeAmount;

		// 位置と回転を適用
		transform.localPosition = originalPosition + positionShake;
		transform.localRotation = originalRotation * Quaternion.Euler(rotationShake);
	}

	/// <summary>
	/// 手振れの強さを設定
	/// </summary>
	public void SetShakeAmount(float amount)
	{
		shakeAmount = amount;
	}

	/// <summary>
	/// 手振れの速度を設定
	/// </summary>
	public void SetShakeSpeed(float speed)
	{
		shakeSpeed = speed;
	}

	/// <summary>
	/// 手振れを有効/無効にする
	/// </summary>
	public void SetShakeEnabled(bool enabled)
	{
		enableShake = enabled;

		if (!enabled)
		{
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
		}
	}

	/// <summary>
	/// 一時的な強い揺れを発生させる（爆発などに使用）
	/// </summary>
	public void TriggerImpulse(float intensity, float duration)
	{
		StartCoroutine(ImpulseShake(intensity, duration));
	}

	private System.Collections.IEnumerator ImpulseShake(float intensity, float duration)
	{
		float elapsed = 0f;
		float originalShakeAmount = shakeAmount;

		while (elapsed < duration)
		{
			float progress = elapsed / duration;
			shakeAmount = Mathf.Lerp(intensity, originalShakeAmount, progress);
			elapsed += Time.deltaTime;
			yield return null;
		}

		shakeAmount = originalShakeAmount;
	}

	/// <summary>
	/// 元の位置と回転を更新（カメラの親が動いた場合など）
	/// </summary>
	public void UpdateOriginalTransform()
	{
		originalPosition = transform.localPosition;
		originalRotation = transform.localRotation;
	}

	private void OnValidate()
	{
		// エディタでの値変更時に元の位置を更新
		if (Application.isPlaying && enableShake == false)
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
		}
	}
}
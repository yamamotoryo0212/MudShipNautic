using UnityEngine;

/// <summary>
/// �J�����Ɏ�U����ʂ�ǉ�����R���|�[�l���g
/// </summary>
public class CameraShake : MonoBehaviour
{
	[Header("��U��ݒ�")]
	[Tooltip("��U��̋���")]
	[SerializeField] private float shakeAmount = 0.1f;

	[Tooltip("��U��̑��x")]
	[SerializeField] private float shakeSpeed = 1.0f;

	[Tooltip("��U���L���ɂ��邩")]
	[SerializeField] private bool enableShake = true;

	[Header("�ڍאݒ�")]
	[Tooltip("�ʒu�̎�U�ꋭ�x")]
	[SerializeField] private Vector3 positionShakeMultiplier = new Vector3(1f, 1f, 0.3f);

	[Tooltip("��]�̎�U�ꋭ�x")]
	[SerializeField] private Vector3 rotationShakeMultiplier = new Vector3(0.5f, 0.5f, 0.2f);

	[Tooltip("�p�[�����m�C�Y�̃I�t�Z�b�g")]
	[SerializeField] private Vector3 noiseOffset = Vector3.zero;

	private Vector3 originalPosition;
	private Quaternion originalRotation;
	private float time;

	private void Start()
	{
		originalPosition = transform.localPosition;
		originalRotation = transform.localRotation;

		// �����_���ȃI�t�Z�b�g�𐶐��i�قȂ��U��p�^�[���ɂ���j
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
			// ��U�ꂪ�����̏ꍇ�͌��̈ʒu�ɖ߂�
			transform.localPosition = originalPosition;
			transform.localRotation = originalRotation;
			return;
		}

		time += Time.deltaTime * shakeSpeed;

		// �p�[�����m�C�Y���g���Ċ��炩�Ȏ�U��𐶐�
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

		// �ʒu�Ɖ�]��K�p
		transform.localPosition = originalPosition + positionShake;
		transform.localRotation = originalRotation * Quaternion.Euler(rotationShake);
	}

	/// <summary>
	/// ��U��̋�����ݒ�
	/// </summary>
	public void SetShakeAmount(float amount)
	{
		shakeAmount = amount;
	}

	/// <summary>
	/// ��U��̑��x��ݒ�
	/// </summary>
	public void SetShakeSpeed(float speed)
	{
		shakeSpeed = speed;
	}

	/// <summary>
	/// ��U���L��/�����ɂ���
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
	/// �ꎞ�I�ȋ����h��𔭐�������i�����ȂǂɎg�p�j
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
	/// ���̈ʒu�Ɖ�]���X�V�i�J�����̐e���������ꍇ�Ȃǁj
	/// </summary>
	public void UpdateOriginalTransform()
	{
		originalPosition = transform.localPosition;
		originalRotation = transform.localRotation;
	}

	private void OnValidate()
	{
		// �G�f�B�^�ł̒l�ύX���Ɍ��̈ʒu���X�V
		if (Application.isPlaying && enableShake == false)
		{
			originalPosition = transform.localPosition;
			originalRotation = transform.localRotation;
		}
	}
}
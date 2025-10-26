using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ShadowCameraController�ƘA�g���ĉe������\������R���|�[�l���g
/// </summary>
[ExecuteAlways]
public class DepthShadowRenderer : MonoBehaviour
{
	[Header("Required References")]
	[Tooltip("ShadowCameraController���A�^�b�`���ꂽ�I�u�W�F�N�g")]
	[SerializeField] private ShadowCameraController shadowCameraController;

	[Tooltip("�e��\������}�e���A���iDepthShadowOnly�V�F�[�_�[���g�p�j")]
	[SerializeField] private Material shadowMaterial;

	[Header("Shadow Appearance")]
	[SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.7f);

	[SerializeField, Range(0f, 0.01f)]
	private float shadowBias = 0.001f;

	private static readonly int ShadowMapID = Shader.PropertyToID("_ShadowMap");
	private static readonly int ShadowMatrixID = Shader.PropertyToID("_ShadowMatrix");
	private static readonly int ShadowColorID = Shader.PropertyToID("_ShadowColor");
	private static readonly int ShadowBiasID = Shader.PropertyToID("_ShadowBiasValue");

	private Camera shadowCamera;
	private RenderTexture shadowMap;

	void Start()
	{
		Initialize();
	}

	void Update()
	{
		if (ValidateReferences())
		{
			UpdateShadowMaterial();
		}
	}

	void OnValidate()
	{
		if (Application.isPlaying && shadowMaterial != null)
		{
			UpdateShadowMaterial();
		}
	}

	private void Initialize()
	{
		if (shadowCameraController == null)
		{
			shadowCameraController = FindObjectOfType<ShadowCameraController>();
			if (shadowCameraController == null)
			{
				Debug.LogError("ShadowCameraController not found in scene!");
				return;
			}
		}

		// ���t���N�V������private�t�B�[���h���擾
		var cameraField = typeof(ShadowCameraController).GetField("shadowCamera",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		var mapField = typeof(ShadowCameraController).GetField("shadowMap",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

		if (cameraField != null)
			shadowCamera = cameraField.GetValue(shadowCameraController) as Camera;
		if (mapField != null)
			shadowMap = mapField.GetValue(shadowCameraController) as RenderTexture;
	}

	private bool ValidateReferences()
	{
		if (shadowCameraController == null)
		{
			Debug.LogWarning("ShadowCameraController is not assigned!");
			return false;
		}

		if (shadowMaterial == null)
		{
			Debug.LogWarning("Shadow Material is not assigned!");
			return false;
		}

		// �K�v�ɉ����čď�����
		if (shadowCamera == null || shadowMap == null)
		{
			Initialize();
		}

		return shadowCamera != null && shadowMap != null;
	}

	private void UpdateShadowMaterial()
	{
		// �V���h�E�}�b�v��ݒ�
		shadowMaterial.SetTexture(ShadowMapID, shadowMap);

		// �V���h�E�}�g���b�N�X���v�Z
		Matrix4x4 shadowMatrix = CalculateShadowMatrix();
		shadowMaterial.SetMatrix(ShadowMatrixID, shadowMatrix);

		// �e�̌����ڂ�ݒ�
		shadowMaterial.SetColor(ShadowColorID, shadowColor);
		shadowMaterial.SetFloat(ShadowBiasID, shadowBias);
	}

	private Matrix4x4 CalculateShadowMatrix()
	{
		// �r���[�s��ƃv���W�F�N�V�����s��
		Matrix4x4 viewMatrix = shadowCamera.worldToCameraMatrix;
		Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(shadowCamera.projectionMatrix, false);

		// �e�N�X�`�����W�ϊ��s�� (NDC [-1,1] -> UV [0,1])
		Matrix4x4 textureMatrix = Matrix4x4.identity;
		textureMatrix.m00 = 0.5f;
		textureMatrix.m11 = 0.5f;
		textureMatrix.m22 = 0.5f;
		textureMatrix.m03 = 0.5f;
		textureMatrix.m13 = 0.5f;
		textureMatrix.m23 = 0.5f;

		// �ŏI�I�ȃV���h�E�}�g���b�N�X
		return textureMatrix * projMatrix * viewMatrix;
	}

	private void OnDrawGizmosSelected()
	{
		if (shadowCamera != null)
		{
			Gizmos.color = Color.cyan;

			// �V���h�E�J�����̎������`��
			Matrix4x4 temp = Gizmos.matrix;
			Gizmos.matrix = shadowCamera.transform.localToWorldMatrix;

			if (shadowCamera.orthographic)
			{
				float size = shadowCamera.orthographicSize;
				float depth = shadowCamera.farClipPlane - shadowCamera.nearClipPlane;
				Vector3 center = Vector3.forward * (shadowCamera.nearClipPlane + depth * 0.5f);
				Gizmos.DrawWireCube(center, new Vector3(size * 2, size * 2, depth));
			}

			Gizmos.matrix = temp;
		}
	}
}
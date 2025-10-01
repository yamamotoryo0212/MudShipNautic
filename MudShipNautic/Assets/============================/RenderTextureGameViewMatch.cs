using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RenderTextureGameViewMatch : MonoBehaviour
{
	public Camera targetCamera;
	public bool useScreenResolution = true;
	public int customWidth = 1920;
	public int customHeight = 1080;

	private RenderTexture renderTexture;

	void Start()
	{
		if (targetCamera == null)
			targetCamera = GetComponent<Camera>();

		SetupRenderTexture();
	}

	void SetupRenderTexture()
	{
		// �𑜓x�ݒ�
		int width = useScreenResolution ? Screen.width : customWidth;
		int height = useScreenResolution ? Screen.height : customHeight;

		// �v���W�F�N�g�̃J���[�X�y�[�X�m�F
		bool isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

		// RenderTexture�̍쐬
		// HDR���L���ȏꍇ��DefaultHDR���g�p
		RenderTextureFormat format = targetCamera.allowHDR
			? RenderTextureFormat.DefaultHDR
			: RenderTextureFormat.Default;

		renderTexture = new RenderTexture(width, height, 24, format);

		// �J���[�X�y�[�X�ɉ�����sRGB�t���O��ݒ�
		//renderTexture.sRGB = !isLinear; // Linear�̏ꍇ��false

		// ���̑��̐ݒ��GameView�ɍ��킹��
		renderTexture.antiAliasing = QualitySettings.antiAliasing > 0 ? QualitySettings.antiAliasing : 1;
		renderTexture.filterMode = FilterMode.Bilinear;
		renderTexture.wrapMode = TextureWrapMode.Clamp;

		renderTexture.Create();

		// �J�����ɓK�p
		targetCamera.targetTexture = renderTexture;

		// URP�̒ǉ��ݒ�
		SetupURPCamera();

		Debug.Log($"RenderTexture Setup Complete:\n" +
				  $"Resolution: {width}x{height}\n" +
				  $"Format: {format}\n" +
				  $"sRGB: {renderTexture.sRGB}\n" +
				  $"HDR: {targetCamera.allowHDR}\n" +
				  $"ColorSpace: {QualitySettings.activeColorSpace}");
	}

	void SetupURPCamera()
	{
		var cameraData = targetCamera.GetUniversalAdditionalCameraData();

		if (cameraData != null)
		{
			// �|�X�g�v���Z�b�V���O�������L����
			cameraData.renderPostProcessing = true;

			// �A���`�G�C���A�V���O�ݒ�
			if (QualitySettings.antiAliasing > 0)
			{
				cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
			}

			// ���̑��̃����_�����O�ݒ�
			cameraData.renderShadows = true;
			cameraData.requiresDepthTexture = true;
			cameraData.requiresColorTexture = true;

			Debug.Log($"URP Camera Data:\n" +
					  $"Post Processing: {cameraData.renderPostProcessing}\n" +
					  $"Anti-aliasing: {cameraData.antialiasing}\n" +
					  $"Render Type: {cameraData.renderType}");
		}
	}

	// �f�o�b�O�p�FGameView��RenderTexture�̐ݒ���r
	[ContextMenu("Compare Settings")]
	void CompareSettings()
	{
		Debug.Log("=== Camera Settings ===");
		Debug.Log($"HDR: {targetCamera.allowHDR}");
		Debug.Log($"MSAA: {targetCamera.allowMSAA}");
		Debug.Log($"Clear Flags: {targetCamera.clearFlags}");
		Debug.Log($"Background: {targetCamera.backgroundColor}");

		if (renderTexture != null)
		{
			Debug.Log("\n=== RenderTexture Settings ===");
			Debug.Log($"Format: {renderTexture.format}");
			Debug.Log($"sRGB: {renderTexture.sRGB}");
			Debug.Log($"Anti-aliasing: {renderTexture.antiAliasing}");
			Debug.Log($"Size: {renderTexture.width}x{renderTexture.height}");
		}

		Debug.Log("\n=== Project Settings ===");
		Debug.Log($"Color Space: {QualitySettings.activeColorSpace}");
		Debug.Log($"Quality Level: {QualitySettings.GetQualityLevel()}");
		Debug.Log($"Anti-aliasing: {QualitySettings.antiAliasing}");
	}

	// ���s���ɉ𑜓x�ύX���������ꍇ
	void OnPreCull()
	{
		if (useScreenResolution && renderTexture != null)
		{
			if (renderTexture.width != Screen.width || renderTexture.height != Screen.height)
			{
				renderTexture.Release();
				SetupRenderTexture();
			}
		}
	}

	void OnDestroy()
	{
		if (renderTexture != null)
		{
			renderTexture.Release();
		}
	}

	// �O������RenderTexture���擾
	public RenderTexture GetRenderTexture()
	{
		return renderTexture;
	}
}
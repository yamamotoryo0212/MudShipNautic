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
		// 解像度設定
		int width = useScreenResolution ? Screen.width : customWidth;
		int height = useScreenResolution ? Screen.height : customHeight;

		// プロジェクトのカラースペース確認
		bool isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

		// RenderTextureの作成
		// HDRが有効な場合はDefaultHDRを使用
		RenderTextureFormat format = targetCamera.allowHDR
			? RenderTextureFormat.DefaultHDR
			: RenderTextureFormat.Default;

		renderTexture = new RenderTexture(width, height, 24, format);

		// カラースペースに応じてsRGBフラグを設定
		//renderTexture.sRGB = !isLinear; // Linearの場合はfalse

		// その他の設定をGameViewに合わせる
		renderTexture.antiAliasing = QualitySettings.antiAliasing > 0 ? QualitySettings.antiAliasing : 1;
		renderTexture.filterMode = FilterMode.Bilinear;
		renderTexture.wrapMode = TextureWrapMode.Clamp;

		renderTexture.Create();

		// カメラに適用
		targetCamera.targetTexture = renderTexture;

		// URPの追加設定
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
			// ポストプロセッシングを強制有効化
			cameraData.renderPostProcessing = true;

			// アンチエイリアシング設定
			if (QualitySettings.antiAliasing > 0)
			{
				cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
				cameraData.antialiasingQuality = AntialiasingQuality.High;
			}

			// その他のレンダリング設定
			cameraData.renderShadows = true;
			cameraData.requiresDepthTexture = true;
			cameraData.requiresColorTexture = true;

			Debug.Log($"URP Camera Data:\n" +
					  $"Post Processing: {cameraData.renderPostProcessing}\n" +
					  $"Anti-aliasing: {cameraData.antialiasing}\n" +
					  $"Render Type: {cameraData.renderType}");
		}
	}

	// デバッグ用：GameViewとRenderTextureの設定を比較
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

	// 実行時に解像度変更があった場合
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

	// 外部からRenderTextureを取得
	public RenderTexture GetRenderTexture()
	{
		return renderTexture;
	}
}
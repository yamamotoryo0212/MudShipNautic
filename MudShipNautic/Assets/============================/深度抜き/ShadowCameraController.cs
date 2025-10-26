using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class ShadowCameraController : MonoBehaviour
{
	[Header("Shadow Settings")]
	[SerializeField] private Camera shadowCamera;
	[SerializeField] private RenderTexture shadowMap;
	[SerializeField] private int shadowResolution = 1024;
	[SerializeField]
	private Material Material;
	
	private RenderTexture depthTexture;
	private Matrix4x4 shadowMatrix;

	void OnEnable()
	{
		SetupShadowCamera();
		CreateDepthTexture();

	}

	void OnDisable()
	{
		CleanupTextures();
	}

	void SetupShadowCamera()
	{
		if (shadowCamera == null)
		{
			GameObject camObj = new GameObject("Shadow Camera");
			camObj.transform.SetParent(transform);
			shadowCamera = camObj.AddComponent<Camera>();
		}

		shadowCamera.enabled = false;
		shadowCamera.clearFlags = CameraClearFlags.SolidColor;
		shadowCamera.backgroundColor = Color.white;
		shadowCamera.depth = -100;

		// 深度のみをレンダリング
		shadowCamera.depthTextureMode = DepthTextureMode.Depth;
	}

	void CreateDepthTexture()
	{
		if (depthTexture != null)
			depthTexture.Release();

		depthTexture = new RenderTexture(shadowResolution, shadowResolution, 24, RenderTextureFormat.Depth);
		depthTexture.filterMode = FilterMode.Bilinear;
		depthTexture.wrapMode = TextureWrapMode.Clamp;
		depthTexture.Create();

		if (shadowMap == null)
		{
			shadowMap = new RenderTexture(shadowResolution, shadowResolution, 0, RenderTextureFormat.R16);
			shadowMap.filterMode = FilterMode.Bilinear;
			shadowMap.wrapMode = TextureWrapMode.Clamp;
			shadowMap.Create();
		}
	}

	void Update()
	{
		if (shadowCamera == null) return;

		RenderShadowMap();
		UpdateShadowMatrix();
	}

	void RenderShadowMap()
	{
		shadowCamera.targetTexture = depthTexture;
		shadowCamera.Render();

		// 深度情報をshadowMapにコピー
		Graphics.Blit(depthTexture, shadowMap);
	}

	void UpdateShadowMatrix()
	{
		// ワールド空間からシャドウカメラの投影空間への変換行列
		Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(shadowCamera.projectionMatrix, false);
		Matrix4x4 viewMatrix = shadowCamera.worldToCameraMatrix;

		// テクスチャ座標への変換 ([-1,1] -> [0,1])
		Matrix4x4 textureMatrix = Matrix4x4.identity;
		textureMatrix.m00 = 0.5f;
		textureMatrix.m11 = 0.5f;
		textureMatrix.m22 = 0.5f;
		textureMatrix.m03 = 0.5f;
		textureMatrix.m13 = 0.5f;
		textureMatrix.m23 = 0.5f;

		shadowMatrix = textureMatrix * projectionMatrix * viewMatrix;
	}

	void CleanupTextures()
	{
		if (depthTexture != null)
		{
			depthTexture.Release();
			depthTexture = null;
		}

		if (shadowMap != null && Application.isPlaying)
		{
			shadowMap.Release();
		}
	}

	void OnDrawGizmos()
	{
		if (shadowCamera != null)
		{
			Gizmos.color = Color.yellow;
			Matrix4x4 temp = Gizmos.matrix;
			Gizmos.matrix = shadowCamera.transform.localToWorldMatrix;

			float size = shadowCamera.orthographicSize;
			float far = shadowCamera.farClipPlane;

			Gizmos.DrawWireCube(new Vector3(0, 0, far / 2), new Vector3(size * 2, size * 2, far));
			Gizmos.matrix = temp;
		}
	}
}
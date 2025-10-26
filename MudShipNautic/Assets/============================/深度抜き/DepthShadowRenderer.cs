using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// ShadowCameraControllerと連携して影だけを表示するコンポーネント
/// </summary>
[ExecuteAlways]
public class DepthShadowRenderer : MonoBehaviour
{
	[Header("Required References")]
	[Tooltip("ShadowCameraControllerがアタッチされたオブジェクト")]
	[SerializeField] private ShadowCameraController shadowCameraController;

	[Tooltip("影を表示するマテリアル（DepthShadowOnlyシェーダーを使用）")]
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

		// リフレクションでprivateフィールドを取得
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

		// 必要に応じて再初期化
		if (shadowCamera == null || shadowMap == null)
		{
			Initialize();
		}

		return shadowCamera != null && shadowMap != null;
	}

	private void UpdateShadowMaterial()
	{
		// シャドウマップを設定
		shadowMaterial.SetTexture(ShadowMapID, shadowMap);

		// シャドウマトリックスを計算
		Matrix4x4 shadowMatrix = CalculateShadowMatrix();
		shadowMaterial.SetMatrix(ShadowMatrixID, shadowMatrix);

		// 影の見た目を設定
		shadowMaterial.SetColor(ShadowColorID, shadowColor);
		shadowMaterial.SetFloat(ShadowBiasID, shadowBias);
	}

	private Matrix4x4 CalculateShadowMatrix()
	{
		// ビュー行列とプロジェクション行列
		Matrix4x4 viewMatrix = shadowCamera.worldToCameraMatrix;
		Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(shadowCamera.projectionMatrix, false);

		// テクスチャ座標変換行列 (NDC [-1,1] -> UV [0,1])
		Matrix4x4 textureMatrix = Matrix4x4.identity;
		textureMatrix.m00 = 0.5f;
		textureMatrix.m11 = 0.5f;
		textureMatrix.m22 = 0.5f;
		textureMatrix.m03 = 0.5f;
		textureMatrix.m13 = 0.5f;
		textureMatrix.m23 = 0.5f;

		// 最終的なシャドウマトリックス
		return textureMatrix * projMatrix * viewMatrix;
	}

	private void OnDrawGizmosSelected()
	{
		if (shadowCamera != null)
		{
			Gizmos.color = Color.cyan;

			// シャドウカメラの視錐台を描画
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
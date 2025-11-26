using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlanarReflectionView : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Camera _mainCamera;// メインカメラ
	[SerializeField] private Camera _reflectionCamera = null;// 反射用テクスチャを取得するためのリフレクションカメラ
	[SerializeField] private GameObject _reflectionTargetPlane = null; // 反射平面を行うオブジェクト
	[SerializeField] private Skybox _mainSkybox;
	[SerializeField] private Skybox _reflectionSkybox;


	[Header("Render Settings")]
	[SerializeField, Range(0.3f, 1.0f)] private float _resolutionScale = 1.0f;// テクスチャ解像度（数値を上げるほど高負荷） 0.3f: 低解像度, 1.0f: フル解像度    

	[Header("Internal Runtime States")]
	private RenderTexture _renderTarget; // リフレクションカメラの撮影結果を格納するRenderTexture
	private Material _floorMaterial; // 平面のマテリアル　シェーダー（PlanarReflection）操作用

	private int _lastScreenWidth;
	private int _lastScreenHeight;
	private float _lastResolutionScale;

	private void Start()
	{
		//反射平面のマテリアル取得
		Renderer renderer = _reflectionTargetPlane.GetComponent<Renderer>();
		_floorMaterial = renderer.sharedMaterial;

		// カメラコンポーネント無効化：リフレクションカメラはUnityのデフォルトレンダリングフローには参加させず、不要なレンダリングや順序の問題を避ける
		_reflectionCamera.enabled = false;

		// 初期スクリーンサイズとスケール
		_lastScreenWidth = Screen.width;
		_lastScreenHeight = Screen.height;
		_lastResolutionScale = _resolutionScale;

		CreateRenderTarget();
	}

	void Update()
	{
		// スクリーンサイズ or 解像度スケール変更検出
		if (Screen.width != _lastScreenWidth ||
			Screen.height != _lastScreenHeight ||
			!Mathf.Approximately(_resolutionScale, _lastResolutionScale))
		{
			_lastScreenWidth = Screen.width;
			_lastScreenHeight = Screen.height;
			_lastResolutionScale = _resolutionScale;
			RecreateRenderTarget();
		}
	}

	private void LateUpdate()
	{
		// フレーム終了時に反射描画
		StartCoroutine(RenderReflectionAtEndOfFrame());
	}

	private IEnumerator RenderReflectionAtEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		RenderReflection();
	}

	private void CreateRenderTarget()
	{
		int width = Mathf.Max(256, Mathf.RoundToInt(Screen.width * _resolutionScale));
		int height = Mathf.Max(256, Mathf.RoundToInt(Screen.height * _resolutionScale));

		// 既存のRenderTextureがあれば解放
		if (_renderTarget != null)
		{
			_reflectionCamera.targetTexture = null;
			_renderTarget.Release();
			DestroyImmediate(_renderTarget);
		}

		// 新しいRenderTextureを作成（透過対応）
		_renderTarget = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
		{
			name = "PlanarReflectionRT",
			useMipMap = false,
			autoGenerateMips = false,
			enableRandomWrite = false
		};

		_floorMaterial.SetTexture("_ReflectionTex", _renderTarget);// マテリアルにリフレクションテクスチャを設定
	}

	private void RecreateRenderTarget()
	{
		if (_renderTarget != null)
		{
			_reflectionCamera.targetTexture = null;
			_renderTarget.Release();
			DestroyImmediate(_renderTarget);
		}
		CreateRenderTarget();
		RenderReflection();//カメラ変更時に真っ黒な床が一瞬表示されるためすぐ描画する。
	}

	private void RenderReflection()
	{
		// メインカメラの設定をコピーし、位置・向きなどを反映
		_reflectionCamera.CopyFrom(_mainCamera);

		// 重要：透過背景の設定（CopyFromの後に実行）
		_reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
		_reflectionCamera.backgroundColor = new Color(1,1,1,0); // (0,0,0,0)

		_reflectionCamera.cullingMask = 247;

		var uac = _reflectionCamera.GetComponent<UniversalAdditionalCameraData>();
		if (uac != null)
		{
			uac.renderPostProcessing = false;
		}

		// Skybox同期
		if (_mainSkybox != null && _mainSkybox.material != null)
		{
			_reflectionSkybox.material = _mainSkybox.material;
		}

		// ワールド空間でのメインカメラの方向・上向き・位置
		Vector3 cameraDirectionWorldSpace = _mainCamera.transform.forward;
		Vector3 cameraUpWorldSpace = _mainCamera.transform.up;
		Vector3 cameraPositionWorldSpace = _mainCamera.transform.position;

		// 反射平面オブジェクトのローカル空間に変換
		Vector3 cameraDirectionPlaneSpace = _reflectionTargetPlane.transform.InverseTransformDirection(cameraDirectionWorldSpace);
		Vector3 cameraUpPlaneSpace = _reflectionTargetPlane.transform.InverseTransformDirection(cameraUpWorldSpace);
		Vector3 cameraPositionPlaneSpace = _reflectionTargetPlane.transform.InverseTransformPoint(cameraPositionWorldSpace);

		// ローカル空間では平面の法線が (0, 1, 0) と仮定し、Y軸方向を反転して鏡面対称を得る
		cameraDirectionPlaneSpace.y *= -1.0f;
		cameraUpPlaneSpace.y *= -1.0f;
		cameraPositionPlaneSpace.y *= -1.0f;

		// 再びワールド空間へ変換
		cameraDirectionWorldSpace = _reflectionTargetPlane.transform.TransformDirection(cameraDirectionPlaneSpace);
		cameraUpWorldSpace = _reflectionTargetPlane.transform.TransformDirection(cameraUpPlaneSpace);
		cameraPositionWorldSpace = _reflectionTargetPlane.transform.TransformPoint(cameraPositionPlaneSpace);

		// 反射カメラに位置と向きを設定
		_reflectionCamera.transform.position = cameraPositionWorldSpace;
		_reflectionCamera.transform.LookAt(cameraPositionWorldSpace + cameraDirectionWorldSpace, cameraUpWorldSpace);

		// レンダリングターゲットを設定して描画
		_reflectionCamera.targetTexture = _renderTarget;
		_reflectionCamera.Render();
	}
}
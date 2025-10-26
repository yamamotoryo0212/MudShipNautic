using UnityEngine;

/// <summary>
/// ライトを使わずに平面に影を投影するコンポーネント
/// キャラクターにアタッチして使用
/// </summary>
public class PlanarShadow : MonoBehaviour
{
	[Header("影の設定")]
	[SerializeField] private Transform targetCharacter; // 影を落とすキャラクター
	[SerializeField] private Material shadowMaterial; // 影用マテリアル
	[SerializeField] private float groundHeight = 0f; // 地面のY座標
	[SerializeField] private Vector3 lightDirection = new Vector3(0.3f, -1f, 0.3f); // 仮想光源の方向

	[Header("影の見た目")]
	[SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);
	[SerializeField] private float maxShadowDistance = 10f; // 影が消える最大距離

	private GameObject shadowObject;
	private MeshFilter shadowMeshFilter;
	private MeshRenderer shadowRenderer;
	private Mesh originalMesh;
	private Mesh shadowMesh;

	void Start()
	{
		if (targetCharacter == null)
			targetCharacter = transform;

		CreateShadowObject();
		SetupShadowMaterial();
	}

	void CreateShadowObject()
	{
		// 影用のゲームオブジェクトを作成
		shadowObject = new GameObject("PlanarShadow");
		shadowObject.transform.SetParent(transform);

		shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
		shadowRenderer = shadowObject.AddComponent<MeshRenderer>();

		// キャラクターのメッシュを取得
		MeshFilter characterMesh = targetCharacter.GetComponentInChildren<MeshFilter>();
		if (characterMesh != null)
		{
			originalMesh = characterMesh.sharedMesh;
			shadowMesh = new Mesh();
			shadowMesh.name = "Shadow Mesh";
			shadowMeshFilter.mesh = shadowMesh;
		}
	}

	void SetupShadowMaterial()
	{
		if (shadowMaterial == null)
		{
			// デフォルトの影マテリアルを作成
			shadowMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
			shadowMaterial.SetColor("_BaseColor", shadowColor);
			shadowMaterial.SetFloat("_Surface", 1); // Transparent
			shadowMaterial.SetFloat("_Blend", 0); // Alpha
			shadowMaterial.renderQueue = 3000; // Transparent queue
		}

		shadowRenderer.material = shadowMaterial;
		shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		shadowRenderer.receiveShadows = false;
	}

	void LateUpdate()
	{
		if (originalMesh == null || shadowMesh == null)
			return;

		UpdateShadowMesh();
		UpdateShadowVisibility();
	}

	void UpdateShadowMesh()
	{
		Vector3[] originalVertices = originalMesh.vertices;
		Vector3[] shadowVertices = new Vector3[originalVertices.Length];

		// 光源方向を正規化
		Vector3 lightDir = lightDirection.normalized;

		// 各頂点を地面に投影
		for (int i = 0; i < originalVertices.Length; i++)
		{
			Vector3 worldPos = targetCharacter.TransformPoint(originalVertices[i]);

			// 影の投影計算
			float distance = (groundHeight - worldPos.y) / lightDir.y;
			Vector3 shadowPos = worldPos + lightDir * distance;
			shadowPos.y = groundHeight + 0.01f; // 地面から少し浮かせる

			shadowVertices[i] = shadowObject.transform.InverseTransformPoint(shadowPos);
		}

		shadowMesh.Clear();
		shadowMesh.vertices = shadowVertices;
		shadowMesh.triangles = originalMesh.triangles;
		shadowMesh.RecalculateNormals();
		shadowMesh.RecalculateBounds();
	}

	void UpdateShadowVisibility()
	{
		// キャラクターと地面の距離に応じて影の濃さを調整
		float distanceToGround = targetCharacter.position.y - groundHeight;
		float alpha = Mathf.Clamp01(1f - (distanceToGround / maxShadowDistance));

		Color color = shadowColor;
		color.a = shadowColor.a * alpha;
		shadowMaterial.SetColor("_BaseColor", color);

		shadowRenderer.enabled = alpha > 0.01f;
	}

	void OnDrawGizmosSelected()
	{
		// デバッグ用：光源方向を表示
		Gizmos.color = Color.yellow;
		Vector3 start = transform.position + Vector3.up * 2f;
		Gizmos.DrawRay(start, lightDirection.normalized * 3f);
	}
}
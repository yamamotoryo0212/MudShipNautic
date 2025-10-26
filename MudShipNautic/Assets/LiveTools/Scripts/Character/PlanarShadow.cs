using UnityEngine;

/// <summary>
/// ���C�g���g�킸�ɕ��ʂɉe�𓊉e����R���|�[�l���g
/// �L�����N�^�[�ɃA�^�b�`���Ďg�p
/// </summary>
public class PlanarShadow : MonoBehaviour
{
	[Header("�e�̐ݒ�")]
	[SerializeField] private Transform targetCharacter; // �e�𗎂Ƃ��L�����N�^�[
	[SerializeField] private Material shadowMaterial; // �e�p�}�e���A��
	[SerializeField] private float groundHeight = 0f; // �n�ʂ�Y���W
	[SerializeField] private Vector3 lightDirection = new Vector3(0.3f, -1f, 0.3f); // ���z�����̕���

	[Header("�e�̌�����")]
	[SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);
	[SerializeField] private float maxShadowDistance = 10f; // �e��������ő勗��

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
		// �e�p�̃Q�[���I�u�W�F�N�g���쐬
		shadowObject = new GameObject("PlanarShadow");
		shadowObject.transform.SetParent(transform);

		shadowMeshFilter = shadowObject.AddComponent<MeshFilter>();
		shadowRenderer = shadowObject.AddComponent<MeshRenderer>();

		// �L�����N�^�[�̃��b�V�����擾
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
			// �f�t�H���g�̉e�}�e���A�����쐬
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

		// ���������𐳋K��
		Vector3 lightDir = lightDirection.normalized;

		// �e���_��n�ʂɓ��e
		for (int i = 0; i < originalVertices.Length; i++)
		{
			Vector3 worldPos = targetCharacter.TransformPoint(originalVertices[i]);

			// �e�̓��e�v�Z
			float distance = (groundHeight - worldPos.y) / lightDir.y;
			Vector3 shadowPos = worldPos + lightDir * distance;
			shadowPos.y = groundHeight + 0.01f; // �n�ʂ��班����������

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
		// �L�����N�^�[�ƒn�ʂ̋����ɉ����ĉe�̔Z���𒲐�
		float distanceToGround = targetCharacter.position.y - groundHeight;
		float alpha = Mathf.Clamp01(1f - (distanceToGround / maxShadowDistance));

		Color color = shadowColor;
		color.a = shadowColor.a * alpha;
		shadowMaterial.SetColor("_BaseColor", color);

		shadowRenderer.enabled = alpha > 0.01f;
	}

	void OnDrawGizmosSelected()
	{
		// �f�o�b�O�p�F����������\��
		Gizmos.color = Color.yellow;
		Vector3 start = transform.position + Vector3.up * 2f;
		Gizmos.DrawRay(start, lightDirection.normalized * 3f);
	}
}
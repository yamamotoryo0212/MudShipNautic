using UnityEngine;

public class AudioSpectrumGradientController : MonoBehaviour
{
	[Header("Spectrum Objects")]
	[Tooltip("横一列に並んだオーディオスペクトラムのオブジェクト")]
	public GameObject[] spectrumObjects;

	[Header("Auto Find Settings")]
	[Tooltip("自動検索を有効にする（子オブジェクトを自動検出）")]
	public bool autoFindChildren = true;

	[Tooltip("特定のタグでフィルタリング")]
	public string filterByTag = "";

	[Header("Gradient Axis")]
	[Tooltip("グラデーションを適用する軸")]
	public GradientAxis axis = GradientAxis.X;

	public enum GradientAxis
	{
		X,
		Y,
		Z
	}

	private Material sharedMaterial;

	void Start()
	{
		if (autoFindChildren)
		{
			FindSpectrumObjects();
		}

		UpdateGradientParameters();
	}

	void FindSpectrumObjects()
	{
		if (string.IsNullOrEmpty(filterByTag))
		{
			// 全ての子オブジェクトを取得
			int childCount = transform.childCount;
			spectrumObjects = new GameObject[childCount];
			for (int i = 0; i < childCount; i++)
			{
				spectrumObjects[i] = transform.GetChild(i).gameObject;
			}
		}
		else
		{
			// タグでフィルタリング
			spectrumObjects = GameObject.FindGameObjectsWithTag(filterByTag);
		}

		Debug.Log($"Found {spectrumObjects.Length} spectrum objects");
	}

	void UpdateGradientParameters()
	{
		if (spectrumObjects == null || spectrumObjects.Length == 0)
		{
			Debug.LogWarning("No spectrum objects assigned!");
			return;
		}

		// 全オブジェクトの位置から範囲を計算
		float minPos = float.MaxValue;
		float maxPos = float.MinValue;

		foreach (GameObject obj in spectrumObjects)
		{
			if (obj == null) continue;

			float pos = GetPositionOnAxis(obj.transform.position);
			minPos = Mathf.Min(minPos, pos);
			maxPos = Mathf.Max(maxPos, pos);
		}

		float gradientOrigin = minPos;
		float gradientRange = maxPos - minPos;

		// 全オブジェクトのマテリアルに設定
		foreach (GameObject obj in spectrumObjects)
		{
			if (obj == null) continue;

			Renderer renderer = obj.GetComponent<Renderer>();
			if (renderer != null && renderer.material != null)
			{
				renderer.material.SetFloat("_GradientOrigin", gradientOrigin);
				renderer.material.SetFloat("_GradientRange", gradientRange);
			}
		}

		Debug.Log($"Gradient Origin: {gradientOrigin}, Range: {gradientRange}");
	}

	float GetPositionOnAxis(Vector3 position)
	{
		switch (axis)
		{
			case GradientAxis.X:
				return position.x;
			case GradientAxis.Y:
				return position.y;
			case GradientAxis.Z:
				return position.z;
			default:
				return position.x;
		}
	}

	// インスペクターから手動で更新する場合
	[ContextMenu("Update Gradient")]
	public void ManualUpdate()
	{
		if (autoFindChildren)
		{
			FindSpectrumObjects();
		}
		UpdateGradientParameters();
	}

	void OnValidate()
	{
		// エディタ上でパラメータが変更されたときに更新
		if (Application.isPlaying)
		{
			UpdateGradientParameters();
		}
	}
}
using System;
using System.Collections.Generic;
using UnityEngine;

public class MS_MaterialEntry : MonoBehaviour
{
	[SerializeField]
	[System.Serializable]
	public class FloatProperty
	{
		public string propertyName = "_Float";
		[SerializeField] private float value;

		public float Value
		{
			get => value;
			set => this.value = value;
		}
	}

	[System.Serializable]
	public class ColorProperty
	{
		public string propertyName = "_Color";
		[SerializeField] private Color value = Color.white;

		public Color Value
		{
			get => value;
			set => this.value = value;
		}
	}

	[SerializeField] private Material targetMaterial;
	[SerializeField] private List<FloatProperty> floatProperties = new List<FloatProperty>();
	[SerializeField] private List<ColorProperty> colorProperties = new List<ColorProperty>();

	private Material runtimeMaterial;

	private void Awake()
	{
		// マテリアルのインスタンスを作成（元のマテリアルを変更しないため）
		if (targetMaterial != null)
		{
			runtimeMaterial = new Material(targetMaterial);

			// Rendererがアタッチされている場合は自動適用
			Renderer renderer = GetComponent<Renderer>();
			if (renderer != null)
			{
				renderer.material = runtimeMaterial;
			}
		}
	}

	private void Update()
	{
		if (runtimeMaterial == null) return;

		// Float プロパティの適用
		foreach (var prop in floatProperties)
		{
			if (!string.IsNullOrEmpty(prop.propertyName))
			{
				runtimeMaterial.SetFloat(prop.propertyName, prop.Value);
			}
		}

		// Color プロパティの適用
		foreach (var prop in colorProperties)
		{
			if (!string.IsNullOrEmpty(prop.propertyName))
			{
				runtimeMaterial.SetColor(prop.propertyName, prop.Value);
			}
		}
	}

	private void OnDestroy()
	{
		// ランタイムで作成したマテリアルを破棄
		if (runtimeMaterial != null)
		{
			Destroy(runtimeMaterial);
		}
	}

	// 外部からマテリアルを設定するメソッド
	public void SetTargetMaterial(Material material)
	{
		targetMaterial = material;
		if (runtimeMaterial != null)
		{
			Destroy(runtimeMaterial);
		}
		runtimeMaterial = new Material(material);
	}

	// 特定のFloatプロパティを取得
	public FloatProperty GetFloatProperty(string propertyName)
	{
		return floatProperties.Find(p => p.propertyName == propertyName);
	}

	// 特定のColorプロパティを取得
	public ColorProperty GetColorProperty(string propertyName)
	{
		return colorProperties.Find(p => p.propertyName == propertyName);
	}
}
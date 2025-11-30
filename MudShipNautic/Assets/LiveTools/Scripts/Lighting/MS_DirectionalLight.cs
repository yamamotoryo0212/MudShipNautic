using PotaToon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class MS_DirectionalLight : MonoBehaviour
{
	public Volume postProcessingVolume = null;
	private PotaToon.PotaToon potaToonEffect = null;
	private float initialPostExposure = 0f;
	private float initialScreenRimWidth = 0f;

	public List<Light> characterLights = new List<Light>();
	public List<Light> stageLights = new List<Light>();

	private List<float> defStageLightIntensities = new List<float>();

	[SerializeField]
	private float maxLightIntensity = 1f;
	public float LightIntensity = 1f;
	private float previousLightIntensity = 0f;

	[SerializeField]
	private List<Material> emissiveMaterials = new List<Material>();
	private List<Color> originalEmissionColors = new List<Color>();

	private void Start()
	{
		postProcessingVolume.profile.TryGet<PotaToon.PotaToon>(out potaToonEffect);
		initialPostExposure = potaToonEffect.charPostExposure.value;
		initialScreenRimWidth = potaToonEffect.screenRimWidth.value;
		previousLightIntensity = maxLightIntensity;

		foreach (Material material in emissiveMaterials)
		{
			Color originalEmissionColor = material.GetColor("_EmissionColor");
			originalEmissionColors.Add(originalEmissionColor);
		}

		// ステージライトの初期強度を保存
		foreach (Light stageLight in stageLights)
		{
			defStageLightIntensities.Add(stageLight.intensity);
		}
	}

	private void Update()
	{
		// すべてのキャラクターライトの強度を設定
		foreach (Light characterLight in characterLights)
		{
			characterLight.intensity = LightIntensity;
		}

		if (LightIntensity == previousLightIntensity)
			return;

		float normalizedLightIntensity = LightIntensity / maxLightIntensity;

		// エミッシブマテリアルの更新
		for (int i = 0; i < emissiveMaterials.Count; i++)
		{
			emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i] * normalizedLightIntensity);
		}

		// ポストプロセスエフェクトの更新
		potaToonEffect.charPostExposure.value = initialPostExposure * normalizedLightIntensity;
		potaToonEffect.screenRimWidth.value = initialScreenRimWidth * normalizedLightIntensity;

		// すべてのステージライトの強度を更新
		for (int i = 0; i < stageLights.Count; i++)
		{
			stageLights[i].intensity = defStageLightIntensities[i] * normalizedLightIntensity;
		}

		previousLightIntensity = LightIntensity;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < emissiveMaterials.Count; i++)
		{
			emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i]);
		}
	}
}
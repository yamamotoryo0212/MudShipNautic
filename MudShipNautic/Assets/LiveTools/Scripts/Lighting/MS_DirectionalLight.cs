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


	public Light characterLight = null;
	public Light _stageLight = null;
	private float _defStageLightIntensity = 0f;

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
		_defStageLightIntensity = _stageLight.intensity;
	}

	private void Update()
	{
		characterLight.intensity = LightIntensity;
		if (characterLight.intensity == previousLightIntensity)
			return;

		float normalizedLightIntensity = characterLight.intensity / maxLightIntensity;

		for (int i = 0; i < emissiveMaterials.Count; i++)
		{
			emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i] * normalizedLightIntensity);
		}

		potaToonEffect.charPostExposure.value = initialPostExposure * normalizedLightIntensity;
		potaToonEffect.screenRimWidth.value = initialScreenRimWidth* normalizedLightIntensity;

		previousLightIntensity = characterLight.intensity;
		_stageLight.intensity = _defStageLightIntensity * normalizedLightIntensity;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < emissiveMaterials.Count; i++)
		{
			emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i]);
		}
	}
}
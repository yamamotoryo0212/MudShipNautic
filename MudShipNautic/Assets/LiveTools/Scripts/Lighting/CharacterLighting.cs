using PotaToon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VisualScripting;

public class CharacterLighting : MonoBehaviour
{
	[SerializeField]
	private Volume postProcessingVolume = null;

	private PotaToon.PotaToon potaToonEffect = null;
	private float initialPostExposure = 0f;
	private float initialScreenRimWidth = 0f;

	[SerializeField]
	private Light characterLight = null;

	[SerializeField]
	private float maxLightIntensity = 0f;

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
	}

	private void Update()
	{
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
	}

	private void OnDestroy()
	{
		for (int i = 0; i < emissiveMaterials.Count; i++)
		{
			emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i]);
		}
	}
}
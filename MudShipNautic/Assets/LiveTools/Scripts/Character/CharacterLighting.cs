using PotaToon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PotaToon;
using Unity.VisualScripting;

public class CharacterLighting : MonoBehaviour
{
	[SerializeField,Range(0,1)]
	private float _intensity = 1f;

	private const float _charalightMultiplier = 15;

	private VolumeProfile _volumeProfile = null;

	private void Start()
	{

		var volume = GetComponent<Volume>();
		if (volume != null)
		{
			_volumeProfile = volume.sharedProfile;
		}

		if (_volumeProfile.TryGet(out PotaToon.PotaToon component))
		{
			//component.charPostExposure.Override(0f);
		}

	}
}

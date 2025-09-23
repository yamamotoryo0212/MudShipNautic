using PotaToon;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using PotaToon;
using Unity.VisualScripting;

public class PotaToonSyncCharaLightEmission : MonoBehaviour
{
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

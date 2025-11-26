using UnityEngine;
using UnityEngine.Rendering;
using VolumetricLights;

namespace MS_LiveTools.Lighting
{
	public enum MS_MonoLightType
	{
		BeamLight,
		WashLight,
		PanelLight
	}

	public class MS_MonoLightController : MonoBehaviour
	{
		[SerializeField, Header("シリアライズメンバー")]
		private Light _projectionLight = null;
		[SerializeField]
		private MeshRenderer[] _emissionMesh = null;
		[SerializeField]
		private VolumetricLight _volumetricLight = null;
		private Light _volumetricLightSource = null;
		[SerializeField]
		private Transform _panTransform = null;
		[SerializeField]
		private Transform _tiltTransform = null;
		[SerializeField]
		private Transform _rollTransform = null;

		[Header("クランプ周り")]
		public float _maxIntensity = 1.5f;
		public float _maxRange = 5f;
		public float _emissionMultiplayer = 3f;

		[Header("共通パラメータ")]
		public MS_MonoLightType LightType = MS_MonoLightType.BeamLight;
		[ColorUsage(false, true)]
		public Color LightColor = Color.white;
		private Color _lastLightColor = Color.white;

		public bool EnableAdjustEmissionColor = false;
		public Color AdjustEmissionColor = Color.white;
		[Range(0, 1)]
		public float LightIntensity = 1f;
		private float _lastLightIntensity = 0;
		public float Pan, Tilt, Roll = 0;
		public float LightAngle = 30f;
		private float _lastLightAngle = 0;


		private float _defProjectionIntensity = 1f;
		private LensFlare _lensFlare = null;

		private void Start()
		{

			if (LightType == MS_MonoLightType.WashLight)
			{
				_emissionMultiplayer = _emissionMultiplayer * 0.25f;
			}
			if (LightType == MS_MonoLightType.BeamLight)
			{
				_emissionMultiplayer = _emissionMultiplayer * 0.5f;
			}

			foreach (var _emissionMesh in _emissionMesh)
			{
				Material emissionMat = _emissionMesh.sharedMaterial;
				_emissionMesh.sharedMaterial = new Material(emissionMat);
			}

			_volumetricLightSource = _volumetricLight.GetComponent<Light>();


			_defProjectionIntensity = _projectionLight.intensity;
		}

		private void Update()
		{
			if (LightIntensity != _lastLightIntensity)
			{
				_volumetricLightSource.intensity = LightIntensity * _maxIntensity;
				_volumetricLightSource.range = LightIntensity * _maxIntensity * _maxRange;

				_projectionLight.intensity = LightIntensity * _defProjectionIntensity;
				_projectionLight.range = LightIntensity * _maxIntensity * _maxRange;

				_lastLightIntensity = LightIntensity;
			}

			if (LightColor != _lastLightColor)
			{
				foreach (var emissionMesh in _emissionMesh)
				{
					if (EnableAdjustEmissionColor)
					{
						emissionMesh.sharedMaterial.SetColor("_EmissionColor", AdjustEmissionColor * LightIntensity * _maxIntensity * _emissionMultiplayer);
					}
					else
					{
						emissionMesh.sharedMaterial.SetColor("_EmissionColor", LightColor * LightIntensity * _maxIntensity * _emissionMultiplayer);
					}
					emissionMesh.sharedMaterial.SetColor("_BaseColor", LightColor);
				}
				_volumetricLightSource.color = LightColor;
				_projectionLight.color = LightColor;

				_lastLightColor = LightColor;
			}

			if (_panTransform != null && _tiltTransform != null && _rollTransform != null)
			{
				_panTransform.localRotation = Quaternion.Euler(0, Pan, 0);
				_tiltTransform.localRotation = Quaternion.Euler(Tilt, 0, 0);
				_rollTransform.localRotation = Quaternion.Euler(0, Roll, 0);
			}

			if (LightAngle != _lastLightAngle)
			{
				_volumetricLightSource.spotAngle = LightAngle;
				_projectionLight.spotAngle = LightAngle;
				_lastLightAngle = LightAngle;
			}
		}
	}
}
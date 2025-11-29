using UnityEngine;

public class MS_CharacterIndirectLight : MonoBehaviour
{
	private Light _light = null;
	
	public float Intensity = 1.0f;
	private float _lastIntensity = 1.0f;
	public Color LightColor = Color.white;
	private Color _lastLightColor = Color.white;

	[SerializeField]
	private bool _enableTrackTarget = false;
	[SerializeField]
	private Transform _targetTransform = null;

	private void Awake()
	{
		_light = GetComponent<Light>();
	}

	private void Update()
	{
		if (_enableTrackTarget && _targetTransform != null)
		{
			transform.LookAt(_targetTransform);
		}

		if (Intensity != _lastIntensity)
		{
			_light.intensity = Intensity;
			_lastIntensity = Intensity;
		}
		if (LightColor != _lastLightColor)
		{
			_light.color = LightColor;
			_lastLightColor = LightColor;
		}
	}
}

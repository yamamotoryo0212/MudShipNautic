using UnityEngine;

public class RGBCameraTracker : MonoBehaviour
{
	[SerializeField]
	private Camera _rgbCamera = null;

	private Camera _thisCamera = null;

	private void Awake()
	{
		_thisCamera = GetComponent<Camera>();
	}
	private void Update()
	{
		_thisCamera.fieldOfView = _rgbCamera.fieldOfView;
	}
}

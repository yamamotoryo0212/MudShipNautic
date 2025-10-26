using UnityEngine;

public class EyeController : MonoBehaviour
{
	[SerializeField]
	private Transform _leftEyeTarget = null;
	[SerializeField]
	private Transform _rightEyeTarget = null;


	private Quaternion _defaultLeftEyeRotation = Quaternion.identity;
	private Quaternion _defaultRightEyeRotation = Quaternion.identity;

	private void Start()
	{
		_defaultLeftEyeRotation = _leftEyeTarget.localRotation;
		_defaultRightEyeRotation = _rightEyeTarget.localRotation;
	}

	private void LateUpdate()
	{
		_leftEyeTarget.localRotation = _defaultLeftEyeRotation * gameObject.transform.localRotation;
		_rightEyeTarget.localRotation = _defaultRightEyeRotation * gameObject.transform.localRotation;
	}
}

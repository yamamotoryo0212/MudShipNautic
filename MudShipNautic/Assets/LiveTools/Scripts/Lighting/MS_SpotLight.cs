using UnityEngine;

public class MS_SpotLight : MonoBehaviour
{
	public Transform _target = null;

	private void LateUpdate()
	{
		gameObject.transform.LookAt(_target);
	}
}

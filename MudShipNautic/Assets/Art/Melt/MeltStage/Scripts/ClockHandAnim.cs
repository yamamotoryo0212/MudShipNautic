using UnityEngine;

public class ClockHandAnim : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 10f;

	private void LateUpdate()
	{
		transform.Rotate(0, rotationSpeed * Time.deltaTime,0);
	}
}

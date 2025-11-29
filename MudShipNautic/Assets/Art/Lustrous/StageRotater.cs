using UnityEngine;

public class StageRotater : MonoBehaviour
{

    [SerializeField]
    private float rotationSpeed = 15f;
	void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
	}
}

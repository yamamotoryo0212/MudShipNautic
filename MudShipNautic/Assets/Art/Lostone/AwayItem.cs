using UnityEngine;

public class AwayItem : MonoBehaviour
{
	[SerializeField]
	private Rigidbody rb;
	[SerializeField]
	private Vector3 forceDirection;

	private void Start()
	{
		rb.gameObject.SetActive(false);
		gameObject.SetActive(true);
	}
	public void Away()
	{
		Debug.Log("AwayItem Away");
		rb.gameObject.SetActive(true);
		rb.AddForce(forceDirection, ForceMode.Force);
	}
}

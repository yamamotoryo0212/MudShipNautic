using UnityEngine;

public class CharacterRippleTrigger : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private RippleController rippleController;
	[SerializeField] private Transform leftFoot;
	[SerializeField] private Transform rightFoot;

	[Header("Settings")]
	[SerializeField] private float triggerInterval = 0.3f;
	[SerializeField] private float velocityThreshold = 0.1f;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private float raycastDistance = 1f;

	private float lastTriggerTime;
	private Vector3 lastPosition;
	private bool leftFootDown = false;
	private bool rightFootDown = false;

	void Start()
	{
		if (rippleController == null)
		{
			rippleController = FindObjectOfType<RippleController>();
		}

		lastPosition = transform.position;
	}

	void Update()
	{
		CheckMovement();

		// 足の位置を使う場合
		if (leftFoot != null && rightFoot != null)
		{
			CheckFootstep(leftFoot, ref leftFootDown);
			CheckFootstep(rightFoot, ref rightFootDown);
		}
	}

	private void CheckMovement()
	{
		// キャラクターの移動速度をチェック
		Vector3 currentPosition = transform.position;
		float velocity = (currentPosition - lastPosition).magnitude / Time.deltaTime;

		// 移動中かつ十分な時間が経過している場合
		if (velocity > velocityThreshold &&
			Time.time - lastTriggerTime > triggerInterval)
		{
			TriggerRippleAtPosition(currentPosition);
			lastTriggerTime = Time.time;
		}

		lastPosition = currentPosition;
	}

	private void CheckFootstep(Transform foot, ref bool wasDown)
	{
		RaycastHit hit;
		bool isDown = Physics.Raycast(foot.position, Vector3.down, out hit, raycastDistance, groundLayer);

		// 足が地面に着地した瞬間
		if (isDown && !wasDown)
		{
			TriggerRippleAtPosition(hit.point);
		}

		wasDown = isDown;
	}

	private void TriggerRippleAtPosition(Vector3 position)
	{
		if (rippleController != null)
		{
			rippleController.TriggerRipple(position);
		}
	}

	// デバッグ用
	void OnDrawGizmos()
	{
		if (leftFoot != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(leftFoot.position, leftFoot.position + Vector3.down * raycastDistance);
		}

		if (rightFoot != null)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(rightFoot.position, rightFoot.position + Vector3.down * raycastDistance);
		}
	}
}
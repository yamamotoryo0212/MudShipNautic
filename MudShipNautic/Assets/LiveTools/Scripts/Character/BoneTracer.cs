using UnityEngine;

public class BoneTracer : MonoBehaviour
{
	[SerializeField]
	private Transform _traceTargetBone;
	[SerializeField]
	private Transform _traceSourceBone;

	private void Update()
	{
		_traceTargetBone.rotation = _traceSourceBone.rotation;
	}
}

using UnityEngine;

public class CameraFollow : MonoBehaviour {

	[SerializeField]
	private Transform target;
	[SerializeField]
	private float lerpFactor = .5f;
	[SerializeField]
	private bool followLocalPosition = false;
	private Vector3 offset;
	private Vector3 targetLocalFollowPos;
	private bool freezeX = false;
	private bool freezeY = false;
	private bool freezeZ = false;

	private void Start() {
		offset = transform.position - target.transform.position;

		if (followLocalPosition)
			targetLocalFollowPos = target.transform.InverseTransformPoint(transform.position);
	}

	private void FixedUpdate() {
		Vector3 targetPosition = followLocalPosition ? target.transform.TransformPoint(targetLocalFollowPos) : target.transform.position + offset;
		float modifier = Vector3.Magnitude(targetPosition - transform.position);
		targetPosition = new Vector3(freezeX ? transform.position.x : targetPosition.x,
			freezeY ? transform.position.y : targetPosition.y,
			freezeZ ? transform.position.z : targetPosition.z);
		transform.position = Vector3.Lerp(transform.position, targetPosition, lerpFactor * modifier);
	}
}

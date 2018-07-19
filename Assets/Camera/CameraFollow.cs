using System.Collections;
using System.Collections.Generic;
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

	private void Start() {
		offset = transform.position - target.transform.position;

		if (followLocalPosition)
			targetLocalFollowPos = target.transform.InverseTransformPoint(transform.position);
	}

	private void FixedUpdate() {
		Vector3 targetPosition = followLocalPosition ? target.transform.TransformPoint(targetLocalFollowPos): target.transform.position + offset;
		float modifier = Vector3.Magnitude(targetPosition - transform.position);
		transform.position = Vector3.Lerp(transform.position, targetPosition, lerpFactor * modifier);
	}
}

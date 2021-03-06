﻿using UnityEngine;

public class SimplePlane3D : MonoBehaviour, ITurnable {

	[SerializeField]
	private float thurst = 10;
	[SerializeField]
	private float torque = 10;
	[SerializeField]
	private float topSpeed = 200;
	[SerializeField]
	private Rigidbody rbody;
	[SerializeField]
	private float turnFactor = 1;
	[SerializeField]
	private VectorMinMax rotationConstraints;
	private Vector2 turnVector;
	private Vector3 startingVector;

	public void Turn(Vector2 turnVector) {
		this.turnVector = turnVector * turnFactor;
	}

	private void Awake() {
		rbody = GetComponent<Rigidbody>();
		startingVector = transform.forward;
	}

	private void FixedUpdate() {
		if (rbody != null) {
			rbody.AddForce(rbody.transform.forward * thurst);
			float angleX = Vector3.SignedAngle(transform.forward, startingVector, Vector3.up);
			rbody.AddRelativeTorque(Vector3.up * torque * turnVector.x);
			rbody.AddRelativeTorque(Vector3.right * torque * turnVector.y);
		}

		if (rbody.velocity.magnitude > topSpeed)
			rbody.velocity = rbody.velocity.normalized * topSpeed;

		turnVector = Vector2.zero;
	}
}

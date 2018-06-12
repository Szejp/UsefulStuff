using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirPlane : Movement {

	private const float _gravity = 9.86f;
	private const float _airDensity = 1.225f;

	[SerializeField]
	private float _coefficent = 1;
	[SerializeField]
	private float _wingArea = 10;
	[SerializeField]
	private float _speed;

	private Vector3 _lift;
	private Rigidbody _rb;
	private float _thrustAcceleration;

	protected override void Start() {
		base.Start();
		_rb = GetComponent<Rigidbody>();
		_rb.velocity += 50 * transform.forward;
	}

	private void Update() {
		_thrustAcceleration = 0;
#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.W))
			_thrustAcceleration += Mathf.Clamp(_speed, 0, 300);
#endif
	}

	private void FixedUpdate() {
		Vector3 vMatters = Vector3.Dot(_rb.velocity, transform.forward) * transform.forward / _rb.velocity.sqrMagnitude;
		_lift = _coefficent * _airDensity * _wingArea / 2 * Mathf.Pow(_rb.velocity.magnitude * Time.deltaTime + _thrustAcceleration * Time.deltaTime, 2) * transform.up;
		Debug.Log("lift: " + _lift + " thrust acceleration: " + _thrustAcceleration);
		_rb.AddForce(_lift + _thrustAcceleration * Time.deltaTime * transform.forward * _rb.mass);
	}
}

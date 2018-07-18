using UnityEngine;

public class SimplePlane3D : MonoBehaviour {

	[SerializeField]
	private float thurst = 10;
	[SerializeField]
	private float torque = 10;
	[SerializeField]
	private float topSpeed = 200;
	private Rigidbody rbody;
	
	private void Awake() {
		rbody = GetComponent<Rigidbody>();
	}

	private void FixedUpdate() {
		if (rbody != null) {
			float turn = Input.GetAxis("Horizontal");
			rbody.AddForce(rbody.transform.forward * thurst);
			rbody.AddRelativeTorque(Vector3.up * torque * turn);
			float turnVertical = Input.GetAxis("Vertical");
			rbody.AddRelativeTorque(Vector3.right * torque * turnVertical);
		}

		if (rbody.velocity.magnitude > topSpeed)
			rbody.velocity = rbody.velocity.normalized * topSpeed;
	}
}

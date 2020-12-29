using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCamera : MonoBehaviour
{
	Rigidbody rb;

	public float lookSensitivity = 50;
	public float speed = 300f;
	public float shiftSpeed = 600f;

	float rotationX = 0.0f;
	float rotationY = 0.0f;


	// Use this for initialization
	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;

		rb = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKey(KeyCode.Mouse1))
		{
			rotationX += Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
			rotationY += Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;
			rotationY = Mathf.Clamp(rotationY, -90, 90);
		}

		transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
		transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

		rb.velocity = Vector3.zero;

		foreach(var body in CelestiaBodiesManager.instance.celestialBodies)
		{
			float otherMass = (float)body.mass * (float)Simulation.earthMass;
			Vector3 thisPosition = this.transform.position;
			Vector3 direction = (Vector3)body.position - thisPosition;
			Vector3 forceDir = direction.normalized;

			double distance = Vector3.Distance(thisPosition, (Vector3)body.position);
			double sqrDistance = Math.Pow(distance, 2);

			Vector3d velocityCalc = Vector3d.zero;

			//force F = G*((m1m2)/r^2)
			//velocity v = F/mass
			Vector3 velocity = (float)(Simulation.G * (otherMass) / sqrDistance) * forceDir * Simulation.timeStep;
			transform.position += velocity * Time.deltaTime * Simulation.timeStep;
		}

		//if (Input.GetKey(KeyCode.LeftShift))
		//{
		//	rb.velocity += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * shiftSpeed;
		//	rb.velocity += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * shiftSpeed;
		//}
		//else
		//{
		//	rb.velocity += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * speed;
		//	rb.velocity += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * speed;
		//}
	}
}

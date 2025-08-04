using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(Rigidbody))]
public class RollABall : MonoBehaviour
{
	public Vector3 tilt;

	public float speed;

	private float circ;

	private Vector3 previousPosition;

	public RollABall()
	{
		tilt = Vector3.zero;
	}

	public void Start()
	{
		Vector3 extents = GetComponent<Collider>().bounds.extents;
		circ = (float)Math.PI * 2f * extents.x;
		previousPosition = transform.position;
	}

	public void Update()
	{
		ref Vector3 reference = ref tilt;
		Vector3 acceleration = Input.acceleration;
		reference.x = 0f - acceleration.y;
		ref Vector3 reference2 = ref tilt;
		Vector3 acceleration2 = Input.acceleration;
		reference2.z = acceleration2.x;
		GetComponent<Rigidbody>().AddForce(tilt * speed * Time.deltaTime);
	}

	public void LateUpdate()
	{
		Vector3 vector = transform.position - previousPosition;
		vector = new Vector3(vector.z, 0f, 0f - vector.x);
		transform.Rotate(vector / circ * 360f, Space.World);
		previousPosition = transform.position;
	}

	public void Main()
	{
	}
}

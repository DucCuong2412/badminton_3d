using System;
using UnityEngine;

[Serializable]
public class SmoothFollow2D : MonoBehaviour
{
	public Transform target;

	public float smoothTime;

	private Transform thisTransform;

	private Vector2 velocity;

	public SmoothFollow2D()
	{
		smoothTime = 0.3f;
	}

	public void Start()
	{
		thisTransform = transform;
	}

	public void Update()
	{
		Vector3 position = thisTransform.position;
		float x = position.x;
		Vector3 position2 = target.position;
		float x2 = Mathf.SmoothDamp(x, position2.x, ref velocity.x, smoothTime);
		Vector3 position3 = thisTransform.position;
		position3.x = x2;
		Vector3 vector2 = thisTransform.position = position3;
		Vector3 position4 = thisTransform.position;
		float y = position4.y;
		Vector3 position5 = target.position;
		float y2 = Mathf.SmoothDamp(y, position5.y, ref velocity.y, smoothTime);
		Vector3 position6 = thisTransform.position;
		position6.y = y2;
		Vector3 vector4 = thisTransform.position = position6;
	}

	public void Main()
	{
	}
}

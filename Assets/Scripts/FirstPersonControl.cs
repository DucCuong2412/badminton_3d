using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(CharacterController))]
public class FirstPersonControl : MonoBehaviour
{
	public Joystick moveTouchPad;

	public Joystick rotateTouchPad;

	public Transform cameraPivot;

	public float forwardSpeed;

	public float backwardSpeed;

	public float sidestepSpeed;

	public float jumpSpeed;

	public float inAirMultiplier;

	public Vector2 rotationSpeed;

	public float tiltPositiveYAxis;

	public float tiltNegativeYAxis;

	public float tiltXAxisMinimum;

	private Transform thisTransform;

	private CharacterController character;

	private Vector3 cameraVelocity;

	private Vector3 velocity;

	private bool canJump;

	public FirstPersonControl()
	{
		forwardSpeed = 4f;
		backwardSpeed = 1f;
		sidestepSpeed = 1f;
		jumpSpeed = 8f;
		inAirMultiplier = 0.25f;
		rotationSpeed = new Vector2(50f, 25f);
		tiltPositiveYAxis = 0.6f;
		tiltNegativeYAxis = 0.4f;
		tiltXAxisMinimum = 0.1f;
		canJump = true;
	}

	public void Start()
	{
		thisTransform = (Transform)GetComponent(typeof(Transform));
		character = (CharacterController)GetComponent(typeof(CharacterController));
		GameObject gameObject = GameObject.Find("PlayerSpawn");
		if ((bool)gameObject)
		{
			thisTransform.position = gameObject.transform.position;
		}
	}

	public void OnEndGame()
	{
		moveTouchPad.Disable();
		if ((bool)rotateTouchPad)
		{
			rotateTouchPad.Disable();
		}
		enabled = false;
	}

	public void Update()
	{
		Vector3 vector = thisTransform.TransformDirection(new Vector3(moveTouchPad.position.x, 0f, moveTouchPad.position.y));
		vector.y = 0f;
		vector.Normalize();
		Vector2 vector2 = new Vector2(Mathf.Abs(moveTouchPad.position.x), Mathf.Abs(moveTouchPad.position.y));
		vector = ((vector2.y <= vector2.x) ? (vector * (sidestepSpeed * vector2.x)) : ((moveTouchPad.position.y <= 0f) ? (vector * (backwardSpeed * vector2.y)) : (vector * (forwardSpeed * vector2.y))));
		if (character.isGrounded)
		{
			bool flag = false;
			Joystick joystick = null;
			joystick = ((!rotateTouchPad) ? moveTouchPad : rotateTouchPad);
			if (!joystick.IsFingerDown())
			{
				canJump = true;
			}
			if (canJump && joystick.tapCount >= 2)
			{
				flag = true;
				canJump = false;
			}
			if (flag)
			{
				velocity = character.velocity;
				velocity.y = jumpSpeed;
			}
		}
		else
		{
			ref Vector3 reference = ref velocity;
			float y = velocity.y;
			Vector3 gravity = Physics.gravity;
			reference.y = y + gravity.y * Time.deltaTime;
			vector.x *= inAirMultiplier;
			vector.z *= inAirMultiplier;
		}
		vector += velocity;
		vector += Physics.gravity;
		vector *= Time.deltaTime;
		character.Move(vector);
		if (character.isGrounded)
		{
			velocity = Vector3.zero;
		}
		if (!character.isGrounded)
		{
			return;
		}
		Vector2 a = Vector2.zero;
		if ((bool)rotateTouchPad)
		{
			a = rotateTouchPad.position;
		}
		else
		{
			Vector3 acceleration = Input.acceleration;
			float num = Mathf.Abs(acceleration.x);
			if (!(acceleration.z >= 0f) && !(acceleration.x >= 0f))
			{
				if (!(num < tiltPositiveYAxis))
				{
					a.y = (num - tiltPositiveYAxis) / (1f - tiltPositiveYAxis);
				}
				else if (!(num > tiltNegativeYAxis))
				{
					a.y = (0f - (tiltNegativeYAxis - num)) / tiltNegativeYAxis;
				}
			}
			if (!(Mathf.Abs(acceleration.y) < tiltXAxisMinimum))
			{
				a.x = (0f - (acceleration.y - tiltXAxisMinimum)) / (1f - tiltXAxisMinimum);
			}
		}
		a.x *= rotationSpeed.x;
		a.y *= rotationSpeed.y;
		a *= Time.deltaTime;
		thisTransform.Rotate(0f, a.x, 0f, Space.World);
		cameraPivot.Rotate(0f - a.y, 0f, 0f);
	}

	public void Main()
	{
	}
}

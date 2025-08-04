using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(CharacterController))]
public class CameraRelativeControl : MonoBehaviour
{
	public Joystick moveJoystick;

	public Joystick rotateJoystick;

	public Transform cameraPivot;

	public Transform cameraTransform;

	public float speed;

	public float jumpSpeed;

	public float inAirMultiplier;

	public Vector2 rotationSpeed;

	private Transform thisTransform;

	private CharacterController character;

	private Vector3 velocity;

	private bool canJump;

	public CameraRelativeControl()
	{
		speed = 5f;
		jumpSpeed = 8f;
		inAirMultiplier = 0.25f;
		rotationSpeed = new Vector2(50f, 25f);
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

	public void FaceMovementDirection()
	{
		Vector3 vector = character.velocity;
		vector.y = 0f;
		if (!(vector.magnitude <= 0.1f))
		{
			thisTransform.forward = vector.normalized;
		}
	}

	public void OnEndGame()
	{
		moveJoystick.Disable();
		rotateJoystick.Disable();
		enabled = false;
	}

	public void Update()
	{
		Vector3 vector = cameraTransform.TransformDirection(new Vector3(moveJoystick.position.x, 0f, moveJoystick.position.y));
		vector.y = 0f;
		vector.Normalize();
		Vector2 vector2 = new Vector2(Mathf.Abs(moveJoystick.position.x), Mathf.Abs(moveJoystick.position.y));
		vector *= speed * ((vector2.x <= vector2.y) ? vector2.y : vector2.x);
		if (character.isGrounded)
		{
			if (!rotateJoystick.IsFingerDown())
			{
				canJump = true;
			}
			if (canJump && rotateJoystick.tapCount == 2)
			{
				velocity = character.velocity;
				velocity.y = jumpSpeed;
				canJump = false;
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
		FaceMovementDirection();
		Vector2 a = rotateJoystick.position;
		a.x *= rotationSpeed.x;
		a.y *= rotationSpeed.y;
		a *= Time.deltaTime;
		cameraPivot.Rotate(0f, a.x, 0f, Space.World);
		cameraPivot.Rotate(a.y, 0f, 0f);
	}

	public void Main()
	{
	}
}

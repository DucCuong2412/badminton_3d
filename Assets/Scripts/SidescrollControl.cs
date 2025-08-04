using System;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(CharacterController))]
public class SidescrollControl : MonoBehaviour
{
	public Joystick moveTouchPad;

	public Joystick jumpTouchPad;

	public float forwardSpeed;

	public float backwardSpeed;

	public float jumpSpeed;

	public float inAirMultiplier;

	private Transform thisTransform;

	private CharacterController character;

	private Vector3 velocity;

	private bool canJump;

	public SidescrollControl()
	{
		forwardSpeed = 4f;
		backwardSpeed = 4f;
		jumpSpeed = 16f;
		inAirMultiplier = 0.25f;
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
		jumpTouchPad.Disable();
		enabled = false;
	}

	public void Update()
	{
		Vector3 vector = Vector3.zero;
		vector = ((moveTouchPad.position.x <= 0f) ? (Vector3.right * backwardSpeed * moveTouchPad.position.x) : (Vector3.right * forwardSpeed * moveTouchPad.position.x));
		if (character.isGrounded)
		{
			bool flag = false;
			Joystick joystick = jumpTouchPad;
			if (!joystick.IsFingerDown())
			{
				canJump = true;
			}
			if (canJump && joystick.IsFingerDown())
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
		}
		vector += velocity;
		vector += Physics.gravity;
		vector *= Time.deltaTime;
		character.Move(vector);
		if (character.isGrounded)
		{
			velocity = Vector3.zero;
		}
	}

	public void Main()
	{
	}
}

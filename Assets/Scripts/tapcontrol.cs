using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class tapcontrol : MonoBehaviour
{
	public GameObject cameraObject;

	public Transform cameraPivot;

	public Image jumpButton;

	public float speed;

	public float jumpSpeed;

	public float inAirMultiplier;

	public float minimumDistanceToMove;

	public float minimumTimeUntilMove;

	public bool zoomEnabled;

	public float zoomEpsilon;

	public float zoomRate;

	public bool rotateEnabled;

	public float rotateEpsilon;

	private ZoomCamera zoomCamera;

	private Camera cam;

	private Transform thisTransform;

	private CharacterController character;

	private Vector3 targetLocation;

	private bool moving;

	private float rotationTarget;

	private float rotationVelocity;

	private Vector3 velocity;

	private ControlState state;

	private int[] fingerDown;

	private Vector2[] fingerDownPosition;

	private int[] fingerDownFrame;

	private float firstTouchTime;

	public tapcontrol()
	{
		inAirMultiplier = 0.25f;
		minimumDistanceToMove = 1f;
		minimumTimeUntilMove = 0.25f;
		rotateEpsilon = 1f;
		state = ControlState.WaitingForFirstTouch;
		fingerDown = new int[2];
		fingerDownPosition = new Vector2[2];
		fingerDownFrame = new int[2];
	}

	public void Start()
	{
		thisTransform = transform;
		zoomCamera = (ZoomCamera)cameraObject.GetComponent(typeof(ZoomCamera));
		cam = cameraObject.GetComponent<Camera>();
		character = (CharacterController)GetComponent(typeof(CharacterController));
		ResetControlState();
		GameObject gameObject = GameObject.Find("PlayerSpawn");
		if ((bool)gameObject)
		{
			thisTransform.position = gameObject.transform.position;
		}
	}

	public void OnEndGame()
	{
		enabled = false;
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

	public void CameraControl(Touch touch0, Touch touch1)
	{
		if (rotateEnabled && state == ControlState.RotatingCamera)
		{
			Vector2 a = touch1.position - touch0.position;
			Vector2 lhs = a / a.magnitude;
			Vector2 a2 = touch1.position - touch1.deltaPosition - (touch0.position - touch0.deltaPosition);
			Vector2 rhs = a2 / a2.magnitude;
			float num = Vector2.Dot(lhs, rhs);
			if (!(num >= 1f))
			{
				Vector3 lhs2 = new Vector3(a.x, a.y);
				Vector3 rhs2 = new Vector3(a2.x, a2.y);
				Vector3 normalized = Vector3.Cross(lhs2, rhs2).normalized;
				float z = normalized.z;
				float num2 = Mathf.Acos(num);
				rotationTarget += num2 * 57.29578f * z;
				if (!(rotationTarget >= 0f))
				{
					rotationTarget += 360f;
				}
				else if (!(rotationTarget < 360f))
				{
					rotationTarget -= 360f;
				}
			}
		}
		else if (zoomEnabled && state == ControlState.ZoomingCamera)
		{
			float magnitude = (touch1.position - touch0.position).magnitude;
			float magnitude2 = (touch1.position - touch1.deltaPosition - (touch0.position - touch0.deltaPosition)).magnitude;
			float num3 = magnitude - magnitude2;
			zoomCamera.zoom += num3 * zoomRate * Time.deltaTime;
		}
	}

	public void CharacterControl()
	{
		int touchCount = UnityEngine.Input.touchCount;
		if (touchCount == 1 && state == ControlState.MovingCharacter)
		{
			Touch touch = UnityEngine.Input.GetTouch(0);
			if (character.isGrounded/* && jumpButton.HitTest(touch.position)*/)
			{
				velocity = character.velocity;
				velocity.y = jumpSpeed;
			}
			else if (/*!jumpButton.HitTest(touch.position) &&*/ touch.phase != 0)
			{
				Camera camera = cam;
				Vector2 position = touch.position;
				float x = position.x;
				Vector2 position2 = touch.position;
				Ray ray = camera.ScreenPointToRay(new Vector3(x, position2.y));
				RaycastHit hitInfo = default(RaycastHit);
				if (Physics.Raycast(ray, out hitInfo))
				{
					float magnitude = (transform.position - hitInfo.point).magnitude;
					if (!(magnitude <= minimumDistanceToMove))
					{
						targetLocation = hitInfo.point;
					}
					moving = true;
				}
			}
		}
		Vector3 vector = Vector3.zero;
		if (moving)
		{
			vector = targetLocation - thisTransform.position;
			vector.y = 0f;
			float magnitude2 = vector.magnitude;
			if (!(magnitude2 >= 1f))
			{
				moving = false;
			}
			else
			{
				vector = vector.normalized * speed;
			}
		}
		if (!character.isGrounded)
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
	}

	public void ResetControlState()
	{
		state = ControlState.WaitingForFirstTouch;
		fingerDown[0] = -1;
		fingerDown[1] = -1;
	}

	public void Update()
	{
		int touchCount = UnityEngine.Input.touchCount;
		if (touchCount == 0)
		{
			ResetControlState();
		}
		else
		{
			int num = default(int);
			Touch touch = default(Touch);
			Touch[] touches = Input.touches;
			Touch touch2 = default(Touch);
			Touch touch3 = default(Touch);
			bool flag = false;
			bool flag2 = false;
			if (state == ControlState.WaitingForFirstTouch)
			{
				for (num = 0; num < touchCount; num++)
				{
					touch = touches[num];
					if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
					{
						state = ControlState.WaitingForSecondTouch;
						firstTouchTime = Time.time;
						fingerDown[0] = touch.fingerId;
						fingerDownPosition[0] = touch.position;
						fingerDownFrame[0] = Time.frameCount;
						break;
					}
				}
			}
			if (state == ControlState.WaitingForSecondTouch)
			{
				for (num = 0; num < touchCount; num++)
				{
					touch = touches[num];
					if (touch.phase == TouchPhase.Canceled)
					{
						continue;
					}
					if (touchCount >= 2 && touch.fingerId != fingerDown[0])
					{
						state = ControlState.WaitingForMovement;
						fingerDown[1] = touch.fingerId;
						fingerDownPosition[1] = touch.position;
						fingerDownFrame[1] = Time.frameCount;
						break;
					}
					if (touchCount == 1)
					{
						Vector2 vector = touch.position - fingerDownPosition[0];
						if (touch.fingerId == fingerDown[0] && (Time.time > firstTouchTime + minimumTimeUntilMove || touch.phase == TouchPhase.Ended))
						{
							state = ControlState.MovingCharacter;
							break;
						}
					}
					continue;
					IL_015b:;
				}
			}
			if (state == ControlState.WaitingForMovement)
			{
				for (num = 0; num < touchCount; num++)
				{
					touch = touches[num];
					if (touch.phase == TouchPhase.Began)
					{
						if (touch.fingerId == fingerDown[0] && fingerDownFrame[0] == Time.frameCount)
						{
							touch2 = touch;
							flag = true;
						}
						else if (touch.fingerId != fingerDown[0] && touch.fingerId != fingerDown[1])
						{
							fingerDown[1] = touch.fingerId;
							touch3 = touch;
							flag2 = true;
						}
					}
					if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Ended)
					{
						if (touch.fingerId == fingerDown[0])
						{
							touch2 = touch;
							flag = true;
						}
						else if (touch.fingerId == fingerDown[1])
						{
							touch3 = touch;
							flag2 = true;
						}
					}
				}
				if (flag)
				{
					if (flag2)
					{
						Vector2 a = fingerDownPosition[1] - fingerDownPosition[0];
						Vector2 a2 = touch3.position - touch2.position;
						Vector2 lhs = a / a.magnitude;
						Vector2 rhs = a2 / a2.magnitude;
						float num2 = Vector2.Dot(lhs, rhs);
						if (!(num2 >= 1f))
						{
							float num3 = Mathf.Acos(num2);
							if (!(num3 <= rotateEpsilon * ((float)Math.PI / 180f)))
							{
								state = ControlState.RotatingCamera;
							}
						}
						if (state == ControlState.WaitingForMovement)
						{
							float f = a.magnitude - a2.magnitude;
							if (!(Mathf.Abs(f) <= zoomEpsilon))
							{
								state = ControlState.ZoomingCamera;
							}
						}
					}
				}
				else
				{
					state = ControlState.WaitingForNoFingers;
				}
			}
			if (state == ControlState.RotatingCamera || state == ControlState.ZoomingCamera)
			{
				for (num = 0; num < touchCount; num++)
				{
					touch = touches[num];
					if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Ended)
					{
						if (touch.fingerId == fingerDown[0])
						{
							touch2 = touch;
							flag = true;
						}
						else if (touch.fingerId == fingerDown[1])
						{
							touch3 = touch;
							flag2 = true;
						}
					}
				}
				if (flag)
				{
					if (flag2)
					{
						CameraControl(touch2, touch3);
					}
				}
				else
				{
					state = ControlState.WaitingForNoFingers;
				}
			}
		}
		CharacterControl();
	}

	public void LateUpdate()
	{
		Vector3 eulerAngles = cameraPivot.eulerAngles;
		float y = Mathf.SmoothDampAngle(eulerAngles.y, rotationTarget, ref rotationVelocity, 0.3f);
		Vector3 eulerAngles2 = cameraPivot.eulerAngles;
		eulerAngles2.y = y;
		Vector3 vector2 = cameraPivot.eulerAngles = eulerAngles2;
	}

	public void Main()
	{
	}
}

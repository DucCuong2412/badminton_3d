using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
	protected Animator myAnimator;

	protected Transform myTransform;

	public static int LeftState = Animator.StringToHash("Base Layer.Left");

	public static int RightState = Animator.StringToHash("Base Layer.Right");

	public static int LeftJumpState = Animator.StringToHash("Base Layer.LeftJump");

	public static int RightJumpState = Animator.StringToHash("Base Layer.RightJump");

	public static int BackhandState = Animator.StringToHash("Base Layer.Backhand");

	public float speed = 10f;

	private Plane floorPlane;

	private Camera mainCamera;

	private const string LeftParamName = "Left";

	private const string LeftJumpParamName = "LeftJump";

	private const string RightParamName = "Right";

	private const string RightJumpParamName = "RightJump";

	private const string BackhandParamName = "Backhand";

	private const string ForehandParamName = "Forehand";

	private Vector3 desiredPosition;

	private Vector3 startPosition;

	private bool move;

	private float moveTime;

	private float moveDuration;

	private Vector3 matchTargetPos;

	private void Awake()
	{
		myTransform = base.transform;
		myAnimator = GetComponent<Animator>();
		floorPlane = new Plane(Vector3.up, Vector3.zero);
		mainCamera = Camera.main;
		desiredPosition = myTransform.position;
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
		{
			myAnimator.SetTrigger("Backhand");
		}
		else if (UnityEngine.Input.GetKeyDown(KeyCode.A))
		{
			myAnimator.SetTrigger("Forehand");
		}
		UpdateMove();
	}

	private void UpdateJump()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
			float enter = 0f;
			if (floorPlane.Raycast(ray, out enter))
			{
				Vector3 point = ray.GetPoint(enter);
				Vector3 position = myTransform.position;
				position.x = point.x;
				position.z = point.z;
				desiredPosition = position;
				UnityEngine.Debug.Log("Pos on floor " + position + " myPosition " + myTransform.position);
				float num = Vector3.Dot(desiredPosition, myTransform.right);
				float num2 = num;
				Vector3 position2 = myTransform.position;
				if (num2 > position2.x)
				{
					myAnimator.SetTrigger("RightJump");
				}
				else
				{
					myAnimator.SetTrigger("LeftJump");
				}
				matchTargetPos = position;
			}
		}
		AnimatorStateInfo currentAnimatorStateInfo = myAnimator.GetCurrentAnimatorStateInfo(0);
		if ((currentAnimatorStateInfo.nameHash == LeftJumpState || currentAnimatorStateInfo.nameHash == RightJumpState) && !myAnimator.IsInTransition(0) && !myAnimator.isMatchingTarget)
		{
			myAnimator.MatchTarget(matchTargetPos, myTransform.rotation, AvatarTarget.Root, new MatchTargetWeightMask(Vector3.one, 1f), 0f, 0.9f);
		}
	}

	private void UpdateMove()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
			float enter = 0f;
			if (floorPlane.Raycast(ray, out enter))
			{
				Vector3 point = ray.GetPoint(enter);
				Vector3 position = myTransform.position;
				position.x = point.x;
				position.z = point.z;
				desiredPosition = position;
				float num = Vector3.Dot(desiredPosition, myTransform.right);
				if (num > 0f)
				{
					myAnimator.SetBool("Right", value: true);
				}
				else
				{
					myAnimator.SetBool("Left", value: true);
				}
				move = true;
				startPosition = myTransform.position;
				float num2 = Vector3.Distance(desiredPosition, myTransform.position);
				moveDuration = num2 / speed;
				moveTime = 0f;
			}
		}
		AnimatorStateInfo currentAnimatorStateInfo = myAnimator.GetCurrentAnimatorStateInfo(0);
		ref Vector3 reference = ref desiredPosition;
		Vector3 position2 = myTransform.position;
		reference.y = position2.y;
		if (move)
		{
			moveTime += Time.deltaTime;
			float num3 = moveTime / moveDuration;
			myTransform.position = Vector3.Lerp(startPosition, desiredPosition, num3);
			float num4 = Vector3.Dot(desiredPosition, myTransform.right);
			if (num3 >= 1f)
			{
				move = false;
			}
		}
		else
		{
			myAnimator.SetBool("Left", value: false);
			myAnimator.SetBool("Right", value: false);
		}
	}
}

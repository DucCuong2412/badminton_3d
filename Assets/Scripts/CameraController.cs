using UnityEngine;

public class CameraController : MonoBehaviour
{
	private Vector3 startPos;

	private Vector3 endPos;

	private float moveTime;

	private bool shouldMove;

	private float moveTimeMult;

	private float speed;

	public float rotationSpeed;

	public Vector3 offsetFromPlayer;

	public Vector3 offsetFromServe;

	protected Vector3 lookAtPoint;

	protected bool lookAtPointSet;

	public Quaternion defaultRotation = Quaternion.AngleAxis(20f, Vector3.right);

	protected Quaternion startRotation;

	public float minDistanceBeforeMove = 0.01f;

	protected Transform trackedTransform;

	protected Vector3 trackedTransformWeights;

	protected Vector3Range tracketTransformRange;

	public Vector3 up = Vector3.up;

	protected Vector3 rotateAround;

	protected float startAngle;

	protected float endAngle;

	protected float rotateDuration;

	protected float rotateDistance;

	protected bool rotate;

	protected Vector3 rotateLookPoint;

	protected int rotateDirection = 1;

	protected float rotateTime;

	protected float endFov;

	protected float startFov;

	protected Vector3 lookAtOffset;

	private Transform cachedTransform_;

	public Vector3 idlePosition
	{
		get;
		private set;
	}

	public float idleFov
	{
		get;
		protected set;
	}

	private Transform myTransform
	{
		get
		{
			if (cachedTransform_ == null)
			{
				cachedTransform_ = base.transform;
			}
			return cachedTransform_;
		}
	}

	public bool isLookAtSet => trackedTransform != null || (shouldMove && lookAtPointSet);

	public void RotateAround(Vector3 rotatePoint, Vector3 lookPoint, float radius, float startAngle, float endAngle, float duration)
	{
		rotateAround = rotatePoint;
		rotateDistance = radius;
		rotateLookPoint = lookPoint;
		this.startAngle = startAngle;
		this.endAngle = endAngle;
		rotateDuration = duration;
		rotate = true;
		rotateTime = 0f;
		rotateDirection = 1;
	}

	public void MoveTo(Vector3 destination, Vector3 lookAtPoint, float speed)
	{
		MoveTo(destination, lookAtPoint, speed, idleFov);
	}

	public void MoveTo(Vector3 destination, Vector3 lookAtPoint, float speed, float fov)
	{
		MoveTo(destination, speed);
		this.lookAtPoint = lookAtPoint;
		startFov = GetComponent<Camera>().fieldOfView;
		endFov = fov;
		GetComponent<Camera>().fieldOfView = fov;
		lookAtPointSet = true;
	}

	public void RefreshUp()
	{
		Ray ray = GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
		myTransform.rotation = Quaternion.LookRotation(ray.GetPoint(10f) - myTransform.position, up);
	}

	public void TrackTransform(Transform transform, Vector3 weights, Vector3Range range, Vector3 offset)
	{
		trackedTransform = transform;
		trackedTransformWeights = weights;
		tracketTransformRange = range;
		lookAtOffset = offset;
		rotate = false;
	}

	public void MoveTo(Vector3 destination, float speed)
	{
		lookAtPointSet = false;
		startRotation = myTransform.rotation;
		MoveTo(destination, speed, minDistanceBeforeMove);
		trackedTransform = null;
	}

	protected void MoveTo(Vector3 destination, float speed, float minDistanceBeforeMove)
	{
		rotate = false;
		shouldMove = true;
		moveTime = 0f;
		this.speed = speed;
		rotationSpeed = speed;
		endPos = destination;
		startPos = myTransform.position;
		float num = Vector3.Distance(startPos, endPos);
		if (num <= minDistanceBeforeMove)
		{
			moveTimeMult = speed;
		}
		else
		{
			moveTimeMult = speed / num;
		}
	}

	private void Awake()
	{
		SetIdle();
	}

	public void SetIdle()
	{
		idlePosition = base.transform.position;
		idleFov = GetComponent<Camera>().fieldOfView;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (rotate)
		{
			rotateTime += (float)rotateDirection * Time.deltaTime / rotateDuration;
			rotateTime = Mathf.Clamp01(rotateTime);
			if (rotateTime >= 1f)
			{
				rotateDirection = -1;
			}
			else if (rotateTime <= 0f)
			{
				rotateDirection = 1;
			}
			float f = Mathf.Lerp(startAngle, endAngle, MathEx.Hermite(rotateTime));
			myTransform.position = rotateAround + new Vector3(rotateDistance * Mathf.Cos(f), 0f, rotateDistance * Mathf.Sin(f));
			myTransform.LookAt(rotateLookPoint);
			return;
		}
		if (shouldMove)
		{
			moveTime += Time.deltaTime * moveTimeMult;
			if (moveTime >= 1f)
			{
			}
			float num = moveTime * moveTime * (3f - 2f * moveTime);
			float num2 = moveTime;
			myTransform.position = Vector3.Lerp(myTransform.position, endPos, Time.deltaTime * speed);
			Quaternion b = defaultRotation;
			if (lookAtPointSet)
			{
				b = Quaternion.LookRotation(lookAtPoint - myTransform.position, up);
			}
			if (trackedTransform == null)
			{
				myTransform.rotation = Quaternion.Lerp(myTransform.rotation, b, Time.deltaTime * rotationSpeed);
			}
		}
		if (trackedTransform != null)
		{
			Vector3 vector = trackedTransform.position;
			vector += lookAtOffset;
			vector.x *= trackedTransformWeights.x;
			vector.y *= trackedTransformWeights.y;
			vector.z *= trackedTransformWeights.z;
			vector = tracketTransformRange.Clamp(vector);
			myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.LookRotation(vector - myTransform.position, up), Time.deltaTime * rotationSpeed);
		}
	}
}

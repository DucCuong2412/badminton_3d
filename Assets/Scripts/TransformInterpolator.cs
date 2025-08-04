using UnityEngine;

public class TransformInterpolator
{
	public delegate void InterpolatorEvent(TransformInterpolator interpolator);

	public float deltaDistance;

	public Vector3 startPos
	{
		get;
		protected set;
	}

	public Vector3 endPos
	{
		get;
		protected set;
	}

	public bool moving
	{
		get;
		protected set;
	}

	public float time
	{
		get;
		protected set;
	}

	public float duration
	{
		get;
		protected set;
	}

	public Transform transform
	{
		get;
		set;
	}

	public int tag
	{
		get;
		set;
	}

	public event InterpolatorEvent onMoveComplete;

	public TransformInterpolator(Transform transform)
	{
		this.transform = transform;
	}

	public float DurationTo(Vector3 to, float speed)
	{
		if (speed == 0f)
		{
			return 0f;
		}
		return Vector3Ex.HorizontalDistance(transform.position, to) / speed;
	}

	public void MoveTo(Vector3 to, float duration, int tag = 0)
	{
		this.tag = tag;
		if (duration == 0f)
		{
			transform.position = to;
			StopMoving();
			return;
		}
		startPos = transform.position;
		endPos = to;
		this.duration = duration;
		time = 0f;
		moving = true;
	}

	public void Update()
	{
		if (!moving)
		{
			deltaDistance = 0f;
			return;
		}
		time += Time.deltaTime;
		float num = time / duration;
		float t = num;
		Vector3 position = transform.position;
		transform.position = Vector3.Lerp(startPos, endPos, t);
		deltaDistance = Vector3.Distance(position, transform.position);
		if (num >= 1f)
		{
			StopMoving();
		}
	}

	public void StopMoving()
	{
		moving = false;
		if (this.onMoveComplete != null)
		{
			this.onMoveComplete(this);
		}
	}
}

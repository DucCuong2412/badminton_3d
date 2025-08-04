using UnityEngine;

public class FlightInterpolator
{
	public delegate void OnJumpEvent();

	public Transform transform
	{
		get;
		set;
	}

	public bool isInJump
	{
		get;
		set;
	}

	public float g
	{
		get;
		protected set;
	}

	public float time
	{
		get;
		set;
	}

	public float apexTime
	{
		get;
		protected set;
	}

	public float totalTime
	{
		get;
		protected set;
	}

	public float initialSpeedY
	{
		get;
		protected set;
	}

	public float initialY
	{
		get;
		protected set;
	}

	public event OnJumpEvent onJumpUp;

	public event OnJumpEvent onJumpDown;

	public event OnJumpEvent onHitTheGround;

	public FlightInterpolator(Transform transform)
	{
		this.transform = transform;
		Vector3 gravity = Physics.gravity;
		g = gravity.y;
	}

	public float ApexTime(float h0, float apexY)
	{
		float num = 2f * (initialY - apexY) * g;
		if (num <= 0f)
		{
			return 0f;
		}
		initialSpeedY = Mathf.Sqrt(num);
		return (0f - initialSpeedY) / g;
	}

	public void Jump(float h0, float apexY, float duration, float hangTime)
	{
		initialY = h0;
		g = -2f * (apexY - h0) / (duration * duration);
		initialSpeedY = (0f - g) * duration;
		apexTime = duration;
		time = 0f;
		totalTime = 2f * apexTime;
		isInJump = true;
		if (this.onJumpUp != null)
		{
			this.onJumpUp();
		}
	}

	public void Update()
	{
		if (!isInJump)
		{
			return;
		}
		float time = this.time;
		this.time += Time.deltaTime;
		if (time <= apexTime && this.time > apexTime && this.onJumpDown != null)
		{
			this.onJumpDown();
		}
		float a = g * this.time * this.time * 0.5f + initialSpeedY * this.time + initialY;
		Vector3 position = transform.position;
		position.y = Mathf.Max(a, initialY);
		transform.position = position;
		if (this.time >= totalTime)
		{
			isInJump = false;
			if (this.onHitTheGround != null)
			{
				this.onHitTheGround();
			}
		}
	}
}

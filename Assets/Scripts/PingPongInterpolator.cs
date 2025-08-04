using System.Runtime.CompilerServices;
using UnityEngine;

public class PingPongInterpolator
{
	private int direction;

	private float max;

	private float min;

	private float speed;

	private float time;

	public bool isActive;

	public float value
	{
		[CompilerGenerated]
		get
		{
			return value;
		}
		[CompilerGenerated]
		protected set
		{
			this.value = value;
		}
	}

	public void Start(float startValue, float min, float max, float speed)
	{
		value = startValue;
		this.min = min;
		this.max = max;
		this.speed = speed;
		time = 0f;
		direction = 1;
		isActive = true;
	}

	public void Stop()
	{
		isActive = false;
	}

	public void Update()
	{
		if (isActive)
		{
			value += (float)direction * speed * RealTime.deltaTime;
			if (value >= max)
			{
				direction = -1;
			}
			else if (value <= min)
			{
				direction = 1;
			}
			value = Mathf.Clamp(value, min, max);
		}
	}
}

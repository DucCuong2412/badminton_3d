using UnityEngine;

public class HangFlightInterpolator
{
	protected float vertMoveStart;

	protected float vertMoveEnd;

	protected bool isInJump;

	protected float vertMoveTime;

	protected float vertMoveDuration;

	protected float vertMoveHangTime;

	public Transform transform
	{
		get;
		set;
	}

	public HangFlightInterpolator(Transform transform)
	{
		this.transform = transform;
	}

	public void Jump(float h0, float apexY, float duration, float hangTime)
	{
		vertMoveHangTime = hangTime;
		vertMoveStart = h0;
		vertMoveEnd = apexY;
		vertMoveDuration = duration;
		vertMoveTime = 0f;
		isInJump = true;
	}

	public void Update()
	{
		if (isInJump)
		{
			Vector3 position = transform.position;
			vertMoveTime += Time.deltaTime;
			if (vertMoveTime >= 2f * vertMoveDuration + vertMoveHangTime)
			{
				isInJump = false;
			}
			else if (vertMoveTime >= vertMoveDuration + vertMoveHangTime)
			{
				float num = Mathf.Clamp01((vertMoveTime - vertMoveDuration - vertMoveHangTime) / vertMoveDuration);
				float t = num * num * (3f - 2f * num);
				position.y = Mathf.Lerp(vertMoveEnd, vertMoveStart, t);
			}
			else
			{
				float num2 = Mathf.Clamp01(vertMoveTime / vertMoveDuration);
				float t2 = num2 * num2 * (3f - 2f * num2);
				position.y = Mathf.Lerp(vertMoveStart, vertMoveEnd, t2);
			}
			transform.position = position;
		}
	}
}

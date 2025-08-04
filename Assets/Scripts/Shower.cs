using UnityEngine;

public class Shower : MonoBehaviour
{
	public UIWidget follow;

	public UIPanel constrain;

	public float speed = 1f;

	protected UIWidget myWidget;

	protected float time;

	protected int direction = 1;

	private void Awake()
	{
		myWidget = GetComponent<UIWidget>();
	}

	private void Update()
	{
		time += Time.deltaTime * (float)direction * speed;
		if (time > 1f)
		{
			direction = -1;
			time = 1f;
		}
		else if (time < 0f)
		{
			direction = 1;
			time = 0f;
		}
		Vector3[] worldCorners = follow.worldCorners;
		Vector3 position = (worldCorners[1] + worldCorners[2]) * 0.5f;
		float num = Mathf.Abs(worldCorners[2].x - worldCorners[1].x) * 0.25f;
		myWidget.cachedTransform.position = position;
		position = myWidget.cachedTransform.position;
		constrain.ConstrainTargetToBounds(myWidget.cachedTransform, immediate: true);
		Vector3 position2 = myWidget.cachedTransform.position;
		if (position2 != position)
		{
			position2.x += (MathEx.Hermite(time) - 0.5f) * num;
			myWidget.cachedTransform.position = position2;
		}
		else
		{
			time = 0f;
		}
	}
}

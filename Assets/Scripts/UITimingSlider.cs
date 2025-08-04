using UnityEngine;

public class UITimingSlider : MonoBehaviour
{
	public UISprite thumb;

	protected UISprite bck;

	protected Transform mTransform;

	public bool vertical;

	private void Awake()
	{
		mTransform = base.transform;
		bck = mTransform.GetComponent<UISprite>();
	}

	public void SetPosition(float val)
	{
		if (!mTransform)
		{
			Awake();
		}
		if (vertical)
		{
			SetVertical(val);
		}
		else
		{
			SetHorizontal(val);
		}
	}

	private void SetHorizontal(float val)
	{
		val = Mathf.Clamp(val, -1f, 1f);
		Vector3[] worldCorners = bck.worldCorners;
		Vector3 position = thumb.cachedTransform.position;
		position.x = Mathf.Lerp(worldCorners[0].x, worldCorners[2].x, (val + 1f) / 2f);
		thumb.cachedTransform.position = position;
		thumb.cachedTransform.localScale = new Vector3(2f, 2f, 1f);
		TweenScale.Begin(thumb.cachedGameObject, 0.1f, Vector3.one);
	}

	private void SetVertical(float val)
	{
		val = Mathf.Clamp(val, -1f, 1f) * -1f;
		UISprite uISprite = bck;
		Vector3[] worldCorners = uISprite.worldCorners;
		Vector3 position = thumb.cachedTransform.position;
		position.y = Mathf.Lerp(worldCorners[0].y, worldCorners[1].y, (val + 1f) / 2f);
		thumb.cachedTransform.position = position;
		thumb.cachedTransform.localScale = new Vector3(2f, 2f, 1f);
		TweenScale.Begin(thumb.cachedGameObject, 0.1f, Vector3.one);
	}
}

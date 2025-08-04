using UnityEngine;

public class UIVertTimingSlider : MonoBehaviour
{
	public UISprite slider;

	public UISprite thumb;

	public void SetPosition(float val, bool animation = false)
	{
		val = Mathf.Clamp(val, -1f, 1f);
		Vector3[] worldCorners = slider.worldCorners;
		Vector3 position = thumb.cachedTransform.position;
		position.y = Mathf.Lerp(worldCorners[0].y, worldCorners[1].y, (val + 1f) / 2f);
		thumb.cachedTransform.position = position;
		if (animation)
		{
			thumb.cachedTransform.localScale = new Vector3(2f, 2f, 1f);
			TweenScale.Begin(thumb.cachedGameObject, 0.1f, Vector3.one);
		}
		else
		{
			thumb.cachedTransform.localScale = Vector3.one;
		}
	}
}

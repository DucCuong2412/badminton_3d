using UnityEngine;

public class UITools
{
	public static void ChangeSprite(UISprite sprite, string spriteName)
	{
		if (sprite != null && sprite.spriteName != spriteName)
		{
			sprite.spriteName = spriteName;
		}
	}

	public static void SetScreenPosition(UIWidget widget, Vector3 screenPos)
	{
		if (!(widget == null))
		{
			Camera mainCamera = UICamera.mainCamera;
			if (!(mainCamera == null))
			{
				widget.cachedTransform.position = mainCamera.ScreenToWorldPoint(screenPos);
			}
		}
	}

	public static void SetActive(UIWidget widget, bool active)
	{
		if (!(widget == null))
		{
			widget.cachedGameObject.SetActive(active);
		}
	}

	public static void ChangeText(UILabel label, string text)
	{
		if (label != null && label.text != text)
		{
			label.text = text;
		}
	}

	public static void CenterOnScroll(Transform target, UIScrollView mScrollView)
	{
		if (!(mScrollView == null) && !(target == null) && !(mScrollView.panel == null))
		{
			Vector3[] worldCorners = mScrollView.panel.worldCorners;
			Vector3 position = (worldCorners[2] + worldCorners[0]) * 0.5f;
			Transform cachedTransform = mScrollView.panel.cachedTransform;
			Vector3 a = cachedTransform.InverseTransformPoint(target.position);
			Vector3 b = cachedTransform.InverseTransformPoint(position);
			Vector3 b2 = a - b;
			if (!mScrollView.canMoveHorizontally)
			{
				b2.x = 0f;
			}
			if (!mScrollView.canMoveVertically)
			{
				b2.y = 0f;
			}
			b2.z = 0f;
			float strength = 8f;
			SpringPanel.Begin(mScrollView.panel.cachedGameObject, cachedTransform.localPosition - b2, strength);
		}
	}

	public static void CenterOnTransform(UIWidget widget, Transform tracked, Camera camera, Vector3 worldDisplace)
	{
		Transform cachedTransform = widget.cachedTransform;
		Vector3[] worldCorners = widget.worldCorners;
		float num = worldCorners[2].x - worldCorners[1].x;
		Vector3 vector = (widget.cachedTransform.position - worldCorners[2]) * 1.1f;
		Vector3 vector2 = (widget.cachedTransform.position - worldCorners[0]) * 1.1f;
		Camera mainCamera = UICamera.mainCamera;
		Vector3 position = tracked.position + worldDisplace;
		Vector3 position2 = camera.WorldToScreenPoint(position);
		Transform parent = cachedTransform.parent;
		Vector3[] localCorners = widget.localCorners;
		Vector3 vector3 = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));
		Vector3 b = vector2;
		if (position2.x > (float)Screen.width * 0.5f)
		{
			b.x = vector.x - num * 0.1f;
		}
		else
		{
			b.x += num * 0.1f;
		}
		Vector3 position3 = mainCamera.ScreenToWorldPoint(position2) + b;
		if (position3.y - vector.y > vector3.y)
		{
			position3.y += vector3.y - position3.y + vector.y;
		}
		Vector3 localPosition = parent.InverseTransformPoint(position3);
		Vector3 localPosition2 = cachedTransform.localPosition;
		localPosition.z = localPosition2.z;
		cachedTransform.localPosition = localPosition;
	}

	public static void AlignToLeftOnScroll(UIWidget target, UIScrollView mScrollView, float padding)
	{
		if (!(mScrollView == null) && !(target == null) && !(mScrollView.panel == null))
		{
			Vector3[] worldCorners = mScrollView.panel.worldCorners;
			Vector3 position = worldCorners[0];
			Transform cachedTransform = mScrollView.panel.cachedTransform;
			Vector3 position2 = target.worldCorners[0];
			Vector3 a = cachedTransform.InverseTransformPoint(position2);
			Vector3 b = cachedTransform.InverseTransformPoint(position);
			Vector3 b2 = a - b - new Vector3(padding, padding, 0f);
			if (!mScrollView.canMoveHorizontally)
			{
				b2.x = 0f;
			}
			if (!mScrollView.canMoveVertically)
			{
				b2.y = 0f;
			}
			b2.z = 0f;
			float strength = 8f;
			SpringPanel.Begin(mScrollView.panel.cachedGameObject, cachedTransform.localPosition - b2, strength);
		}
	}

	public static void AlignToTopOnScroll(UIWidget target, UIScrollView mScrollView, float padding)
	{
		if (!(mScrollView == null) && !(target == null) && !(mScrollView.panel == null))
		{
			Vector3[] worldCorners = mScrollView.panel.worldCorners;
			Vector3 position = worldCorners[1];
			Transform cachedTransform = mScrollView.panel.cachedTransform;
			Vector3 position2 = target.worldCorners[1];
			Vector3 a = cachedTransform.InverseTransformPoint(position2);
			Vector3 b = cachedTransform.InverseTransformPoint(position);
			Vector3 b2 = a - b - new Vector3(0f, padding, 0f);
			if (!mScrollView.canMoveHorizontally)
			{
				b2.x = 0f;
			}
			if (!mScrollView.canMoveVertically)
			{
				b2.y = 0f;
			}
			b2.z = 0f;
			float strength = 8f;
			SpringPanel.Begin(mScrollView.panel.cachedGameObject, cachedTransform.localPosition - b2, strength);
		}
	}
}

using System;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleToFitInPanel : MonoBehaviour
{
	public UIPanel panel;

	public UIWidget widget;

	public bool centerVertically;

	public float verticalSpace = 0.9f;

	public bool schrinkOnly;

	public float bottomPercent = 0.55f;

	protected Transform mTransform;

	protected bool started;

	private void OnScreenResize()
	{
		if (started)
		{
			Update();
		}
	}

	private void Awake()
	{
		mTransform = base.transform;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		NavigationManager.onLayerChanged += OnScreenResize;
		started = true;
	}

	private void OnDestroy()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(OnScreenResize));
		NavigationManager.onLayerChanged -= OnScreenResize;
	}

	public void Update()
	{
		if (!started)
		{
			Awake();
		}
		if (widget == null && panel == null)
		{
			panel = NGUITools.FindInParents<UIPanel>(mTransform);
		}
		Vector3[] worldCorners = panel.worldCorners;
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(mTransform);
		Vector3 max = bounds.max;
		float y = max.y;
		Vector3 min = bounds.min;
		float num = y - min.y;
		Vector3 localScale = mTransform.localScale;
		float num2 = num / Mathf.Max(localScale.y, 0.01f);
		if (num2 == 0f)
		{
			return;
		}
		float num3 = worldCorners[0].y - worldCorners[1].y;
		float num4 = verticalSpace * Mathf.Abs(num3 / num2);
		if (schrinkOnly)
		{
			num4 = Mathf.Min(1f, num4);
		}
		if (centerVertically)
		{
			Vector3 vector = panel.cachedTransform.InverseTransformPoint(worldCorners[0] * bottomPercent + worldCorners[1] * (1f - bottomPercent) - ((bounds.max + bounds.min) * 0.5f - mTransform.position));
			Vector3 localPosition = mTransform.localPosition;
			vector.z = localPosition.z;
			Vector3 localPosition2 = mTransform.localPosition;
			vector.x = localPosition2.x;
			float num5 = num4;
			Vector3 localScale2 = mTransform.localScale;
			if (num5 != localScale2.x)
			{
				Transform transform = mTransform;
				float x = num4;
				float y2 = num4;
				Vector3 localScale3 = mTransform.localScale;
				transform.localScale = new Vector3(x, y2, localScale3.z);
			}
			if (mTransform.localPosition != vector)
			{
				mTransform.localPosition = vector;
			}
		}
	}
}

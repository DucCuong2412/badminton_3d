using System;
using UnityEngine;

[ExecuteInEditMode]
public class TopAdsAnchor : MonoBehaviour
{
	protected UIWidget mWidget;

	protected Transform mTransform;

	public bool onlyIncludeTopAnchor;

	protected bool started;

	public float adHeight = 60f;

	public float screenDimensionFor2x = 800f;

	protected float usedAdHeight;

	public bool checkInGameAds;

	public float noAdHeight = 10f;

	private void ScreenSizeChanged()
	{
		if (started)
		{
			Update();
		}
	}

	private void Awake()
	{
		mTransform = base.transform;
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
		NavigationManager.onLayerChanged += ScreenSizeChanged;
	}

	private void OnDestroy()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
		NavigationManager.onLayerChanged -= ScreenSizeChanged;
	}

	private void Start()
	{
		mWidget = GetComponent<UIWidget>();
		started = true;
		usedAdHeight = adHeight;
		if (checkInGameAds && (Ads.instance.hideAdsInGame() || !Ads.instance.shouldShowAds))
		{
			usedAdHeight = noAdHeight;
			Ads.instance.hideBanner(hideBanner: true);
		}
		if ((float)Mathf.Max(Screen.width, Screen.height) >= screenDimensionFor2x)
		{
			usedAdHeight *= 2f;
		}
		Update();
	}

	private void Update()
	{
		Camera mainCamera = UICamera.mainCamera;
		Transform parent = mTransform.parent;
		Vector3 position = mainCamera.ScreenToWorldPoint(new Vector3(0f, (float)Screen.height - usedAdHeight, 0f));
		Vector3[] worldCorners = mWidget.worldCorners;
		Vector3 vector = parent.InverseTransformPoint(position);
		Vector3 vector2 = parent.InverseTransformPoint(mWidget.topAnchor.rect.worldCorners[1]);
		int num = -(int)(vector2.y - vector.y);
		if (num != 0)
		{
			int num2 = mWidget.topAnchor.absolute - mWidget.bottomAnchor.absolute;
			mWidget.topAnchor.absolute = num;
			if (!onlyIncludeTopAnchor)
			{
				mWidget.bottomAnchor.absolute = num - num2;
			}
		}
		mWidget.UpdateAnchors();
		if (Application.isPlaying)
		{
			base.enabled = false;
		}
	}
}

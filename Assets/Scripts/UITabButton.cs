using UnityEngine;

public class UITabButton : UIButtonColor
{
	public delegate void OnTabSelectedDelegate(UITabButton button);

	public OnTabSelectedDelegate onSelected;

	public Color disabledColor = Color.grey;

	public Color activeColor = Color.white;

	public GameObject tabLayer;

	protected bool active_;

	public UITabController controller
	{
		get;
		set;
	}

	public bool isActive
	{
		get
		{
			return active_;
		}
		set
		{
			UnityEngine.Debug.Log("Tab " + base.name + " isActive " + value);
			active_ = value;
			UpdateColor(value, immediate: true);
		}
	}

	private void OnClick()
	{
		if (controller != null)
		{
			controller.OnTabSelected(this);
		}
		if (onSelected != null)
		{
			onSelected(this);
		}
	}

	public void UpdateColor(bool shouldBeEnabled, bool immediate)
	{
		Color color = (!shouldBeEnabled) ? disabledColor : activeColor;
		if ((bool)tweenTarget)
		{
			if (!mStarted)
			{
				mStarted = true;
				Init();
			}
			TweenColor tweenColor = TweenColor.Begin(tweenTarget, 0.15f, color);
			mWidget.color = color;
			if (tweenColor != null && immediate)
			{
				tweenColor.value = color;
				tweenColor.enabled = false;
			}
		}
		else
		{
			mWidget.color = color;
		}
	}

	protected override void OnPress(bool isPressed)
	{
		if (!isActive)
		{
			base.OnPress(isPressed);
		}
	}

	protected override void OnHover(bool isOver)
	{
	}

	protected override void OnDragOver()
	{
		if (!isActive)
		{
			base.OnDragOver();
		}
	}

	protected override void OnDragOut()
	{
		if (!isActive)
		{
			base.OnDragOut();
		}
	}

	protected override void OnSelect(bool isSelected)
	{
		if (!isActive)
		{
			base.OnSelect(isSelected);
		}
	}
}

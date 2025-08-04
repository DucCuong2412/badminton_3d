using UnityEngine;

public class NavigationButton : UIButtonColor
{
	public enum NavigationType
	{
		PushLayer,
		PopLayer
	}

	public NavigationType navigationType;

	public GameObject pushLayer;

	public Color disabledColor = Color.grey;

	public bool dragHighlight;

	public bool isEnabled
	{
		get
		{
			if (!base.enabled)
			{
				return false;
			}
			Collider component = GetComponent<Collider>();
			return (bool)component && component.enabled;
		}
		set
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = value;
			}
			else
			{
				base.enabled = value;
			}
			UpdateColor(value, immediate: false);
		}
	}

	protected override void OnEnable()
	{
		pressed = new Color(0.5882353f, 0.5882353f, 0.5882353f, 1f);
		hover = Color.white;
		duration = 0.1f;
		if (isEnabled)
		{
			if (mStarted)
			{
				if (UICamera.currentScheme == UICamera.ControlScheme.Controller)
				{
					OnHover(UICamera.selectedObject == base.gameObject);
				}
				else if (UICamera.currentScheme == UICamera.ControlScheme.Mouse)
				{
					OnHover(UICamera.hoveredObject == base.gameObject);
				}
				else
				{
					UpdateColor(shouldBeEnabled: true, immediate: false);
				}
			}
		}
		else
		{
			UpdateColor(shouldBeEnabled: false, immediate: true);
		}
	}

	protected override void OnHover(bool isOver)
	{
		if (isEnabled)
		{
			base.OnHover(isOver);
		}
	}

	protected override void OnPress(bool isPressed)
	{
		if (isEnabled)
		{
			base.OnPress(isPressed);
		}
	}

	protected override void OnDragOver()
	{
		if (isEnabled && (dragHighlight || UICamera.currentTouch.pressed == base.gameObject))
		{
			base.OnDragOver();
		}
	}

	protected override void OnDragOut()
	{
		if (isEnabled && (dragHighlight || UICamera.currentTouch.pressed == base.gameObject))
		{
			base.OnDragOut();
		}
	}

	protected override void OnSelect(bool isSelected)
	{
		if (isEnabled)
		{
			base.OnSelect(isSelected);
		}
	}

	private void OnClick()
	{
		if (isEnabled)
		{
			switch (navigationType)
			{
			case NavigationType.PushLayer:
				NavigationManager.instance.Push(pushLayer);
				break;
			case NavigationType.PopLayer:
				NavigationManager.instance.Pop();
				break;
			}
		}
	}

	public void UpdateColor(bool shouldBeEnabled, bool immediate)
	{
		if ((bool)tweenTarget)
		{
			if (!mStarted)
			{
				mStarted = true;
				Init();
			}
			Color color = (!shouldBeEnabled) ? disabledColor : base.defaultColor;
			TweenColor tweenColor = TweenColor.Begin(tweenTarget, 0.15f, color);
			if (tweenColor != null && immediate)
			{
				tweenColor.value = color;
				tweenColor.enabled = false;
			}
		}
	}
}

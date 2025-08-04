using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Button Color")]
public class UIButtonColor : UIWidgetContainer
{
	public GameObject tweenTarget;

	public Color hover = new Color(0.882352948f, 40f / 51f, 0.5882353f, 1f);

	public Color pressed = new Color(61f / 85f, 163f / 255f, 41f / 85f, 1f);

	public float duration = 0.2f;

	protected Color mColor;

	protected bool mStarted;

	protected UIWidget mWidget;

	public Color defaultColor
	{
		get
		{
			Start();
			return mColor;
		}
		set
		{
			Start();
			mColor = value;
		}
	}

	private void Start()
	{
		if (!mStarted)
		{
			mStarted = true;
			Init();
		}
	}

	protected virtual void OnEnable()
	{
		if (mStarted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
		if (UICamera.currentTouch != null)
		{
			if (UICamera.currentTouch.pressed == base.gameObject)
			{
				OnPress(isPressed: true);
			}
			else if (UICamera.currentTouch.current == base.gameObject)
			{
				OnHover(isOver: true);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (mStarted && tweenTarget != null)
		{
			TweenColor component = tweenTarget.GetComponent<TweenColor>();
			if (component != null)
			{
				component.value = mColor;
				component.enabled = false;
			}
		}
	}

	protected void Init()
	{
		if (tweenTarget == null)
		{
			tweenTarget = base.gameObject;
		}
		mWidget = tweenTarget.GetComponent<UIWidget>();
		if (mWidget != null)
		{
			mColor = mWidget.color;
		}
		else
		{
			Renderer component = tweenTarget.GetComponent<Renderer>();
			if (component != null)
			{
				mColor = component.material.color;
			}
			else
			{
				Light component2 = tweenTarget.GetComponent<Light>();
				if (component2 != null)
				{
					mColor = component2.color;
				}
				else
				{
					tweenTarget = null;
					if (Application.isPlaying)
					{
						UnityEngine.Debug.LogWarning(NGUITools.GetHierarchy(base.gameObject) + " has nothing for UIButtonColor to color", this);
						base.enabled = false;
					}
				}
			}
		}
		OnEnable();
	}

	protected virtual void OnPress(bool isPressed)
	{
		UnityEngine.Debug.Log("Press " + base.name);
		if (base.enabled)
		{
			if (!mStarted)
			{
				Start();
			}
			if (isPressed)
			{
				TweenColor.Begin(tweenTarget, duration, pressed);
			}
			else if (UICamera.currentTouch.current == base.gameObject && UICamera.currentScheme == UICamera.ControlScheme.Controller)
			{
				TweenColor.Begin(tweenTarget, duration, hover);
			}
			else
			{
				TweenColor.Begin(tweenTarget, duration, mColor);
			}
		}
	}

	protected virtual void OnHover(bool isOver)
	{
		if (base.enabled)
		{
			if (!mStarted)
			{
				Start();
			}
			TweenColor.Begin(tweenTarget, duration, (!isOver) ? mColor : hover);
		}
	}

	protected virtual void OnDragOver()
	{
		if (base.enabled)
		{
			if (!mStarted)
			{
				Start();
			}
			TweenColor.Begin(tweenTarget, duration, pressed);
		}
	}

	protected virtual void OnDragOut()
	{
		if (base.enabled)
		{
			if (!mStarted)
			{
				Start();
			}
			TweenColor.Begin(tweenTarget, duration, mColor);
		}
	}

	protected virtual void OnSelect(bool isSelected)
	{
		if (base.enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
		{
			OnHover(isSelected);
		}
	}
}

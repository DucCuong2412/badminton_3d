using AnimationOrTween;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Play Animation")]
public class UIPlayAnimation : MonoBehaviour
{
	public Animation target;

	public string clipName;

	public Trigger trigger;

	public Direction playDirection = Direction.Forward;

	public bool resetOnPlay;

	public bool clearSelection;

	public EnableCondition ifDisabledOnPlay;

	public DisableCondition disableWhenFinished;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	[HideInInspector]
	[SerializeField]
	private GameObject eventReceiver;

	[HideInInspector]
	[SerializeField]
	private string callWhenFinished;

	private bool mStarted;

	private bool mActivated;

	private bool dragHighlight;

	private bool dualState => trigger == Trigger.OnPress || trigger == Trigger.OnHover;

	private void Awake()
	{
		UIButton component = GetComponent<UIButton>();
		if (component != null)
		{
			dragHighlight = component.dragHighlight;
		}
		if (eventReceiver != null && EventDelegate.IsValid(onFinished))
		{
			eventReceiver = null;
			callWhenFinished = null;
		}
	}

	private void Start()
	{
		mStarted = true;
		if (target == null)
		{
			target = GetComponentInChildren<Animation>();
		}
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
		if (UICamera.currentTouch != null)
		{
			if (trigger == Trigger.OnPress || trigger == Trigger.OnPressTrue)
			{
				mActivated = (UICamera.currentTouch.pressed == base.gameObject);
			}
			if (trigger == Trigger.OnHover || trigger == Trigger.OnHoverTrue)
			{
				mActivated = (UICamera.currentTouch.current == base.gameObject);
			}
		}
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled && (trigger == Trigger.OnHover || (trigger == Trigger.OnHoverTrue && isOver) || (trigger == Trigger.OnHoverFalse && !isOver)))
		{
			Play(isOver, dualState);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && (trigger == Trigger.OnPress || (trigger == Trigger.OnPressTrue && isPressed) || (trigger == Trigger.OnPressFalse && !isPressed)))
		{
			Play(isPressed, dualState);
		}
	}

	private void OnClick()
	{
		if (base.enabled && trigger == Trigger.OnClick)
		{
			Play(forward: true, onlyIfDifferent: false);
		}
	}

	private void OnDoubleClick()
	{
		if (base.enabled && trigger == Trigger.OnDoubleClick)
		{
			Play(forward: true, onlyIfDifferent: false);
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (base.enabled && (trigger == Trigger.OnSelect || (trigger == Trigger.OnSelectTrue && isSelected) || (trigger == Trigger.OnSelectFalse && !isSelected)))
		{
			Play(isSelected, dualState);
		}
	}

	private void OnActivate(bool isActive)
	{
		if (base.enabled && (trigger == Trigger.OnActivate || (trigger == Trigger.OnActivateTrue && isActive) || (trigger == Trigger.OnActivateFalse && !isActive)))
		{
			Play(isActive, dualState);
		}
	}

	private void OnDragOver()
	{
		if (base.enabled && dualState)
		{
			if (UICamera.currentTouch.dragged == base.gameObject)
			{
				Play(forward: true, onlyIfDifferent: true);
			}
			else if (dragHighlight && trigger == Trigger.OnPress)
			{
				Play(forward: true, onlyIfDifferent: true);
			}
		}
	}

	private void OnDragOut()
	{
		if (base.enabled && dualState && UICamera.hoveredObject != base.gameObject)
		{
			Play(forward: false, onlyIfDifferent: true);
		}
	}

	private void OnDrop(GameObject go)
	{
		if (base.enabled && trigger == Trigger.OnPress && UICamera.currentTouch.dragged != base.gameObject)
		{
			Play(forward: false, onlyIfDifferent: true);
		}
	}

	public void Play(bool forward)
	{
		Play(forward, onlyIfDifferent: true);
	}

	public void Play(bool forward, bool onlyIfDifferent)
	{
		if (!target)
		{
			return;
		}
		if (onlyIfDifferent)
		{
			if (mActivated == forward)
			{
				return;
			}
			mActivated = forward;
		}
		if (clearSelection && UICamera.selectedObject == base.gameObject)
		{
			UICamera.selectedObject = null;
		}
		int num = 0 - playDirection;
		Direction direction = (Direction)((!forward) ? num : ((int)playDirection));
		ActiveAnimation activeAnimation = ActiveAnimation.Play(target, clipName, direction, ifDisabledOnPlay, disableWhenFinished);
		if (activeAnimation != null)
		{
			if (resetOnPlay)
			{
				activeAnimation.Reset();
			}
			for (int i = 0; i < onFinished.Count; i++)
			{
				EventDelegate.Add(activeAnimation.onFinished, OnFinished, oneShot: true);
			}
		}
	}

	private void OnFinished()
	{
		EventDelegate.Execute(onFinished);
		if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
		{
			eventReceiver.SendMessage(callWhenFinished, SendMessageOptions.DontRequireReceiver);
		}
		eventReceiver = null;
	}
}

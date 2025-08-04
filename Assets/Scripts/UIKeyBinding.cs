using UnityEngine;

[AddComponentMenu("Game/UI/Key Binding")]
public class UIKeyBinding : MonoBehaviour
{
	public enum Action
	{
		PressAndClick,
		Select
	}

	public enum Modifier
	{
		None,
		Shift,
		Control,
		Alt
	}

	public KeyCode keyCode;

	public Modifier modifier;

	public Action action;

	private bool mIgnoreUp;

	private bool mIsInput;

	private void Start()
	{
		UIInput component = GetComponent<UIInput>();
		mIsInput = (component != null);
		if (component != null)
		{
			EventDelegate.Add(component.onSubmit, OnSubmit);
		}
	}

	private void OnSubmit()
	{
		if (UICamera.currentKey == keyCode && IsModifierActive())
		{
			mIgnoreUp = true;
		}
	}

	private bool IsModifierActive()
	{
		if (modifier == Modifier.None)
		{
			return true;
		}
		if (modifier == Modifier.Alt)
		{
			if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Control)
		{
			if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
			{
				return true;
			}
		}
		else if (modifier == Modifier.Shift && (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift)))
		{
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (keyCode == KeyCode.None || !IsModifierActive())
		{
			return;
		}
		if (action == Action.PressAndClick)
		{
			if (!UICamera.inputHasFocus)
			{
				if (UnityEngine.Input.GetKeyDown(keyCode))
				{
					UICamera.currentTouch.current = base.gameObject;
					UICamera.Notify(base.gameObject, "OnPress", true);
					UICamera.currentTouch.current = null;
				}
				if (UnityEngine.Input.GetKeyUp(keyCode))
				{
					UICamera.currentTouch.current = base.gameObject;
					UICamera.Notify(base.gameObject, "OnPress", false);
					UICamera.Notify(base.gameObject, "OnClick", null);
					UICamera.currentTouch.current = null;
				}
			}
		}
		else
		{
			if (action != Action.Select || !Input.GetKeyUp(keyCode))
			{
				return;
			}
			if (mIsInput)
			{
				if (!mIgnoreUp && !UICamera.inputHasFocus)
				{
					UICamera.selectedObject = base.gameObject;
				}
				mIgnoreUp = false;
			}
			else
			{
				UICamera.selectedObject = base.gameObject;
			}
		}
	}
}

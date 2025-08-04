using UnityEngine;

public class UIDelegateButton : UIButtonColor
{
	public delegate void OnClickDelegate();

	public event OnClickDelegate onClickDelegate;

	protected override void OnPress(bool isPressed)
	{
		UnityEngine.Debug.Log("OnPress");
		base.OnPress(isPressed);
	}

	private void OnClick()
	{
		UnityEngine.Debug.Log("OnClick");
		if (this.onClickDelegate != null)
		{
			this.onClickDelegate();
		}
	}
}

using UnityEngine;

public class MultiplayerSelectLayer : UILayer
{
	public UITable table;

	public GameObject buttonPrefab;

	public MultiplayerLobbyLayer lobbyLayer;

	private int buttonIndex;

	private void Awake()
	{
	}

	private void CreateButton(string text, string methodName)
	{
		GameObject gameObject = table.gameObject;
		GameObject gameObject2 = NGUITools.AddChild(gameObject, buttonPrefab);
		gameObject2.name = buttonIndex++.ToString();
		ButtonWithText component = gameObject2.GetComponent<ButtonWithText>();
		if (component != null)
		{
			component.text.text = text;
		}
		UIButton component2 = gameObject2.GetComponent<UIButton>();
		if (!(component2 == null))
		{
			component2.onClick.Add(new EventDelegate(this, methodName));
		}
	}

	public void FindServer()
	{
		lobbyLayer.FindMatch();
	}

	public void StartSplitScreen()
	{
		ScreenNavigation.instance.LoadSplitScreenMatch();
	}
}

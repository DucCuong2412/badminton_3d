using UnityEngine;

public class LeagueMainButton : MonoBehaviour
{
	public UILabel label;

	public UISprite sprite;

	private bool showLabel;

	private void OnEnable()
	{
		Update();
	}

	private void Update()
	{
		LeagueController instance = LeagueController.instance;
		showLabel = (instance.isLeagueInProgress() && !instance.isNextMatchActive());
		GameObject cachedGameObject = sprite.cachedGameObject;
		if (cachedGameObject.activeSelf != showLabel)
		{
			cachedGameObject.SetActive(showLabel);
		}
		if (showLabel)
		{
			UITools.ChangeText(label, GGFormat.FormatTimeSpan(instance.TimeTillNextMatchActive()));
		}
	}
}

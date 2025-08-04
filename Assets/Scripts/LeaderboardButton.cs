using UnityEngine;

public class LeaderboardButton : MonoBehaviour
{
	public UILabel text;

	private void OnEnable()
	{
		UITools.ChangeText(text, PlayerSettings.instance.Model.score + " Exp");
	}

	public void OnClick()
	{
		SocialAuthentication.ShowLeaderboard();
	}

	private void OnSignIn(bool success)
	{
		Social instance = BehaviourSingleton<Social>.instance;
		if (success)
		{
			instance.showLeaderboardAfterSignIn = true;
			instance.signIn();
		}
		NavigationManager.instance.Pop(force: true);
	}
}

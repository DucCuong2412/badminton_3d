using UnityEngine;

public class Social : BehaviourSingleton<Social>
{
	protected static string leaderboardId = "CgkIhPXz_58QEAIQAQ";

	public bool showLeaderboardAfterSignIn;

	private void Awake()
	{
		BehaviourSingleton<GGNotificationCenter>.instance.onMessage += onGGMessage;
	}

	public void submitScore(int score)
	{
		PlayerSettings instance = PlayerSettings.instance;
		instance.Model.score = Mathf.Max(score, instance.Model.score);
		instance.Save();
		if (isSignedIn())
		{
			GGSocialGaming.instance.submitScore(leaderboardId, score);
		}
	}

	public void showLeaderboard()
	{
		GGSocialGaming.instance.displayLeaderboards(leaderboardId);
	}

	public void signIn()
	{
		GGSocialGaming.instance.signIn();
	}

	public bool isSignedIn()
	{
		return GGSocialGaming.instance.isSignedIn();
	}

	public void showAchivements()
	{
		if (GGSocialGaming.instance.isSignedIn())
		{
			GGSocialGaming.instance.displayAchivements();
		}
		else
		{
			GGSocialGaming.instance.signIn();
		}
	}

	private void onGGMessage(string message)
	{
		if ("GGGooglePlayServices.onSignInSucceeded" == message)
		{
			if (PlayerSettings.instance.Model.score > 0)
			{
				GGSocialGaming.instance.submitScore(leaderboardId, PlayerSettings.instance.Model.score);
			}
			if (showLeaderboardAfterSignIn)
			{
				showLeaderboardAfterSignIn = false;
				showLeaderboard();
			}
			Analytics.instance.loginToLeaderboard(success: true);
		}
		else if ("GGGooglePlayServices.onSignInFail" == message)
		{
			showLeaderboardAfterSignIn = false;
			Analytics.instance.loginToLeaderboard(success: false);
		}
	}
}

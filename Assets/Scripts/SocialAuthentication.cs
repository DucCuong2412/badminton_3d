using UnityEngine;

public class SocialAuthentication : MonoBehaviour
{
	public RuntimePlatform platform = RuntimePlatform.IPhonePlayer;

	private void Awake()
	{
		if (Application.platform == platform)
		{
			GGSocialGaming.instance.signIn();
		}
	}

	public static void ShowLeaderboard()
	{
		Social social = BehaviourSingleton<Social>.instance;
		if (social.isSignedIn())
		{
			social.showLeaderboard();
		}
		else if (Application.platform != RuntimePlatform.IPhonePlayer && ConfigBase.instance.socialProvider == ConfigBase.SocialProvider.GooglePlayServices)
		{
			UIDialog.instance.ShowSignIn("Sign In", "See how Good you are and compare Your Score with the World! Sign in with Google to access Leaderboards and Achivements!", "Later", delegate(bool success)
			{
				if (success)
				{
					social.showLeaderboardAfterSignIn = true;
					social.signIn();
				}
				NavigationManager.instance.Pop(force: true);
			});
		}
		else
		{
			social.showLeaderboard();
		}
	}

	public static void ShowAchivements()
	{
		Social social = BehaviourSingleton<Social>.instance;
		if (social.isSignedIn())
		{
			social.showAchivements();
		}
		else if (Application.platform != RuntimePlatform.IPhonePlayer && ConfigBase.instance.socialProvider == ConfigBase.SocialProvider.GooglePlayServices)
		{
			UIDialog.instance.ShowSignIn("Sign In", "Sign in with Google to access Leaderboards and Achivements!", "Later", delegate(bool success)
			{
				if (success)
				{
					social.showAchivements();
				}
				NavigationManager.instance.Pop(force: true);
			});
		}
		else
		{
			social.showAchivements();
		}
	}
}

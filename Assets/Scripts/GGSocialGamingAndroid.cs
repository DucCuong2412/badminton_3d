using UnityEngine;

public class GGSocialGamingAndroid : GGSocialGaming
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	public GGSocialGamingAndroid()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGGooglePlayServices"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public override bool isAvailable()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("UnityIsGooglePlayServicesAvailable", new object[0]);
	}

	public override void submitScore(string leaderboard, long score)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnitySubmitScore", leaderboard, score);
		}
	}

	public override void displayAllLeaderboards()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnityDisplayAllLeaderboards");
		}
	}

	public override void displayLeaderboards(string id)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnityDisplayLeaderboard", id);
		}
	}

	public override bool isSignedIn()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("isSignedIn", new object[0]);
	}

	public override void unlockAchievement(string achivement)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnityUnlockAchievement", achivement);
		}
	}

	public override void displayAchivements()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnityDisplayAchivements");
		}
	}

	public override void signIn()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnityBeginUserInitiatedSignIn");
		}
	}

	public override void signOut()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("UnitySignOut");
		}
	}
}

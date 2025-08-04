using UnityEngine;

public class GGSocialGamingGameCircle : GGSocialGaming
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	public GGSocialGamingGameCircle()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGAmazonGameCircle"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public override bool isAvailable()
	{
		if (Application.platform != platform)
		{
			return true;
		}
		return javaInstance.Call<bool>("isGameCircleAvailable", new object[0]);
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
		return true;
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
		}
	}

	public override void signOut()
	{
		if (Application.platform == platform)
		{
		}
	}
}

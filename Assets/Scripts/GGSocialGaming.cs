public class GGSocialGaming
{
	public const string GGSocialGamingSignInSuccessNotification = "GGGooglePlayServices.onSignInSucceeded";

	public const string GGSocialGamingSingInFailNotification = "GGGooglePlayServices.onSignInFail";

	protected static GGSocialGaming instance_;

	public static GGSocialGaming instance
	{
		get
		{
			if (instance_ == null)
			{
				if (ConfigBase.instance.socialProvider == ConfigBase.SocialProvider.AmazonGameCircle)
				{
					instance_ = new GGSocialGamingGameCircle();
				}
				else
				{
					instance_ = new GGSocialGamingAndroid();
				}
			}
			return instance_;
		}
	}

	public virtual bool isSignedIn()
	{
		return true;
	}

	public virtual void unlockAchievement(string achivement)
	{
	}

	public virtual void displayAchivements()
	{
	}

	public virtual void signIn()
	{
	}

	public virtual void signOut()
	{
	}

	public virtual bool isAvailable()
	{
		return true;
	}

	public virtual void submitScore(string leaderboard, long score)
	{
	}

	public virtual void displayAllLeaderboards()
	{
	}

	public virtual void displayLeaderboards(string id)
	{
	}
}

public class GGGameCenter
{
	private static GGGameCenter instance_;

	public static GGGameCenter instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new GGGameCenter();
			}
			return instance_;
		}
	}

	public virtual void authenticate()
	{
	}

	public virtual void reportScore(int score, string category)
	{
	}

	public virtual void showLeaderboard(string category)
	{
	}
}

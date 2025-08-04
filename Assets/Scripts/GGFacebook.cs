public class GGFacebook
{
	public static GGFacebook instance_;

	public static GGFacebook instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new GGFacebookAndroid();
				instance_.Init();
			}
			return instance_;
		}
	}

	protected virtual void Init()
	{
	}

	public virtual void showShareDialog(string applicationName, string title, string subtitle, string description, string link)
	{
	}

	public virtual bool isAvailable()
	{
		return true;
	}
}

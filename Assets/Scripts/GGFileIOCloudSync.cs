public class GGFileIOCloudSync
{
	public const string NOTIFICATION_NEW_DATA = "CloudSync.NewData";

	public const string NOTIFICATION_DATA_UPLOADED = "CloudSync.DataUploaded";

	public static GGFileIOCloudSync instance_;

	public static GGFileIOCloudSync instance
	{
		get
		{
			if (instance_ == null)
			{
				if (ConfigBase.instance.useCloudSync)
				{
					instance_ = new GGWhisperSyncFileIOSync();
				}
				else
				{
					instance_ = new GGFileIOCloudSync();
				}
			}
			return instance_;
		}
	}

	public static bool isCloudSyncNotification(string message)
	{
		return "CloudSync.NewData" == message || "CloudSync.DataUploaded" == message;
	}

	public virtual GGFileIO GetDefaultFileIO()
	{
		return GGFileIO.instance;
	}

	public virtual GGFileIO GetCloudFileIO()
	{
		return GGFileIO.instance;
	}

	public virtual bool isInConflict(string name)
	{
		return false;
	}

	public virtual void synchronize()
	{
	}
}

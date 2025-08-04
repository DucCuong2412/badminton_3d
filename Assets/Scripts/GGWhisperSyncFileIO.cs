using System.Text;

public class GGWhisperSyncFileIO : GGFileIO
{
	protected bool localVersion;

	public GGWhisperSyncFileIO(bool localVersion)
	{
		this.localVersion = localVersion;
	}

	public override void Write(string path, string text)
	{
		Write(path, Encoding.UTF8.GetBytes(text));
	}

	public override void Write(string path, byte[] bytes)
	{
		BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsSetBytes(path, bytes, !localVersion);
	}

	public override string ReadText(string path)
	{
		byte[] array = Read(path);
		if (array == null)
		{
			return null;
		}
		return Encoding.UTF8.GetString(array);
	}

	public override byte[] Read(string path)
	{
		if (localVersion)
		{
			return BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsGetBytes(path);
		}
		return BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsGetCloudBytes(path);
	}

	public override bool FileExists(string path)
	{
		if (localVersion)
		{
			return BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsIsSet(path);
		}
		string value = BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsGetCloudString(path);
		return !string.IsNullOrEmpty(value);
	}
}

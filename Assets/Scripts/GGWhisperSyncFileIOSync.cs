public class GGWhisperSyncFileIOSync : GGFileIOCloudSync
{
	private GGWhisperSyncFileIO local = new GGWhisperSyncFileIO(localVersion: true);

	private GGWhisperSyncFileIO cloud = new GGWhisperSyncFileIO(localVersion: false);

	public override GGFileIO GetDefaultFileIO()
	{
		return local;
	}

	public override GGFileIO GetCloudFileIO()
	{
		return cloud;
	}

	public override bool isInConflict(string name)
	{
		return BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsIsInConflict(name);
	}

	public override void synchronize()
	{
		BehaviourSingletonInit<GGAmazonGameCircle>.instance.wsSynchronize();
	}
}

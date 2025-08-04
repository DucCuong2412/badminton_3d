using System;
using System.Collections.Generic;
using UnityEngine;

public class GGBuildConfig : ScriptableObject
{
	[Serializable]
	public class GameAnalyticsKeys
	{
		public BuildPlatform platform;

		public string gameKey;

		public string secretKey;
	}

	public enum BuildPlatform
	{
		Unknown,
		GooglePlay,
		Amazon,
		IOS,
		WinPhone8,
		Metro
	}

	public List<string> scenesToBuild = new List<string>();

	public List<string> optionalScenesToBuild = new List<string>();

	public List<GameAnalyticsKeys> gameAnalytics = new List<GameAnalyticsKeys>();

	public List<string> standaloneScenesToBuild = new List<string>();

	public BuildPlatform buildPlatform;

	public bool enableProfiler;

	[HideInInspector]
	public BuildPlatform activeBuildPlatform;

	public string FacebookAppId => ConfigBase.instance.facebookAppId;

	public GameAnalyticsKeys gameAnalyticsKeysForPlatform(BuildPlatform platform)
	{
		foreach (GameAnalyticsKeys gameAnalytic in gameAnalytics)
		{
			if (gameAnalytic.platform == platform)
			{
				return gameAnalytic;
			}
		}
		return null;
	}
}

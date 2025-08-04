using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineRankings : BehaviourSingleton<OnlineRankings>
{
	public delegate void OnComplete(OnlineRankings rankigs);

	private Rankings rankings = new Rankings();

	protected long timestampDifference;

	protected bool timestampDifferenceSet;

	public long sentCountryPoints;

	public Rankings rank => rankings;

	public bool inRequest
	{
		get;
		protected set;
	}

	public long countryPoints
	{
		get
		{
			return PlayerSettings.instance.Model.countryPoints;
		}
		set
		{
			PlayerSettings.instance.Model.countryPoints = value;
		}
	}

	public long myPoints
	{
		get
		{
			return PlayerSettings.instance.Model.rankingsScore;
		}
		set
		{
			PlayerSettings.instance.Model.rankingsScore = value;
		}
	}

	public long myPointsTimestamp
	{
		get
		{
			return PlayerSettings.instance.Model.timeStartedCollectingPoints;
		}
		set
		{
			PlayerSettings.instance.Model.timeStartedCollectingPoints = value;
		}
	}

	public long competitionStart
	{
		get
		{
			return PlayerSettings.instance.Model.competitionStart;
		}
		set
		{
			PlayerSettings.instance.Model.competitionStart = value;
		}
	}

	public long competitionEnd
	{
		get
		{
			return PlayerSettings.instance.Model.competitionEnd;
		}
		set
		{
			PlayerSettings.instance.Model.competitionEnd = value;
		}
	}

	public event OnComplete onSuccess;

	public event OnComplete onFail;

	public void Save()
	{
		PlayerSettings.instance.Save();
	}

	private void CheckPoints()
	{
		if ((competitionStart != 0 && myPointsTimestamp < competitionStart) || (competitionEnd != 0 && myPointsTimestamp >= competitionEnd))
		{
			UnityEngine.Debug.Log("Deleting points " + competitionStart + " " + competitionEnd + " my " + myPointsTimestamp);
			myPoints = 0L;
			myPointsTimestamp = timestamp(DateTime.UtcNow);
			Save();
		}
	}

	private long timestamp(DateTime time)
	{
		return (long)time.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
	}

	public int ReportGameDone(int addedScore)
	{
		CheckPoints();
		int num = addedScore / 100;
		countryPoints += num;
		myPoints += num;
		Save();
		RequestRankings();
		return num;
	}

	public int checksum(string str, int numPos)
	{
		int num = Mathf.Min(str.Length, numPos);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += str[i];
		}
		return num2;
	}

	public long sChecksum(long myPoints, long addPoints, string app, string pid, long timestamp)
	{
		long num = 650000L;
		return ((myPoints + 1) * (addPoints + 1) + checksum(app, 5) * checksum(pid, 5) + timestamp) % num;
	}

	public void RequestTimestamp(bool addCountryPoints)
	{
		string url = ConfigBase.instance.rankingsServerUrl + "/l/ts";
		StartCoroutine(LoadTimestampFrom(url, addCountryPoints));
	}

	public void RequestRankings(bool addCountryPoints = false)
	{
		if (!timestampDifferenceSet)
		{
			RequestTimestamp(addCountryPoints);
		}
		CheckPoints();
		ConfigBase instance = ConfigBase.instance;
		long num = timestamp(DateTime.UtcNow) + timestampDifference;
		string text = instance.rankingsServerUrl + "/l/r?s=" + myPoints + "&app=" + instance.rankingsApp + "&c=" + CareerBackend.instance.Flag() + "&pid=" + GGUID.InstallId() + "&n=" + WWW.EscapeURL(CareerBackend.instance.Name()) + "&t=" + myPointsTimestamp + "&nt=" + num + "&cs=" + sChecksum(myPoints, (!addCountryPoints) ? 0 : countryPoints, instance.rankingsApp, GGUID.InstallId(), num);
		text = ((!addCountryPoints) ? (text + "&ns=0") : (text + "&ns=" + countryPoints));
		if (!inRequest)
		{
			if (addCountryPoints)
			{
				sentCountryPoints = countryPoints;
				countryPoints = 0L;
			}
			else
			{
				sentCountryPoints = 0L;
			}
			Save();
			StartCoroutine(LoadFromURL(text, addCountryPoints));
		}
	}

	private IEnumerator LoadTimestampFrom(string url, bool addCountryPoints)
	{
		if (inRequest)
		{
			yield break;
		}
		inRequest = true;
		UnityEngine.Debug.Log("URL: " + url);
		WWW www = new WWW(url);
		yield return www;
		inRequest = false;
		if (!string.IsNullOrEmpty(www.error))
		{
			if (this.onFail != null)
			{
				this.onFail(this);
			}
		}
		else
		{
			try
			{
				long num = long.Parse(www.text);
				timestampDifference = num - timestamp(DateTime.UtcNow);
				timestampDifferenceSet = true;
				RequestRankings(addCountryPoints);
			}
			catch
			{
				if (this.onFail != null)
				{
					this.onFail(this);
				}
			}
		}
	}

	private IEnumerator LoadFromURL(string url, bool addCountryPoints)
	{
		if (inRequest)
		{
			yield break;
		}
		inRequest = true;
		UnityEngine.Debug.Log("URL: " + url);
		WWW www = new WWW(url);
		yield return www;
		inRequest = false;
		if (!string.IsNullOrEmpty(www.error))
		{
			UnityEngine.Debug.Log("Error " + www.error);
			if (addCountryPoints)
			{
				countryPoints += sentCountryPoints;
				Save();
			}
			if (this.onFail != null)
			{
				this.onFail(this);
			}
		}
		else
		{
			try
			{
				string text = www.text;
				Rankings rankings = new Rankings();
				string[] array = text.Split('\t');
				int num = int.Parse(array[0]);
				int num2 = 1;
				long num3 = long.Parse(array[num2 + 4]);
				timestampDifference = num3 - timestamp(DateTime.UtcNow);
				timestampDifferenceSet = true;
				rankings.nationalEnd = long.Parse(array[num2]) - timestampDifference;
				rankings.playerEnd = long.Parse(array[num2 + 1]) - timestampDifference;
				rankings.nationalStart = long.Parse(array[num2 + 2]) - timestampDifference;
				rankings.playerStart = long.Parse(array[num2 + 3]) - timestampDifference;
				num2 += num;
				num2 = ReadPlayerList(array, num2, rankings.byCountry);
				num2 = ReadNationalList(array, num2, rankings.national);
				num2 = ReadPlayerList(array, num2, rankings.prevByCountry);
				ReadNationalList(array, num2, rankings.prevNational);
				this.rankings = rankings;
				competitionStart = rankings.playerStart;
				competitionEnd = rankings.playerEnd;
				Save();
				CheckPoints();
			}
			catch (Exception arg)
			{
				UnityEngine.Debug.Log("Exception " + arg);
				if (this.onFail != null)
				{
					this.onFail(this);
				}
				yield break;
			}
			if (this.onSuccess != null)
			{
				this.onSuccess(this);
			}
		}
	}

	private int ReadPlayerList(string[] c, int i, List<PlayerRank> rank)
	{
		rank.Clear();
		int num = int.Parse(c[i]);
		int num2 = int.Parse(c[i + 1]);
		i += 2;
		UnityEngine.Debug.Log("Count " + num + " Size " + num2);
		for (int j = 0; j < num; j++)
		{
			int num3 = i + j * num2;
			PlayerRank playerRank = new PlayerRank();
			playerRank.flag = int.Parse(c[num3]);
			playerRank.name = c[num3 + 1];
			playerRank.score = int.Parse(c[num3 + 2]);
			playerRank.pid = c[num3 + 3];
			rank.Add(playerRank);
		}
		if (num == 0)
		{
			i++;
		}
		i += num * num2;
		return i;
	}

	private int ReadNationalList(string[] c, int i, List<CountryRank> rank)
	{
		rank.Clear();
		int num = int.Parse(c[i]);
		int num2 = int.Parse(c[i + 1]);
		i += 2;
		UnityEngine.Debug.Log("Count " + num + " Size " + num2);
		for (int j = 0; j < num; j++)
		{
			int num3 = i + j * num2;
			CountryRank countryRank = new CountryRank();
			countryRank.flag = int.Parse(c[num3]);
			countryRank.score = int.Parse(c[num3 + 1]);
			countryRank.position = rank.Count;
			rank.Add(countryRank);
		}
		if (num == 0)
		{
			i++;
		}
		i += num * num2;
		return i;
	}
}

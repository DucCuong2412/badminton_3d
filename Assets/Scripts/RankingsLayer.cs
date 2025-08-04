using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankingsLayer : UILayer
{
	public UITabButton countryRanks;

	public UITabButton myRank;

	public UILabel status;

	public UILabel topHeader;

	public UILabel bottomHeader;

	public UILabel buttonLabel;

	public GameObject button;

	public UITabButton selectedButton;

	public UITable table;

	public LeagueListItem headerList;

	public LeagueListItem itemPrefab;

	public List<LeagueListItem> listItems = new List<LeagueListItem>();

	protected OnlineRankings rankings;

	protected bool receivedData;

	public UIScrollView scroll;

	public UILabel myRankLabel;

	public UISprite buttonFlag;

	public UISprite exclamation;

	public UILabel addedScoreLabel;

	private bool updatedTable;

	private void Awake()
	{
		rankings = BehaviourSingleton<OnlineRankings>.instance;
		countryRanks.onSelected = OnTabSelected;
		myRank.onSelected = OnTabSelected;
		selectedButton = countryRanks;
		listItems.Add(itemPrefab);
	}

	private void EnsureSize(int size)
	{
		GameObject gameObject = table.gameObject;
		while (listItems.Count < size)
		{
			GameObject gameObject2 = NGUITools.AddChild(gameObject, itemPrefab.gameObject);
			listItems.Add(gameObject2.GetComponent<LeagueListItem>());
		}
	}

	private void OnEnable()
	{
		updatedTable = false;
		Ads.instance.hideBanner(hideBanner: true);
		receivedData = false;
		BehaviourSingleton<OnlineRankings>.instance.onSuccess += OnSuccess;
		BehaviourSingleton<OnlineRankings>.instance.onFail += OnFail;
		BehaviourSingleton<OnlineRankings>.instance.RequestRankings();
		OnTabSelected(selectedButton);
		int flag = CareerBackend.instance.Flag();
		myRankLabel.text = "My Rank in " + GameConstants.Instance.CountryForFlag(flag).countryName;
		GameConstants.SetFlag(buttonFlag, flag);
		addedScoreLabel.cachedGameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		BehaviourSingleton<OnlineRankings>.instance.onSuccess -= OnSuccess;
		BehaviourSingleton<OnlineRankings>.instance.onFail -= OnFail;
	}

	private void OnSuccess(OnlineRankings rankings)
	{
		updatedTable = false;
		receivedData = true;
		OnTabSelected(selectedButton);
		if (rankings.sentCountryPoints > 0)
		{
			int num = CareerBackend.instance.Flag();
			string text = "You Added +" + rankings.sentCountryPoints + " points to " + GameConstants.Instance.CountryForFlag(num).countryName;
			CountryRank countryRank = rankings.rank.RankForCountry(num);
			if (countryRank != null)
			{
				string text2 = text;
				text = text2 + "! Ranked " + (countryRank.position + 1).ToString() + " pos, " + countryRank.score + " points!";
			}
			else
			{
				text += " Score!";
			}
			addedScoreLabel.text = text;
			addedScoreLabel.cachedGameObject.SetActive(value: true);
			addedScoreLabel.alpha = 1f;
			addedScoreLabel.cachedTransform.localScale = Vector3.one * 1.3f;
			TweenScale.Begin(addedScoreLabel.cachedGameObject, 0.5f, Vector3.one);
			StartCoroutine(DoAnimateAddedScore());
		}
	}

	private IEnumerator DoAnimateAddedScore()
	{
		yield return new WaitForSeconds(1.5f);
		TweenAlpha.Begin(addedScoreLabel.cachedGameObject, 3f, 0f);
	}

	private void ShowStatus(string statusText)
	{
		status.cachedGameObject.SetActive(value: true);
		status.text = statusText;
		headerList.gameObject.SetActive(value: false);
		foreach (LeagueListItem listItem in listItems)
		{
			listItem.gameObject.SetActive(value: false);
		}
		scroll.ResetPosition();
		table.Reposition();
	}

	private void OnFail(OnlineRankings rankings)
	{
		updatedTable = false;
		if (!receivedData)
		{
			ShowStatus("Error connecting to Server. Please try later...");
		}
		OnTabSelected(selectedButton);
	}

	private void ChangeButtonLabel()
	{
		if (!receivedData)
		{
			UITools.ChangeText(buttonLabel, "Refresh List");
			exclamation.cachedGameObject.SetActive(value: false);
		}
		else if (selectedButton == myRank)
		{
			UITools.ChangeText(buttonLabel, "Your Score: " + rankings.myPoints + " Refresh List!");
			exclamation.cachedGameObject.SetActive(value: false);
		}
		else if (rankings.countryPoints == 0)
		{
			UITools.ChangeText(buttonLabel, "Add your points to " + GameConstants.Instance.CountryForFlag(CareerBackend.instance.Flag()).countryName + " score!");
			exclamation.cachedGameObject.SetActive(value: false);
		}
		else
		{
			UnityEngine.Debug.Log("Exclamation true");
			exclamation.cachedGameObject.SetActive(value: true);
			UITools.ChangeText(buttonLabel, "Add your " + rankings.countryPoints + " points to " + GameConstants.Instance.CountryForFlag(CareerBackend.instance.Flag()).countryName + " score!");
		}
	}

	private void OnTabSelected(UITabButton selected)
	{
		countryRanks.isActive = (selected == countryRanks);
		myRank.isActive = (selected == myRank);
		selectedButton = selected;
		ChangeButtonLabel();
		UpdateTimeRemaining();
		if (!GGSupportMenu.instance.isNetworkConnected())
		{
			ShowStatus("Connect to Internet to Load Rankings...");
		}
		else if (rankings.inRequest)
		{
			ShowStatus("Loading Rankings...");
		}
		else if (!receivedData)
		{
			ShowStatus("Error happened, try again later...");
		}
		else if (selected == countryRanks)
		{
			OnCountryTab();
		}
		else
		{
			OnMyTab();
		}
	}

	private void UpdateTimeRemaining()
	{
		if (!receivedData)
		{
			bottomHeader.cachedGameObject.SetActive(value: false);
			return;
		}
		long num = (!(selectedButton == myRank)) ? rankings.rank.nationalEnd : rankings.rank.playerEnd;
		if (num == 0)
		{
			bottomHeader.cachedGameObject.SetActive(value: false);
			return;
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(num);
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan span = dateTime - utcNow;
		if (utcNow <= dateTime)
		{
			string str = GGFormat.FormatTimeSpanHuman(span);
			bottomHeader.text = "Time Remaining: " + str;
		}
		else
		{
			bottomHeader.text = "Competition Done";
		}
		bottomHeader.cachedGameObject.SetActive(value: true);
	}

	private void OnMyTab()
	{
		status.text = string.Empty;
		headerList.name.text = "Name";
		List<PlayerRank> byCountry = rankings.rank.byCountry;
		EnsureSize(byCountry.Count);
		headerList.gameObject.name = "a";
		headerList.gameObject.SetActive(value: true);
		int num = 0;
		foreach (LeagueListItem listItem in listItems)
		{
			listItem.transform.name = ((char)(ushort)(98 + num)).ToString();
			if (num >= byCountry.Count)
			{
				listItem.gameObject.SetActive(value: false);
			}
			else
			{
				PlayerRank playerRank = byCountry[num];
				listItem.gameObject.SetActive(value: true);
				int flag = playerRank.flag;
				listItem.SetParams(num + 1, playerRank.name, 0, playerRank.score, flag);
				num++;
			}
		}
		scroll.ResetPosition();
		table.Reposition();
		UITools.AlignToTopOnScroll(headerList.GetComponent<UIWidget>(), scroll, 0f);
		List<PlayerRank> prevByCountry = rankings.rank.prevByCountry;
		int a = 1;
		int num2 = Mathf.Min(a, prevByCountry.Count);
		string text = null;
		for (int i = 0; i < num2; i++)
		{
			PlayerRank playerRank2 = prevByCountry[i];
			text = ((text != null) ? (text + ", ") : "Previous Rankings: ");
			text = text + (i + 1).ToString() + ". " + playerRank2.name;
		}
		if (text != null)
		{
			topHeader.text = text;
			topHeader.gameObject.SetActive(value: true);
		}
		else
		{
			topHeader.gameObject.SetActive(value: false);
		}
	}

	private void OnCountryTab()
	{
		List<CountryRank> national = rankings.rank.national;
		status.text = string.Empty;
		EnsureSize(national.Count);
		headerList.gameObject.name = "a";
		headerList.name.text = "Country";
		headerList.gameObject.SetActive(value: true);
		int num = 0;
		foreach (LeagueListItem listItem in listItems)
		{
			listItem.transform.name = ((char)(ushort)(98 + num)).ToString();
			if (num >= national.Count)
			{
				listItem.gameObject.SetActive(value: false);
			}
			else
			{
				CountryRank countryRank = national[num];
				listItem.gameObject.SetActive(value: true);
				int flag = countryRank.flag;
				listItem.SetParams(num + 1, GameConstants.Instance.CountryForFlag(flag).countryName, 0, countryRank.score, flag);
				num++;
			}
		}
		scroll.ResetPosition();
		table.Reposition();
		UITools.AlignToTopOnScroll(headerList.GetComponent<UIWidget>(), scroll, 0f);
		List<CountryRank> prevNational = rankings.rank.prevNational;
		int a = 3;
		int num2 = Mathf.Min(a, prevNational.Count);
		string text = null;
		for (int i = 0; i < num2; i++)
		{
			CountryRank countryRank2 = prevNational[i];
			text = ((text != null) ? (text + ", ") : "Previous Rankings: ");
			text = text + (i + 1).ToString() + ". " + GameConstants.Instance.CountryForFlag(countryRank2.flag).countryName;
		}
		if (text != null)
		{
			topHeader.text = text;
			topHeader.gameObject.SetActive(value: true);
		}
		else
		{
			topHeader.gameObject.SetActive(value: false);
		}
	}

	public void OnButton()
	{
		if (selectedButton == countryRanks && receivedData)
		{
			if (rankings.countryPoints == 0)
			{
				UIDialog.instance.ShowOk("How to Win Country Points", "No country points. You can win country points by playing Career, Tournament and League matches", "Ok", null);
			}
			rankings.RequestRankings(addCountryPoints: true);
		}
		else
		{
			rankings.RequestRankings();
		}
	}

	public void FixedUpdate()
	{
		UpdateTimeRemaining();
		if (!updatedTable)
		{
			table.Reposition();
			updatedTable = true;
		}
	}
}

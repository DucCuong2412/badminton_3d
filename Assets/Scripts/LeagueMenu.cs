using ProtoModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeagueMenu : UILayer
{
	public GameObject listItemPrefab;

	public int numItems = 10;

	public UITable table;

	public UIScrollView scrollView;

	public UISprite button;

	public UILabel buttonLabel;

	public UILabel infoLabel;

	public UILabel nextMatchLabel;

	public UILabel playButtonLabel;

	private List<GameObject> list = new List<GameObject>();

	protected GameObject humanObject;

	private LeagueController league;

	private void Awake()
	{
		league = LeagueController.instance;
		bool flag = league.isLeagueInProgress();
		bool flag2 = league.isLeagueOver();
		int num = league.LeagueLevel();
		int num2 = 15;
		int num3 = 0;
		if (!flag)
		{
			league.CreateLeagueWithFlag(CareerBackend.instance.Flag(), CareerBackend.instance.Name());
			league.Save();
		}
		else if (flag2)
		{
			num3 = league.RankForMember(league.HumanPlayer());
			if (league.shouldAdvance())
			{
				UnityEngine.Debug.Log("Advance League");
				league.AdvanceLeague();
			}
			else
			{
				league.CreateLeagueWithFlag(CareerBackend.instance.Flag(), CareerBackend.instance.Name(), league.LeagueLevel());
			}
		}
		SetLabels();
		List<LeagueMemberDAO> list = league.CreateListSortedByRank();
		int num4 = 0;
		foreach (LeagueMemberDAO item in list)
		{
			GameObject gameObject = NGUITools.AddChild(table.gameObject, listItemPrefab);
			gameObject.GetComponent<LeagueListItem>().SetParams(num4++ + 1, item.name, item.wins, item.points, item.flag);
			if (item.isHuman)
			{
				humanObject = gameObject;
			}
			this.list.Add(gameObject);
		}
		StartCoroutine(LateStart());
		if (!flag)
		{
			UIDialog.instance.ShowOk("Welcome to the League", "Welcome to the league! By playing the league you earn balls, try to win your first opponent!", "Ok", null);
		}
		else if (flag2)
		{
			if (num < league.LeagueLevel())
			{
				UIDialog.instance.ShowOk("League Complete!", "Congratulations! You finished in " + (num3 + 1) + " place! You have advanced to " + league.LeagueName(), "Ok", null);
			}
			else
			{
				UIDialog.instance.ShowOk("League Complete!", "You finished in " + (num3 + 1) + " place! Finish in top " + league.PlacesThatAdvance() + " to advance to the Next League!", "Ok", null);
			}
		}
	}

	private void OnEnable()
	{
		if (!(humanObject == null))
		{
			LeagueListItem component = humanObject.GetComponent<LeagueListItem>();
			if (!(component == null))
			{
				LeagueController instance = LeagueController.instance;
				LeagueMemberDAO leagueMemberDAO = instance.HumanPlayer();
				component.name.text = leagueMemberDAO.name;
				GameConstants.SetFlag(component.flag, leagueMemberDAO.flag);
				Ads.instance.hideBanner(hideBanner: false);
			}
		}
	}

	private void SetLabels()
	{
		infoLabel.text = league.LeagueName() + ", Round " + (league.CurrentMatch() + 1) + "/" + league.TotalMatchesForLeague();
		LeagueMemberDAO leagueMemberDAO = league.NextOpponent();
		nextMatchLabel.text = "Next Match: " + league.HumanPlayer().name + " VS " + leagueMemberDAO.name;
		UpdateButton();
	}

	private void UpdateButton()
	{
		string text = "Play";
		if (!league.isNextMatchActive())
		{
			text = "Next Match in\n" + GGFormat.FormatTimeSpan(league.TimeTillNextMatchActive());
		}
		UITools.ChangeText(buttonLabel, text);
		UITools.ChangeSprite(button, (!league.isNextMatchActive()) ? "btn-red" : "btn-green");
	}

	private IEnumerator LateStart()
	{
		yield return null;
		table.Reposition();
	}

	public new void Update()
	{
		UpdateButton();
		base.Update();
	}

	public void OnPlay()
	{
		if (league.isNextMatchActive())
		{
			LeagueMemberDAO opponent = league.NextOpponent();
			league.AdvanceToNextRound();
			ScreenNavigation.instance.LoadLeagueMatch(opponent);
		}
	}
}

using ProtoModels;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UIWinDialog : MonoBehaviour
{
	public delegate void WinDialogDelegate();

	public GameObject dialog;

	public GameObject winDialogCareer;

	public GameObject winDialogLeague;

	public UILabel header;

	public List<UISprite> stars = new List<UISprite>();

	public UILabel dominate;

	public UILabel opponentName;

	public UISprite opponentFlag;

	public UILabel careerText;

	public UILabel leagueText;

	public UILabel okText;

	public UILabel leadScore;

	public UILabel leadMessage;

	public UILabel leadWonScore;

	public UILabel countryPoints;

	public GameObject countryPointsGameObject;

	public GameObject shareButton;

	public UILabel shareLabel;

	private int startScore;

	private int endScore;

	private float time;

	private float duration;

	private bool animateScoreActive;

	private UILabel fadeOutLabel;

	private UILabel fadeInLabel;

	private bool activateCrossFadeLabels;

	private float crossTime;

	private float crossFadeDuration;

	private float crossInitialDelay;

	private float crossDelay;

	protected WinDialogDelegate onOk;

	protected string shareTitle;

	protected string shareSubtitle;

	protected string shareText;

	protected bool shareIsGroupPassed;

	protected void Show(GameObject dialogObj)
	{
		dialog.SetActive(value: true);
		winDialogCareer.SetActive(dialogObj == winDialogCareer);
		winDialogLeague.SetActive(dialogObj == winDialogLeague);
	}

	public void HideShareButton()
	{
		shareButton.SetActive(value: false);
	}

	public void ShowShareButton(string title, string subtitle, string text, bool isGroupPassed = false)
	{
		if (!GGFacebook.instance.isAvailable())
		{
			HideShareButton();
			return;
		}
		shareIsGroupPassed = isGroupPassed;
		shareTitle = title;
		shareSubtitle = subtitle;
		shareText = text;
		shareLabel.text = GGFormat.RandomFrom("Share This!", "Let Friends Know!", "Brag!", "Share");
		shareButton.SetActive(value: true);
	}

	public void ShowTournamentDialog(Tournament.TournamentResult result, WinDialogDelegate onComplete)
	{
		onOk = onComplete;
		Tournament tournament = result.tournament;
		Show(winDialogLeague);
		if (tournament.isTournamentComplete())
		{
			int num = tournament.humanRanking();
			if (num < 2)
			{
				header.text = "Congradulations on the Won Medal!";
			}
			else
			{
				header.text = "Tournament Complete";
			}
			ShowShareButton(num + 1 + " Place in " + TournamentController.tournamentName((TournamentController.Tournaments)tournament.TournamentType()), "Can you beat my score!", "Just Won " + (num + 1) + " Place in " + TournamentController.tournamentName((TournamentController.Tournaments)tournament.TournamentType()));
		}
		else
		{
			header.text = "Next " + tournament.NameForCurrentRound();
			shareButton.SetActive(value: false);
		}
		if (tournament.isTournamentComplete())
		{
			int position = tournament.humanRanking();
			leagueText.text = "Tournament Done! " + TournamentController.NameForPosition(position);
		}
		else
		{
			leagueText.text = "Next " + tournament.NameForCurrentRound();
		}
		okText.text = "Ok";
		UITools.ChangeText(leadWonScore, "+" + result.addedScore.ToString() + " Exp");
		animateScore(result.startingScore, result.startingScore + result.addedScore, 0.5f, 0.5f);
		ShowCountryScore(result.addedCountryScore);
	}

	public void ShowCountryScore(int countryScore)
	{
		if (countryScore <= 0)
		{
			countryPointsGameObject.SetActive(value: false);
			return;
		}
		countryPointsGameObject.SetActive(value: true);
		countryPoints.text = "Country Points: +" + countryScore;
	}

	public void ShowLeagueDialog(LeagueController.LeagueGameResult result, WinDialogDelegate onComplete)
	{
		bool isWin = result.isWin;
		int ballsWon = result.ballsWon;
		onOk = onComplete;
		Show(winDialogLeague);
		header.text = ((!isWin) ? "You Lost!" : "You Win!");
		string empty = string.Empty;
		LeagueController instance = LeagueController.instance;
		LeagueMemberDAO member = instance.HumanPlayer();
		int num = instance.RankForMember(member);
		empty = ((!isWin) ? (empty + GGFormat.RandomFrom("It was close!", "Better luck next Time!", "No matter, you'll get the next one!", "Still plenty of chance to improve!")) : (empty + GGFormat.RandomFrom("Great Game!", "This is an Important Victory!", "You are showing good potential!")));
		string text = empty;
		empty = text + " You are " + (num + 1) + " in the League. You win +" + ballsWon + " (Ball)";
		leagueText.text = empty;
		okText.text = "Next";
		UITools.ChangeText(leadWonScore, "+" + result.addedScore.ToString() + " Exp");
		animateScore(result.startingScore, result.startingScore + result.addedScore, 0.5f, 0.5f);
		ShowShareButton(num + 1 + " Place in the League", "Can you beat my score!", ((!isWin) ? "Played" : "Victory") + " against " + opponentName.text + " for " + (num + 1) + " place in the League!");
		ShowCountryScore(result.addedCountryScore);
	}

	public void ShowCareerDialog(CareerGameMode.GameComplete outcome, WinDialogDelegate onComplete)
	{
		int num = outcome.stars;
		CareerGameMode.CareerGroup group = outcome.opponent.group;
		CareerGameMode.CareerPlayer opponent = outcome.opponent;
		bool isGroupPassed = outcome.isGroupPassed;
		UITools.ChangeText(leadWonScore, "+" + outcome.addedScore.ToString() + " Exp");
		onOk = onComplete;
		Show(winDialogCareer);
		bool flag = num > 0;
		header.text = ((!flag) ? "You Lost!" : "You Win!");
		PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(opponent.playerDef);
		GameConstants.SetFlag(opponentFlag, (int)playerDef.flag);
		opponentName.text = playerDef.name;
		string text = string.Empty;
		okText.text = "Ok";
		if (isGroupPassed)
		{
			text = group.name + " Passed!";
			if (group.prizes.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (CareerGameMode.CareerPrize prize in group.prizes)
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append(" and ");
					}
					stringBuilder.Append(prize.name);
				}
				stringBuilder.Append("!");
				text = text + " You Unlocked " + stringBuilder.ToString();
				if (group.prizes.Count > 1)
				{
					okText.text = "Unlock Won Items";
				}
				else
				{
					okText.text = "Unlock " + group.GetPrizeName();
				}
			}
		}
		else
		{
			switch (num)
			{
			case 0:
				text = GGFormat.RandomFrom("You'll get him the next time!", "Better luck next time!", "Your game looked promising, you can do it!") + " Win with " + opponent.threeStars + " pts difference to Dominate!";
				break;
			case 1:
				text = GGFormat.RandomFrom("Good Game, you are on your way to Dominate!", "You are a skilled player, soon you'll be a star!", "You show potential, you are ready to Dominate!") + " Win with " + opponent.threeStars + " pts difference to Dominate!";
				break;
			case 2:
				text = GGFormat.RandomFrom("Great Win! You are so Close To Dominating!", "You are great to watch! Just a little bit more to Dominate!", "Such great potential in your game! You can Dominate!") + " Win with " + opponent.threeStars + " pts difference to Dominate!";
				break;
			case 3:
				text = GGFormat.RandomFrom("What a supreme victory... This was some great playing!", "You did it! Such a beautiful game, you opponent did not stand a chance!", "The crowd is estatic at your glorious win! Bravo!!!");
				break;
			}
		}
		careerText.text = text;
		StartCoroutine(ShowStars(num));
		animateScore(outcome.startingScore, outcome.startingScore + outcome.addedScore, 0.5f, (float)outcome.stars * 0.5f + 0.5f);
		if (flag)
		{
			if (outcome.isGroupPassed && outcome.opponent.group.prizes.Count > 0)
			{
				ShowShareButton("Unlocked " + outcome.opponent.group.prizes[0].name, string.Empty, "Great Victory against " + playerDef.name + " to unlock " + outcome.opponent.group.prizes[0].name, isGroupPassed: true);
			}
			else
			{
				ShowShareButton("Dominated " + playerDef.name, string.Empty, "Just Dominated " + playerDef.name + " in my Badminton 3D Career! Try and beat my score.");
			}
		}
		else
		{
			HideShareButton();
		}
		ShowCountryScore(outcome.addedCountryScore);
	}

	public void animateScore(int from, int to, float duration, float delay)
	{
		this.duration = duration;
		startScore = from;
		endScore = to;
		UITools.ChangeText(leadScore, from.ToString() + " Exp");
		StartCoroutine(DoAnimateScore(from != to, delay));
	}

	private IEnumerator DoAnimateScore(bool animate, float delay)
	{
		yield return new WaitForSeconds(delay);
		animateScoreActive = animate;
	}

	private void Update()
	{
		if (animateScoreActive)
		{
			time += Time.deltaTime;
			float num = time / duration;
			UITools.ChangeText(text: ((int)Mathf.Lerp(startScore, endScore, num)).ToString() + " Exp", label: leadScore);
			if (num > 1f)
			{
				animateScoreActive = false;
				float num2 = 1.5f;
				leadScore.cachedTransform.localScale = new Vector3(num2, num2, 1f);
				TweenScale.Begin(leadScore.cachedGameObject, 0.2f, Vector3.one);
				activateCrossFadeLabels = true;
				crossInitialDelay = 0.5f;
				crossDelay = 1.5f;
				crossFadeDuration = 0.5f;
				fadeInLabel = leadMessage;
				fadeOutLabel = leadScore;
			}
		}
		if (activateCrossFadeLabels)
		{
			if (fadeInLabel == leadMessage && crossTime == 0f)
			{
				fadeInLabel.text = GGFormat.RandomFrom("Leaderboards", "World Rank", "World Position", "Compare your Score!");
			}
			crossTime += Time.deltaTime;
			float t = (crossTime - crossInitialDelay) / crossFadeDuration;
			float num3 = Mathf.Lerp(0f, 1f, t);
			fadeInLabel.alpha = num3;
			fadeOutLabel.alpha = 1f - num3;
			if (crossTime > crossInitialDelay + crossFadeDuration + crossDelay)
			{
				crossTime = 0f;
				UILabel uILabel = fadeInLabel;
				fadeInLabel = fadeOutLabel;
				fadeOutLabel = uILabel;
			}
		}
	}

	private IEnumerator ShowStars(int numStars)
	{
		foreach (UISprite star2 in stars)
		{
			star2.color = Color.black;
		}
		bool isWin = numStars > 0;
		float duration = 0.15f;
		float delay = (!isWin) ? 0.05f : 0.3f;
		yield return new WaitForSeconds(delay);
		int starsCount = (!isWin) ? 3 : numStars;
		for (int i = 0; i < starsCount; i++)
		{
			UISprite star = stars[i];
			if (i < numStars)
			{
				star.color = Color.white;
				star.cachedTransform.localScale = new Vector3(2f, 2f, 1f);
			}
			else
			{
				star.color = Color.black;
				star.cachedTransform.localScale = new Vector3(1.3f, 1.3f, 1f);
			}
			TweenScale.Begin(star.cachedGameObject, duration, Vector3.one);
			yield return new WaitForSeconds(duration + delay);
		}
		if (numStars >= 3)
		{
			dominate.cachedGameObject.SetActive(value: true);
			dominate.cachedTransform.localScale = new Vector3(2f, 2f, 1f);
			Color color = dominate.color;
			color.a = 0f;
			dominate.color = color;
			TweenScale.Begin(dominate.cachedGameObject, duration, Vector3.one);
			TweenAlpha.Begin(dominate.cachedGameObject, duration, 1f);
		}
	}

	public void OnShare()
	{
		GGFacebook.instance.showShareDialog("Badminton 3D", shareTitle, shareSubtitle, shareText, GGSupportMenu.instance.appUrl(ConfigBase.instance.rateProvider, webFormat: true));
		Analytics.instance.shareFromWinDialog(MatchController.InitParameters.gameMode, shareIsGroupPassed);
	}

	public void OnOk()
	{
		dialog.SetActive(value: false);
		if (onOk != null)
		{
			onOk();
		}
	}

	public void OnLeaderboard()
	{
		SocialAuthentication.ShowLeaderboard();
	}
}

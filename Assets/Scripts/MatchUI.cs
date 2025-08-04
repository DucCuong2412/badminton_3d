using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchUI : MonoBehaviour
{
	public delegate void OnStringDelegate(string message);

	public delegate void OnCompleteDelegate();

	public delegate void OnFinishDelegate();

	[Serializable]
	public class MultiplayerShowdown
	{
		public UILabel wins;

		public UILabel winPercent;

		public UILabel score;
	}

	public class CameraSetup
	{
		public Vector3 up1;

		public Vector3 up2;

		public float camRotation;
	}

	public List<AudioClip> scoreAudio0 = new List<AudioClip>();

	public List<AudioClip> scoreAudio15 = new List<AudioClip>();

	public List<AudioClip> scoreAudio30 = new List<AudioClip>();

	public List<AudioClip> scoreAudio40 = new List<AudioClip>();

	public AudioClip advantageAudio;

	public AudioClip netAudio;

	public AudioClip foulAudio;

	public AudioClip outAudio;

	public AudioClip gameAudio;

	public AudioClip setAudio;

	public AudioClip matchAudio;

	public List<UIPlayerScoreBoard> scoreBoards = new List<UIPlayerScoreBoard>();

	public List<UIPlayerScoreBoard> splitScreenScoreBoards = new List<UIPlayerScoreBoard>();

	public UITimingSlider timingSlider;

	public UINotification notification;

	public List<UINotification> textNotification = new List<UINotification>();

	public UIWinDialog winDialog;

	public UIHand hand;

	public GameObject pauseButton;

	public UILabel qualityLabel;

	public Color qualityGreatCol;

	public Color qualityGoodCol;

	public Color qualityBadCol;

	protected float qualityTime;

	public float qualityDuration = 3f;

	public float qualityFade = 0.2f;

	public GameObject ballMarker;

	public UIVertTimingSlider vertTimingSlider;

	public UISprite pointer;

	public List<MultiplayerShowdown> multVSUI = new List<MultiplayerShowdown>();

	public GameObject multVSUIGameObject;

	public EffectManager textEffect;

	public EffectManager textEffectCam2;

	public UILabel fps;

	public GameObject sparkParticle;

	public TrailRenderer trail;

	public TrailRenderer fingerTrail;

	protected int numWrongMoves;

	public UILabel alertText;

	public GameObject singlesGUI;

	public GameObject singlesUI;

	public GameObject splitScreenUI;

	public GameObject cameraButton;

	protected List<UISprite> pointers = new List<UISprite>();

	protected List<UISprite> helpPointers = new List<UISprite>();

	public GameObject vsGameObject;

	public UISprite vsFlag1;

	public UISprite vsFlag2;

	public UILabel vsName1;

	public UILabel vsName2;

	public UILabel vsText;

	public GameObject vsButton;

	public SpinMarker spinMarker;

	public GameObject messageButtons;

	public UILabel goodButtonLabel;

	public UILabel badButtonLabel;

	public static CameraSetup[] cameraSetups = new CameraSetup[3]
	{
		new CameraSetup
		{
			up1 = Vector3.left,
			up2 = Vector3.left,
			camRotation = 90f
		},
		new CameraSetup
		{
			up1 = Vector3.up,
			up2 = Vector3.down,
			camRotation = 0f
		},
		new CameraSetup
		{
			up1 = Vector3.up,
			up2 = Vector3.up,
			camRotation = 0f
		}
	};

	public int cameraSetup;

	protected MatchController match;

	public event OnCompleteDelegate onPause;

	public event OnCompleteDelegate onResume;

	public event OnCompleteDelegate onRateComplete;

	public event OnCompleteDelegate onVs;

	public event OnStringDelegate onMessageButton;

	public Color ColorForQuality(int qualityIndex)
	{
		Color white = Color.white;
		switch (qualityIndex)
		{
		case 0:
			white = qualityGreatCol;
			break;
		case 1:
			white = qualityGoodCol;
			break;
		default:
			white = qualityBadCol;
			break;
		}
		white.a = 1f;
		return white;
	}

	public void ShowQualityText(float penalty, Vector3 screenPosition, int qualityIndex, bool isOnTable, float spinX)
	{
		qualityTime = 0f;
		float num = Mathf.Abs(penalty);
		Color value = qualityBadCol;
		string empty = string.Empty;
		if (qualityIndex < 2)
		{
			numWrongMoves = 0;
		}
		else
		{
			numWrongMoves++;
		}
		switch (qualityIndex)
		{
		case 0:
			empty = "Great";
			value = qualityGreatCol;
			break;
		case 1:
			empty = "Good";
			value = qualityGoodCol;
			break;
		default:
			empty = ((!(penalty < 0f)) ? "Late" : "Early");
			value = qualityBadCol;
			if (!match.isInTutorial && numWrongMoves >= 2)
			{
				empty = ((!(penalty < 0f)) ? GGFormat.RandomFrom("Swing Sooner", "Start Swing Sooner") : GGFormat.RandomFrom("Swing Later", "Wait before swing"));
				numWrongMoves = 0;
			}
			break;
		}
		if (!isOnTable)
		{
			empty = "Wide";
		}
		if (MatchController.InitParameters.gameMode != MatchController.GameMode.SplitScreen)
		{
			UITools.SetScreenPosition(qualityLabel, screenPosition);
			qualityLabel.text = empty;
			qualityLabel.color = Color.white;
			qualityLabel.alpha = 1f;
			qualityLabel.cachedGameObject.SetActive(value: true);
		}
		value.a = 1f;
		trail.material.SetColor("_Color", value);
		showSpin(spinX);
	}

	public void showSpin(float spinX)
	{
		if (spinX == 0f)
		{
			spinMarker.myTrailRenderer.enabled = false;
			return;
		}
		spinMarker.myTrailRenderer.enabled = true;
		spinMarker.myTrailRenderer.material.SetColor("_Color", trail.material.color);
	}

	public void Init(MatchController match, string player1Name, int player1Flag, string player2Name, int player2Flag)
	{
		this.match = match;
		if (spinMarker != null)
		{
			spinMarker.match = match;
		}
		scoreBoards[0].SetNameAndFlag(player1Name, player1Flag);
		scoreBoards[1].SetNameAndFlag(player2Name, player2Flag);
		GameConstants.SetFlag(vsFlag1, player1Flag);
		UITools.ChangeText(vsName1, player1Name);
		GameConstants.SetFlag(vsFlag2, player2Flag);
		UITools.ChangeText(vsName2, player2Name);
		vsGameObject.SetActive(value: false);
		if (MatchController.InitParameters.gameMode == MatchController.GameMode.SplitScreen)
		{
			singlesUI.SetActive(value: false);
			splitScreenUI.SetActive(value: true);
			UISprite item = CreatePointer();
			pointers.Add(item);
			helpPointers.Add(CreatePointer());
			textEffectCam2.gameObject.SetActive(value: true);
		}
		else if (MatchController.InitParameters.gameMode == MatchController.GameMode.Tutorial)
		{
			splitScreenUI.SetActive(value: false);
			singlesUI.SetActive(value: true);
			singlesGUI.SetActive(value: false);
			pauseButton.SetActive(value: false);
		}
		else
		{
			splitScreenUI.SetActive(value: false);
			singlesUI.SetActive(value: true);
		}
		pointers.Add(pointer);
		helpPointers.Add(CreatePointer());
	}

	public void SetTextToPlayer(int tag, string text, float delay)
	{
		UINotification uINotification = textNotification[tag];
		PlayerBase playerBase = match.PlayerForIndex(tag);
		uINotification.ShowOnTransform(playerBase.myTransform, new Vector3(0f, 1f, 0f), text, delay);
	}

	private UISprite CreatePointer()
	{
		UISprite component = NGUITools.AddChild(pointer.cachedTransform.parent.gameObject, pointer.cachedGameObject).GetComponent<UISprite>();
		component.cachedTransform.localScale = pointer.cachedTransform.localScale;
		component.cachedGameObject.SetActive(value: false);
		return component;
	}

	public void ShowVS(int wins1, int score1, float winPercent1, int wins2, int score2, float winPercent2, bool winPercent2Known)
	{
		MultiplayerShowdown multiplayerShowdown = multVSUI[0];
		MultiplayerShowdown multiplayerShowdown2 = multVSUI[1];
		UITools.ChangeText(multiplayerShowdown.wins, wins1.ToString());
		UITools.ChangeText(multiplayerShowdown2.wins, wins2.ToString());
		UITools.ChangeText(multiplayerShowdown.score, score1.ToString());
		UITools.ChangeText(multiplayerShowdown2.score, score2.ToString());
		UITools.ChangeText(multiplayerShowdown.winPercent, GGFormat.FormatPercent(winPercent1) + "%");
		if (winPercent2Known)
		{
			UITools.ChangeText(multiplayerShowdown2.winPercent, GGFormat.FormatPercent(winPercent2) + "%");
		}
		else
		{
			UITools.ChangeText(multiplayerShowdown2.winPercent, "?");
		}
		multVSUIGameObject.SetActive(value: true);
		ShowVS(string.Empty);
	}

	public void ShowVS(string text)
	{
		vsGameObject.SetActive(value: true);
		vsText.text = text;
		StartCoroutine(StartVSClick());
	}

	public void HideVS()
	{
		vsGameObject.SetActive(value: false);
	}

	private IEnumerator StartVSClick()
	{
		yield return new WaitForSeconds(0.5f);
		vsButton.SetActive(value: true);
	}

	public void OnVSClick()
	{
		vsGameObject.SetActive(value: false);
		if (this.onVs != null)
		{
			this.onVs();
		}
	}

	public void SetCameraSetup(int index)
	{
		this.cameraSetup = index % cameraSetups.Length;
		CameraSetup cameraSetup = cameraSetups[this.cameraSetup];
		CameraController cameraController = match.cameras[0];
		cameraController.up = cameraSetup.up1;
		cameraController.RefreshUp();
		if (match.cameras.Count > 1)
		{
			CameraController cameraController2 = match.cameras[1];
			cameraController2.up = cameraSetup.up2;
			cameraController2.RefreshUp();
		}
		TweenRotation.Begin(cameraButton, 0.25f, Quaternion.Euler(0f, 0f, cameraSetup.camRotation));
	}

	public UISprite getPointer(PlayerBase player)
	{
		if (pointers.Count == 0)
		{
			return pointer;
		}
		return pointers[player.playerTag % pointers.Count];
	}

	public UISprite getHelpPointer(PlayerBase player)
	{
		if (helpPointers.Count == 0)
		{
			return pointer;
		}
		return helpPointers[player.playerTag % helpPointers.Count];
	}

	public void HideNotification()
	{
		notification.Hide();
	}

	public void ShowNotificationBelow(string text, Vector3 worldPos, bool animate = false)
	{
		notification.ShowOnWorld(worldPos, text);
	}

	public void ScoreStringForPoints(int server, TableTennisScores scores, out string p1, out string p2)
	{
		int num = scores.ScoreForPlayer(server);
		int num2 = scores.ScoreForPlayer((server + 1) % 2);
		p1 = num.ToString();
		p2 = num2.ToString();
	}

	public AudioClip ScoreAudio(int pt1, int pt2)
	{
		if (pt1 == 0 && pt2 == 0)
		{
			return null;
		}
		if (pt1 >= 3 && pt2 >= 3)
		{
			if (pt1 == pt2)
			{
				return scoreAudio40[3];
			}
			return advantageAudio;
		}
		List<AudioClip> list = null;
		if (pt1 > 3 || pt2 > 3)
		{
			return null;
		}
		switch (pt1)
		{
		case 0:
			list = scoreAudio0;
			break;
		case 1:
			list = scoreAudio15;
			break;
		case 2:
			list = scoreAudio30;
			break;
		default:
			list = scoreAudio40;
			break;
		}
		return list[pt2];
	}

	protected string scoreForPoint(int pt1)
	{
		switch (pt1)
		{
		case 3:
			return "40";
		case 2:
			return "30";
		case 1:
			return "15";
		default:
			return "0";
		}
	}

	public void UpdateServe(int servingPlayerTag)
	{
		List<UIPlayerScoreBoard> list = (MatchController.InitParameters.gameMode != MatchController.GameMode.SplitScreen) ? scoreBoards : splitScreenScoreBoards;
		list[0].SetServing(servingPlayerTag == 0);
		list[1].SetServing(servingPlayerTag == 1);
	}

	public void UpdateScoreBoard(TableTennisScores scores)
	{
		UpdateScoreBoard(scores, scores.GamesForPlayer(0), scores.GamesForPlayer(1));
	}

	public void UpdateScoreBoard(TableTennisScores scores, int games1, int games2)
	{
		ScoreStringForPoints(0, scores, out string p, out string p2);
		List<UIPlayerScoreBoard> list = (MatchController.InitParameters.gameMode != MatchController.GameMode.SplitScreen) ? scoreBoards : splitScreenScoreBoards;
		list[0].SetScore(games1, p);
		list[1].SetScore(games2, p2);
	}

	public void showText(string text, EffectManager.OnAnimationFinish onFinish)
	{
		textEffect.SetText(text);
		textEffect.PlayAnimation(onFinish);
		if (textEffectCam2 != null && textEffectCam2.gameObject.activeSelf)
		{
			textEffectCam2.SetText(text);
			textEffectCam2.PlayAnimation();
		}
	}

	public void showTextsForPlayers(string textPlayer1, string textPlayer2, EffectManager.OnAnimationFinish onFinish)
	{
		textEffect.SetText(textPlayer1);
		textEffect.PlayAnimation(onFinish);
		textEffectCam2.SetText(textPlayer2);
		textEffectCam2.PlayAnimation();
	}

	public void showRateLayer()
	{
	}

	public void ShowLeagueGameDoneDialog(int stars, int coins, int prevRank, int rank)
	{
	}

	public void ShowGameDoneDialog(int prevStars, int numStars, int coins)
	{
	}

	public void ShowTournamentGameDoneDialog(TournamentController tournament, int coins, int prevStars, int numStars)
	{
	}

	public void ShowTiming(float value, Vector3 screenPos, int qualityIndex, bool inTable, float spinX)
	{
		timingSlider.SetPosition(value);
		ShowQualityText(value, screenPos, qualityIndex, inTable, spinX);
	}

	private void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && pauseButton.activeSelf && !match.isGameDone)
		{
			showPauseMenu();
		}
		if (qualityLabel.cachedGameObject.activeSelf)
		{
			qualityTime += Time.deltaTime;
			qualityLabel.alpha = Mathf.Lerp(1f, 0f, qualityTime - qualityDuration + qualityFade);
			if (qualityTime > qualityDuration)
			{
				qualityLabel.cachedGameObject.SetActive(value: false);
			}
		}
		if (fps != null)
		{
			fps.text = ((int)(1f / RealTime.deltaTime)).ToString();
		}
	}

	public void showPauseMenu()
	{
		if (this.onPause != null)
		{
			this.onPause();
		}
		UIDialog.instance.ShowYesNo("Paused", "Resume Game?", "Resume", "Menu\n(Forfeit Game)", OnPauseMenu);
	}

	private void OnPauseMenu(bool success)
	{
		if (!success)
		{
			match.PlayerWantsToExit();
		}
		NavigationManager.instance.Pop(force: true);
		if (this.onResume != null)
		{
			this.onResume();
		}
	}

	private void hidePauseMenu()
	{
	}

	public void OnChangeCameraView()
	{
		SetCameraSetup(cameraSetup + 1);
	}

	public void showMessageButtons(string good, string bad)
	{
		messageButtons.SetActive(value: true);
		goodButtonLabel.text = good;
		badButtonLabel.text = bad;
	}

	public void hideMessageButtons()
	{
		messageButtons.SetActive(value: false);
	}

	public void OnGoodButton()
	{
		if (this.onMessageButton != null)
		{
			this.onMessageButton(goodButtonLabel.text);
		}
	}

	public void OnBadButton()
	{
		if (this.onMessageButton != null)
		{
			this.onMessageButton(badButtonLabel.text);
		}
	}
}

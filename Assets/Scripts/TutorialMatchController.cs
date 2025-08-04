using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMatchController : MatchController
{
	[Serializable]
	public class TableMarkerSetup
	{
		public float x;

		public float tickness;
	}

	public enum TutorialStep
	{
		ReturnShotStep,
		StepSuccess,
		StepHitMarker,
		StepFailed,
		TimingLearnStep,
		SpinReturnStep,
		AccuracyLearnStep,
		ServeStep
	}

	public HumanPlayer human;

	public GameObject crowdObject;

	public GameObject umpireObject;

	public TableMarker tableMarker;

	public List<TableMarkerSetup> markerSetups = new List<TableMarkerSetup>();

	protected int markerSetupIndex;

	private TutorialStep step;

	private int goodShots;

	private int allShots;

	private float timeToRemoveText;

	public UILabel testLabel;

	private bool speedUpActive;

	private float speedUpTime;

	private float speedUpDuration = 1f;

	private float speedUpTimeScale;

	private bool canSkipTutorial = true;

	private float lastTimeScale;

	public TutorialPlayer tutorialPlayer
	{
		get;
		protected set;
	}

	public override void ShowDebug(string text)
	{
		testLabel.text = text;
		testLabel.cachedTransform.localScale = new Vector3(1.1f, 1.1f, 0f);
		TweenScale.Begin(testLabel.cachedGameObject, 0.2f, Vector3.one);
	}

	private new void Awake()
	{
		base.Awake();
		GameObject gameObject = UnityEngine.Object.Instantiate(characterPrefab);
		human = gameObject.AddComponent<HumanPlayer>();
		human.Init(this, players.Count, -1, cameraController, null);
		players.Add(human);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(characterPrefab);
		tutorialPlayer = gameObject2.AddComponent<TutorialPlayer>();
		tutorialPlayer.Init(this, players.Count, 1);
		players.Add(tutorialPlayer);
		testLabel.cachedGameObject.SetActive(value: true);
		tutorialPlayer.TakeBall(base.ball);
		Analytics.instance.ReportTutorial("Start");
		crowdObject.SetActive(value: false);
		umpireObject.SetActive(value: false);
	}

	private void Start()
	{
		ui.Init(this, string.Empty, 0, string.Empty, 0);
		foreach (PlayerBase player in players)
		{
			player.OnGameStart();
		}
		StartCoroutine(Tutorial());
	}

	public override void ShowTutorialNotification(string text)
	{
		ui.notification.ShowOnTransform(tutorialPlayer.transform, new Vector3(0f, 0f, 0f), text);
	}

	private IEnumerator Tutorial()
	{
		ShowTutorialNotification("Hi " + CareerBackend.instance.Name() + "! I'm your Trainer");
		yield return new WaitForSeconds(2f);
		ShowTutorialNotification("I'll show you how to play this game in a short tutorial!");
		yield return new WaitForSeconds(2f);
		step = TutorialStep.ReturnShotStep;
		StartServe(tutorialPlayer);
		human.StartCoroutine(human.ReturnShotTutorial());
		while (step != TutorialStep.StepSuccess)
		{
			if (step == TutorialStep.StepFailed)
			{
				step = TutorialStep.ReturnShotStep;
				ShowTutorialNotification("Try it again! Aim for the center of the court!");
				yield return new WaitForSeconds(1f);
				StartServe(tutorialPlayer);
				human.StartCoroutine(human.ReturnShotTutorial());
			}
			else
			{
				if (base.numShotsInPoint >= 4)
				{
					tutorialPlayer.canReturnBall = false;
				}
				yield return null;
			}
		}
		step = TutorialStep.TimingLearnStep;
		ShowTutorialNotification("Excellent!");
		yield return new WaitForSeconds(2f);
		ShowTutorialNotification("Next, let's learn timing!");
		yield return new WaitForSeconds(2f);
		ShowTutorialNotification("Timing your swipe is critical for a fast and long shot!");
		yield return new WaitForSeconds(2f);
		ShowTutorialNotification("Try to return a couple of well timed shots!");
		yield return new WaitForSeconds(2f);
		StartServe(tutorialPlayer);
		tutorialPlayer.canReturnBall = true;
		int numGoodShotsToPass = 7;
		int numAllShotsToPass = 20;
		while (true)
		{
			bool isPassed = goodShots >= numGoodShotsToPass || allShots >= numAllShotsToPass;
			if (isPassed)
			{
				tutorialPlayer.canReturnBall = false;
			}
			if (step == TutorialStep.StepSuccess || step == TutorialStep.StepFailed)
			{
				if (isPassed)
				{
					break;
				}
				if (allShots == 0)
				{
					ShowTutorialNotification("Swipe to Shoot the Ball!");
				}
				else
				{
					ShowTutorialNotification("Doing good, return some more good shots!");
				}
				yield return new WaitForSeconds(1f);
				StartServe(tutorialPlayer);
				step = TutorialStep.TimingLearnStep;
			}
			yield return null;
		}
		tutorialPlayer.canReturnBall = false;
		ShowTutorialNotification("Excellent!");
		yield return new WaitForSeconds(2f);
		step = TutorialStep.AccuracyLearnStep;
		ShowTutorialNotification("Try to hit the yellow markers!");
		markerSetupIndex = 0;
		TableMarkerSetup curSetup = markerSetups[markerSetupIndex];
		tableMarker.SetToTable(curSetup.x, curSetup.tickness, -human.tableSide, table);
		tableMarker.SetColor(Color.yellow);
		tableMarker.gameObject.SetActive(value: true);
		yield return new WaitForSeconds(2f);
		tutorialPlayer.canReturnBall = true;
		tutorialPlayer.maxReturns = 0;
		StartServe(tutorialPlayer);
		while (step != TutorialStep.StepSuccess)
		{
			if (step == TutorialStep.StepFailed)
			{
				StartServe(tutorialPlayer);
			}
			step = TutorialStep.AccuracyLearnStep;
			yield return null;
		}
		tutorialPlayer.maxReturns = -1;
		tutorialPlayer.canReturnBall = false;
		tableMarker.gameObject.SetActive(value: false);
		ShowTutorialNotification("Excellent!");
		yield return new WaitForSeconds(2.5f);
		ShowTutorialNotification("Now let's try the serve");
		yield return new WaitForSeconds(2f);
		step = TutorialStep.ServeStep;
		human.StartCoroutine(human.ServeTutorial());
		StartServe(human);
		tutorialPlayer.canReturnBall = true;
		while (step != TutorialStep.StepSuccess)
		{
			if (step == TutorialStep.StepFailed)
			{
				ShowTutorialNotification("Try to hit my side of the court!");
				yield return new WaitForSeconds(2f);
				step = TutorialStep.ServeStep;
				human.StartCoroutine(human.ServeTutorial());
				StartServe(human);
			}
			yield return null;
		}
		ShowTutorialNotification("Excellent! You are ready for your first opponent!");
		yield return new WaitForSeconds(1f);
		canSkipTutorial = false;
		OnTutorialComplete();
		UIDialog.instance.ShowOk("Tutorial Complete", "Awesome training! You are now ready to face your training partner! Good Luck!", "Play", OnComplete);
		Analytics.instance.ReportTutorial("End");
	}

	private void OnComplete(bool success)
	{
		MatchParameters initParameters = MatchController.InitParameters;
		initParameters.gameMode = initParameters.originalGameMode;
		ScreenNavigation.instance.LoadMatch(initParameters);
	}

	public override void ShowTiming(float value, Vector3 screenPos, int qualityIndex, bool inTable, float spinX)
	{
		if (step == TutorialStep.TimingLearnStep)
		{
			int num = goodShots;
			switch (qualityIndex)
			{
			case 0:
				goodShots += 2;
				ShowDebug("Great Timing");
				break;
			case 1:
				goodShots++;
				ShowDebug("Good Timing");
				break;
			default:
				ShowDebug((!(value < 0f)) ? "Late, Swipe Earlier" : "Early, Swipe Later");
				break;
			}
			if (num < 2 && goodShots >= 2)
			{
				ShowTutorialNotification("Great, Just a few more good shots!");
			}
			allShots++;
			timeToRemoveText = 3f;
			testLabel.color = ui.ColorForQuality(qualityIndex);
		}
		base.ShowTiming(value, screenPos, qualityIndex, inTable, spinX);
	}

	public override void OnPlayerHitBall(PlayerBase player, HitParams p)
	{
		base.OnPlayerHitBall(player, p);
	}

	protected override void RepeatServe(PlayerBase player)
	{
		base.RepeatServe(player);
		if (player == human)
		{
			step = TutorialStep.StepFailed;
		}
		else
		{
			base.StartServe(player);
		}
	}

	public IEnumerator DoChangeHitMarker()
	{
		ui.notification.ShowOnTransform(tutorialPlayer.myTransform, Vector3.zero, "Great!", 0.75f);
		yield return new WaitForSeconds(0.75f);
		TableMarkerSetup curSetup = markerSetups[markerSetupIndex];
		tableMarker.SetToTable(curSetup.x, curSetup.tickness, -human.tableSide, table);
		tableMarker.SetColor(Color.yellow);
	}

	public override void OnBallHitSurface(Ball ball, string hitTag, bool changeFlight)
	{
		if (step == TutorialStep.AccuracyLearnStep && hitTag == "Table" && tableMarker.isIn(ball.myTransform.position) && base.activePlayer == tutorialPlayer)
		{
			tableMarker.SetColor(Color.green);
			markerSetupIndex++;
			if (markerSetupIndex >= markerSetups.Count)
			{
				step = TutorialStep.StepSuccess;
			}
			else
			{
				StartCoroutine(DoChangeHitMarker());
			}
		}
		base.OnBallHitSurface(ball, hitTag, changeFlight);
	}

	protected override void AwardPointTo(PlayerBase player, FaultReason fault)
	{
		base.ballInGame = false;
		base.ball.isBallInGame = false;
		foreach (PlayerBase player2 in players)
		{
			player2.OnPointWon(player);
		}
		if (step == TutorialStep.AccuracyLearnStep || step == TutorialStep.StepHitMarker)
		{
			step = TutorialStep.StepFailed;
		}
		else if (step == TutorialStep.ReturnShotStep || step == TutorialStep.TimingLearnStep || step == TutorialStep.ServeStep || step == TutorialStep.SpinReturnStep)
		{
			if (player == human || base.numShotsInPoint > 2)
			{
				step = TutorialStep.StepSuccess;
			}
			else
			{
				step = TutorialStep.StepFailed;
			}
		}
		human.ShowPointer(show: false);
	}

	private void SpeedUp(float timeScale, float duration)
	{
		speedUpTimeScale = timeScale;
		speedUpDuration = duration;
		speedUpActive = true;
	}

	private void OnSkip(bool success)
	{
		base.isPaused = false;
		Time.timeScale = lastTimeScale;
		NavigationManager.instance.Pop(force: true);
		if (!success)
		{
			OnTutorialComplete();
			OnComplete(success: true);
			Analytics.instance.ReportTutorial("Skip");
		}
	}

	private new void Update()
	{
		if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && canSkipTutorial)
		{
			base.isPaused = true;
			lastTimeScale = Time.timeScale;
			Time.timeScale = 0f;
			UIDialog.instance.ShowYesNo("Skip Tutorial?", "Do you want to Skip tutorial and face your first opponent right now?", "Resume", "Skip", OnSkip);
		}
		base.Update();
		float num = timeToRemoveText;
		timeToRemoveText -= RealTime.deltaTime;
		if (num > 0f && timeToRemoveText <= 0f)
		{
			timeToRemoveText = 0f;
			testLabel.text = string.Empty;
		}
		if (speedUpActive)
		{
			speedUpTime += RealTime.deltaTime;
			float num2 = speedUpTime / speedUpDuration;
			Time.timeScale = Mathf.Lerp(speedUpTimeScale, timeScale, num2);
			if (num2 >= 1f)
			{
				speedUpActive = false;
			}
		}
	}
}

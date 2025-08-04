using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class MatchController : MonoBehaviour
{
	public enum GameMode
	{
		GameModeArcade,
		GameModeTournament,
		GameModeCareer,
		GameModeLeague,
		SplitScreen,
		Tutorial,
		Multiplayer
	}

	public enum MatchLocation
	{
		Europe,
		Asia
	}

	public class MatchParameters
	{
		public struct MultiplayerParams
		{
			public string opponentName;

			public int opponentFlag;

			public int serverTag;

			public int shoeIndex;

			public int racketIndex;

			public int score;

			public int multiplayerWins;

			public int lookIndex;

			public int multiplayerLoses;

			public bool multiplayerLosesKnown;
		}

		public CareerGameMode.CareerPlayer opponent;

		public GameMode originalGameMode;

		public Tournament tournament;

		public int opponentShoeIndex;

		public int opponentRacketIndex;

		public int serverTag;

		public int player1Score;

		public int player2Score;

		public MultiplayerParams multiplayerParams;

		public bool showTutorial;

		public int player1Flag;

		public int player2Flag = 3;

		public string player1Name = "You";

		public string player2Name = "AI";

		public GameMode gameMode = GameMode.GameModeCareer;

		public int numSetsInMatch = 1;

		public int numGamesInSet = 1;

		public int numPointsInGame = 11;

		protected int playerDefIndex_;

		public int playerDefIndex
		{
			get
			{
				return playerDefIndex_;
			}
			set
			{
				playerDefIndex_ = value;
				PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(playerDefIndex_);
				player2Flag = (int)playerDef.flag;
				player2Name = playerDef.name;
			}
		}

		public void SetMultiplayer(MultiplayerParams mp)
		{
			CareerBackend instance = CareerBackend.instance;
			player1Name = instance.Name();
			player1Flag = instance.Flag();
			player2Name = mp.opponentName;
			player2Flag = mp.opponentFlag;
			gameMode = GameMode.Multiplayer;
			opponentShoeIndex = mp.shoeIndex;
			serverTag = mp.serverTag;
			opponentRacketIndex = mp.racketIndex;
			player1Score = (player2Score = 0);
			multiplayerParams = mp;
			numSetsInMatch = 1;
			numGamesInSet = 1;
		}

		public void SetTournament(Tournament t)
		{
			tournament = t;
			numSetsInMatch = 1;
			numGamesInSet = t.numGamesInMatch;
			numPointsInGame = t.numPointsInGame;
			playerDefIndex = tournament.NextOpponent().difficulty;
			player1Name = CareerBackend.instance.Name();
			player1Flag = CareerBackend.instance.Flag();
			gameMode = GameMode.GameModeTournament;
		}

		public void SetCareer(CareerGameMode.CareerPlayer opponent)
		{
			this.opponent = opponent;
			gameMode = GameMode.GameModeCareer;
			CareerBackend instance = CareerBackend.instance;
			player1Name = instance.Name();
			player1Flag = instance.Flag();
			numGamesInSet = opponent.gamesInSet;
			numPointsInGame = opponent.pointsIngame;
			playerDefIndex = opponent.playerDef;
		}

		public void SetSplitScreen()
		{
			gameMode = GameMode.SplitScreen;
			numGamesInSet = 1;
			numPointsInGame = 11;
			player1Name = "Player 1";
			player2Name = "Player 2";
		}

		public void SetLeague(LeagueMemberDAO opponent)
		{
			LeagueController instance = LeagueController.instance;
			LeagueMemberDAO leagueMemberDAO = instance.HumanPlayer();
			gameMode = GameMode.GameModeLeague;
			player1Name = leagueMemberDAO.name;
			player1Flag = leagueMemberDAO.flag;
			numPointsInGame = instance.numPoints;
			numGamesInSet = instance.numGamesInSet;
			playerDefIndex = opponent.difficulty;
		}

		public void SetTutorial()
		{
			originalGameMode = gameMode;
			gameMode = GameMode.Tutorial;
		}

		public static MatchParameters CreateDefault()
		{
			return new MatchParameters();
		}
	}

	public enum BallHitEnum
	{
		Table,
		Net,
		Out
	}

	public enum FaultReason
	{
		Out,
		PointWon,
		Net
	}

	public bool TestHorizontalShots;

	public float normalCrowdLevel = 1f;

	public float quietCrowdLevel = 0.5f;

	protected static MatchParameters parameters;

	public bool isInTutorial;

	protected TableTennisScores score = new TableTennisScores();

	public AudioClip racketHit;

	public AudioSource ambience;

	public AudioSource crowd;

	protected float desiredCrowdVolume;

	protected float volumeSpeed;

	public AudioClip cheerGood;

	public AudioClip cheerBad;

	public AudioClip excitement;

	public int serveMistakes;

	public Table table;

	public GameObject ballPrefab;

	public GameObject characterPrefab;

	public CameraController cameraController;

	public PositionFollower trail;

	protected bool isStagePassed;

	protected List<PlayerBase> players = new List<PlayerBase>(2);

	public MatchUI ui;

	public float timeScale = 0.8f;

	public bool showGame;

	public bool isGameDone
	{
		get;
		protected set;
	}

	public static MatchParameters InitParameters
	{
		get
		{
			if (parameters == null)
			{
				parameters = MatchParameters.CreateDefault();
			}
			return parameters;
		}
		set
		{
			parameters = value;
		}
	}

	public bool isPaused
	{
		get;
		protected set;
	}

	public int numShotsInPoint
	{
		get;
		protected set;
	}

	public List<CameraController> cameras
	{
		get;
		protected set;
	}

	public int activePlayerTag
	{
		get;
		protected set;
	}

	public PlayerBase activePlayer => players[activePlayerTag % players.Count];

	public Ball ball
	{
		get;
		protected set;
	}

	public bool ballInGame
	{
		get;
		protected set;
	}

	public PlayerBase servingPlayer
	{
		get;
		protected set;
	}

	public static void Load(MatchParameters p)
	{
		InitParameters = p;
		UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
	}

	public int PlayerScore(PlayerBase player)
	{
		return score.ScoreForPlayer(player.playerTag);
	}

	public int ScoreDifference(int playerTag)
	{
		return score.ScoreForPlayer(playerTag) - score.ScoreForPlayer((playerTag + 1) % 2);
	}

	protected void Awake()
	{
		Application.targetFrameRate = 60;
		ballInGame = false;
		score.numPointsInGame = InitParameters.numPointsInGame;
		score.numGamesInSet = InitParameters.numGamesInSet;
		score.numSetsInMatch = 1;
		cameras = new List<CameraController>();
		cameras.Add(cameraController);
		GameObject gameObject = UnityEngine.Object.Instantiate(ballPrefab);
		ball = gameObject.GetComponent<Ball>();
		ball.Init(table);
		ball.onCollision += OnBallHitSurface;
		trail.match = this;
		trail.TrackedTransform = ball.transform;
		Ads.instance.hideBanner(Ads.instance.hideAdsInGame());
	}

	public PlayerBase OtherPlayer(PlayerBase player)
	{
		return players[(player.playerTag + 1) % players.Count];
	}

	protected void ChangeCrowdIntensity(float volume, float changeSpeed)
	{
		desiredCrowdVolume = volume;
		volumeSpeed = changeSpeed;
	}

	protected void StartShowGame()
	{
		showGame = true;
	}

	public void PlaySound(AudioClip clip, float volume)
	{
		if (!(clip == null) && !(ambience == null))
		{
			ambience.PlayOneShot(clip, volume);
		}
	}

	protected void OnTutorialComplete()
	{
		isInTutorial = false;
		PlayerSettings instance = PlayerSettings.instance;
		instance.Model.shownTutorial = true;
		instance.Model.spinTutorialShown = true;
		instance.Save();
	}

	public virtual void OnRepeatServe(PlayerBase player)
	{
		player.StartServe(ball);
	}

	public virtual void OnThrowServeBallInAir(PlayerBase player)
	{
	}

	public virtual void OnCanCompleteSwing(PlayerBase player)
	{
	}

	public virtual void OnPlayerHitBall(PlayerBase player, HitParams p)
	{
		if (ballInGame || numShotsInPoint <= 0)
		{
			ballInGame = true;
			ball.isBallInGame = true;
			PlayerBase playerBase = OtherPlayer(player);
			activePlayerTag = playerBase.playerTag;
			playerBase.OnOtherPlayerHitBall(ball);
			numShotsInPoint++;
		}
	}

	protected virtual void RepeatServe(PlayerBase servingPlayer)
	{
		ballInGame = false;
	}

	public virtual void OnBallHitSurface(Ball ball, string hitTag, bool changeFlight)
	{
		if (!ballInGame)
		{
			return;
		}
		UnityEngine.Debug.Log("Ball hit surface " + hitTag);
		PlayerBase activePlayer = this.activePlayer;
		PlayerBase player = OtherPlayer(this.activePlayer);
		BallHitEnum ballHitEnum = BallHitEnum.Out;
		if (hitTag == null)
		{
			ballHitEnum = BallHitEnum.Out;
		}
		else if (hitTag == "Table")
		{
			ballHitEnum = BallHitEnum.Table;
		}
		else if (hitTag == "Net")
		{
			ballHitEnum = BallHitEnum.Net;
		}
		int num = 1;
		int num2 = 1;
		if (ballHitEnum == BallHitEnum.Net)
		{
			AwardPointTo(activePlayer, FaultReason.Out);
			return;
		}
		PlayerBase player2 = activePlayer;
		bool flag = table.isOnPlayerSide(player2, ball);
		bool flag2 = table.isIn(player, ball);
		if (!flag || !flag2)
		{
			ballHitEnum = BallHitEnum.Out;
			AwardPointTo(activePlayer, FaultReason.Out);
		}
		else
		{
			AwardPointTo(player, FaultReason.PointWon);
		}
	}

	public virtual void ShowTiming(float value, Vector3 screenPos, int qualityIndex, bool inTable, float spinX = 0f)
	{
		ui.ShowTiming(value, screenPos, qualityIndex, inTable, spinX);
	}

	public virtual void ShowDebug(string text)
	{
	}

	public virtual void PlayerWantsToExit()
	{
		LoadNextScene();
	}

	public virtual void LoadNextScene()
	{
		ScreenNavigation.instance.LoadMain();
	}

	protected void Update()
	{
		crowd.volume = Mathf.Lerp(crowd.volume, desiredCrowdVolume, volumeSpeed * Time.deltaTime);
	}

	protected virtual void AwardPointTo(PlayerBase player, FaultReason fault)
	{
	}

	protected virtual void StartServe(PlayerBase player)
	{
		StartServe(player, newServe: true);
	}

	protected virtual void StartServe(PlayerBase player, bool newServe)
	{
		if (newServe)
		{
			serveMistakes = 0;
		}
		numShotsInPoint = 0;
		servingPlayer = player;
		activePlayerTag = player.playerTag;
		player.StartServe(ball);
		OtherPlayer(player).OnOtherPlayerStartServe();
	}

	public virtual void ShowTutorialNotification(string text)
	{
	}

	public PlayerBase PlayerForIndex(int index)
	{
		return players[index];
	}
}

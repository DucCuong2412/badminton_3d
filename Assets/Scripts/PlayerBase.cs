using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
	public enum SwingType
	{
		Forehand,
		Backhand,
		ForehandHigh,
		Smash,
		Serve
	}

	public class ShotParameters
	{
		public float contactTime;

		public Vector3 ballAimPosition;

		public Vector3 myPositionForAim;

		public SwingType chosenSwing;

		public bool canShoot = true;

		public bool canReachInTime;

		public float timeToMoveToPosition;

		public bool swingStarted;

		public float ballPossesionTime;

		public bool needsToJump;

		public float jumpTime;

		public float jumpDuration;

		public float jumpVelocityY;

		public bool jumped;

		public float timeToStartSwing;

		public float timeWhenShouldStartSwipe;

		public bool debugged;

		public bool isIn;

		public float speed;
	}

	private enum TimeCalculation
	{
		OldContinuous,
		Grouped
	}

	public enum ShotType
	{
		DropShot,
		LongShot,
		Smash
	}

	public enum MoveType
	{
		Run,
		Strafe,
		Back
	}

	protected Animator myAnimator;

	public static int LeftState = Animator.StringToHash("Base Layer.Left");

	public static int RightState = Animator.StringToHash("Base Layer.Right");

	public static int LeftJumpState = Animator.StringToHash("Base Layer.LeftJump");

	public static int RightJumpState = Animator.StringToHash("Base Layer.RightJump");

	public static int BackhandState = Animator.StringToHash("Base Layer.Backhand");

	public static int ForehandState = Animator.StringToHash("Base Layer.Forehand");

	protected int shotsInPoint;

	protected float maxSpeed = 15f;

	protected float minSpeed = 10f;

	protected MathEx.Range shotsToMinSpeed = new MathEx.Range();

	public const string LeftParamName = "Left";

	public const string RightParamName = "Right";

	public const string LeftBackParamName = "LeftBack";

	public const string RightBackParamName = "RightBack";

	public const string ForwardParamName = "Forward";

	public const string BackParamName = "Back";

	public const string BackhandParamName = "Backhand";

	public const string ForehandParamName = "Forehand";

	public const string SwingFinishParamName = "SwingFinish";

	public const string IdleParamName = "Idle";

	public const string ReceiveServeIdleParamName = "WaitingForServe";

	public const string ServeIdleParamName = "ServeIdle";

	public const string ThrowBallParamName = "ThrowBallInAir";

	protected TransformInterpolator moveInterpolator;

	protected HangFlightInterpolator flightInterpolator;

	protected FlightInterpolator jumpHeightInterpolator;

	protected bool useRealJump = true;

	protected Vector3 backhandPadleDisplace = new Vector3(0.3244817f, 1.008842f, -0.1966369f);

	protected Vector3 forehandPadleDisplace = new Vector3(-0.2179748f, 1.844752f, -0.3046381f);

	protected Vector3 servePadleDisplace = new Vector3(0.1577426f, 10.58585f, -0.6935806f) * 0.25f * 0.875f;

	protected Vector3 smashPadleDisplace = new Vector3(-0.2179748f, 1.844752f, -0.3046381f);

	protected float swingTime = 0.64f;

	public bool showGame;

	private Dictionary<SwingType, SwingDescription> swings = new Dictionary<SwingType, SwingDescription>();

	protected Transform paddleCenter;

	protected Transform leftHand;

	public AudioSource audio;

	public ShotParameters shotParams;

	protected RacketSpeedParams racketSpeed = new RacketSpeedParams();

	protected RacketSpeedParams serveRacketSpeed = new RacketSpeedParams();

	protected RacketSpeedParams longSpeed = new RacketSpeedParams();

	protected RacketSpeedParams smashSpeed = new RacketSpeedParams();

	protected MathEx.Range timingRange = new MathEx.Range
	{
		min = 0.01f,
		max = 0.1f
	};

	protected MathEx.Range serveTimingRange = new MathEx.Range
	{
		min = 0.01f,
		max = 0.05f
	};

	public ServeFlight serveFlight;

	protected RacketItem racketItem;

	public ShoeItem shoeItem;

	protected bool isDefenseShot;

	protected static int waitingForServeMoveTag = 1;

	protected static int startServeServeMoveTag = 2;

	protected float onFloorMaxPerfection = 0.9f;

	protected float inAirMaxPerfection = 0.9f;

	protected int goodShots;

	public float minShotTime = 0.45f;

	public float maxPenalty = 3f;

	protected int shootsToHalphRange = 5;

	public float minMoveDistance = 0.05f;

	public int staminaShotsToHalphMoveSpeed = 12;

	private TimeCalculation tc = TimeCalculation.Grouped;

	protected float racketTimingMult = 1f;

	protected float maxSpin = 45f;

	protected float minSpin = 10f;

	public MoveType moveType;

	public Ball ball
	{
		get;
		protected set;
	}

	public MatchController match
	{
		get;
		protected set;
	}

	public Table table
	{
		get;
		protected set;
	}

	public int playerTag
	{
		get;
		protected set;
	}

	public int tableSide
	{
		get;
		protected set;
	}

	public Transform myTransform
	{
		get;
		protected set;
	}

	public float speed => (maxSpeed - minSpeed) * (1f - shotsToMinSpeed.InverseLerp(shotsInPoint)) + minSpeed;

	public float pressure
	{
		get;
		protected set;
	}

	public float padleHeight
	{
		get;
		protected set;
	}

	public float minZ
	{
		get;
		protected set;
	}

	public float maxZ
	{
		get;
		protected set;
	}

	public bool isInServe => serveFlight != null && !serveFlight.serveFinished;

	public int courtSide => ServeSide(this);

	public float speedForDistance(float distance)
	{
		return speed;
	}

	public SwingType swingTypeFromAimPosition(Vector3 ballAimPosition)
	{
		Vector3 position = myTransform.position;
		position.y = 0f;
		Vector3 vector = rootPositionFromAimPosition(ballAimPosition, SwingType.Forehand);
		Vector3 vector2 = rootPositionFromAimPosition(ballAimPosition, SwingType.Backhand);
		Vector3 vector3 = rootPositionFromAimPosition(ballAimPosition, SwingType.Smash);
		vector.y = Mathf.Max(0f, vector.y);
		vector2.y = Mathf.Max(0f, vector2.y);
		vector3.y = Mathf.Max(0f, vector3.y);
		if (vector3.y > 0.6f)
		{
			return SwingType.Smash;
		}
		if (vector.y < vector2.y)
		{
			return SwingType.Forehand;
		}
		return SwingType.Backhand;
	}

	public SwingDescription swingDesc(SwingType type)
	{
		return swings[type];
	}

	public Vector3 rootPositionFromAimPosition(Vector3 ballAimPosition, SwingType type)
	{
		Vector3 vector = ballAimPosition;
		SwingDescription swingDescription = swingDesc(type);
		vector -= swingDescription.displace;
		vector.y = Mathf.Max(0f, vector.y);
		if (type != SwingType.Smash)
		{
			vector.y = 0f;
		}
		return vector;
	}

	public virtual void Init(MatchController match, int tag, int side)
	{
		this.match = match;
		audio = GetComponent<AudioSource>();
		table = match.table;
		playerTag = tag;
		tableSide = side;
		myTransform = base.transform;
		myAnimator = GetComponent<Animator>();
		paddleCenter = (myTransform.Find("Racket/RacketCenter") ?? myTransform.Find("BadmintonPlayer__Root/BadmintonPlayer__Spine_1/BadmintonPlayer__Spine_2/BadmintonPlayer__Spine_3/BadmintonPlayer__Chest/BadmintonPlayer__R.Shoulder/BadmintonPlayer__R.Arm/BadmintonPlayer__R.ForeArm/BadmintonPlayer__R.Hand/BadmintonPlayer__Racket/Badmington Racket"));
		leftHand = (myTransform.Find("Bip001 L Finger1") ?? myTransform.Find("BadmintonPlayer__Root/BadmintonPlayer__Spine_1/BadmintonPlayer__Spine_2/BadmintonPlayer__Spine_3/BadmintonPlayer__Chest/BadmintonPlayer__L.Shoulder/BadmintonPlayer__L.Arm/BadmintonPlayer__L.ForeArm/BadmintonPlayer__L.Hand"));
		myTransform.position = IdlePosition();
		myTransform.rotation = Quaternion.LookRotation(-Vector3.forward * tableSide);
		backhandPadleDisplace.x *= side;
		forehandPadleDisplace.x *= side;
		servePadleDisplace.x *= -side;
		smashPadleDisplace.x *= side;
		backhandPadleDisplace.z *= side;
		forehandPadleDisplace.z *= side;
		servePadleDisplace.z *= -side;
		smashPadleDisplace.z *= side;
		float num = 2f;
		float num2 = 2f;
		float num3 = 1.36f / num;
		swings.Add(SwingType.Serve, new SwingDescription
		{
			displace = servePadleDisplace,
			paramName = "ThrowBallInAir",
			delayTillHit = num3 + 0.447381884f / num2,
			timeToPrepare = num3
		});
		float num4 = 1f;
		float num5 = 2f;
		float num6 = 2f;
		swings.Add(SwingType.Backhand, new SwingDescription
		{
			displace = backhandPadleDisplace,
			paramName = "Backhand",
			delayTillHit = 0.4f / num5 + 497f / (904f * (float)Math.PI) / num6,
			timeToPrepare = 0.4f / num5
		});
		float num7 = 2f;
		float num8 = 2.5f;
		swings.Add(SwingType.Forehand, new SwingDescription
		{
			displace = forehandPadleDisplace,
			paramName = "Forehand",
			delayTillHit = 0.36f / num7 + 77f / 600f / num8,
			timeToPrepare = 0.36f / num7
		});
		swings.Add(SwingType.Smash, new SwingDescription
		{
			displace = smashPadleDisplace,
			paramName = "Forehand",
			delayTillHit = 0.36f / num7 + 77f / 600f / num8,
			timeToPrepare = 0.36f / num7
		});
		minZ = 0.2f * table.halphLength;
		maxZ = 1.2f * table.halphLength;
		padleHeight = MathEx.Avg(forehandPadleDisplace.y, backhandPadleDisplace.y);
		moveInterpolator = new TransformInterpolator(myTransform);
		moveInterpolator.onMoveComplete += OnMoveInterpolatorStopMoving;
		flightInterpolator = new HangFlightInterpolator(myTransform);
		jumpHeightInterpolator = new FlightInterpolator(myTransform);
		jumpHeightInterpolator.onJumpDown += OnDown;
		jumpHeightInterpolator.onHitTheGround += OnHitTheGround;
	}

	public void OnHitTheGround()
	{
		if (match.ballInGame && shotParams == null && serveFlight == null && !moveInterpolator.moving)
		{
			MoveTo(returnPoint());
		}
	}

	public void OnUp()
	{
		myAnimator.SetBool("JumpUp", value: true);
	}

	public void OnDown()
	{
		myAnimator.SetBool("JumpUp", value: false);
	}

	public void PlaySound(AudioClip clip, float volume)
	{
		if (!(clip == null) && !(audio == null))
		{
			audio.PlayOneShot(clip, volume);
		}
	}

	public void SetRacket(int index, bool isMultiplayer = false)
	{
		SetRacket(ShopItems.instance.GetRacket(index), isMultiplayer);
	}

	public void SetRacket(RacketItem item, bool isMultiplayer)
	{
		racketItem = item;
		racketSpeed = item.speed.Clone();
		longSpeed = item.longSpeed.Clone();
		smashSpeed = item.smashSpeed.Clone();
		serveRacketSpeed = item.serveSpeed.Clone();
		timingRange = item.timingRange.Clone();
		serveTimingRange.min = timingRange.min;
		serveTimingRange.max = timingRange.max;
	}

	public void SetShoe(int index, bool isMultiplayer = false)
	{
		ShopItems instance = ShopItems.instance;
		SetShoe(instance.GetShoe(index), isMultiplayer);
	}

	public void SetShoe(ShoeItem shoe, bool isMultiplayer)
	{
		minSpeed = shoe.shoesSpeed;
		maxSpeed = shoe.shoesSpeed;
		minSpeed = shoe.shoesMinSpeed;
		shotsToMinSpeed = shoe.shotsToMinSpeed.Clone();
		shoeItem = shoe;
	}

	private bool isPositionAcceptable(Vector3 pos, float minTime, float maxJumpHeight)
	{
		SwingType type = swingTypeFromAimPosition(pos);
		pos = rootPositionFromAimPosition(pos, type);
		float num = Vector3Ex.HorizontalDistance(pos, myTransform.position);
		float num2 = speedForDistance(num);
		if (Mathf.Abs(pos.z) / table.halphLength <= 0.1f)
		{
			Vector3 landingPositionOnTable = ball.landingPositionOnTable;
			if (Mathf.Abs(landingPositionOnTable.z) * 0.75f > Mathf.Abs(pos.z))
			{
				return false;
			}
		}
		float num3 = num / num2;
		return num3 + jumpHeightInterpolator.ApexTime(0f, pos.y) <= minTime && pos.y <= maxJumpHeight;
	}

	public float FindFirstReacheableTimeForRange(float minTime, float maxTime, int maxIterations)
	{
		Vector3 minPos = ball.PositionAfterTime(minTime);
		Vector3 maxPos = ball.PositionAfterTime(maxTime);
		float maxJumpHeight = 1.5f;
		return FindFirstReacheableTimeForRange(minTime, minPos, maxTime, maxPos, maxJumpHeight, maxIterations);
	}

	public float FindFirstReacheableTimeForRange(float minTime, Vector3 minPos, float maxTime, Vector3 maxPos, float maxJumpHeight, int maxLevel, int level = 0)
	{
		if (minPos.y > maxPos.y)
		{
			MathEx.Swap(ref minPos, ref maxPos);
			MathEx.Swap(ref minTime, ref maxTime);
		}
		float num = (!isInAir()) ? 0f : (jumpHeightInterpolator.totalTime - jumpHeightInterpolator.time);
		bool flag = isPositionAcceptable(minPos, minTime - num, maxJumpHeight);
		if (isPositionAcceptable(maxPos, maxTime - num, maxJumpHeight))
		{
			return maxTime;
		}
		if (!flag)
		{
			return -1f;
		}
		level++;
		float num2 = (maxTime + minTime) * 0.5f;
		Vector3 vector = ball.PositionAfterTime(num2);
		if (isPositionAcceptable(vector, num2, maxJumpHeight))
		{
			if (level >= maxLevel)
			{
				return num2;
			}
			return FindFirstReacheableTimeForRange(num2, vector, maxTime, maxPos, maxJumpHeight, maxLevel, level);
		}
		if (level >= maxLevel)
		{
			return minTime;
		}
		return FindFirstReacheableTimeForRange(minTime, minPos, num2, vector, maxJumpHeight, maxLevel, level);
	}

	public ShotParameters FindShotParameters()
	{
		float minTime = Mathf.Max(ball.timeToReachNet, ball.apexTime);
		float num = ball.flightTime * 0.95f;
		float num2 = FindFirstReacheableTimeForRange(minTime, num, 7);
		if (num2 <= 0f)
		{
			num2 = num;
		}
		Vector3 vector = ball.PositionAfterTime(num2);
		SwingType swingType = swingTypeFromAimPosition(vector);
		Vector3 myPositionForAim = rootPositionFromAimPosition(vector, swingType);
		SwingDescription swingDescription = swingDesc(swingType);
		float distance = Vector3Ex.HorizontalDistance(vector, myTransform.position);
		float speed = speedForDistance(distance);
		ShotParameters shotParameters = new ShotParameters();
		shotParameters.speed = speed;
		shotParameters.chosenSwing = swingType;
		shotParameters.contactTime = num2;
		shotParameters.ballAimPosition = vector;
		shotParameters.myPositionForAim = myPositionForAim;
		shotParameters.timeToMoveToPosition = moveInterpolator.DurationTo(shotParameters.myPositionForAim, speed);
		shotParameters.timeWhenShouldStartSwipe = shotParameters.contactTime - swingDescription.delayTillHit + swingDescription.timeToPrepare;
		shotParameters.canReachInTime = (shotParameters.timeToMoveToPosition < shotParameters.contactTime);
		bool flag = shotParameters.isIn = table.isIn(match.OtherPlayer(this), ball, ball.landingPositionOnTable);
		shotParameters.timeToStartSwing = Mathf.Min(shotParameters.contactTime - swingDescription.delayTillHit, shotParameters.timeToMoveToPosition);
		if (shotParameters.myPositionForAim.y > 0f && shotParameters.canReachInTime && shotParameters.chosenSwing == SwingType.Smash)
		{
			shotParameters.needsToJump = true;
			shotParameters.jumpDuration = 0.3f;
			shotParameters.jumpTime = shotParameters.contactTime - shotParameters.jumpDuration;
			shotParameters.timeToStartSwing = Mathf.Min(shotParameters.jumpTime, shotParameters.timeToStartSwing);
		}
		return shotParameters;
	}

	public float PaddleDistanceFromBall()
	{
		return Vector3.Distance(ball.myTransform.position, paddleCenter.position);
	}

	public float HorizontalPaddleDistanceFromBall()
	{
		Vector3 position = ball.myTransform.position;
		Vector3 position2 = paddleCenter.position;
		position.y = (position2.y = 0f);
		return Vector3.Distance(position, position2);
	}

	public Vector3 IdlePosition()
	{
		return new Vector3(0f, 0f, (float)tableSide * table.halphLength * 0.75f);
	}

	public Vector3 NetIdlePosition()
	{
		return IdlePosition();
	}

	public int ServeSide(PlayerBase servingPlayer)
	{
		return (match.PlayerScore(servingPlayer) % 2 != 0) ? 1 : (-1);
	}

	public virtual Vector3 IdleReceiveServePosition()
	{
		return new Vector3((float)ServeSide(match.OtherPlayer(this)) * table.halphwidth * 0.2f * (float)tableSide, 0f, (float)tableSide * (table.serveLength + table.halphLength) * 0.5f);
	}

	public virtual Vector3 IdleServePosition()
	{
		return new Vector3((float)courtSide * table.halphwidth * 0.2f * (float)tableSide, 0f, (float)tableSide * (table.serveLength + table.halphLength) * 0.5f);
	}

	public virtual void OnGameStart()
	{
	}

	protected void GoToTheGround()
	{
		Vector3 position = myTransform.position;
		position.y = 0f;
		myTransform.position = position;
		jumpHeightInterpolator.isInJump = false;
		myAnimator.SetTrigger("Idle");
		myAnimator.SetBool("JumpUp", value: false);
	}

	public virtual void OnOtherPlayerHitBall(Ball ball)
	{
		this.ball = ball;
		if (match.numShotsInPoint < 1)
		{
			myAnimator.SetTrigger("Idle");
		}
		isDefenseShot = match.OtherPlayer(this).isInAir();
		shotParams = FindShotParameters();
		if (shotParams.canShoot)
		{
			GoToTheGround();
			if (!shotParams.canReachInTime)
			{
				Vector3 a = shotParams.myPositionForAim.OnGround() - myTransform.position;
				MoveTo(myTransform.position + a * 0.5f, 0, shotParams.speed);
			}
			else
			{
				MoveTo(shotParams.myPositionForAim.OnGround(), 0, shotParams.speed);
			}
		}
	}

	public virtual void OnPointWon(PlayerBase player)
	{
		pressure = 0f;
		shotsInPoint = 0;
		goodShots = 0;
		ball = null;
		shotParams = null;
		myAnimator.SetTrigger("Idle");
		match.ui.spinMarker.spin = 0f;
		match.ui.showSpin(0f);
		GoToTheGround();
		if (!jumpHeightInterpolator.isInJump && player == this)
		{
			moveInterpolator.StopMoving();
			myAnimator.SetTrigger("Happy");
		}
	}

	public void TakeBall(Ball ball)
	{
		ball.MakeKinematic(isKinematic: true);
		ball.myTransform.parent = leftHand;
		ball.myTransform.localPosition = Vector3.zero;
	}

	public virtual void StartServe(Ball ball)
	{
		shotsInPoint = 0;
		pressure = 0f;
		ball.pressure = 1f;
		goodShots = 0;
		this.ball = ball;
		moveInterpolator.StopMoving();
		myAnimator.SetTrigger("ServeIdle");
		myAnimator.SetBool("SwingFinish", value: false);
		myTransform.position = IdleServePosition();
		TakeBall(ball);
		serveFlight = new ServeFlight();
		shotParams = null;
		match.ui.showSpin(0f);
	}

	public virtual void OnOtherPlayerStartServe()
	{
		pressure = 0f;
		shotsInPoint = 0;
		goodShots = 0;
		moveInterpolator.StopMoving();
		myTransform.position = IdleReceiveServePosition();
		myAnimator.SetTrigger("WaitingForServe");
	}

	protected virtual void OnCanHitServeBall()
	{
	}

	private void OnMoveInterpolatorStopMoving(TransformInterpolator inter)
	{
		myAnimator.SetBool("Left", value: false);
		myAnimator.SetBool("Right", value: false);
		myAnimator.SetBool("LeftBack", value: false);
		myAnimator.SetBool("RightBack", value: false);
		myAnimator.SetBool("Forward", value: false);
		myAnimator.SetBool("Back", value: false);
		myAnimator.SetFloat("Speed", 0f);
		myAnimator.SetFloat("StrafeSpeed", 0f);
		myAnimator.SetBool("Hop", value: false);
		myAnimator.SetBool("Back", value: false);
		if (inter.tag == waitingForServeMoveTag)
		{
			myAnimator.SetTrigger("WaitingForServe");
		}
		else if (inter.tag == startServeServeMoveTag)
		{
			myAnimator.SetTrigger("ServeIdle");
		}
		inter.tag = 0;
	}

	public virtual bool CanCompleteSwing()
	{
		return true;
	}

	private void ReleaseBall()
	{
	}

	protected void ThrowServeBallInAir()
	{
		ball.MakeKinematic(isKinematic: true);
		SwingDescription chosenSwing = swingDesc(SwingType.Serve);
		myAnimator.SetBool("SwingFinish", value: false);
		myAnimator.SetTrigger(chosenSwing.paramName);
		serveFlight.thrownServeBall = true;
		serveFlight.chosenSwing = chosenSwing;
		serveFlight.ballPossesionTime = 0f;
		serveFlight.timeToHit = 0.5f + chosenSwing.delayTillHit;
		serveFlight.timeToStartSwing = chosenSwing.timeToPrepare * 0.62f;
		serveFlight.timeToFinishSwing = serveFlight.timeToHit - chosenSwing.delayTillHit + chosenSwing.timeToPrepare;
		serveFlight.ballPositionWhenHit = myTransform.position + chosenSwing.displace;
		serveFlight.minHeightForFall = table.netHeight;
		match.OnThrowServeBallInAir(this);
	}

	protected MathEx.Range GetZRange(float normalizedPenalty)
	{
		Vector3 position = ball.transform.position;
		float num = 0.4f;
		float min = 0.2f;
		min = Mathf.Clamp(num - Mathf.Abs(position.z * 2f / table.length), min, 0.4f);
		if (isInAir())
		{
			min = 0.3f;
		}
		float num2 = 0.2f + Mathf.Clamp(ball.flightTime - 0.4f * (1f - normalizedPenalty), 0f, 0.7f);
		if (isInServe)
		{
			num2 = 1f;
		}
		float num3 = Mathf.Abs(position.z * 2f / table.length);
		if (isInAir())
		{
			min = 0.6f;
			num2 = 0.9f;
		}
		if (num3 < 0.4f)
		{
			min = 0.8f;
			num2 = 1f;
		}
		min = 0.3f;
		num2 = 1f;
		MathEx.Range range = new MathEx.Range();
		range.min = min;
		range.max = num2;
		return range;
	}

	protected float ShootTime(Vector3 ballLandingPos, float normalizedPenalty, float heightAboweTheNet, bool isServe)
	{
		if (tc == TimeCalculation.OldContinuous)
		{
			return ShootTimeOld(ballLandingPos, normalizedPenalty, heightAboweTheNet);
		}
		if (tc == TimeCalculation.Grouped)
		{
			return ShootTimeGrouped(ballLandingPos, normalizedPenalty, heightAboweTheNet, isServe);
		}
		return ShootTimeOld(ballLandingPos, normalizedPenalty, heightAboweTheNet);
	}

	protected RacketSpeedParams racketSpeedForShotType(ShotType shotType, bool isServe)
	{
		if (isServe)
		{
			return serveRacketSpeed;
		}
		switch (shotType)
		{
		case ShotType.DropShot:
			return racketSpeed;
		case ShotType.LongShot:
			return longSpeed;
		case ShotType.Smash:
			return smashSpeed;
		default:
			return racketSpeed;
		}
	}

	protected float ShootTimeGrouped(Vector3 ballLandingPos, float normalizedPenalty, float heightAboweTheNet, bool isServe)
	{
		RacketSpeedParams racketSpeedParams = racketSpeedForShotType(GetShotType(isServe, normalizedPenalty), isServe);
		float num = racketSpeedParams.TimingForPenalty(normalizedPenalty) * racketTimingMult;
		float num2 = ball.minTimeForHeightAndPosition(heightAboweTheNet, ballLandingPos);
		float value = num2 + num;
		return Mathf.Clamp(value, minShotTime, 3f);
	}

	protected float ShootTimeOld(Vector3 ballLandingPos, float normalizedPenalty, float heightAboweTheNet)
	{
		float a = ball.minTimeForHeightAndPosition(heightAboweTheNet, ballLandingPos);
		float num = Mathf.Max(a, minShotTime);
		float num2 = maxPenalty * (float)(1 + goodShots / shootsToHalphRange);
		num += num2 * normalizedPenalty;
		if (num < 1.5f)
		{
			goodShots++;
		}
		else
		{
			goodShots = 0;
		}
		return Mathf.Clamp(num, minShotTime, 3f);
	}

	protected bool isInAir()
	{
		Vector3 position = myTransform.position;
		return position.y > minMoveDistance;
	}

	protected float NormalizedPenalty(float shotTimingDelay, bool isInAir)
	{
		if (tc == TimeCalculation.OldContinuous)
		{
			return NormalizedPenaltyOld(shotTimingDelay, isInAir);
		}
		if (tc == TimeCalculation.Grouped)
		{
			return NormalizedPenaltyGrouped(shotTimingDelay, isInAir);
		}
		return NormalizedPenaltyOld(shotTimingDelay, isInAir);
	}

	protected float NormalizedPenaltyGrouped(float shotTimingDelay, bool isInAir)
	{
		float num = timingRange.InverseLerp(Mathf.Abs(shotTimingDelay));
		if (ball.pressure != 0f)
		{
			num *= ball.pressure;
		}
		return num;
	}

	protected float NormalizedPenaltyOld(float shotTimingDelay, bool isInAir)
	{
		shotTimingDelay = Mathf.Abs(shotTimingDelay);
		float num = 0.05f;
		float num2 = onFloorMaxPerfection;
		if (isInAir)
		{
			num = 0.05f;
			num2 = inAirMaxPerfection;
		}
		float num3 = Mathf.Clamp01(goodShots / shootsToHalphRange);
		num2 -= num2 * 0.5f * num3;
		num -= num * 0.5f * num3;
		return Mathf.Clamp(Mathf.Max(shotTimingDelay - num, 0f) / (num2 - num), 0f, 0.99f);
	}

	public float NormalizedPenalty(float timing, MathEx.Range timingRange)
	{
		return timingRange.InverseLerp(Mathf.Abs(timing)) * Mathf.Lerp(2f + 0.5f * pressure, 1f, ball.penalty);
	}

	public ShotType GetShotType(bool isServe, float normalizedPenalty)
	{
		ShotType shotType = ShotType.LongShot;
		if (isServe)
		{
			if (normalizedPenalty > racketItem.longShotPenalty)
			{
				return ShotType.LongShot;
			}
			return ShotType.DropShot;
		}
		if (isDefenseShot)
		{
			if (normalizedPenalty < 0.2f)
			{
				return ShotType.DropShot;
			}
			return ShotType.LongShot;
		}
		if (isInAir())
		{
			return ShotType.Smash;
		}
		Vector3 position = ball.myTransform.position;
		if (position.y < racketItem.longShotHeigh * table.netHeight || normalizedPenalty > racketItem.longShotPenalty)
		{
			return ShotType.LongShot;
		}
		return ShotType.DropShot;
	}

	public HitParams CreateHitParams(Vector3 landingPos, float normalizedPenalty, bool isDefenseShot, bool isServe)
	{
		HitParams result = default(HitParams);
		float halphLength = table.halphLength;
		landingPos.x = Mathf.Clamp(landingPos.x, (0f - table.halphwidth) * 1f, table.halphwidth * 1f);
		if (match.TestHorizontalShots)
		{
			landingPos.z = Mathf.Sign(landingPos.z) * 0.75f * table.halphLength;
			landingPos.x = 0f;
		}
		ShotType shotType = GetShotType(isServe, normalizedPenalty);
		RacketSpeedParams racketSpeedParams = racketSpeedForShotType(shotType, isServe);
		RacketSpeedParams.RacketParam racketParam = racketSpeedParams.ParamForPenalty(normalizedPenalty);
		float num = 0.2f;
		float min = 0.1f;
		float max = 3f;
		switch (shotType)
		{
		case ShotType.Smash:
			landingPos.z = Mathf.Sign(landingPos.z) * racketItem.smashZ.Lerp(1f - normalizedPenalty) * table.halphLength;
			num = 0.2f;
			min = racketSpeedParams.TimingForPenalty(normalizedPenalty);
			landingPos.x *= racketParam.xMult;
			break;
		case ShotType.DropShot:
			landingPos.z = Mathf.Sign(landingPos.z) * racketItem.dropZ.Lerp(normalizedPenalty) * table.halphLength;
			num = 0.1f;
			landingPos.x *= racketParam.xMult;
			break;
		case ShotType.LongShot:
			landingPos.z = Mathf.Sign(landingPos.z) * Mathf.Lerp(0.8f, 0.99f, 1f - normalizedPenalty) * table.halphLength;
			landingPos.x *= racketParam.xMult;
			num = 1f;
			break;
		}
		result.landingPosition = landingPos;
		result.heightOverTheNet = num;
		result.penalty = normalizedPenalty;
		result.time = ShootTime(landingPos, normalizedPenalty, num, isServe);
		float value = ball.minTimeForHeightAndPosition(num, landingPos);
		result.time = Mathf.Clamp(value, min, max);
		result.pressure = racketParam.pressureMult;
		return result;
	}

	protected virtual bool HorizontalDistanceOk()
	{
		return true;
	}

	private void UpdateNormalShot()
	{
		if (shotParams == null || ball == null || !ball.isBallInGame)
		{
			return;
		}
		shotParams.ballPossesionTime += Time.deltaTime;
		SwingDescription swingDescription = swingDesc(shotParams.chosenSwing);
		if (shotParams.ballPossesionTime >= shotParams.timeToStartSwing && match.ballInGame && shotParams.canReachInTime && !shotParams.swingStarted)
		{
			myAnimator.SetTrigger(swingDescription.paramName);
			shotParams.swingStarted = true;
		}
		bool value = (HorizontalDistanceOk() || shotParams.ballPossesionTime < shotParams.contactTime) && shotParams.swingStarted && shotParams.ballPossesionTime >= shotParams.contactTime - swingDescription.delayTillHit + swingDescription.timeToPrepare && CanCompleteSwing();
		myAnimator.SetBool("SwingFinish", value);
		if (shotParams.needsToJump && shotParams.jumpTime <= shotParams.ballPossesionTime && !shotParams.jumped)
		{
			if (useRealJump)
			{
				jumpHeightInterpolator.Jump(0f, shotParams.myPositionForAim.y, shotParams.jumpDuration, 0.1f);
			}
			else
			{
				flightInterpolator.Jump(0f, shotParams.myPositionForAim.y, shotParams.jumpDuration, 0.1f);
			}
			myAnimator.SetBool("JumpUp", value: true);
			shotParams.jumped = true;
		}
		if (shotParams.swingStarted && shotParams.ballPossesionTime >= shotParams.contactTime && HorizontalDistanceOk() && CanCompleteSwing())
		{
			OnCanHitBall();
		}
		else if (shotParams.swingStarted && shotParams.ballPossesionTime >= shotParams.contactTime && !HorizontalDistanceOk() && !CanCompleteSwing())
		{
			myAnimator.SetTrigger("Idle");
		}
	}

	private void UpdateServeShot()
	{
		if (serveFlight != null && !(ball == null) && serveFlight.thrownServeBall && !serveFlight.serveFinished)
		{
		}
	}

	public virtual void Update()
	{
		if (moveInterpolator != null)
		{
			moveInterpolator.Update();
		}
		if (flightInterpolator != null)
		{
			flightInterpolator.Update();
		}
		if (jumpHeightInterpolator != null)
		{
			jumpHeightInterpolator.Update();
		}
		UpdateNormalShot();
		UpdateServeShot();
		if (moveInterpolator == null)
		{
			return;
		}
		Vector3 vector = moveInterpolator.endPos - moveInterpolator.startPos;
		if (moveInterpolator.moving && !jumpHeightInterpolator.isInJump)
		{
			if (moveType == MoveType.Strafe)
			{
				myAnimator.SetBool("Hop", value: true);
				myAnimator.SetFloat("Speed", 0f);
			}
			else if (moveType == MoveType.Run)
			{
				myAnimator.SetBool("Hop", value: false);
				myAnimator.SetFloat("Speed", moveInterpolator.deltaDistance / Time.deltaTime);
			}
			else
			{
				myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.LookRotation(moveInterpolator.startPos - moveInterpolator.endPos), Time.deltaTime * 10f);
			}
		}
		else
		{
			myTransform.rotation = Quaternion.Lerp(myTransform.rotation, Quaternion.LookRotation(new Vector3(0f, 0f, -tableSide)), Time.deltaTime * 100f);
			myAnimator.SetBool("Hop", value: false);
		}
	}

	protected virtual void OnCanHitBall()
	{
	}

	protected float MoveTo(Vector3 pos, int tag = 0)
	{
		return MoveTo(pos, tag, speed);
	}

	protected float MoveTo(Vector3 pos, int tag, float chosenSpeed)
	{
		myAnimator.SetBool("Left", value: false);
		myAnimator.SetBool("Right", value: false);
		myAnimator.SetBool("LeftBack", value: false);
		myAnimator.SetBool("RightBack", value: false);
		myAnimator.SetBool("Forward", value: false);
		myAnimator.SetBool("Back", value: false);
		Vector3 position = myTransform.position;
		position.x = pos.x;
		position.z = pos.z;
		float num = Vector3.Dot(position - myTransform.position, myTransform.right);
		float num2 = Vector3.Dot(position - myTransform.position, myTransform.forward);
		float num3 = moveInterpolator.DurationTo(position, chosenSpeed);
		moveInterpolator.MoveTo(position, num3, tag);
		Vector3 vector = moveInterpolator.endPos - moveInterpolator.startPos;
		moveType = MoveType.Strafe;
		float magnitude = vector.magnitude;
		if ((Mathf.Sign(vector.z) == (float)(-tableSide) && Mathf.Abs(vector.z) < Mathf.Abs(Mathf.Tan(1.22173047f))) || (Mathf.Sign(vector.z) != (float)(-tableSide) && Mathf.Abs(vector.z) < Mathf.Abs(Mathf.Tan(1.22173047f))))
		{
			moveType = MoveType.Strafe;
		}
		else if (Mathf.Sign(vector.z) != (float)(-tableSide))
		{
			moveType = MoveType.Back;
		}
		else
		{
			moveType = MoveType.Run;
		}
		return num3;
	}

	public void HitBall(HitParams p)
	{
		HitBall(p.landingPosition, p.heightOverTheNet, p.time, p.penalty, p.spinX, p);
	}

	public void HitServe(HitParams p)
	{
		HitServe(p.landingPosition, p.heightOverTheNet, p.time, p.penalty, p.spinX, p);
	}

	protected void HitBall(Vector3 pointOnTable, float height, float timeToLand, float penalty, float spinX, HitParams p)
	{
		if (ball == null)
		{
			return;
		}
		ShotType shotType = GetShotType(isInServe, Mathf.Abs(penalty));
		int num = racketSpeedForShotType(shotType, isInServe).IndexForPenalty(penalty);
		ball.Shoot(pointOnTable, timeToLand, penalty, p.pressure);
		if (num < 1)
		{
			GameObject nextObject = CFX_SpawnSystem.GetNextObject(match.ui.sparkParticle);
			if (nextObject != null)
			{
				nextObject.transform.position = ball.myTransform.position;
			}
		}
		HitBall(penalty, p);
	}

	protected void HitServe(Vector3 pointOnTable, float height, float timeToLand, float penalty, float spinX, HitParams p)
	{
		myAnimator.SetBool("SwingFinish", value: true);
		serveFlight = null;
		HitBall(pointOnTable, height, timeToLand, penalty, spinX, p);
	}

	private Vector3 returnPoint()
	{
		float num = 0.75f;
		Vector3 result = IdlePosition() * num + myTransform.position * (1f - num);
		result.y = 0f;
		return result;
	}

	private void HitBall(float penalty, HitParams p)
	{
		shotsInPoint++;
		p.jumpTimeWhenHit = ((!jumpHeightInterpolator.isInJump) ? (-1f) : jumpHeightInterpolator.time);
		match.OnPlayerHitBall(this, p);
		Ball ball = this.ball;
		this.ball = null;
		shotParams = null;
		serveFlight = null;
		PlaySound(match.racketHit, ((1f - Mathf.Clamp01(Mathf.Abs(penalty / 0.5f))) * 5f + 0.5f) * 0.1f);
		moveInterpolator.StopMoving();
		Vector3 a = IdlePosition();
		bool flag = !table.isIn(match.OtherPlayer(this), ball, p.landingPosition);
		if (Vector3.Distance(a, myTransform.position) > 1f && !isInAir())
		{
			Vector3 pos = returnPoint();
			MoveTo(pos);
		}
	}

	protected virtual bool WantsToMoveToNet()
	{
		return false;
	}
}

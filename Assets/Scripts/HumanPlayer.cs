using System.Collections;
using UnityEngine;

public class HumanPlayer : PlayerBase
{
	public bool autoPilot;

	private GestureTracker gestureTracker = new GestureTracker();

	private GestureTracker gestureTrackerSpin = new GestureTracker();

	private bool trackingTouch;

	private float touchTime;

	private bool readyToTakeServe;

	protected CameraController myCamera;

	private float cameraSpeed = 2.5f;

	private Plane floorPlane;

	private float fwdTime;

	private float swipeDuration = 0.1f;

	private Camera uiCamera;

	private PingPongInterpolator timingPingPong = new PingPongInterpolator();

	private int activeGestureTrackerTag;

	private SingleReadParam<bool> tutorialServe = new SingleReadParam<bool>();

	protected AiPlayer ai;

	private bool isInTutorial;

	private bool disableSwipe;

	private float serveDifficultySpeed = 2f;

	private bool tutorialOver;

	private MathEx.Range penaltyCanMissShotRange = new MathEx.Range
	{
		min = 0.05f,
		max = 1f
	};

	private MathEx.Range gestureSpeedRange = new MathEx.Range
	{
		min = 15f,
		max = 125f
	};

	public bool useExpertControl;

	private float xSkew = 0.7f;

	private Vector2 cameraSpeedOffset = new Vector2(0f, 0f);

	private int trackingFingerId;

	private float pointerAlpha;

	private bool showPointer;

	public bool TEST;

	protected UISprite pointer => base.match.ui.getPointer(this);

	protected UISprite helpPointer => base.match.ui.getHelpPointer(this);

	public void Init(MatchController match, int tag, int side, CameraController camera, AiPlayer ai)
	{
		base.Init(match, tag, side);
		this.ai = ai;
		myCamera = camera;
		floorPlane = new Plane(Vector3.up, base.table.tabletopy);
		PlayerSettings instance = PlayerSettings.instance;
		SetShoe(instance.Model.usedShoe);
		SetRacket(instance.Model.usedRacket);
		RacketItem racket = ShopItems.instance.GetRacket(instance.Model.usedRacket);
		serveDifficultySpeed = racket.serveDifficulty;
		gestureTracker.minDistance = (float)Mathf.Max(Screen.height, Screen.width) * 0.01f;
		uiCamera = UICamera.mainCamera.GetComponent<Camera>();
		CharacterLook.instance.SetPlayerLook(PlayerSettings.instance.Model.usedLook, base.myTransform);
	}

	public override void OnPointWon(PlayerBase player)
	{
		if (shotParams != null)
		{
			UnityEngine.Debug.Log("contact " + shotParams.contactTime + " time " + shotParams.ballPossesionTime + " swing strted " + shotParams.swingStarted);
		}
		base.OnPointWon(player);
		base.match.ui.ballMarker.SetActive(value: false);
		ShowPointer(show: false);
		Vector3 vector = base.myTransform.position.OnGround();
		if (player == this)
		{
			myCamera.MoveTo(vector + new Vector3(0f, 3f, (float)base.tableSide * 5f), new Vector3(0f, base.table.netHeight * 0.5f, 0f), cameraSpeed);
		}
		else
		{
			MoveCameraTo(vector);
		}
	}

	public override void OnOtherPlayerHitBall(Ball ball)
	{
		base.OnOtherPlayerHitBall(ball);
		ShowPointer(show: false);
		gestureTrackerClear();
		if (shotParams != null)
		{
			base.match.ui.ballMarker.SetActive(value: true);
			base.match.ui.ballMarker.transform.localScale = Vector3.one;
			base.match.ui.ballMarker.transform.position = shotParams.ballAimPosition;
		}
		trackingTouch = false;
		Vector3 ballAimPosition = shotParams.ballAimPosition;
		ballAimPosition = shotParams.ballAimPosition;
		ballAimPosition.y = myCamera.offsetFromPlayer.y;
		ballAimPosition.z += (float)base.tableSide * myCamera.offsetFromPlayer.z;
		Vector3 a = new Vector3(ballAimPosition.x * 0.5f, 0f, (float)base.tableSide * 0.5f * base.table.halphLength);
		Vector3 vector = a - ballAimPosition;
		vector.y = 0f;
		vector.Normalize();
		if (!showGame)
		{
			Vector3 ballAimPosition2 = shotParams.ballAimPosition;
			ballAimPosition2.y *= 0.2f;
			MoveCameraTo(ballAimPosition2);
			myCamera.TrackTransform(ball.transform, new Vector3(0.5f, 0.45f, 0.5f), new Vector3Range
			{
				min = new Vector3(0f - base.table.halphwidth, -50f, -5f),
				max = new Vector3(base.table.halphwidth, 50f, 5f)
			}, new Vector3(0f, 0f, 0f));
		}
	}

	public override void OnGameStart()
	{
		MoveCameraTo(base.myTransform.position);
	}

	private void MoveCameraTo(Vector3 ballAimPosition, bool instant = false)
	{
		float value = Mathf.Abs(ballAimPosition.z) / base.table.halphLength;
		value = 1f - Mathf.Clamp(value, 0f, 1.4f);
		value = Mathf.Clamp(value, -0.8f, 0.8f);
		myCamera.rotationSpeed = 5f;
		float num = 1.5f * ballAimPosition.y;
		float z = (float)(-base.tableSide) * value * base.table.halphLength + (float)base.tableSide * num;
		Vector3 destination = myCamera.idlePosition + new Vector3(ballAimPosition.x * 0.95f, num, z);
		Vector3 lookAtPoint = new Vector3(Mathf.Clamp(ballAimPosition.x * 0.5f, 0f - base.table.halphwidth, base.table.halphwidth), ballAimPosition.y * 1.2f, (float)base.tableSide * 2f + (float)(-base.tableSide) * Mathf.Clamp01(value) * base.table.halphLength * 0.75f);
		if (!instant)
		{
			myCamera.MoveTo(destination, lookAtPoint, cameraSpeed);
		}
		else
		{
			myCamera.MoveTo(destination, lookAtPoint, 300f);
		}
	}

	public override void OnOtherPlayerStartServe()
	{
		base.OnOtherPlayerStartServe();
		ShowPointer(show: false);
		gestureTrackerClear();
		gestureTrackerSpin.Clear();
		Vector3 ballAimPosition = IdleReceiveServePosition();
		ballAimPosition.y = myCamera.offsetFromPlayer.y;
		ballAimPosition.z += (float)base.tableSide * myCamera.offsetFromPlayer.z;
		MoveCameraTo(ballAimPosition);
		if (showGame)
		{
		}
	}

	public void ChoseHandPosition()
	{
		Vector3 from = paddleCenter.position;
		if (shotParams != null)
		{
			from = shotParams.ballAimPosition;
		}
		base.match.ui.hand.PointFromTo(from, new Vector3(Random.Range(0f - base.table.halphwidth, base.table.halphwidth), from.y, (0f - base.table.halphLength) * (float)base.tableSide));
	}

	protected void gestureTrackerClear()
	{
		activeGestureTrackerTag = 0;
		gestureTracker.Clear();
		gestureTrackerSpin.Clear();
	}

	protected void AdvanceGestureTracker()
	{
		activeGestureTrackerTag++;
		if (activeGestureTrackerTag > 1)
		{
			activeGestureTrackerTag = 1;
		}
	}

	public GestureTracker ActiveGestureTracker()
	{
		if (activeGestureTrackerTag == 1)
		{
			return gestureTrackerSpin;
		}
		return gestureTracker;
	}

	public IEnumerator ReturnShotTutorial()
	{
		disableSwipe = true;
		gestureTrackerClear();
		while (shotParams == null)
		{
			yield return null;
		}
		Time.timeScale = shotParams.timeWhenShouldStartSwipe / 4f;
		base.match.ui.notification.ShowOnTransform(base.match.ui.ballMarker.transform, new Vector3(0f, 0f, 0f), "This Marker shows where you will hit the ball!");
		while (shotParams == null || shotParams.ballPossesionTime + fwdTime < shotParams.timeWhenShouldStartSwipe)
		{
			yield return null;
		}
		Time.timeScale = base.match.timeScale;
		Vector3 rightHand = shotParams.ballAimPosition;
		base.match.ShowTutorialNotification("Swipe from the marker in the direction you want the ball to go");
		Time.timeScale = 0f;
		base.match.ui.hand.onComplete = ChoseHandPosition;
		base.match.ui.hand.PointFromTo(rightHand, new Vector3(0f - base.table.halphwidth, rightHand.y, (0f - base.table.halphLength) * (float)base.tableSide));
		float time = 0f;
		while (time < 0.5f)
		{
			time += RealTime.deltaTime;
			yield return null;
		}
		disableSwipe = false;
		while (!CanCompleteSwing())
		{
			yield return null;
		}
		base.match.ui.hand.Hide();
		base.match.ui.HideNotification();
		Time.timeScale = base.match.timeScale;
	}

	public IEnumerator TimeShotTutorial()
	{
		Vector3 rightHand = paddleCenter.position;
		disableSwipe = true;
		gestureTrackerClear();
		while (shotParams == null)
		{
			yield return null;
		}
		Time.timeScale = shotParams.timeWhenShouldStartSwipe / 3f;
		base.match.ShowTutorialNotification("Swipe when the ball is close, not early or late! Wait for it...");
		while (shotParams.ballPossesionTime + fwdTime < shotParams.timeWhenShouldStartSwipe)
		{
			yield return null;
		}
		Time.timeScale = 0f;
		base.match.ui.hand.onComplete = ChoseHandPosition;
		base.match.ui.hand.PointFromTo(rightHand, new Vector3(0f - base.table.halphwidth, rightHand.y, (0f - base.table.halphLength) * (float)base.tableSide));
		float time = 0f;
		while (time < 0.5f)
		{
			time += RealTime.deltaTime;
			yield return null;
		}
		disableSwipe = false;
		while (!CanCompleteSwing())
		{
			yield return null;
		}
		base.match.ui.hand.Hide();
		base.match.ui.HideNotification();
		Time.timeScale = base.match.timeScale;
	}

	public IEnumerator ServeTutorial()
	{
		while (base.ball == null)
		{
			yield return null;
		}
		disableSwipe = true;
		base.match.ShowTutorialNotification("For a best shot, try to swipe while the marker is in the middle!");
		yield return new WaitForSeconds(0.5f);
		disableSwipe = false;
		while (!CanCompleteSwing())
		{
			yield return null;
		}
		Time.timeScale = base.match.timeScale;
		base.match.ui.HideNotification();
		base.match.ui.hand.Hide();
	}

	public override void StartServe(Ball ball)
	{
		ShowPointer(show: false);
		base.StartServe(ball);
		gestureTrackerClear();
		ball.MakeKinematic(isKinematic: true);
		float time = Mathf.Max(MoveTo(IdleServePosition(), PlayerBase.startServeServeMoveTag), 0.5f);
		trackingTouch = false;
		serveFlight = new ServeFlight();
		readyToTakeServe = false;
		this.WaitAndExecute(time, OnServePosition);
		SwingDescription swingDescription = swingDesc(SwingType.Serve);
		Vector3 vector = IdleServePosition();
		vector += swingDescription.displace;
		vector.y = myCamera.offsetFromServe.y;
		vector.z += (float)base.tableSide * myCamera.offsetFromServe.z;
		if (!showGame)
		{
			MoveCameraTo(vector);
		}
		timingPingPong.Start(0f, -1f, 1f, serveDifficultySpeed * 1.3f);
	}

	private void OnServePosition()
	{
		readyToTakeServe = true;
	}

	private new bool isDefenseShot(float x, float y)
	{
		Vector2 vector = new Vector2(x, y);
		vector.Normalize();
		return Mathf.Abs(vector.y) > 0.2f && Mathf.Sign(y) != (float)(-base.tableSide);
	}

	private HitParams HitParamsFromGestureTracker(float normalizedPenalty, bool isServe = false)
	{
		if (showGame || autoPilot)
		{
			return CreateHitParams(new Vector3(0f, 0f, (float)(-base.tableSide) * base.table.halphLength * 0.5f), 0f, isDefenseShot: false, isServe);
		}
		if (gestureTracker.Count() <= 1)
		{
			return CreateHitParams(new Vector3(0f, 0f, (float)(-base.tableSide) * base.table.halphLength * 0.5f), 0f, isDefenseShot: false, isServe);
		}
		MathEx.Range zRange = GetZRange(normalizedPenalty);
		Vector3 vector = gestureTracker.CalculateFirstToLastPoint();
		GestureTracker.OrtogonalDistance ortogonalDistance = gestureTracker.CalculateMaxOrtogonalDistance();
		float num = 0f;
		Vector3 vector2 = vector;
		float num2 = Mathf.Sign(vector.y);
		bool flag = true;
		bool flag2 = true;
		Vector3 position = base.ball.myTransform.position;
		float num3 = position.x;
		if (flag)
		{
			num3 = Mathf.Clamp(num3, 0f - base.table.halphwidth, base.table.halphwidth);
		}
		float num4 = base.table.halphwidth + Mathf.Abs(num3);
		float num5 = 0f;
		float num6 = 0f;
		bool flag3 = true;
		float value = GestureSpeeed(0f);
		float num7 = gestureSpeedRange.InverseLerp(value);
		bool isDefenseShot = false;
		if (flag3)
		{
			Camera component = myCamera.GetComponent<Camera>();
			Vector3 vector3 = gestureTracker.FirstPos();
			Ray ray = component.ScreenPointToRay(vector3);
			float enter = 0f;
			base.table.tablePlane.Raycast(ray, out enter);
			Vector3 point = ray.GetPoint(enter);
			Ray ray2 = component.ScreenPointToRay(vector3 + vector);
			base.table.tablePlane.Raycast(ray2, out enter);
			Vector3 point2 = ray2.GetPoint(enter);
			vector = RotateDirectionForSpin(point2 - point);
			vector = new Vector3(vector.x, vector.z, 0f);
			Vector3 normalized = vector.normalized;
			value = GestureSpeeed(Vector3.Distance(point, point2));
			num7 = gestureSpeedRange.InverseLerp(value);
			isDefenseShot = this.isDefenseShot(normalized.x, normalized.y);
			if (Mathf.Sign(normalized.y) != (float)(-base.tableSide))
			{
				vector.x *= -1f;
			}
			Vector3 normalized2 = vector.normalized;
			num6 = (float)(-base.tableSide) * base.table.halphLength * zRange.Lerp(Mathf.Abs(normalized.y));
			float num8 = (float)(-base.tableSide) * base.table.halphLength;
			Vector3 position2 = base.ball.myTransform.position;
			float num9 = num8 - position2.z;
			float num10 = num6;
			Vector3 position3 = base.ball.myTransform.position;
			num9 = num10 - position3.z;
			float num11 = vector.x / (Mathf.Sign(vector.y) * Mathf.Max(Mathf.Abs(vector.y), 0.05f)) * num9 * xSkew;
			num5 = num3 + num11;
		}
		if (isInTutorial)
		{
			num5 = Mathf.Clamp(num5, (0f - base.table.halphwidth) * 0.9f, base.table.halphwidth * 0.9f);
		}
		Vector3 landingPos = new Vector3(num5, 0f, num6);
		int num12 = 0;
		return CreateHitParams(landingPos, normalizedPenalty, isDefenseShot, isServe);
	}

	protected override void OnCanHitServeBall()
	{
		base.match.ui.ballMarker.SetActive(value: false);
		if (CanCompleteSwing())
		{
			base.match.ui.vertTimingSlider.gameObject.SetActive(value: false);
			float value = timingPingPong.value;
			float normalizedPenalty = Mathf.Abs(timingPingPong.value);
			HitParams hitParams = HitParamsFromGestureTracker(normalizedPenalty, isServe: true);
			Ball ball = base.ball;
			HitServe(hitParams);
			if (!showGame)
			{
				ShowTiming(normalizedPenalty, value, gestureTracker.LastPos(), hitParams.landingPosition, hitParams.spinX, ball, isServe: true);
				SetCameraAfterHit(ball, hitParams);
			}
		}
	}

	protected void ShowTiming(float normalizedPenalty, float timing, Vector3 position, Vector3 pointOnTable, float spinX, Ball ball, bool isServe)
	{
		ShotType shotType = GetShotType(isServe, normalizedPenalty);
		RacketSpeedParams racketSpeedParams = racketSpeedForShotType(shotType, isServe);
		position.z = 0f;
		base.match.ShowTiming(normalizedPenalty * (0f - Mathf.Sign(timing)), position, racketSpeedParams.IndexForPenalty(normalizedPenalty), base.table.isIn(this, ball, pointOnTable), spinX);
	}

	protected override void OnCanHitBall()
	{
		if (CanCompleteSwing())
		{
			base.match.ui.ballMarker.SetActive(value: false);
			SwingDescription swingDescription = swingDesc(shotParams.chosenSwing);
			float shotTimingDelay = shotParams.contactTime - swingDescription.delayTillHit + swingDescription.timeToPrepare - touchTime;
			float normalizedPenalty = NormalizedPenalty(shotTimingDelay, isInAir());
			HitParams hitParams = HitParamsFromGestureTracker(normalizedPenalty);
			Ball ball = base.ball;
			HitBall(hitParams);
			if (!showGame)
			{
				SetCameraAfterHit(ball, hitParams);
			}
		}
	}

	protected void SetCameraAfterHit(Ball localBall, HitParams hitParams)
	{
		Vector3 position = myCamera.transform.position;
		Vector3 landingPosition = hitParams.landingPosition;
		landingPosition.y = base.table.tabletopy;
		landingPosition.x *= 0.25f;
		Vector3 position2 = base.myTransform.position;
		landingPosition.z = position2.z * 1.2f;
		MoveCameraTo(landingPosition);
		myCamera.TrackTransform(localBall.myTransform, new Vector3(0f, 0.4f, 0f), new Vector3Range
		{
			min = new Vector3(0f - base.table.halphwidth, -20f, -5f),
			max = new Vector3(base.table.halphwidth, 20f, 5f)
		}, new Vector3(0f, 0f, 0f));
	}

	public override bool CanCompleteSwing()
	{
		if (showGame || autoPilot)
		{
			return true;
		}
		if (base.isInServe)
		{
			return gestureTracker.Count() >= 1;
		}
		return gestureTracker.Count() >= 2;
	}

	private float GetTiming()
	{
		if (shotParams != null)
		{
			SwingDescription swingDescription = swingDesc(shotParams.chosenSwing);
			return shotParams.contactTime - swingDescription.delayTillHit + swingDescription.timeToPrepare - touchTime;
		}
		if (serveFlight != null)
		{
			return serveFlight.timeToFinishSwing - touchTime;
		}
		return 0f;
	}

	private bool GetTrackedTouch(out Touch touch)
	{
		touch = default(Touch);
		if (UnityEngine.Input.touchCount == 0)
		{
			return false;
		}
		for (int i = 0; i < UnityEngine.Input.touchCount; i++)
		{
			Touch touch2 = UnityEngine.Input.GetTouch(i);
			if (touch2.fingerId == trackingFingerId)
			{
				touch = touch2;
				return true;
			}
		}
		return false;
	}

	private bool GetIsButtonDown()
	{
		if (trackingTouch)
		{
			return false;
		}
		trackingFingerId = -1;
		Camera component = myCamera.GetComponent<Camera>();
		if (UnityEngine.Input.touchCount != 0)
		{
			for (int i = 0; i < UnityEngine.Input.touchCount; i++)
			{
				Touch touch = UnityEngine.Input.GetTouch(i);
				if (touch.phase == TouchPhase.Began)
				{
					Vector3 vector = component.ScreenToViewportPoint(touch.position);
					if (!(vector.y > 1f) && !(vector.y < 0f) && !(vector.x > 1f) && !(vector.x < 0f))
					{
						trackingFingerId = touch.fingerId;
						return true;
					}
				}
			}
		}
		else if (Input.GetMouseButtonDown(0))
		{
			Vector3 vector2 = component.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
			return vector2.y <= 1f && vector2.y >= 0f && vector2.x <= 1f && vector2.x >= 0f;
		}
		return false;
	}

	private bool GetIsButtonUp()
	{
		if (!trackingTouch)
		{
			return false;
		}
		if (UnityEngine.Input.touchCount != 0)
		{
			if (!GetTrackedTouch(out Touch touch))
			{
				return false;
			}
			return touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
		}
		return Input.GetMouseButtonUp(0);
	}

	private Vector3 MousePosition()
	{
		if (UnityEngine.Input.touchCount != 0)
		{
			if (!GetTrackedTouch(out Touch touch))
			{
				return UnityEngine.Input.mousePosition;
			}
			return touch.position;
		}
		return UnityEngine.Input.mousePosition;
	}

	private float GestureSpeeed(float distance)
	{
		float num = swipeDuration;
		return distance / num;
	}

	private float RotationForSpin()
	{
		return 0f;
	}

	private Vector3 RotateDirectionForSpin(Vector3 direction)
	{
		return Quaternion.AngleAxis(RotationForSpin(), Vector3.up) * direction;
	}

	private void TryToTrackTouch(float time)
	{
		if (showGame || autoPilot || disableSwipe)
		{
			return;
		}
		if (base.match.isPaused)
		{
			trackingTouch = false;
			return;
		}
		GestureTracker gestureTracker = ActiveGestureTracker();
		if (GetIsButtonDown())
		{
			if (TEST)
			{
				float enter = 0f;
				Ray ray = myCamera.GetComponent<Camera>().ScreenPointToRay(MousePosition());
				base.table.tablePlane.Raycast(ray, out enter);
				Vector3 point = ray.GetPoint(enter);
				MoveTo(point);
				return;
			}
			if (gestureTracker == this.gestureTracker)
			{
				gestureTrackerClear();
				touchTime = time + fwdTime;
			}
			trackingTouch = true;
		}
		else if (trackingTouch && GetIsButtonUp())
		{
			trackingTouch = false;
		}
		if (time > touchTime - fwdTime + swipeDuration && CanCompleteSwing())
		{
			return;
		}
		if (UnityEngine.Input.touchCount > 1 && MatchController.InitParameters.gameMode != MatchController.GameMode.SplitScreen)
		{
			base.match.ui.alertText.text = "Use only one finger to control the game! " + UnityEngine.Input.touchCount + " Fingers are now touching...";
			base.match.ui.alertText.cachedGameObject.SetActive(value: true);
		}
		else if (base.match.ui.alertText.cachedGameObject.activeSelf)
		{
			base.match.ui.alertText.cachedGameObject.SetActive(value: false);
		}
		float num = 250f;
		float num2 = 500f;
		float num3 = num;
		if (!trackingTouch)
		{
			return;
		}
		Vector3 vector = MousePosition();
		Camera component = myCamera.GetComponent<Camera>();
		bool flag = CanCompleteSwing();
		gestureTracker.AddPos(new FingerPos
		{
			pos = vector,
			screenPos = vector,
			deltaTime = Time.deltaTime,
			realTime = time
		});
		if (gestureTracker == gestureTrackerSpin)
		{
			return;
		}
		if (!flag && CanCompleteSwing() && gestureTracker == this.gestureTracker)
		{
			touchTime = time + fwdTime;
			float timing = GetTiming();
			float normalizedPenalty = NormalizedPenalty(timing, isInAir());
			ShowTiming(normalizedPenalty, timing, this.gestureTracker.LastPos(), Vector3.zero, 0f, base.ball, base.isInServe);
		}
		if (!flag && CanCompleteSwing())
		{
			base.match.OnCanCompleteSwing(this);
		}
		if ((shotParams != null && shotParams.canShoot && CanCompleteSwing()) || serveFlight != null)
		{
			float timing2 = GetTiming();
			float normalizedPenalty2 = NormalizedPenalty(timing2, isInAir());
			int qualityIndex = racketSpeedForShotType(GetShotType(base.isInServe, normalizedPenalty2), base.isInServe).IndexForPenalty(normalizedPenalty2);
			Color color = base.match.ui.ColorForQuality(qualityIndex);
			pointer.color = color;
		}
		if (base.ball == null)
		{
			ShowPointer(show: false);
		}
		else if (CanCompleteSwing())
		{
			float enter2 = 0f;
			Ray ray2 = component.ScreenPointToRay(vector);
			base.table.tablePlane.Raycast(ray2, out enter2);
			Vector3 point2 = ray2.GetPoint(enter2);
			Ray ray3 = component.ScreenPointToRay(this.gestureTracker.FirstPos());
			base.table.tablePlane.Raycast(ray3, out enter2);
			Vector3 point3 = ray3.GetPoint(enter2);
			Vector3 vector2 = point2 - point3;
			vector2.y = 0f;
			Vector3 position = base.myTransform.position;
			if (shotParams != null)
			{
				position = shotParams.ballAimPosition;
			}
			else if (serveFlight != null)
			{
				position = serveFlight.ballPositionWhenHit;
			}
			position.y = base.table.tabletopy * 1.1f;
			num3 = 450f;
			if (point3 != point2)
			{
				Transform cachedTransform = pointer.cachedTransform;
				float num4 = vector2.x;
				float x = (float)(-base.tableSide) * Mathf.Abs(vector2.z);
				if (isDefenseShot(vector2.x, vector2.z))
				{
					num4 *= -1f;
				}
				float num5 = Mathf.Atan2(num4, x) * 57.29578f + 90f;
				cachedTransform.rotation = Quaternion.Euler(90f, num5, 0f);
				if (shotParams != null)
				{
					position = shotParams.ballAimPosition;
				}
				cachedTransform.position = position;
				float num6 = RotationForSpin();
				if (num6 != 0f)
				{
					helpPointer.cachedGameObject.SetActive(value: true);
					cachedTransform = helpPointer.cachedTransform;
					cachedTransform.rotation = Quaternion.Euler(90f, num5 + num6, 0f);
					cachedTransform.position = position;
				}
			}
		}
		pointer.width = (int)num3;
		helpPointer.width = (int)num3;
	}

	public void ShowPointer(bool show)
	{
		showPointer = show;
		if (showGame)
		{
			showPointer = false;
		}
		if (pointer.cachedGameObject.activeSelf != showPointer)
		{
			pointer.cachedGameObject.SetActive(showPointer);
			if (!showPointer && helpPointer.cachedGameObject.activeSelf != showPointer)
			{
				helpPointer.cachedGameObject.SetActive(showPointer);
			}
		}
	}

	private void UpdateNormal()
	{
		if (!(base.ball == null) && !base.match.isPaused && shotParams != null)
		{
			TryToTrackTouch(shotParams.ballPossesionTime);
			base.match.ui.ballMarker.transform.localScale = Vector3.Lerp(Vector3.one * 0.3f, Vector3.one * 0.06f, shotParams.ballPossesionTime / shotParams.contactTime);
			Material material = base.match.ui.ballMarker.GetComponent<Renderer>().material;
		}
	}

	private void UpdateServe()
	{
		if (!base.isInServe || !readyToTakeServe || disableSwipe)
		{
			return;
		}
		if (TEST)
		{
			TryToTrackTouch(serveFlight.ballPossesionTime);
			return;
		}
		if (showGame || autoPilot)
		{
			OnCanHitServeBall();
			return;
		}
		base.match.ui.vertTimingSlider.gameObject.SetActive(value: true);
		UITools.CenterOnTransform(base.match.ui.vertTimingSlider.slider, base.myTransform, myCamera.GetComponent<Camera>(), new Vector3(-1f, 0f, 0f));
		if (!serveFlight.thrownServeBall)
		{
			timingPingPong.Update();
		}
		base.match.ui.vertTimingSlider.SetPosition(timingPingPong.value);
		if (GetIsButtonDown() && !serveFlight.thrownServeBall)
		{
			serveFlight.ballPossesionTime = 0f;
			serveFlight.thrownServeBall = true;
		}
		else
		{
			serveFlight.ballPossesionTime += Time.deltaTime;
		}
		TryToTrackTouch(serveFlight.ballPossesionTime);
		if (serveFlight.ballPossesionTime >= 0.1f)
		{
			OnCanHitServeBall();
		}
	}

	public override void Update()
	{
		if (showGame || autoPilot)
		{
			UpdateServe();
			base.Update();
			return;
		}
		UpdateNormal();
		UpdateServe();
		base.Update();
		float b = (!showPointer) ? 0f : 0.85f;
		pointerAlpha = Mathf.Lerp(pointerAlpha, b, RealTime.deltaTime * 10f);
		if (!showPointer && pointerAlpha < 0.1f)
		{
			pointerAlpha = 0f;
		}
		pointer.alpha = pointerAlpha;
		helpPointer.alpha = pointerAlpha * 0.5f;
	}
}

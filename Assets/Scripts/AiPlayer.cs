using UnityEngine;

public class AiPlayer : PlayerBase
{
	protected float minReacionTime;

	protected PlayerBase opponent;

	protected bool goToNet;

	public SingleReadParam<bool> tutorialReturn = new SingleReadParam<bool>();

	private PlayerDeffinition.PlayerDef playerDeffinition;

	private MathEx.Range heightRange = new MathEx.Range
	{
		min = 0.5f,
		max = 0.5f
	};

	public static MathEx.Range netHeightRange = new MathEx.Range
	{
		min = 0.5f,
		max = 0.5f
	};

	private MathEx.Range serveHeightRange = new MathEx.Range
	{
		min = 0.1f,
		max = 1.2f
	};

	private MathEx.Range penaltyCanMissShotRange = new MathEx.Range
	{
		min = 0.5f,
		max = 1f
	};

	public int scoreDifference => base.match.ScoreDifference(base.playerTag);

	public void Init(MatchController match, int tag, int side, PlayerDeffinition.PlayerDef def, PlayerBase opponent, int flag)
	{
		base.Init(match, tag, side);
		this.opponent = opponent;
		playerDeffinition = def;
		if (def != null)
		{
			SetShoe(def.shoeIndex);
			SetRacket(def.racketIndex);
			maxSpeed *= def.speedMult;
			minSpeed *= def.speedMult;
			racketTimingMult = def.racketSpeedMult;
		}
		CharacterLook.instance.SetLook(def.look, base.transform);
	}

	public override void OnOtherPlayerHitBall(Ball ball)
	{
		base.OnOtherPlayerHitBall(ball);
		minReacionTime = playerDeffinition.reactionTime.Random(scoreDifference);
	}

	public override void StartServe(Ball ball)
	{
		base.StartServe(ball);
		minReacionTime = 0f;
		float a = MoveTo(IdleServePosition(), PlayerBase.startServeServeMoveTag);
		//((MonoBehaviour)this).WaitAndExecute(Mathf.Max(a, 0.5f), (VoidDelegate)((PlayerBase)this).OnCanHitServeBall);
		base.match.ui.trail.material.SetColor("_Color", Color.white);
	}

	public override void OnOtherPlayerStartServe()
	{
		base.OnOtherPlayerStartServe();
	}

	public override void OnPointWon(PlayerBase player)
	{
		base.OnPointWon(player);
	}

	public override bool CanCompleteSwing()
	{
		return base.CanCompleteSwing();
	}

	protected override bool WantsToMoveToNet()
	{
		return true;
	}

	protected HitParams AIHitParams(Vector3 landingPos, bool isServe)
	{
		Vector3 position = base.myTransform.position;
		Vector3 position2 = opponent.myTransform.position;
		Vector3 vector = position2;
		int num = (!(vector.x > 0f)) ? 1 : (-1);
		int num2 = (!(position.x > 0f)) ? 1 : (-1);
		GaussParams gaussParams = playerDeffinition.timing;
		if (isServe)
		{
			gaussParams = playerDeffinition.serveTiming;
		}
		else if (isDefenseShot)
		{
			gaussParams = playerDeffinition.defenseTiming;
		}
		else if (isInAir())
		{
			gaussParams = playerDeffinition.inAirTiming;
		}
		float shotTimingDelay = gaussParams.Random(scoreDifference);
		float num3 = NormalizedPenalty(shotTimingDelay, isInAir());
		UnityEngine.Debug.Log("Normalized penalty " + num3);
		ShotType shotType = GetShotType(isServe, num3);
		float num4 = 0f;
		switch (shotType)
		{
		case ShotType.LongShot:
		{
			int num8 = (playerDeffinition.longPositionReference != 0) ? num : num2;
			float num9 = playerDeffinition.horizontalPositionLong.Random(scoreDifference);
			num4 = (float)num8 * (1f - 2f * num9) * base.table.halphwidth;
			break;
		}
		case ShotType.Smash:
		{
			int num6 = (playerDeffinition.smashPositionReference != 0) ? num : num2;
			float num7 = playerDeffinition.horizontalPositionSmash.Random(scoreDifference);
			num4 = (float)num6 * (1f - 2f * num7) * base.table.halphwidth;
			break;
		}
		default:
		{
			float num5 = playerDeffinition.horizontalPosition.Random(scoreDifference);
			num4 = (float)num * (1f - 2f * num5) * base.table.halphwidth;
			break;
		}
		}
		num4 = (landingPos.x = Mathf.Clamp(num4, 0f - base.table.halphwidth, base.table.halphwidth));
		bool flag = false;
		if (shotParams != null)
		{
			float num10 = shotParams.ballPossesionTime - shotParams.timeToMoveToPosition;
			if (!(num10 < minReacionTime))
			{
			}
		}
		HitParams result = CreateHitParams(landingPos, num3, isDefenseShot: false, isServe);
		result.pressure *= playerDeffinition.pressureMult;
		return result;
	}

	protected override void OnCanHitBall()
	{
		base.match.ui.trail.material.SetColor("_Color", Color.white);
		bool isServe = false;
		HitParams p = AIHitParams(new Vector3(Random.Range(0f - base.table.halphwidth, base.table.halphwidth), 0f, (float)(-base.tableSide) * base.table.halphLength * Random.Range(0.3f, 0.99f)), isServe);
		HitBall(p);
		base.match.ui.showSpin(0f - p.spinX);
	}

	protected override void OnCanHitServeBall()
	{
		bool isServe = true;
		HitParams p = AIHitParams(new Vector3(Random.Range(0f - base.table.halphwidth, base.table.halphwidth), 0f, (float)(-base.tableSide) * base.table.halphLength * Random.Range(0.3f, 0.99f)), isServe);
		HitServe(p);
		base.match.ui.showSpin(0f - p.spinX);
	}
}

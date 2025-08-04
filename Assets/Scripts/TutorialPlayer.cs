using UnityEngine;

public class TutorialPlayer : PlayerBase
{
	public bool canReturnBall = true;

	public int maxReturns = -1;

	public bool useSpin;

	private MathEx.Range heightRange = new MathEx.Range
	{
		min = 1.5f,
		max = 1.4f
	};

	public new void Init(MatchController match, int tag, int side)
	{
		base.Init(match, tag, side);
		PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(0);
		if (playerDef != null)
		{
			SetShoe(playerDef.shoeIndex);
			SetRacket(playerDef.racketIndex);
		}
		CharacterLook.instance.SetLook(0, base.myTransform);
	}

	public override void OnOtherPlayerHitBall(Ball ball)
	{
		if (canReturnBall && (maxReturns < 0 || base.match.numShotsInPoint < maxReturns))
		{
			base.OnOtherPlayerHitBall(ball);
		}
	}

	public override void StartServe(Ball ball)
	{
		base.StartServe(ball);
		//((MonoBehaviour)this).WaitAndExecute(0.5f, (VoidDelegate)((PlayerBase)this).OnCanHitServeBall);
		base.match.ui.trail.material.SetColor("_Color", Color.white);
	}

	protected HitParams HitParams(bool isServe)
	{
		float num = 0f;
		float normalizedPenalty = 1f;
		Vector3 landingPos = new Vector3(0f, 0f, (float)(-base.tableSide) * base.table.halphLength * 0.5f);
		if (isServe)
		{
			landingPos.x = (float)(-base.courtSide) * base.table.halphwidth * 0.5f * (float)base.tableSide;
			landingPos.z = (float)(-base.tableSide) * base.table.serveLength * 0.9f;
		}
		float num2 = 0f;
		if (useSpin)
		{
			num2 = maxSpin * (float)((Random.Range(0, 100) < 50) ? 1 : (-1));
			landingPos.x = (float)(-base.tableSide) * Mathf.Sign(num2) * base.table.halphwidth * 0.5f;
		}
		base.match.ui.showSpin(num2);
		return CreateHitParams(landingPos, normalizedPenalty, isDefenseShot: false, isServe: false);
	}

	protected override void OnCanHitBall()
	{
		base.match.ui.trail.material.SetColor("_Color", Color.white);
		bool isServe = false;
		HitBall(HitParams(isServe));
	}

	protected override void OnCanHitServeBall()
	{
		bool isServe = true;
		HitServe(HitParams(isServe));
	}
}

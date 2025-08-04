using UnityEngine;

public class RemotePlayer : PlayerBase
{
	private bool readyToTakeServe;

	public bool remotePlayerCanCompleteSwing;

	private bool serve;

	public void Init(MatchController match, int tag, int side, int shoeIndex, int racketIndex, int lookIndex)
	{
		base.Init(match, tag, side);
		SetShoe(shoeIndex, isMultiplayer: true);
		SetRacket(racketIndex, isMultiplayer: true);
		CharacterLook.instance.SetPlayerLook(lookIndex, base.myTransform);
	}

	public override void OnOtherPlayerHitBall(Ball ball)
	{
		base.OnOtherPlayerHitBall(ball);
		remotePlayerCanCompleteSwing = false;
		serve = false;
	}

	protected override bool HorizontalDistanceOk()
	{
		return true;
	}

	public override void StartServe(Ball ball)
	{
		serve = true;
		base.StartServe(ball);
		remotePlayerCanCompleteSwing = false;
		float time = MoveTo(IdleServePosition());
		readyToTakeServe = false;
		this.WaitAndExecute(time, OnServePosition);
		base.match.ui.trail.material.SetColor("_Color", Color.white);
	}

	private void OnServePosition()
	{
		readyToTakeServe = true;
	}

	public void ThrowServeBall()
	{
		if (!readyToTakeServe)
		{
			readyToTakeServe = true;
			base.myTransform.position = IdleServePosition();
			moveInterpolator.StopMoving();
		}
		ThrowServeBallInAir();
	}

	public void Hit(MHitBall hit)
	{
		HitParams hitParams = default(HitParams);
		hitParams.landingPosition = hit.pointOnTable.Mirror();
		hitParams.time = hit.timeToLand;
		hitParams.heightOverTheNet = hit.height;
		hitParams.penalty = hit.penalty;
		hitParams.spinX = 0f - hit.spinX;
		hitParams.pressure = hit.pressure;
		HitParams p = hitParams;
		Vector3 position = hit.ballPosition.Mirror();
		base.ball.myTransform.position = position;
		base.myTransform.position = hit.myPosition.Mirror();
		UnityEngine.Debug.Log("My Pos " + base.myTransform.position + " ball landing " + p.landingPosition + " height " + p.heightOverTheNet + " time " + hit.timeToLand);
		myAnimator.SetBool("SwingFinish", value: true);
		if (serve)
		{
			UnityEngine.Debug.Log("Serve");
			serve = false;
			HitServe(p);
		}
		else
		{
			if (shotParams != null && shotParams.needsToJump)
			{
				if (hit.jumpInterpolatorTime == -1f)
				{
					jumpHeightInterpolator.isInJump = false;
				}
				else
				{
					jumpHeightInterpolator.Jump(0f, shotParams.myPositionForAim.y, shotParams.jumpDuration, 0.1f);
					jumpHeightInterpolator.time = hit.jumpInterpolatorTime;
				}
			}
			HitBall(p);
		}
		base.match.ui.trail.material.SetColor("_Color", Color.white);
	}

	public override bool CanCompleteSwing()
	{
		if (base.isInServe)
		{
			return readyToTakeServe && remotePlayerCanCompleteSwing;
		}
		return remotePlayerCanCompleteSwing;
	}

	protected override void OnCanHitBall()
	{
	}

	protected override void OnCanHitServeBall()
	{
	}
}

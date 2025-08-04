public class StandardDeserializer : GGNetworkDeserializer
{
	public delegate void OnAction();

	public delegate void OnAllAction(int type);

	public delegate void OnBallTiming(MBallTiming pos);

	public delegate void OnHitBall(MHitBall hit);

	public delegate void OnIntro(MIntro intro);

	public delegate void OnAwardPoint(MAwardPoint award);

	public delegate void OnFaultServe(MServeFault fault);

	public delegate void OnText(MText text);

	public OnBallTiming onBallTiming;

	public OnAction onStartServe;

	public OnAction onHandShake;

	public OnAllAction onAllActionMessages;

	public OnIntro onIntro;

	public OnHitBall onHitBall;

	public OnAwardPoint onAwardPoint;

	public OnFaultServe onFaultServe;

	public OnText onText;

	void GGNetworkDeserializer.Deserialize(int type, GGNetwork network)
	{
		switch (type)
		{
		case 3:
		case 4:
		case 5:
		case 9:
		case 10:
		case 12:
			break;
		case 0:
			CallOnActionMessage(onStartServe, type);
			break;
		case 7:
		{
			MBallTiming pos = default(MBallTiming);
			pos.Deserialize(network);
			if (onBallTiming != null)
			{
				onBallTiming(pos);
			}
			break;
		}
		case 6:
		{
			MHitBall hit = default(MHitBall);
			hit.Deserialize(network);
			if (onHitBall != null)
			{
				onHitBall(hit);
			}
			break;
		}
		case 1:
			CallOnActionMessage(onHandShake, type);
			break;
		case 2:
		{
			MIntro intro = default(MIntro);
			intro.Deserialize(network);
			if (onIntro != null)
			{
				onIntro(intro);
			}
			break;
		}
		case 8:
		{
			MAwardPoint award = default(MAwardPoint);
			award.Deserialize(network);
			if (onAwardPoint != null)
			{
				onAwardPoint(award);
			}
			break;
		}
		case 13:
		{
			MServeFault fault = default(MServeFault);
			fault.Deserialize(network);
			if (onFaultServe != null)
			{
				onFaultServe(fault);
			}
			break;
		}
		case 11:
		{
			MText text = default(MText);
			text.Deserialize(network);
			if (onText != null)
			{
				onText(text);
			}
			break;
		}
		}
	}

	private void CallOnActionMessage(OnAction action, int type)
	{
		if (onAllActionMessages != null)
		{
			onAllActionMessages(type);
		}
		action?.Invoke();
	}
}

using UnityEngine;

public struct MHitBall
{
	public Vector3 ballPosition;

	public Vector3 myPosition;

	public Vector3 opponentPosition;

	public Vector3 pointOnTable;

	public float height;

	public float timeToLand;

	public float penalty;

	public float spinX;

	public float pressure;

	public float jumpInterpolatorTime;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(6);
		network.AddVector3(ballPosition);
		network.AddVector3(myPosition);
		network.AddVector3(opponentPosition);
		network.AddVector3(pointOnTable);
		network.AddFloat(height);
		network.AddFloat(timeToLand);
		network.AddFloat(penalty);
		network.AddFloat(spinX);
		network.AddFloat(pressure);
		network.AddFloat(jumpInterpolatorTime);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		ballPosition = network.GetVector3();
		myPosition = network.GetVector3();
		opponentPosition = network.GetVector3();
		pointOnTable = network.GetVector3();
		height = network.GetFloat();
		timeToLand = network.GetFloat();
		penalty = network.GetFloat();
		spinX = network.GetFloat();
		pressure = network.GetFloat();
		jumpInterpolatorTime = network.GetFloat();
	}
}

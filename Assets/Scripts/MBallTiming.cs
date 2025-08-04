public struct MBallTiming
{
	public float possesionTime;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(7);
		network.AddFloat(possesionTime);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		possesionTime = network.GetFloat();
	}
}

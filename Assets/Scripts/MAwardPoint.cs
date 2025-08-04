public struct MAwardPoint
{
	public int playerTag;

	public int fault;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(8);
		network.AddInt(playerTag);
		network.AddInt(fault);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		playerTag = network.GetInt();
		fault = network.GetInt();
	}
}

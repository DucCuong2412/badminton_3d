public struct MServeFault
{
	public int playerTag;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(13);
		network.AddInt(playerTag);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		playerTag = network.GetInt();
	}
}

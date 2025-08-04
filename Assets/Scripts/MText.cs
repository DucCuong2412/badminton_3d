public struct MText
{
	public string text;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(11);
		network.AddString(text);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		text = network.GetString();
	}
}

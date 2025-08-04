public struct MIntro
{
	public int version;

	public float timeStamp;

	public int myFlag;

	public string myName;

	public int shoeIndex;

	public int racketIndex;

	public int score;

	public int multiplayerWins;

	public int playerLook;

	public int multiplayerLoses;

	public float multiplayerWinPercent
	{
		get
		{
			int num = multiplayerWins + multiplayerLoses;
			if (num == 0)
			{
				return 0f;
			}
			return (float)multiplayerWins / (float)num;
		}
	}

	public bool knowsLoses => version > 1;

	public void Send(GGNetwork network)
	{
		network.BeginWrite(2);
		network.AddInt(version);
		network.AddFloat(timeStamp);
		network.AddInt(myFlag);
		network.AddString(myName);
		network.AddInt(shoeIndex);
		network.AddInt(racketIndex);
		network.AddInt(score);
		network.AddInt(multiplayerWins);
		network.AddInt(playerLook);
		network.AddInt(multiplayerLoses);
		network.EndWrite();
		network.Send();
	}

	public void Deserialize(GGNetwork network)
	{
		version = network.GetInt();
		timeStamp = network.GetFloat();
		myFlag = network.GetInt();
		myName = network.GetString();
		if (network.curMessageProtocolVersion > 0)
		{
			shoeIndex = network.GetInt();
			racketIndex = network.GetInt();
			score = network.GetInt();
			multiplayerWins = network.GetInt();
		}
		if (version > 0)
		{
			playerLook = network.GetInt();
		}
		if (version > 1)
		{
			multiplayerLoses = network.GetInt();
		}
	}
}

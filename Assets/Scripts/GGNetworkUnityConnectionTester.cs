using UnityEngine;

public class GGNetworkUnityConnectionTester : Singleton<GGNetworkUnityConnectionTester>
{
	public string testMessage;

	private bool doneTesting;

	private bool probingPublicIP;

	//private ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;

	private float timer;

	private bool useNat;

	public bool isDoneTesting()
	{
		return doneTesting;
	}

	//public ConnectionTesterStatus TestConnection()
	//{
	//	if (doneTesting)
	//	{
	//		return connectionTestResult;
	//	}
	//	connectionTestResult = Network.TestConnection();
	//	switch (connectionTestResult)
	//	{
	//	case ConnectionTesterStatus.Error:
	//		testMessage = "Problem determining NAT capabilities";
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.Undetermined:
	//		testMessage = "Undetermined NAT capabilities";
	//		doneTesting = false;
	//		break;
	//	case ConnectionTesterStatus.PublicIPIsConnectable:
	//		testMessage = "Directly connectable public IP address.";
	//		useNat = false;
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.PublicIPPortBlocked:
	//		testMessage = "Non-connectable public IP address, running a server is impossible.";
	//		useNat = false;
	//		if (!probingPublicIP)
	//		{
	//			connectionTestResult = Network.TestConnectionNAT();
	//			probingPublicIP = true;
	//			timer = Time.time + 10f;
	//		}
	//		else if (Time.time > timer)
	//		{
	//			probingPublicIP = false;
	//			useNat = true;
	//			doneTesting = true;
	//		}
	//		break;
	//	case ConnectionTesterStatus.PublicIPNoServerStarted:
	//		testMessage = "Public IP address but server not initialized, it must be started to check server accessibility. Restart connection test when ready.";
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
	//		testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers. Running a server is ill advised as not everyone can connect.";
	//		useNat = true;
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
	//		testMessage = "Limited NAT punchthrough capabilities. Cannot connect to all types of NAT servers. Running a server is ill advised as not everyone can connect.";
	//		useNat = true;
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
	//		testMessage = "NAT punchthrough capable. Can connect to all servers and receive connections from all clients. Enabling NAT punchthrough functionality.";
	//		useNat = true;
	//		doneTesting = true;
	//		break;
	//	case ConnectionTesterStatus.NATpunchthroughFullCone:
	//		testMessage = "NAT punchthrough capable. Can connect to all servers and receive connections from all clients. Enabling NAT punchthrough functionality.";
	//		useNat = true;
	//		doneTesting = true;
	//		break;
	//	default:
	//		testMessage = "Error in test routine, got " + connectionTestResult;
	//		break;
	//	}
	//	return connectionTestResult;
//}
}

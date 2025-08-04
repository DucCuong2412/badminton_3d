using UnityEngine;

public class GGNotificationCenter : BehaviourSingleton<GGNotificationCenter>
{
	public delegate void GGNotificationCenterDelegate(string message);

	public event GGNotificationCenterDelegate onMessage;

	public void Broadcast(string message)
	{
		UnityEngine.Debug.Log("GGNotificationCenter.Broadcast('" + message + "')");
		this.onMessage(message);
	}
}

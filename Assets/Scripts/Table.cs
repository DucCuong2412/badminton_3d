using UnityEngine;

public class Table : MonoBehaviour
{
	public Transform tableTop;

	public Transform net;

	public float length;

	public float width;

	public float halphwidth;

	public float tabletopy;

	public float netHeight;

	public float halphLength;

	public Plane tablePlane;

	public float playgroundHalphWidth;

	public float playgroundHalphLength;

	public float serveLength;

	private void Awake()
	{
		float num = 0.6595f;
		Vector3 vector = new Vector3(5f, 0f, 11.76f);
		serveLength = 1.98f;
		length = vector.z;
		width = vector.x;
		tabletopy = 0f;
		netHeight = 2.35f * num;
		halphLength = length * 0.5f;
		halphwidth = width * 0.5f;
		tablePlane = new Plane(Vector3.up, new Vector3(0f, tabletopy, 0f));
		playgroundHalphWidth = 9.5f;
		playgroundHalphLength = 22.5f;
	}

	public bool isIn(PlayerBase player, Ball ball, Vector3 ballPos)
	{
		return Mathf.Abs(ballPos.z) <= halphLength && Mathf.Abs(ballPos.x) <= halphwidth;
	}

	public bool isIn(PlayerBase player, Ball ball)
	{
		Vector3 position = ball.myTransform.position;
		return isIn(player, ball, position);
	}

	public bool isInBoundsOfPlayground(Vector3 position)
	{
		return Mathf.Abs(position.x) < playgroundHalphWidth && Mathf.Abs(position.z) < playgroundHalphLength && position.y > -5f;
	}

	public bool isOnPlayerSide(PlayerBase player, Ball ball)
	{
		Vector3 position = ball.myTransform.position;
		return Mathf.Sign(position.z) == (float)player.tableSide;
	}
}

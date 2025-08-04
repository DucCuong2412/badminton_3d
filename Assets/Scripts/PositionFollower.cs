using UnityEngine;

public class PositionFollower : MonoBehaviour
{
	public Transform MyTransform;

	public Transform TrackedTransform;

	public Vector3 offset = Vector3.zero;

	public TrailRenderer myTrailRenderer;

	public bool disableRenderWhenBallNotInGame;

	private GameObject myGameObject;

	public MatchController match;

	private float trailTime;

	public bool disableTrail
	{
		get;
		set;
	}

	private void Start()
	{
		MyTransform = base.transform;
		myGameObject = base.gameObject;
		if (myTrailRenderer != null)
		{
			trailTime = myTrailRenderer.time;
		}
	}

	private void Update()
	{
		if (TrackedTransform == null)
		{
			return;
		}
		MyTransform.position = TrackedTransform.position + offset;
		if (disableTrail)
		{
			GetComponent<Renderer>().enabled = false;
		}
		else if (disableRenderWhenBallNotInGame && myTrailRenderer != null)
		{
			if (myTrailRenderer.enabled && !match.ballInGame)
			{
				GetComponent<Renderer>().enabled = false;
				trailTime = myTrailRenderer.time;
				myTrailRenderer.time = 0f;
			}
			else if (!myTrailRenderer.enabled && match.ballInGame)
			{
				GetComponent<Renderer>().enabled = true;
				myTrailRenderer.time = trailTime;
			}
		}
	}
}

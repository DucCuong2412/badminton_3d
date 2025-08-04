using UnityEngine;

public class SpinMarker : MonoBehaviour
{
	protected Transform cachedTransform_;

	public Vector3 scale = Vector3.one;

	public Transform camera;

	public float spin;

	public float angle;

	public float spinSpeed = 100f;

	public float trailTime;

	public TrailRenderer myTrailRenderer;

	public MatchController match;

	public Transform cachedTransform
	{
		get
		{
			if (cachedTransform_ == null)
			{
				cachedTransform_ = base.transform;
			}
			return cachedTransform_;
		}
	}

	public void SetColor(Color col)
	{
		myTrailRenderer.material.SetColor("_Color", col);
	}

	private void Awake()
	{
		if (myTrailRenderer != null)
		{
			trailTime = myTrailRenderer.time;
			myTrailRenderer.enabled = false;
		}
		Update();
	}

	private void Update()
	{
		if (camera == null)
		{
			return;
		}
		if (Mathf.Abs(spin) < 5f)
		{
			cachedTransform.localScale = Vector3.zero;
			return;
		}
		cachedTransform.localScale = scale;
		angle += Time.deltaTime * Mathf.Abs(spin) * spinSpeed;
		cachedTransform.LookAt(camera);
		Vector3 forward = camera.position - cachedTransform.position;
		cachedTransform.rotation = Quaternion.LookRotation(forward) * Quaternion.AngleAxis(angle, Mathf.Sign(spin) * Vector3.forward);
		Vector3 localScale = cachedTransform.localScale;
		localScale.x = (0f - Mathf.Sign(spin)) * scale.x;
		cachedTransform.localScale = localScale;
		if (!(match == null))
		{
			if (myTrailRenderer.enabled && !match.ballInGame)
			{
				trailTime = myTrailRenderer.time;
				myTrailRenderer.time = 0f;
			}
			else if (!myTrailRenderer.enabled && match.ballInGame)
			{
				myTrailRenderer.time = trailTime;
			}
		}
	}
}

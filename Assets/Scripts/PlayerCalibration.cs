using UnityEngine;

public class PlayerCalibration : MonoBehaviour
{
	private Transform myTransform;

	public Transform paddleCenter;

	private Animator anim;

	public Vector3 distance;

	public float relativeTime;

	public float nextRelativeTime;

	private void Awake()
	{
		myTransform = base.transform;
		if (paddleCenter == null)
		{
			paddleCenter = myTransform.Find("Racket/RacketCenter");
		}
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		distance = paddleCenter.position - myTransform.position;
		AnimatorStateInfo currentAnimatorStateInfo = anim.GetCurrentAnimatorStateInfo(0);
		AnimatorStateInfo nextAnimatorStateInfo = anim.GetNextAnimatorStateInfo(0);
		relativeTime = currentAnimatorStateInfo.normalizedTime;
		nextRelativeTime = nextAnimatorStateInfo.normalizedTime;
	}
}

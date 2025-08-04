using UnityEngine;

public class AnimationSetup : MonoBehaviour
{
	protected Animator myAnimator;

	protected Transform myTransform;

	public Vector3 strikeBoxDisplace;

	public float animationTime;

	public Transform strikeBox;

	public string trigger;

	private static Vector3 backhand = new Vector3(-1.969088f, 6.295623f, 3.317307f);

	private static Vector3 forehand = new Vector3(-4.063288f, 5.912395f, 1.382378f);

	private void Awake()
	{
		myAnimator = GetComponent<Animator>();
		myTransform = base.transform;
	}

	private void Start()
	{
		myAnimator.SetTrigger(trigger);
		UnityEngine.Debug.Break();
	}

	private void Update()
	{
		strikeBoxDisplace = strikeBox.position - myTransform.position;
		AnimatorClipInfo animatorClipInfo = myAnimator.GetCurrentAnimatorClipInfo(0)[0];
		animationTime = myAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
	}
}

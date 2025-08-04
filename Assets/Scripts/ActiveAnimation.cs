using AnimationOrTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animation))]
[AddComponentMenu("NGUI/Internal/Active Animation")]
public class ActiveAnimation : MonoBehaviour
{
	public static ActiveAnimation current;

	public List<EventDelegate> onFinished = new List<EventDelegate>();

	[HideInInspector]
	public GameObject eventReceiver;

	[HideInInspector]
	public string callWhenFinished;

	private Animation mAnim;

	private Direction mLastDirection;

	private Direction mDisableDirection;

	private bool mNotify;

	public bool isPlaying
	{
		get
		{
			if (mAnim == null)
			{
				return false;
			}
			IEnumerator enumerator = mAnim.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					AnimationState animationState = (AnimationState)enumerator.Current;
					if (mAnim.IsPlaying(animationState.name))
					{
						if (mLastDirection == Direction.Forward)
						{
							if (animationState.time < animationState.length)
							{
								return true;
							}
						}
						else
						{
							if (mLastDirection != Direction.Reverse)
							{
								return true;
							}
							if (animationState.time > 0f)
							{
								return true;
							}
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			return false;
		}
	}

	public void Reset()
	{
		if (mAnim != null)
		{
			IEnumerator enumerator = mAnim.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					AnimationState animationState = (AnimationState)enumerator.Current;
					if (mLastDirection == Direction.Reverse)
					{
						animationState.time = animationState.length;
					}
					else if (mLastDirection == Direction.Forward)
					{
						animationState.time = 0f;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	private void Start()
	{
		if (eventReceiver != null && EventDelegate.IsValid(onFinished))
		{
			eventReceiver = null;
			callWhenFinished = null;
		}
	}

	private void Update()
	{
		float deltaTime = RealTime.deltaTime;
		if (deltaTime == 0f)
		{
			return;
		}
		if (mAnim != null)
		{
			bool flag = false;
			IEnumerator enumerator = mAnim.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					AnimationState animationState = (AnimationState)enumerator.Current;
					if (mAnim.IsPlaying(animationState.name))
					{
						float num = animationState.speed * deltaTime;
						animationState.time += num;
						if (num < 0f)
						{
							if (animationState.time > 0f)
							{
								flag = true;
							}
							else
							{
								animationState.time = 0f;
							}
						}
						else if (animationState.time < animationState.length)
						{
							flag = true;
						}
						else
						{
							animationState.time = animationState.length;
						}
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			mAnim.Sample();
			if (flag)
			{
				return;
			}
			base.enabled = false;
			if (mNotify)
			{
				mNotify = false;
				current = this;
				EventDelegate.Execute(onFinished);
				if (eventReceiver != null && !string.IsNullOrEmpty(callWhenFinished))
				{
					eventReceiver.SendMessage(callWhenFinished, SendMessageOptions.DontRequireReceiver);
				}
				current = null;
				if (mDisableDirection != 0 && mLastDirection == mDisableDirection)
				{
					NGUITools.SetActive(base.gameObject, state: false);
				}
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void Play(string clipName, Direction playDirection)
	{
		if (!(mAnim != null))
		{
			return;
		}
		base.enabled = true;
		mAnim.enabled = false;
		if (playDirection == Direction.Toggle)
		{
			playDirection = ((mLastDirection != Direction.Forward) ? Direction.Forward : Direction.Reverse);
		}
		if (string.IsNullOrEmpty(clipName))
		{
			if (!mAnim.isPlaying)
			{
				mAnim.Play();
			}
		}
		else if (!mAnim.IsPlaying(clipName))
		{
			mAnim.Play(clipName);
		}
		IEnumerator enumerator = mAnim.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				AnimationState animationState = (AnimationState)enumerator.Current;
				if (string.IsNullOrEmpty(clipName) || animationState.name == clipName)
				{
					float num = Mathf.Abs(animationState.speed);
					animationState.speed = num * (float)playDirection;
					if (playDirection == Direction.Reverse && animationState.time == 0f)
					{
						animationState.time = animationState.length;
					}
					else if (playDirection == Direction.Forward && animationState.time == animationState.length)
					{
						animationState.time = 0f;
					}
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		mLastDirection = playDirection;
		mNotify = true;
		mAnim.Sample();
	}

	public static ActiveAnimation Play(Animation anim, string clipName, Direction playDirection, EnableCondition enableBeforePlay, DisableCondition disableCondition)
	{
		if (!NGUITools.GetActive(anim.gameObject))
		{
			if (enableBeforePlay != EnableCondition.EnableThenPlay)
			{
				return null;
			}
			NGUITools.SetActive(anim.gameObject, state: true);
			UIPanel[] componentsInChildren = anim.gameObject.GetComponentsInChildren<UIPanel>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				componentsInChildren[i].Refresh();
			}
		}
		ActiveAnimation activeAnimation = anim.GetComponent<ActiveAnimation>();
		if (activeAnimation == null)
		{
			activeAnimation = anim.gameObject.AddComponent<ActiveAnimation>();
		}
		activeAnimation.mAnim = anim;
		activeAnimation.mDisableDirection = (Direction)disableCondition;
		activeAnimation.onFinished.Clear();
		activeAnimation.Play(clipName, playDirection);
		return activeAnimation;
	}

	public static ActiveAnimation Play(Animation anim, string clipName, Direction playDirection)
	{
		return Play(anim, clipName, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
	}

	public static ActiveAnimation Play(Animation anim, Direction playDirection)
	{
		return Play(anim, null, playDirection, EnableCondition.DoNothing, DisableCondition.DoNotDisable);
	}
}

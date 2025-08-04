using System.Collections;
using UnityEngine;

public static class MonoBehaviourExtensions
{
	public static void ResetTransformation(this Transform trans)
	{
		trans.position = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = new Vector3(1f, 1f, 1f);
	}

	public static void WaitAndExecute<T>(this MonoBehaviour mb, float time, SingleParameterDelegate<T> d, T param)
	{
		if (time <= 0f)
		{
			d?.Invoke(param);
		}
		else
		{
			mb.StartCoroutine(mb.DoWaitAndExecute(time, d, param));
		}
	}

	private static IEnumerator DoWaitAndExecute<T>(this MonoBehaviour mb, float time, SingleParameterDelegate<T> d, T param)
	{
		yield return new WaitForSeconds(time);
		d?.Invoke(param);
	}

	public static void WaitAndExecute(this MonoBehaviour mb, float time, VoidDelegate d)
	{
		if (time <= 0f)
		{
			d?.Invoke();
		}
		else
		{
			mb.StartCoroutine(mb.DoWaitAndExecute(time, d));
		}
	}

	private static IEnumerator DoWaitAndExecute(this MonoBehaviour mb, float time, VoidDelegate d)
	{
		yield return new WaitForSeconds(time);
		d?.Invoke();
	}
}

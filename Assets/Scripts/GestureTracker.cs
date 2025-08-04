using System.Collections.Generic;
using UnityEngine;

public class GestureTracker
{
	public struct OrtogonalDistance
	{
		public FingerPos pos;

		public float distance;
	}

	public float minDistance = 7f;

	protected LinkedList<FingerPos> fingerPosQueue = new LinkedList<FingerPos>();

	public int MaxQueueSize = 50;

	public Vector3 FirstPos()
	{
		if (fingerPosQueue.First == null)
		{
			return UnityEngine.Input.mousePosition;
		}
		FingerPos value = fingerPosQueue.First.Value;
		return value.screenPos;
	}

	public Vector3 LastPos()
	{
		if (fingerPosQueue.Last == null)
		{
			return UnityEngine.Input.mousePosition;
		}
		FingerPos value = fingerPosQueue.Last.Value;
		return value.screenPos;
	}

	public void AddPos(FingerPos pos)
	{
		LinkedListNode<FingerPos> last = fingerPosQueue.Last;
		if (last != null)
		{
			FingerPos value = last.Value;
			if (value.pos == pos.pos)
			{
				return;
			}
			FingerPos value2 = last.Value;
			if (Vector3.Distance(value2.pos, pos.pos) < minDistance)
			{
				return;
			}
		}
		fingerPosQueue.AddLast(pos);
		int num = Mathf.Max(0, fingerPosQueue.Count - MaxQueueSize);
		for (int i = 0; i < num; i++)
		{
			fingerPosQueue.RemoveFirst();
		}
	}

	public void Clear()
	{
		fingerPosQueue.Clear();
	}

	public int Count()
	{
		return fingerPosQueue.Count;
	}

	public Vector3 CalculateAverageVelocity()
	{
		Vector3 vector = Vector3.zero;
		if (fingerPosQueue.Count < 2)
		{
			return vector;
		}
		LinkedList<FingerPos>.Enumerator enumerator = fingerPosQueue.GetEnumerator();
		enumerator.MoveNext();
		FingerPos fingerPos = enumerator.Current;
		do
		{
			FingerPos current = enumerator.Current;
			vector += (current.pos - fingerPos.pos) / current.deltaTime / (fingerPosQueue.Count - 1);
			fingerPos = current;
		}
		while (enumerator.MoveNext());
		return vector;
	}

	public Vector3 CalculateFirstToLastPoint()
	{
		if (fingerPosQueue.Count < 2)
		{
			return Vector3.zero;
		}
		FingerPos value = fingerPosQueue.Last.Value;
		Vector3 pos = value.pos;
		FingerPos value2 = fingerPosQueue.First.Value;
		return pos - value2.pos;
	}

	public OrtogonalDistance CalculateMaxOrtogonalDistance()
	{
		OrtogonalDistance result = default(OrtogonalDistance);
		if (fingerPosQueue.Count < 3)
		{
			return result;
		}
		Vector3 vector = CalculateFirstToLastPoint();
		Vector3 normalized = new Vector3(vector.y, 0f - vector.x, 0f).normalized;
		int num = 0;
		FingerPos value = fingerPosQueue.First.Value;
		foreach (FingerPos item in fingerPosQueue)
		{
			FingerPos current = item;
			Vector3 rhs = current.pos - value.pos;
			rhs.z = 0f;
			float num2 = Vector3.Dot(normalized, rhs);
			if (Mathf.Abs(result.distance) < Mathf.Abs(num2))
			{
				result.pos = current;
				result.distance = num2;
			}
			num++;
		}
		return result;
	}

	public float FirstToLastTime()
	{
		if (fingerPosQueue.Count < 2)
		{
			return 0f;
		}
		FingerPos value = fingerPosQueue.Last.Value;
		float realTime = value.realTime;
		FingerPos value2 = fingerPosQueue.First.Value;
		return realTime - value2.realTime;
	}

	public float FirstToLastSpeed()
	{
		if (fingerPosQueue.Count < 2)
		{
			return 0f;
		}
		FingerPos value = fingerPosQueue.Last.Value;
		Vector3 pos = value.pos;
		FingerPos value2 = fingerPosQueue.First.Value;
		float magnitude = (pos - value2.pos).magnitude;
		FingerPos value3 = fingerPosQueue.Last.Value;
		float realTime = value3.realTime;
		FingerPos value4 = fingerPosQueue.First.Value;
		return magnitude / Mathf.Max(0.001f, realTime - value4.realTime);
	}
}

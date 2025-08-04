using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Table")]
public class UITable : UIWidgetContainer
{
	public delegate void OnReposition();

	public enum Direction
	{
		Down,
		Up
	}

	public int columns;

	public Direction direction;

	public bool sorted;

	public bool hideInactive = true;

	public bool keepWithinPanel;

	public Vector2 padding = Vector2.zero;

	public OnReposition onReposition;

	private UIPanel mPanel;

	private bool mInitDone;

	private bool mReposition;

	private List<Transform> mChildren = new List<Transform>();

	[CompilerGenerated]
	private static Comparison<Transform> _003C_003Ef__mg_0024cache0;

	public bool repositionNow
	{
		set
		{
			if (value)
			{
				mReposition = true;
				base.enabled = true;
			}
		}
	}

	public List<Transform> children
	{
		get
		{
			if (mChildren.Count == 0)
			{
				Transform transform = base.transform;
				mChildren.Clear();
				for (int i = 0; i < transform.childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if ((bool)child && (bool)child.gameObject && (!hideInactive || NGUITools.GetActive(child.gameObject)))
					{
						mChildren.Add(child);
					}
				}
				if (sorted)
				{
					mChildren.Sort(SortByName);
				}
			}
			return mChildren;
		}
	}

	public static int SortByName(Transform a, Transform b)
	{
		return string.Compare(a.name, b.name);
	}

	private void RepositionVariableSize(List<Transform> children)
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = (columns <= 0) ? 1 : (children.Count / columns + 1);
		int num4 = (columns <= 0) ? children.Count : columns;
		Bounds[,] array = new Bounds[num3, num4];
		Bounds[] array2 = new Bounds[num4];
		Bounds[] array3 = new Bounds[num3];
		int num5 = 0;
		int num6 = 0;
		int i = 0;
		for (int count = children.Count; i < count; i++)
		{
			Transform transform = children[i];
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(transform);
			Vector3 localScale = transform.localScale;
			bounds.min = Vector3.Scale(bounds.min, localScale);
			bounds.max = Vector3.Scale(bounds.max, localScale);
			array[num6, num5] = bounds;
			array2[num5].Encapsulate(bounds);
			array3[num6].Encapsulate(bounds);
			if (++num5 >= columns && columns > 0)
			{
				num5 = 0;
				num6++;
			}
		}
		num5 = 0;
		num6 = 0;
		int j = 0;
		for (int count2 = children.Count; j < count2; j++)
		{
			Transform transform2 = children[j];
			Bounds bounds2 = array[num6, num5];
			Bounds bounds3 = array2[num5];
			Bounds bounds4 = array3[num6];
			Vector3 localPosition = transform2.localPosition;
			float num7 = num;
			Vector3 extents = bounds2.extents;
			float num8 = num7 + extents.x;
			Vector3 center = bounds2.center;
			localPosition.x = num8 - center.x;
			float x = localPosition.x;
			Vector3 min = bounds2.min;
			float x2 = min.x;
			Vector3 min2 = bounds3.min;
			localPosition.x = x + (x2 - min2.x + padding.x);
			if (direction == Direction.Down)
			{
				float num9 = 0f - num2;
				Vector3 extents2 = bounds2.extents;
				float num10 = num9 - extents2.y;
				Vector3 center2 = bounds2.center;
				localPosition.y = num10 - center2.y;
				float y = localPosition.y;
				Vector3 max = bounds2.max;
				float y2 = max.y;
				Vector3 min3 = bounds2.min;
				float num11 = y2 - min3.y;
				Vector3 max2 = bounds4.max;
				float num12 = num11 - max2.y;
				Vector3 min4 = bounds4.min;
				localPosition.y = y + ((num12 + min4.y) * 0.5f - padding.y);
			}
			else
			{
				float num13 = num2;
				Vector3 extents3 = bounds2.extents;
				float y3 = extents3.y;
				Vector3 center3 = bounds2.center;
				localPosition.y = num13 + (y3 - center3.y);
				float y4 = localPosition.y;
				Vector3 max3 = bounds2.max;
				float y5 = max3.y;
				Vector3 min5 = bounds2.min;
				float num14 = y5 - min5.y;
				Vector3 max4 = bounds4.max;
				float num15 = num14 - max4.y;
				Vector3 min6 = bounds4.min;
				localPosition.y = y4 - ((num15 + min6.y) * 0.5f - padding.y);
			}
			float num16 = num;
			Vector3 max5 = bounds3.max;
			float x3 = max5.x;
			Vector3 min7 = bounds3.min;
			num = num16 + (x3 - min7.x + padding.x * 2f);
			transform2.localPosition = localPosition;
			if (++num5 >= columns && columns > 0)
			{
				num5 = 0;
				num6++;
				num = 0f;
				float num17 = num2;
				Vector3 size = bounds4.size;
				num2 = num17 + (size.y + padding.y * 2f);
			}
		}
	}

	[ContextMenu("Execute")]
	public void Reposition()
	{
		if (Application.isPlaying && !mInitDone && NGUITools.GetActive(this))
		{
			mReposition = true;
			return;
		}
		if (!mInitDone)
		{
			Init();
		}
		mReposition = false;
		Transform transform = base.transform;
		mChildren.Clear();
		List<Transform> children = this.children;
		if (children.Count > 0)
		{
			RepositionVariableSize(children);
		}
		if (keepWithinPanel && mPanel != null)
		{
			mPanel.ConstrainTargetToBounds(transform, immediate: true);
		}
		if (onReposition != null)
		{
			onReposition();
		}
	}

	private void Start()
	{
		Init();
		Reposition();
		base.enabled = false;
	}

	private void Init()
	{
		mInitDone = true;
		mPanel = NGUITools.FindInParents<UIPanel>(base.gameObject);
	}

	private void LateUpdate()
	{
		if (mReposition)
		{
			Reposition();
		}
		base.enabled = false;
	}
}

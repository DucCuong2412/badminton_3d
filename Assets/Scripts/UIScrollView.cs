using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Interaction/Scroll View")]
public class UIScrollView : MonoBehaviour
{
	public enum Movement
	{
		Horizontal,
		Vertical,
		Unrestricted,
		Custom
	}

	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring
	}

	public enum ShowCondition
	{
		Always,
		OnlyIfNeeded,
		WhenDragging
	}

	public delegate void OnDragFinished();

	public Movement movement;

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	public bool restrictWithinPanel = true;

	public bool disableDragIfFits;

	public bool smoothDragStart = true;

	public bool iOSDragEmulation = true;

	public float scrollWheelFactor = 0.25f;

	public float momentumAmount = 35f;

	public UIScrollBar horizontalScrollBar;

	public UIScrollBar verticalScrollBar;

	public ShowCondition showScrollBars = ShowCondition.OnlyIfNeeded;

	public Vector2 customMovement = new Vector2(1f, 0f);

	public Vector2 relativePositionOnReset = Vector2.zero;

	public OnDragFinished onDragFinished;

	[HideInInspector]
	[SerializeField]
	private Vector3 scale = new Vector3(1f, 0f, 0f);

	private Transform mTrans;

	private UIPanel mPanel;

	private Plane mPlane;

	private Vector3 mLastPos;

	private bool mPressed;

	private Vector3 mMomentum = Vector3.zero;

	private float mScroll;

	private Bounds mBounds;

	private bool mCalculatedBounds;

	private bool mShouldMove;

	private bool mIgnoreCallbacks;

	private int mDragID = -10;

	private Vector2 mDragStartOffset = Vector2.zero;

	private bool mDragStarted;

	public UIPanel panel => mPanel;

	public Bounds bounds
	{
		get
		{
			if (!mCalculatedBounds)
			{
				mCalculatedBounds = true;
				mTrans = base.transform;
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
			}
			return mBounds;
		}
	}

	public bool canMoveHorizontally => movement == Movement.Horizontal || movement == Movement.Unrestricted || (movement == Movement.Custom && customMovement.x != 0f);

	public bool canMoveVertically => movement == Movement.Vertical || movement == Movement.Unrestricted || (movement == Movement.Custom && customMovement.y != 0f);

	public virtual bool shouldMoveHorizontally
	{
		get
		{
			Vector3 size = bounds.size;
			float num = size.x;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				float num2 = num;
				Vector2 clipSoftness = mPanel.clipSoftness;
				num = num2 + clipSoftness.x * 2f;
			}
			return num > mPanel.width;
		}
	}

	public virtual bool shouldMoveVertically
	{
		get
		{
			Vector3 size = bounds.size;
			float num = size.y;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				float num2 = num;
				Vector2 clipSoftness = mPanel.clipSoftness;
				num = num2 + clipSoftness.y * 2f;
			}
			return num > mPanel.height;
		}
	}

	protected virtual bool shouldMove
	{
		get
		{
			if (!disableDragIfFits)
			{
				return true;
			}
			if (mPanel == null)
			{
				mPanel = GetComponent<UIPanel>();
			}
			Vector4 finalClipRegion = mPanel.finalClipRegion;
			Bounds bounds = this.bounds;
			float num = (finalClipRegion.z != 0f) ? (finalClipRegion.z * 0.5f) : ((float)Screen.width);
			float num2 = (finalClipRegion.w != 0f) ? (finalClipRegion.w * 0.5f) : ((float)Screen.height);
			if (canMoveHorizontally)
			{
				Vector3 min = bounds.min;
				if (min.x < finalClipRegion.x - num)
				{
					return true;
				}
				Vector3 max = bounds.max;
				if (max.x > finalClipRegion.x + num)
				{
					return true;
				}
			}
			if (canMoveVertically)
			{
				Vector3 min2 = bounds.min;
				if (min2.y < finalClipRegion.y - num2)
				{
					return true;
				}
				Vector3 max2 = bounds.max;
				if (max2.y > finalClipRegion.y + num2)
				{
					return true;
				}
			}
			return false;
		}
	}

	public Vector3 currentMomentum
	{
		get
		{
			return mMomentum;
		}
		set
		{
			mMomentum = value;
			mShouldMove = true;
		}
	}

	private void Awake()
	{
		mTrans = base.transform;
		mPanel = GetComponent<UIPanel>();
		if (mPanel.clipping == UIDrawCall.Clipping.None)
		{
			mPanel.clipping = UIDrawCall.Clipping.ConstrainButDontClip;
		}
		if (movement != Movement.Custom && scale.sqrMagnitude > 0.001f)
		{
			if (scale.x == 1f && scale.y == 0f)
			{
				movement = Movement.Horizontal;
			}
			else if (scale.x == 0f && scale.y == 1f)
			{
				movement = Movement.Vertical;
			}
			else if (scale.x == 1f && scale.y == 1f)
			{
				movement = Movement.Unrestricted;
			}
			else
			{
				movement = Movement.Custom;
				customMovement.x = scale.x;
				customMovement.y = scale.y;
			}
			scale = Vector3.zero;
		}
		if (Application.isPlaying)
		{
			UIPanel uIPanel = mPanel;
			uIPanel.onChange = (UIPanel.OnChangeDelegate)Delegate.Combine(uIPanel.onChange, new UIPanel.OnChangeDelegate(OnPanelChange));
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying && mPanel != null)
		{
			UIPanel uIPanel = mPanel;
			uIPanel.onChange = (UIPanel.OnChangeDelegate)Delegate.Remove(uIPanel.onChange, new UIPanel.OnChangeDelegate(OnPanelChange));
		}
	}

	private void OnPanelChange()
	{
		UpdateScrollbars(recalculateBounds: true);
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			UpdateScrollbars(recalculateBounds: true);
			if (horizontalScrollBar != null)
			{
				EventDelegate.Add(horizontalScrollBar.onChange, OnHorizontalBar);
				horizontalScrollBar.alpha = ((showScrollBars != 0 && !shouldMoveHorizontally) ? 0f : 1f);
			}
			if (verticalScrollBar != null)
			{
				EventDelegate.Add(verticalScrollBar.onChange, OnVerticalBar);
				verticalScrollBar.alpha = ((showScrollBars != 0 && !shouldMoveVertically) ? 0f : 1f);
			}
		}
	}

	public bool RestrictWithinBounds(bool instant)
	{
		return RestrictWithinBounds(instant, horizontal: true, vertical: true);
	}

	public bool RestrictWithinBounds(bool instant, bool horizontal, bool vertical)
	{
		Bounds bounds = this.bounds;
		Vector3 vector = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);
		if (!horizontal)
		{
			vector.x = 0f;
		}
		if (!vertical)
		{
			vector.y = 0f;
		}
		if (vector.magnitude > 1f)
		{
			if (!instant && dragEffect == DragEffect.MomentumAndSpring)
			{
				Vector3 pos = mTrans.localPosition + vector;
				pos.x = Mathf.Round(pos.x);
				pos.y = Mathf.Round(pos.y);
				SpringPanel.Begin(mPanel.gameObject, pos, 13f);
			}
			else
			{
				MoveRelative(vector);
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
			return true;
		}
		return false;
	}

	public void DisableSpring()
	{
		SpringPanel component = GetComponent<SpringPanel>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public virtual void UpdateScrollbars(bool recalculateBounds)
	{
		if (mPanel == null)
		{
			return;
		}
		if (horizontalScrollBar != null || verticalScrollBar != null)
		{
			if (recalculateBounds)
			{
				mCalculatedBounds = false;
				mShouldMove = shouldMove;
			}
			Bounds bounds = this.bounds;
			Vector2 vector = bounds.min;
			Vector2 vector2 = bounds.max;
			if (horizontalScrollBar != null && vector2.x > vector.x)
			{
				Vector4 finalClipRegion = mPanel.finalClipRegion;
				float num = finalClipRegion.z * 0.5f;
				if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
				{
					float num2 = num;
					Vector2 clipSoftness = mPanel.clipSoftness;
					num = num2 - clipSoftness.x;
				}
				float num3 = finalClipRegion.x - num;
				Vector3 min = bounds.min;
				float num4 = num3 - min.x;
				Vector3 max = bounds.max;
				float num5 = max.x - num - finalClipRegion.x;
				float num6 = vector2.x - vector.x;
				num4 = Mathf.Clamp01(num4 / num6);
				num5 = Mathf.Clamp01(num5 / num6);
				float num7 = num4 + num5;
				mIgnoreCallbacks = true;
				horizontalScrollBar.barSize = 1f - num7;
				horizontalScrollBar.value = ((!(num7 > 0.001f)) ? 0f : (num4 / num7));
				mIgnoreCallbacks = false;
			}
			if (verticalScrollBar != null && vector2.y > vector.y)
			{
				Vector4 finalClipRegion2 = mPanel.finalClipRegion;
				float num8 = finalClipRegion2.w * 0.5f;
				if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
				{
					float num9 = num8;
					Vector2 clipSoftness2 = mPanel.clipSoftness;
					num8 = num9 - clipSoftness2.y;
				}
				float num10 = finalClipRegion2.y - num8 - vector.y;
				float num11 = vector2.y - num8 - finalClipRegion2.y;
				float num12 = vector2.y - vector.y;
				num10 = Mathf.Clamp01(num10 / num12);
				num11 = Mathf.Clamp01(num11 / num12);
				float num13 = num10 + num11;
				mIgnoreCallbacks = true;
				verticalScrollBar.barSize = 1f - num13;
				verticalScrollBar.value = ((!(num13 > 0.001f)) ? 0f : (1f - num10 / num13));
				mIgnoreCallbacks = false;
			}
		}
		else if (recalculateBounds)
		{
			mCalculatedBounds = false;
		}
	}

	public virtual void SetDragAmount(float x, float y, bool updateScrollbars)
	{
		DisableSpring();
		Bounds bounds = this.bounds;
		Vector3 min = bounds.min;
		float x2 = min.x;
		Vector3 max = bounds.max;
		if (x2 == max.x)
		{
			return;
		}
		Vector3 min2 = bounds.min;
		float y2 = min2.y;
		Vector3 max2 = bounds.max;
		if (y2 == max2.y)
		{
			return;
		}
		Vector4 finalClipRegion = mPanel.finalClipRegion;
		finalClipRegion.x = Mathf.Round(finalClipRegion.x);
		finalClipRegion.y = Mathf.Round(finalClipRegion.y);
		finalClipRegion.z = Mathf.Round(finalClipRegion.z);
		finalClipRegion.w = Mathf.Round(finalClipRegion.w);
		float num = finalClipRegion.z * 0.5f;
		float num2 = finalClipRegion.w * 0.5f;
		Vector3 min3 = bounds.min;
		float num3 = min3.x + num;
		Vector3 max3 = bounds.max;
		float num4 = max3.x - num;
		Vector3 min4 = bounds.min;
		float num5 = min4.y + num2;
		Vector3 max4 = bounds.max;
		float num6 = max4.y - num2;
		if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			float num7 = num3;
			Vector2 clipSoftness = mPanel.clipSoftness;
			num3 = num7 - clipSoftness.x;
			float num8 = num4;
			Vector2 clipSoftness2 = mPanel.clipSoftness;
			num4 = num8 + clipSoftness2.x;
			float num9 = num5;
			Vector2 clipSoftness3 = mPanel.clipSoftness;
			num5 = num9 - clipSoftness3.y;
			float num10 = num6;
			Vector2 clipSoftness4 = mPanel.clipSoftness;
			num6 = num10 + clipSoftness4.y;
		}
		float f = Mathf.Lerp(num3, num4, x);
		float f2 = Mathf.Lerp(num6, num5, y);
		f = Mathf.Round(f);
		f2 = Mathf.Round(f2);
		if (!updateScrollbars)
		{
			Vector3 localPosition = mTrans.localPosition;
			if (canMoveHorizontally)
			{
				localPosition.x += finalClipRegion.x - f;
			}
			if (canMoveVertically)
			{
				localPosition.y += finalClipRegion.y - f2;
			}
			mTrans.localPosition = localPosition;
		}
		if (canMoveHorizontally)
		{
			finalClipRegion.x = f;
		}
		if (canMoveVertically)
		{
			finalClipRegion.y = f2;
		}
		Vector4 baseClipRegion = mPanel.baseClipRegion;
		mPanel.clipOffset = new Vector2(finalClipRegion.x - baseClipRegion.x, finalClipRegion.y - baseClipRegion.y);
		if (updateScrollbars)
		{
			UpdateScrollbars(recalculateBounds: false);
		}
	}

	[ContextMenu("Reset Clipping Position")]
	public void ResetPosition()
	{
		if (NGUITools.GetActive(this))
		{
			mCalculatedBounds = false;
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, updateScrollbars: false);
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, updateScrollbars: true);
		}
	}

	private void OnHorizontalBar()
	{
		if (!mIgnoreCallbacks)
		{
			float x = (!(horizontalScrollBar != null)) ? 0f : horizontalScrollBar.value;
			float y = (!(verticalScrollBar != null)) ? 0f : verticalScrollBar.value;
			SetDragAmount(x, y, updateScrollbars: false);
		}
	}

	private void OnVerticalBar()
	{
		if (!mIgnoreCallbacks)
		{
			float x = (!(horizontalScrollBar != null)) ? 0f : horizontalScrollBar.value;
			float y = (!(verticalScrollBar != null)) ? 0f : verticalScrollBar.value;
			SetDragAmount(x, y, updateScrollbars: false);
		}
	}

	public virtual void MoveRelative(Vector3 relative)
	{
		mTrans.localPosition += relative;
		Vector2 clipOffset = mPanel.clipOffset;
		clipOffset.x -= relative.x;
		clipOffset.y -= relative.y;
		mPanel.clipOffset = clipOffset;
		UpdateScrollbars(recalculateBounds: false);
	}

	public void MoveAbsolute(Vector3 absolute)
	{
		Vector3 a = mTrans.InverseTransformPoint(absolute);
		Vector3 b = mTrans.InverseTransformPoint(Vector3.zero);
		MoveRelative(a - b);
	}

	public void Press(bool pressed)
	{
		if (smoothDragStart && pressed)
		{
			mDragStarted = false;
			mDragStartOffset = Vector2.zero;
		}
		if (!base.enabled || !NGUITools.GetActive(base.gameObject))
		{
			return;
		}
		if (!pressed && mDragID == UICamera.currentTouchID)
		{
			mDragID = -10;
		}
		mCalculatedBounds = false;
		mShouldMove = shouldMove;
		if (!mShouldMove)
		{
			return;
		}
		mPressed = pressed;
		if (pressed)
		{
			mMomentum = Vector3.zero;
			mScroll = 0f;
			DisableSpring();
			mLastPos = UICamera.lastHit.point;
			mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);
			Vector2 clipOffset = mPanel.clipOffset;
			clipOffset.x = Mathf.Round(clipOffset.x);
			clipOffset.y = Mathf.Round(clipOffset.y);
			mPanel.clipOffset = clipOffset;
			Vector3 localPosition = mTrans.localPosition;
			localPosition.x = Mathf.Round(localPosition.x);
			localPosition.y = Mathf.Round(localPosition.y);
			mTrans.localPosition = localPosition;
		}
		else
		{
			if (restrictWithinPanel && mPanel.clipping != 0 && dragEffect == DragEffect.MomentumAndSpring)
			{
				RestrictWithinBounds(instant: false, canMoveHorizontally, canMoveVertically);
			}
			if ((!smoothDragStart || mDragStarted) && onDragFinished != null)
			{
				onDragFinished();
			}
		}
	}

	public void Drag()
	{
		if (!base.enabled || !NGUITools.GetActive(base.gameObject) || !mShouldMove)
		{
			return;
		}
		if (mDragID == -10)
		{
			mDragID = UICamera.currentTouchID;
		}
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;
		if (smoothDragStart && !mDragStarted)
		{
			mDragStarted = true;
			mDragStartOffset = UICamera.currentTouch.totalDelta;
		}
		Ray ray = (!smoothDragStart) ? UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos) : UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset);
		float enter = 0f;
		if (!mPlane.Raycast(ray, out enter))
		{
			return;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector = point - mLastPos;
		mLastPos = point;
		if (vector.x != 0f || vector.y != 0f)
		{
			vector = mTrans.InverseTransformDirection(vector);
			if (movement == Movement.Horizontal)
			{
				vector.y = 0f;
				vector.z = 0f;
			}
			else if (movement == Movement.Vertical)
			{
				vector.x = 0f;
				vector.z = 0f;
			}
			else if (movement == Movement.Unrestricted)
			{
				vector.z = 0f;
			}
			else
			{
				vector.Scale(customMovement);
			}
			vector = mTrans.TransformDirection(vector);
		}
		mMomentum = Vector3.Lerp(mMomentum, mMomentum + vector * (0.01f * momentumAmount), 0.67f);
		if (!iOSDragEmulation)
		{
			MoveAbsolute(vector);
		}
		else if (mPanel.CalculateConstrainOffset(bounds.min, bounds.max).magnitude > 1f)
		{
			MoveAbsolute(vector * 0.5f);
			mMomentum *= 0.5f;
		}
		else
		{
			MoveAbsolute(vector);
		}
		if (restrictWithinPanel && mPanel.clipping != 0 && dragEffect != DragEffect.MomentumAndSpring)
		{
			RestrictWithinBounds(instant: true, canMoveHorizontally, canMoveVertically);
		}
	}

	public void Scroll(float delta)
	{
		if (base.enabled && NGUITools.GetActive(base.gameObject) && scrollWheelFactor != 0f)
		{
			DisableSpring();
			mShouldMove = shouldMove;
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta))
			{
				mScroll = 0f;
			}
			mScroll += delta * scrollWheelFactor;
		}
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		float deltaTime = RealTime.deltaTime;
		if (showScrollBars != 0)
		{
			bool flag = false;
			bool flag2 = false;
			if (showScrollBars != ShowCondition.WhenDragging || mDragID != -10 || mMomentum.magnitude > 0.01f)
			{
				flag = shouldMoveVertically;
				flag2 = shouldMoveHorizontally;
			}
			if ((bool)verticalScrollBar)
			{
				float alpha = verticalScrollBar.alpha;
				alpha += ((!flag) ? ((0f - deltaTime) * 3f) : (deltaTime * 6f));
				alpha = Mathf.Clamp01(alpha);
				if (verticalScrollBar.alpha != alpha)
				{
					verticalScrollBar.alpha = alpha;
				}
			}
			if ((bool)horizontalScrollBar)
			{
				float alpha2 = horizontalScrollBar.alpha;
				alpha2 += ((!flag2) ? ((0f - deltaTime) * 3f) : (deltaTime * 6f));
				alpha2 = Mathf.Clamp01(alpha2);
				if (horizontalScrollBar.alpha != alpha2)
				{
					horizontalScrollBar.alpha = alpha2;
				}
			}
		}
		if (mShouldMove && !mPressed)
		{
			if (movement == Movement.Horizontal || movement == Movement.Unrestricted)
			{
				mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * 0.05f, 0f, 0f));
			}
			else if (movement == Movement.Vertical)
			{
				mMomentum -= mTrans.TransformDirection(new Vector3(0f, mScroll * 0.05f, 0f));
			}
			else
			{
				mMomentum -= mTrans.TransformDirection(new Vector3(mScroll * customMovement.x * 0.05f, mScroll * customMovement.y * 0.05f, 0f));
			}
			if (mMomentum.magnitude > 0.0001f)
			{
				mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, deltaTime);
				Vector3 absolute = NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
				MoveAbsolute(absolute);
				if (restrictWithinPanel && mPanel.clipping != 0)
				{
					RestrictWithinBounds(instant: false, canMoveHorizontally, canMoveVertically);
				}
				if (mMomentum.magnitude < 0.0001f && onDragFinished != null)
				{
					onDragFinished();
				}
				return;
			}
			mScroll = 0f;
			mMomentum = Vector3.zero;
		}
		else
		{
			mScroll = 0f;
		}
		NGUIMath.SpringDampen(ref mMomentum, 9f, deltaTime);
	}
}

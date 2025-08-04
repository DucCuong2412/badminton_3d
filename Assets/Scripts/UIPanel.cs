using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Panel")]
public class UIPanel : UIRect
{
    public enum DebugInfo
    {
        None,
        Gizmos,
        Geometry
    }

    public enum RenderQueue
    {
        Automatic,
        StartAt,
        Explicit
    }

    public delegate void OnChangeDelegate();

    public static BetterList<UIPanel> list = new BetterList<UIPanel>();

    public OnChangeDelegate onChange;

    public bool showInPanelTool = true;

    public bool generateNormals;

    public bool widgetsAreStatic;

    public bool cullWhileDragging;

    public bool alwaysOnScreen;

    public bool anchorOffset;

    public RenderQueue renderQueue;

    public int startingRenderQueue = 3000;

    [NonSerialized]
    public BetterList<UIWidget> widgets = new BetterList<UIWidget>();

    [NonSerialized]
    public BetterList<UIDrawCall> drawCalls = new BetterList<UIDrawCall>();

    [NonSerialized]
    public Matrix4x4 worldToLocal = Matrix4x4.identity;

    [HideInInspector]
    [SerializeField]
    private float mAlpha = 1f;

    [HideInInspector]
    [SerializeField]
    private UIDrawCall.Clipping mClipping;

    [HideInInspector]
    [SerializeField]
    private Vector4 mClipRange = new Vector4(0f, 0f, 300f, 200f);

    [HideInInspector]
    [SerializeField]
    private Vector2 mClipSoftness = new Vector2(4f, 4f);

    [HideInInspector]
    [SerializeField]
    private int mDepth;

    private bool mRebuild;

    private bool mResized;

    private static BetterList<Vector3> mVerts = new BetterList<Vector3>();

    private static BetterList<Vector3> mNorms = new BetterList<Vector3>();

    private static BetterList<Vector4> mTans = new BetterList<Vector4>();

    private static BetterList<Vector2> mUvs = new BetterList<Vector2>();

    private static BetterList<Color32> mCols = new BetterList<Color32>();

    private Camera mCam;

    private Vector2 mClipOffset = Vector2.zero;

    private float mCullTime;

    private float mUpdateTime;

    private int mMatrixFrame = -1;

    private int mLayer = -1;

    private static float[] mTemp = new float[4];

    private Vector2 mMin = Vector2.zero;

    private Vector2 mMax = Vector2.zero;

    private bool mHalfPixelOffset;

    private bool mSortWidgets;

    private static Vector3[] mCorners = new Vector3[4];

    private int mAlphaFrameID;

    private static int mUpdateFrame = -1;

    [CompilerGenerated]
    private static BetterList<UIPanel>.CompareFunc _003C_003Ef__mg_0024cache0;

    [CompilerGenerated]
    private static BetterList<UIPanel>.CompareFunc _003C_003Ef__mg_0024cache1;

    [CompilerGenerated]
    private static BetterList<UIWidget>.CompareFunc _003C_003Ef__mg_0024cache2;

    public static int nextUnusedDepth
    {
        get
        {
            int num = int.MinValue;
            for (int i = 0; i < list.size; i++)
            {
                num = Mathf.Max(num, list[i].depth);
            }
            return (num != int.MinValue) ? (num + 1) : 0;
        }
    }

    public override float alpha
    {
        get
        {
            return mAlpha;
        }
        set
        {
            float num = Mathf.Clamp01(value);
            if (mAlpha != num)
            {
                mAlpha = num;
                SetDirty();
            }
        }
    }

    public int depth
    {
        get
        {
            return mDepth;
        }
        set
        {
            if (mDepth != value)
            {
                mDepth = value;
                list.Sort(CompareFunc);
            }
        }
    }

    public float width
    {
        get
        {
            Vector2 viewSize = GetViewSize();
            return viewSize.x;
        }
    }

    public float height
    {
        get
        {
            Vector2 viewSize = GetViewSize();
            return viewSize.y;
        }
    }

    public bool halfPixelOffset => mHalfPixelOffset;

    public bool usedForUI => mCam != null && mCam.orthographic;

    public Vector3 drawCallOffset
    {
        get
        {
            if (mHalfPixelOffset && mCam != null && mCam.orthographic)
            {
                Vector2 windowSize = GetWindowSize();
                float num = 1f / windowSize.y / mCam.orthographicSize;
                return new Vector3(0f - num, num);
            }
            return Vector3.zero;
        }
    }

    public UIDrawCall.Clipping clipping
    {
        get
        {
            return mClipping;
        }
        set
        {
            if (mClipping != value)
            {
                mResized = true;
                mClipping = value;
                mMatrixFrame = -1;
            }
        }
    }

    public Vector2 clipOffset
    {
        get
        {
            return mClipOffset;
        }
        set
        {
            if (Mathf.Abs(mClipOffset.x - value.x) > 0.001f || Mathf.Abs(mClipOffset.y - value.y) > 0.001f)
            {
                mResized = true;
                mCullTime = ((mCullTime != 0f) ? (RealTime.time + 0.15f) : 0.001f);
                mClipOffset = value;
                mMatrixFrame = -1;
            }
        }
    }

    [Obsolete("Use 'finalClipRegion' or 'baseClipRegion' instead")]
    public Vector4 clipRange
    {
        get
        {
            return baseClipRegion;
        }
        set
        {
            baseClipRegion = value;
        }
    }

    public Vector4 baseClipRegion
    {
        get
        {
            return mClipRange;
        }
        set
        {
            if (Mathf.Abs(mClipRange.x - value.x) > 0.001f || Mathf.Abs(mClipRange.y - value.y) > 0.001f || Mathf.Abs(mClipRange.z - value.z) > 0.001f || Mathf.Abs(mClipRange.w - value.w) > 0.001f)
            {
                mResized = true;
                mCullTime = ((mCullTime != 0f) ? (RealTime.time + 0.15f) : 0.001f);
                mClipRange = value;
                mMatrixFrame = -1;
            }
        }
    }

    public Vector4 finalClipRegion
    {
        get
        {
            Vector2 viewSize = GetViewSize();
            if (mClipping != 0)
            {
                return new Vector4(mClipRange.x + mClipOffset.x, mClipRange.y + mClipOffset.y, viewSize.x, viewSize.y);
            }
            return new Vector4(0f, 0f, viewSize.x, viewSize.y);
        }
    }

    public Vector2 clipSoftness
    {
        get
        {
            return mClipSoftness;
        }
        set
        {
            if (mClipSoftness != value)
            {
                mClipSoftness = value;
            }
        }
    }

    public override Vector3[] localCorners
    {
        get
        {
            if (mClipping == UIDrawCall.Clipping.None)
            {
                Vector2 viewSize = GetViewSize();
                float num = -0.5f * viewSize.x;
                float num2 = -0.5f * viewSize.y;
                float x = num + viewSize.x;
                float y = num2 + viewSize.y;
                Transform transform = (!(mCam != null)) ? null : mCam.transform;
                if (transform != null)
                {
                    mCorners[0] = transform.TransformPoint(num, num2, 0f);
                    mCorners[1] = transform.TransformPoint(num, y, 0f);
                    mCorners[2] = transform.TransformPoint(x, y, 0f);
                    mCorners[3] = transform.TransformPoint(x, num2, 0f);
                    transform = base.cachedTransform;
                    for (int i = 0; i < 4; i++)
                    {
                        mCorners[i] = transform.InverseTransformPoint(mCorners[i]);
                    }
                }
                else
                {
                    mCorners[0] = new Vector3(num, num2);
                    mCorners[1] = new Vector3(num, y);
                    mCorners[2] = new Vector3(x, y);
                    mCorners[3] = new Vector3(x, num2);
                }
            }
            else
            {
                float num3 = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
                float num4 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
                float x2 = num3 + mClipRange.z;
                float y2 = num4 + mClipRange.w;
                mCorners[0] = new Vector3(num3, num4);
                mCorners[1] = new Vector3(num3, y2);
                mCorners[2] = new Vector3(x2, y2);
                mCorners[3] = new Vector3(x2, num4);
            }
            return mCorners;
        }
    }

    public override Vector3[] worldCorners
    {
        get
        {
            if (mClipping == UIDrawCall.Clipping.None)
            {
                Vector2 viewSize = GetViewSize();
                float num = -0.5f * viewSize.x;
                float num2 = -0.5f * viewSize.y;
                float x = num + viewSize.x;
                float y = num2 + viewSize.y;
                Transform transform = (!(mCam != null)) ? null : mCam.transform;
                if (transform != null)
                {
                    mCorners[0] = transform.TransformPoint(num, num2, 0f);
                    mCorners[1] = transform.TransformPoint(num, y, 0f);
                    mCorners[2] = transform.TransformPoint(x, y, 0f);
                    mCorners[3] = transform.TransformPoint(x, num2, 0f);
                }
            }
            else
            {
                float num3 = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
                float num4 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
                float x2 = num3 + mClipRange.z;
                float y2 = num4 + mClipRange.w;
                Transform cachedTransform = base.cachedTransform;
                mCorners[0] = cachedTransform.TransformPoint(num3, num4, 0f);
                mCorners[1] = cachedTransform.TransformPoint(num3, y2, 0f);
                mCorners[2] = cachedTransform.TransformPoint(x2, y2, 0f);
                mCorners[3] = cachedTransform.TransformPoint(x2, num4, 0f);
            }
            return mCorners;
        }
    }

    public static int CompareFunc(UIPanel a, UIPanel b)
    {
        if (a != b && a != null && b != null)
        {
            if (a.mDepth < b.mDepth)
            {
                return -1;
            }
            if (a.mDepth > b.mDepth)
            {
                return 1;
            }
            return (a.GetInstanceID() >= b.GetInstanceID()) ? 1 : (-1);
        }
        return 0;
    }

    public override Vector3[] GetSides(Transform relativeTo)
    {
        if (mClipping != 0 || anchorOffset)
        {
            Vector2 viewSize = GetViewSize();
            Vector2 vector = (mClipping == UIDrawCall.Clipping.None) ? Vector2.zero : ((Vector2)mClipRange + mClipOffset);
            float num = vector.x - 0.5f * viewSize.x;
            float num2 = vector.y - 0.5f * viewSize.y;
            float num3 = num + viewSize.x;
            float num4 = num2 + viewSize.y;
            float x = (num + num3) * 0.5f;
            float y = (num2 + num4) * 0.5f;
            Matrix4x4 localToWorldMatrix = base.cachedTransform.localToWorldMatrix;
            mCorners[0] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(num, y));
            mCorners[1] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(x, num4));
            mCorners[2] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(num3, y));
            mCorners[3] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(x, num2));
            if (relativeTo != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    mCorners[i] = relativeTo.InverseTransformPoint(mCorners[i]);
                }
            }
            return mCorners;
        }
        return base.GetSides(relativeTo);
    }

    public override float CalculateFinalAlpha(int frameID)
    {
        if (mAlphaFrameID != frameID)
        {
            mAlphaFrameID = frameID;
            UIRect parent = base.parent;
            finalAlpha = ((!(base.parent != null)) ? mAlpha : (parent.CalculateFinalAlpha(frameID) * mAlpha));
        }
        return finalAlpha;
    }

    public bool IsVisible(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        UpdateTransformMatrix();
        a = worldToLocal.MultiplyPoint3x4(a);
        b = worldToLocal.MultiplyPoint3x4(b);
        c = worldToLocal.MultiplyPoint3x4(c);
        d = worldToLocal.MultiplyPoint3x4(d);
        mTemp[0] = a.x;
        mTemp[1] = b.x;
        mTemp[2] = c.x;
        mTemp[3] = d.x;
        float num = Mathf.Min(mTemp);
        float num2 = Mathf.Max(mTemp);
        mTemp[0] = a.y;
        mTemp[1] = b.y;
        mTemp[2] = c.y;
        mTemp[3] = d.y;
        float num3 = Mathf.Min(mTemp);
        float num4 = Mathf.Max(mTemp);
        if (num2 < mMin.x)
        {
            return false;
        }
        if (num4 < mMin.y)
        {
            return false;
        }
        if (num > mMax.x)
        {
            return false;
        }
        if (num3 > mMax.y)
        {
            return false;
        }
        return true;
    }

    public bool IsVisible(Vector3 worldPos)
    {
        if (mAlpha < 0.001f)
        {
            return false;
        }
        if (mClipping == UIDrawCall.Clipping.None)
        {
            return true;
        }
        UpdateTransformMatrix();
        Vector3 vector = worldToLocal.MultiplyPoint3x4(worldPos);
        if (vector.x < mMin.x)
        {
            return false;
        }
        if (vector.y < mMin.y)
        {
            return false;
        }
        if (vector.x > mMax.x)
        {
            return false;
        }
        if (vector.y > mMax.y)
        {
            return false;
        }
        return true;
    }

    public bool IsVisible(UIRect w)
    {
        Vector3[] worldCorners = w.worldCorners;
        return IsVisible(worldCorners[0], worldCorners[1], worldCorners[2], worldCorners[3]);
    }

    [ContextMenu("Force Refresh")]
    public void RebuildAllDrawCalls()
    {
        mRebuild = true;
    }

    public void SetDirty()
    {
        for (int i = 0; i < drawCalls.size; i++)
        {
            drawCalls.buffer[i].isDirty = true;
        }
        Invalidate(includeChildren: true);
    }

    private void Awake()
    {
        mGo = base.gameObject;
        mTrans = base.transform;
        mHalfPixelOffset = (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.XBOX360 || /*Application.platform == RuntimePlatform.WindowsWebPlayer ||*/ Application.platform == RuntimePlatform.WindowsEditor);
        if (mHalfPixelOffset)
        {
            mHalfPixelOffset = (SystemInfo.graphicsShaderLevel < 40);
        }
    }

    protected override void OnStart()
    {
        mLayer = mGo.layer;
        UICamera uICamera = UICamera.FindCameraForLayer(mLayer);
        mCam = ((!(uICamera != null)) ? NGUITools.FindCameraForLayer(mLayer) : uICamera.cachedCamera);
    }

    protected override void OnInit()
    {
        base.OnInit();
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rigidbody = base.gameObject.AddComponent<Rigidbody>();
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }
        mRebuild = true;
        list.Add(this);
        list.Sort(CompareFunc);
    }

    protected override void OnDisable()
    {
        for (int i = 0; i < drawCalls.size; i++)
        {
            UIDrawCall.Destroy(drawCalls.buffer[i]);
        }
        drawCalls.Clear();
        list.Remove(this);
        if (list.size == 0)
        {
            UIDrawCall.ReleaseAll();
            mUpdateFrame = -1;
        }
        base.OnDisable();
    }

    private void UpdateTransformMatrix()
    {
        int frameCount = Time.frameCount;
        if (mMatrixFrame != frameCount)
        {
            mMatrixFrame = frameCount;
            worldToLocal = base.cachedTransform.worldToLocalMatrix;
            Vector2 vector = GetViewSize() * 0.5f;
            float num = mClipOffset.x + mClipRange.x;
            float num2 = mClipOffset.y + mClipRange.y;
            mMin.x = num - vector.x;
            mMin.y = num2 - vector.y;
            mMax.x = num + vector.x;
            mMax.y = num2 + vector.y;
        }
    }

    protected override void OnAnchor()
    {
        if (mClipping == UIDrawCall.Clipping.None)
        {
            return;
        }
        Transform cachedTransform = base.cachedTransform;
        Transform parent = cachedTransform.parent;
        Vector2 viewSize = GetViewSize();
        Vector2 vector = cachedTransform.localPosition;
        float num;
        float num2;
        float num3;
        float num4;
        if (leftAnchor.target == bottomAnchor.target && leftAnchor.target == rightAnchor.target && leftAnchor.target == topAnchor.target)
        {
            Vector3[] sides = leftAnchor.GetSides(parent);
            if (sides != null)
            {
                num = NGUIMath.Lerp(sides[0].x, sides[2].x, leftAnchor.relative) + (float)leftAnchor.absolute;
                num2 = NGUIMath.Lerp(sides[0].x, sides[2].x, rightAnchor.relative) + (float)rightAnchor.absolute;
                num3 = NGUIMath.Lerp(sides[3].y, sides[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute;
                num4 = NGUIMath.Lerp(sides[3].y, sides[1].y, topAnchor.relative) + (float)topAnchor.absolute;
            }
            else
            {
                Vector2 vector2 = GetLocalPos(leftAnchor, parent);
                num = vector2.x + (float)leftAnchor.absolute;
                num3 = vector2.y + (float)bottomAnchor.absolute;
                num2 = vector2.x + (float)rightAnchor.absolute;
                num4 = vector2.y + (float)topAnchor.absolute;
            }
        }
        else
        {
            if ((bool)leftAnchor.target)
            {
                Vector3[] sides2 = leftAnchor.GetSides(parent);
                if (sides2 != null)
                {
                    num = NGUIMath.Lerp(sides2[0].x, sides2[2].x, leftAnchor.relative) + (float)leftAnchor.absolute;
                }
                else
                {
                    Vector3 localPos = GetLocalPos(leftAnchor, parent);
                    num = localPos.x + (float)leftAnchor.absolute;
                }
            }
            else
            {
                num = mClipRange.x - 0.5f * viewSize.x;
            }
            if ((bool)rightAnchor.target)
            {
                Vector3[] sides3 = rightAnchor.GetSides(parent);
                if (sides3 != null)
                {
                    num2 = NGUIMath.Lerp(sides3[0].x, sides3[2].x, rightAnchor.relative) + (float)rightAnchor.absolute;
                }
                else
                {
                    Vector3 localPos2 = GetLocalPos(rightAnchor, parent);
                    num2 = localPos2.x + (float)rightAnchor.absolute;
                }
            }
            else
            {
                num2 = mClipRange.x + 0.5f * viewSize.x;
            }
            if ((bool)bottomAnchor.target)
            {
                Vector3[] sides4 = bottomAnchor.GetSides(parent);
                if (sides4 != null)
                {
                    num3 = NGUIMath.Lerp(sides4[3].y, sides4[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute;
                }
                else
                {
                    Vector3 localPos3 = GetLocalPos(bottomAnchor, parent);
                    num3 = localPos3.y + (float)bottomAnchor.absolute;
                }
            }
            else
            {
                num3 = mClipRange.y - 0.5f * viewSize.y;
            }
            if ((bool)topAnchor.target)
            {
                Vector3[] sides5 = topAnchor.GetSides(parent);
                if (sides5 != null)
                {
                    num4 = NGUIMath.Lerp(sides5[3].y, sides5[1].y, topAnchor.relative) + (float)topAnchor.absolute;
                }
                else
                {
                    Vector3 localPos4 = GetLocalPos(topAnchor, parent);
                    num4 = localPos4.y + (float)topAnchor.absolute;
                }
            }
            else
            {
                num4 = mClipRange.y + 0.5f * viewSize.y;
            }
        }
        num -= vector.x + mClipOffset.x;
        num2 -= vector.x + mClipOffset.x;
        num3 -= vector.y + mClipOffset.y;
        num4 -= vector.y + mClipOffset.y;
        float x = Mathf.Lerp(num, num2, 0.5f);
        float y = Mathf.Lerp(num3, num4, 0.5f);
        float num5 = num2 - num;
        float num6 = num4 - num3;
        float num7 = Mathf.Max(20f, mClipSoftness.x);
        float num8 = Mathf.Max(20f, mClipSoftness.y);
        if (num5 < num7)
        {
            num5 = num7;
        }
        if (num6 < num8)
        {
            num6 = num8;
        }
        baseClipRegion = new Vector4(x, y, num5, num6);
    }

    private void LateUpdate()
    {
        if (mUpdateFrame == Time.frameCount)
        {
            return;
        }
        mUpdateFrame = Time.frameCount;
        for (int i = 0; i < list.size; i++)
        {
            list[i].UpdateSelf();
        }
        int num = 3000;
        for (int j = 0; j < list.size; j++)
        {
            UIPanel uIPanel = list.buffer[j];
            if (uIPanel.renderQueue == RenderQueue.Automatic)
            {
                uIPanel.startingRenderQueue = num;
                uIPanel.UpdateDrawCalls();
                num += uIPanel.drawCalls.size;
            }
            else if (uIPanel.renderQueue == RenderQueue.StartAt)
            {
                uIPanel.UpdateDrawCalls();
                if (uIPanel.drawCalls.size != 0)
                {
                    num = Mathf.Max(num, uIPanel.startingRenderQueue + uIPanel.drawCalls.size);
                }
            }
            else
            {
                uIPanel.UpdateDrawCalls();
                if (uIPanel.drawCalls.size != 0)
                {
                    num = Mathf.Max(num, uIPanel.startingRenderQueue + 1);
                }
            }
        }
    }

    private void UpdateSelf()
    {
        if (!Application.isPlaying) return;
        mUpdateTime = RealTime.time;
        UpdateTransformMatrix();
        UpdateLayers();
        UpdateWidgets();
        if (mRebuild)
        {
            mRebuild = false;
            FillAllDrawCalls();
            return;
        }
        int num = 0;
        while (num < drawCalls.size)
        {
            UIDrawCall uIDrawCall = drawCalls.buffer[num];
            if (uIDrawCall.isDirty && !FillDrawCall(uIDrawCall))
            {
                UIDrawCall.Destroy(uIDrawCall);
                drawCalls.RemoveAt(num);
            }
            else
            {
                num++;
            }
        }
    }

    public void SortWidgets()
    {
        mSortWidgets = false;
        widgets.Sort(UIWidget.PanelCompareFunc);
    }

    private void FillAllDrawCalls()
    {
        for (int i = 0; i < drawCalls.size; i++)
        {
            UIDrawCall.Destroy(drawCalls.buffer[i]);
        }
        drawCalls.Clear();
        Material material = null;
        Texture texture = null;
        Shader shader = null;
        UIDrawCall uIDrawCall = null;
        if (mSortWidgets)
        {
            SortWidgets();
        }
        for (int j = 0; j < widgets.size; j++)
        {
            UIWidget uIWidget = widgets.buffer[j];
            if (uIWidget.isVisible && uIWidget.hasVertices)
            {
                Material material2 = uIWidget.material;
                Texture mainTexture = uIWidget.mainTexture;
                Shader shader2 = uIWidget.shader;
                if (material != material2 || texture != mainTexture || shader != shader2)
                {
                    if (mVerts.size != 0)
                    {
                        SubmitDrawCall(uIDrawCall);
                        uIDrawCall = null;
                    }
                    material = material2;
                    texture = mainTexture;
                    shader = shader2;
                }
                if (!(material != null) && !(shader != null) && !(texture != null))
                {
                    continue;
                }
                if (uIDrawCall == null)
                {
                    uIDrawCall = UIDrawCall.Create(this, material, texture, shader);
                    uIDrawCall.depthStart = uIWidget.depth;
                    uIDrawCall.depthEnd = uIDrawCall.depthStart;
                    uIDrawCall.panel = this;
                }
                else
                {
                    int depth = uIWidget.depth;
                    if (depth < uIDrawCall.depthStart)
                    {
                        uIDrawCall.depthStart = depth;
                    }
                    if (depth > uIDrawCall.depthEnd)
                    {
                        uIDrawCall.depthEnd = depth;
                    }
                }
                uIWidget.drawCall = uIDrawCall;
                if (generateNormals)
                {
                    uIWidget.WriteToBuffers(mVerts, mUvs, mCols, mNorms, mTans);
                }
                else
                {
                    uIWidget.WriteToBuffers(mVerts, mUvs, mCols, null, null);
                }
            }
            else
            {
                uIWidget.drawCall = null;
            }
        }
        if (mVerts.size != 0)
        {
            SubmitDrawCall(uIDrawCall);
        }
    }

    private void SubmitDrawCall(UIDrawCall dc)
    {
        drawCalls.Add(dc);
        dc.Set(mVerts, (!generateNormals) ? null : mNorms, (!generateNormals) ? null : mTans, mUvs, mCols);
        mVerts.Clear();
        mNorms.Clear();
        mTans.Clear();
        mUvs.Clear();
        mCols.Clear();
    }

    private bool FillDrawCall(UIDrawCall dc)
    {
        if (dc != null)
        {
            dc.isDirty = false;
            int num = 0;
            while (num < widgets.size)
            {
                UIWidget uIWidget = widgets[num];
                if (uIWidget == null)
                {
                    widgets.RemoveAt(num);
                    continue;
                }
                if (uIWidget.drawCall == dc)
                {
                    if (uIWidget.isVisible && uIWidget.hasVertices)
                    {
                        if (generateNormals)
                        {
                            uIWidget.WriteToBuffers(mVerts, mUvs, mCols, mNorms, mTans);
                        }
                        else
                        {
                            uIWidget.WriteToBuffers(mVerts, mUvs, mCols, null, null);
                        }
                    }
                    else
                    {
                        uIWidget.drawCall = null;
                    }
                }
                num++;
            }
            if (mVerts.size != 0)
            {
                dc.Set(mVerts, (!generateNormals) ? null : mNorms, (!generateNormals) ? null : mTans, mUvs, mCols);
                mVerts.Clear();
                mNorms.Clear();
                mTans.Clear();
                mUvs.Clear();
                mCols.Clear();
                return true;
            }
        }
        return false;
    }

    private void UpdateDrawCalls()
    {
        Transform cachedTransform = base.cachedTransform;
        Vector4 clipRange = Vector4.zero;
        bool usedForUI = this.usedForUI;
        if (clipping != 0)
        {
            Vector4 finalClipRegion = this.finalClipRegion;
            clipRange = new Vector4(finalClipRegion.x, finalClipRegion.y, finalClipRegion.z * 0.5f, finalClipRegion.w * 0.5f);
        }
        if (clipRange.z == 0f)
        {
            clipRange.z = (float)Screen.width * 0.5f;
        }
        if (clipRange.w == 0f)
        {
            clipRange.w = (float)Screen.height * 0.5f;
        }
        if (halfPixelOffset)
        {
            clipRange.x -= 0.5f;
            clipRange.y += 0.5f;
        }
        Vector3 vector;
        if (usedForUI)
        {
            Transform parent = base.cachedTransform.parent;
            vector = base.cachedTransform.localPosition;
            if (parent != null)
            {
                float num = Mathf.Round(vector.x);
                float num2 = Mathf.Round(vector.y);
                clipRange.x += vector.x - num;
                clipRange.y += vector.y - num2;
                vector.x = num;
                vector.y = num2;
                vector = parent.TransformPoint(vector);
            }
            vector += drawCallOffset;
        }
        else
        {
            vector = cachedTransform.position;
        }
        Quaternion rotation = cachedTransform.rotation;
        Vector3 lossyScale = cachedTransform.lossyScale;
        for (int i = 0; i < drawCalls.size; i++)
        {
            UIDrawCall uIDrawCall = drawCalls.buffer[i];
            Transform cachedTransform2 = uIDrawCall.cachedTransform;
            cachedTransform2.position = vector;
            cachedTransform2.rotation = rotation;
            cachedTransform2.localScale = lossyScale;
            uIDrawCall.renderQueue = startingRenderQueue + i;
            uIDrawCall.clipping = clipping;
            uIDrawCall.clipRange = clipRange;
            uIDrawCall.clipSoftness = mClipSoftness;
            uIDrawCall.alwaysOnScreen = (alwaysOnScreen && (mClipping == UIDrawCall.Clipping.None || mClipping == UIDrawCall.Clipping.ConstrainButDontClip));
        }
    }

    private void UpdateLayers()
    {
        if (mLayer != base.cachedGameObject.layer)
        {
            mLayer = mGo.layer;
            UICamera uICamera = UICamera.FindCameraForLayer(mLayer);
            mCam = ((!(uICamera != null)) ? NGUITools.FindCameraForLayer(mLayer) : uICamera.cachedCamera);
            NGUITools.SetChildLayer(base.cachedTransform, mLayer);
            for (int i = 0; i < drawCalls.size; i++)
            {
                drawCalls.buffer[i].gameObject.layer = mLayer;
            }
        }
    }

    private void UpdateWidgets()
    {
        bool flag = !cullWhileDragging && mCullTime > mUpdateTime;
        bool flag2 = false;
        int i = 0;
        for (int size = widgets.size; i < size; i++)
        {
            UIWidget uIWidget = widgets.buffer[i];
            if (!(uIWidget.panel == this) || !uIWidget.enabled)
            {
                continue;
            }
            int frameCount = Time.frameCount;
            if (uIWidget.UpdateTransform(frameCount) || mResized)
            {
                bool visible = flag || (mClipping == UIDrawCall.Clipping.None && !uIWidget.hideIfOffScreen) || (uIWidget.CalculateCumulativeAlpha(frameCount) > 0.001f && IsVisible(uIWidget));
                uIWidget.UpdateVisibility(visible);
            }
            if (!uIWidget.UpdateGeometry(frameCount))
            {
                continue;
            }
            flag2 = true;
            if (!mRebuild)
            {
                if (uIWidget.drawCall != null)
                {
                    uIWidget.drawCall.isDirty = true;
                }
                else
                {
                    FindDrawCall(uIWidget);
                }
            }
        }
        if (flag2 && onChange != null)
        {
            onChange();
        }
        mResized = false;
    }

    public UIDrawCall FindDrawCall(UIWidget w)
    {
        Material material = w.material;
        Texture mainTexture = w.mainTexture;
        int depth = w.depth;
        for (int i = 0; i < drawCalls.size; i++)
        {
            UIDrawCall uIDrawCall = drawCalls.buffer[i];
            int num = (i != 0) ? (drawCalls.buffer[i - 1].depthEnd + 1) : int.MinValue;
            int num2 = (i + 1 != drawCalls.size) ? (drawCalls.buffer[i + 1].depthStart - 1) : int.MaxValue;
            if (num > depth || num2 < depth)
            {
                continue;
            }
            if (uIDrawCall.baseMaterial == material && uIDrawCall.mainTexture == mainTexture)
            {
                if (w.isVisible)
                {
                    w.drawCall = uIDrawCall;
                    if (w.hasVertices)
                    {
                        uIDrawCall.isDirty = true;
                    }
                    return uIDrawCall;
                }
            }
            else
            {
                mRebuild = true;
            }
            return null;
        }
        mRebuild = true;
        return null;
    }

    public void AddWidget(UIWidget w)
    {
        if (widgets.size == 0)
        {
            widgets.Add(w);
        }
        else if (mSortWidgets)
        {
            widgets.Add(w);
            SortWidgets();
        }
        else if (UIWidget.PanelCompareFunc(w, widgets[0]) == -1)
        {
            widgets.Insert(0, w);
        }
        else
        {
            int num = widgets.size;
            while (num > 0)
            {
                if (UIWidget.PanelCompareFunc(w, widgets[--num]) == -1)
                {
                    continue;
                }
                widgets.Insert(num + 1, w);
                break;
            }
        }
        FindDrawCall(w);
    }

    public void RemoveWidget(UIWidget w)
    {
        if (widgets.Remove(w) && w.drawCall != null)
        {
            int depth = w.depth;
            if (depth == w.drawCall.depthStart || depth == w.drawCall.depthEnd)
            {
                mRebuild = true;
            }
            w.drawCall.isDirty = true;
            w.drawCall = null;
        }
    }

    public void Refresh()
    {
        mRebuild = true;
        if (list.size > 0)
        {
            list[0].LateUpdate();
        }
    }

    public virtual Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max)
    {
        Vector4 finalClipRegion = this.finalClipRegion;
        float num = finalClipRegion.z * 0.5f;
        float num2 = finalClipRegion.w * 0.5f;
        Vector2 minRect = new Vector2(min.x, min.y);
        Vector2 maxRect = new Vector2(max.x, max.y);
        Vector2 minArea = new Vector2(finalClipRegion.x - num, finalClipRegion.y - num2);
        Vector2 maxArea = new Vector2(finalClipRegion.x + num, finalClipRegion.y + num2);
        if (clipping == UIDrawCall.Clipping.SoftClip)
        {
            float x = minArea.x;
            Vector2 clipSoftness = this.clipSoftness;
            minArea.x = x + clipSoftness.x;
            float y = minArea.y;
            Vector2 clipSoftness2 = this.clipSoftness;
            minArea.y = y + clipSoftness2.y;
            float x2 = maxArea.x;
            Vector2 clipSoftness3 = this.clipSoftness;
            maxArea.x = x2 - clipSoftness3.x;
            float y2 = maxArea.y;
            Vector2 clipSoftness4 = this.clipSoftness;
            maxArea.y = y2 - clipSoftness4.y;
        }
        return NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
    }

    public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate)
    {
        Vector3 vector = CalculateConstrainOffset(targetBounds.min, targetBounds.max);
        if (vector.magnitude > 0f)
        {
            if (immediate)
            {
                target.localPosition += vector;
                targetBounds.center += vector;
                SpringPosition component = target.GetComponent<SpringPosition>();
                if (component != null)
                {
                    component.enabled = false;
                }
            }
            else
            {
                SpringPosition springPosition = SpringPosition.Begin(target.gameObject, target.localPosition + vector, 13f);
                springPosition.ignoreTimeScale = true;
                springPosition.worldSpace = false;
            }
            return true;
        }
        return false;
    }

    public bool ConstrainTargetToBounds(Transform target, bool immediate)
    {
        Bounds targetBounds = NGUIMath.CalculateRelativeWidgetBounds(base.cachedTransform, target);
        return ConstrainTargetToBounds(target, ref targetBounds, immediate);
    }

    public static UIPanel Find(Transform trans)
    {
        return Find(trans, createIfMissing: false, -1);
    }

    public static UIPanel Find(Transform trans, bool createIfMissing)
    {
        return Find(trans, createIfMissing, -1);
    }

    public static UIPanel Find(Transform trans, bool createIfMissing, int layer)
    {
        UIPanel uIPanel = null;
        while (uIPanel == null && trans != null)
        {
            uIPanel = trans.GetComponent<UIPanel>();
            if (uIPanel != null)
            {
                return uIPanel;
            }
            if (trans.parent == null)
            {
                break;
            }
            trans = trans.parent;
        }
        return (!createIfMissing) ? null : NGUITools.CreateUI(trans, advanced3D: false, layer);
    }

    private Vector2 GetWindowSize()
    {
        UIRoot root = base.root;
        Vector2 vector = new Vector2(Screen.width, Screen.height);
        if (root != null)
        {
            return vector * root.GetPixelSizeAdjustment(Screen.height);
        }
        return vector;
    }

    public Vector2 GetViewSize()
    {
        bool flag = mClipping != UIDrawCall.Clipping.None;
        Vector2 vector = (!flag) ? new Vector2(Screen.width, Screen.height) : new Vector2(mClipRange.z, mClipRange.w);
        if (!flag)
        {
            UIRoot root = base.root;
            if (root != null)
            {
                vector *= root.GetPixelSizeAdjustment(Screen.height);
            }
        }
        return vector;
    }
}

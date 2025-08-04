using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Label")]
public class UILabel : UIWidget
{
	public enum Effect
	{
		None,
		Shadow,
		Outline
	}

	public enum Overflow
	{
		ShrinkContent,
		ClampContent,
		ResizeFreely,
		ResizeHeight
	}

	public enum Crispness
	{
		Never,
		OnDesktop,
		Always
	}

	public Crispness keepCrispWhenShrunk = Crispness.OnDesktop;

	[HideInInspector]
	[SerializeField]
	private Font mTrueTypeFont;

	[HideInInspector]
	[SerializeField]
	private UIFont mFont;

	[Multiline(6)]
	[HideInInspector]
	[SerializeField]
	private string mText = string.Empty;

	[HideInInspector]
	[SerializeField]
	private int mFontSize = 16;

	[HideInInspector]
	[SerializeField]
	private FontStyle mFontStyle;

	[HideInInspector]
	[SerializeField]
	private bool mEncoding = true;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineCount;

	[HideInInspector]
	[SerializeField]
	private Effect mEffectStyle;

	[HideInInspector]
	[SerializeField]
	private Color mEffectColor = Color.black;

	[HideInInspector]
	[SerializeField]
	private NGUIText.SymbolStyle mSymbols = NGUIText.SymbolStyle.Uncolored;

	[HideInInspector]
	[SerializeField]
	private Vector2 mEffectDistance = Vector2.one;

	[HideInInspector]
	[SerializeField]
	private Overflow mOverflow;

	[HideInInspector]
	[SerializeField]
	private Material mMaterial;

	[HideInInspector]
	[SerializeField]
	private bool mApplyGradient;

	[HideInInspector]
	[SerializeField]
	private Color mGradientTop = Color.white;

	[HideInInspector]
	[SerializeField]
	private Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[HideInInspector]
	[SerializeField]
	private int mSpacingX;

	[HideInInspector]
	[SerializeField]
	private int mSpacingY;

	[HideInInspector]
	[SerializeField]
	private bool mShrinkToFit;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineWidth;

	[HideInInspector]
	[SerializeField]
	private int mMaxLineHeight;

	[HideInInspector]
	[SerializeField]
	private float mLineWidth;

	[HideInInspector]
	[SerializeField]
	private bool mMultiline = true;

	private Font mActiveTTF;

	private bool mShouldBeProcessed = true;

	private string mProcessedText;

	private bool mPremultiply;

	private Vector2 mCalculatedSize = Vector2.zero;

	private float mScale = 1f;

	private int mPrintedSize;

	private int mLastWidth;

	private int mLastHeight;

	private static BetterList<Vector3> mTempVerts = new BetterList<Vector3>();

	private static BetterList<int> mTempIndices = new BetterList<int>();

	private bool hasChanged
	{
		get
		{
			return mShouldBeProcessed;
		}
		set
		{
			if (value)
			{
				mChanged = true;
				mShouldBeProcessed = true;
			}
			else
			{
				mShouldBeProcessed = false;
			}
		}
	}

	public override bool isAnchoredHorizontally => base.isAnchoredHorizontally || mOverflow == Overflow.ResizeFreely;

	public override bool isAnchoredVertically => base.isAnchoredVertically || mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight;

	public override Material material
	{
		get
		{
			if (mMaterial != null)
			{
				return mMaterial;
			}
			if (mFont != null)
			{
				return mFont.material;
			}
			if (mTrueTypeFont != null)
			{
				return mTrueTypeFont.material;
			}
			return null;
		}
		set
		{
			if (mMaterial != value)
			{
				MarkAsChanged();
				mMaterial = value;
				MarkAsChanged();
			}
		}
	}

	[Obsolete("Use UILabel.bitmapFont instead")]
	public UIFont font
	{
		get
		{
			return bitmapFont;
		}
		set
		{
			bitmapFont = value;
		}
	}

	public UIFont bitmapFont
	{
		get
		{
			return mFont;
		}
		set
		{
			if (!(mFont != value))
			{
				return;
			}
			if (value != null && value.dynamicFont != null)
			{
				trueTypeFont = value.dynamicFont;
				return;
			}
			if (trueTypeFont != null)
			{
				trueTypeFont = null;
			}
			else
			{
				RemoveFromPanel();
			}
			mFont = value;
			MarkAsChanged();
		}
	}

	public Font trueTypeFont
	{
		get
		{
			return mTrueTypeFont;
		}
		set
		{
			if (mTrueTypeFont != value)
			{
				SetActiveFont(null);
				RemoveFromPanel();
				mTrueTypeFont = value;
				hasChanged = true;
				mFont = null;
				SetActiveFont(value);
				ProcessAndRequest();
				if (mActiveTTF != null)
				{
					base.MarkAsChanged();
				}
			}
		}
	}

	public UnityEngine.Object ambigiousFont
	{
		get
		{
			return (!(mFont != null)) ? ((UnityEngine.Object)mTrueTypeFont) : ((UnityEngine.Object)mFont);
		}
		set
		{
			UIFont uIFont = value as UIFont;
			if (uIFont != null)
			{
				bitmapFont = uIFont;
			}
			else
			{
				trueTypeFont = (value as Font);
			}
		}
	}

	public string text
	{
		get
		{
			return mText;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mText))
				{
					mText = string.Empty;
					hasChanged = true;
					ProcessAndRequest();
				}
			}
			else if (mText != value)
			{
				mText = value;
				hasChanged = true;
				ProcessAndRequest();
			}
		}
	}

	public int fontSize
	{
		get
		{
			if (mFont != null)
			{
				return mFont.defaultSize;
			}
			return mFontSize;
		}
		set
		{
			value = Mathf.Clamp(value, 0, 144);
			if (mFontSize != value)
			{
				mFontSize = value;
				hasChanged = true;
				ProcessAndRequest();
			}
		}
	}

	public FontStyle fontStyle
	{
		get
		{
			return mFontStyle;
		}
		set
		{
			if (mFontStyle != value)
			{
				mFontStyle = value;
				hasChanged = true;
				ProcessAndRequest();
			}
		}
	}

	public bool applyGradient
	{
		get
		{
			return mApplyGradient;
		}
		set
		{
			if (mApplyGradient != value)
			{
				mApplyGradient = value;
				MarkAsChanged();
			}
		}
	}

	public Color gradientTop
	{
		get
		{
			return mGradientTop;
		}
		set
		{
			if (mGradientTop != value)
			{
				mGradientTop = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public Color gradientBottom
	{
		get
		{
			return mGradientBottom;
		}
		set
		{
			if (mGradientBottom != value)
			{
				mGradientBottom = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public int spacingX
	{
		get
		{
			return mSpacingX;
		}
		set
		{
			if (mSpacingX != value)
			{
				mSpacingX = value;
				MarkAsChanged();
			}
		}
	}

	public int spacingY
	{
		get
		{
			return mSpacingY;
		}
		set
		{
			if (mSpacingY != value)
			{
				mSpacingY = value;
				MarkAsChanged();
			}
		}
	}

	private bool keepCrisp
	{
		get
		{
			if (trueTypeFont != null && keepCrispWhenShrunk != 0)
			{
				return keepCrispWhenShrunk == Crispness.Always;
			}
			return false;
		}
	}

	public bool supportEncoding
	{
		get
		{
			return mEncoding;
		}
		set
		{
			if (mEncoding != value)
			{
				mEncoding = value;
				hasChanged = true;
			}
		}
	}

	public NGUIText.SymbolStyle symbolStyle
	{
		get
		{
			return mSymbols;
		}
		set
		{
			if (mSymbols != value)
			{
				mSymbols = value;
				hasChanged = true;
			}
		}
	}

	public Overflow overflowMethod
	{
		get
		{
			return mOverflow;
		}
		set
		{
			if (mOverflow != value)
			{
				mOverflow = value;
				hasChanged = true;
			}
		}
	}

	[Obsolete("Use 'width' instead")]
	public int lineWidth
	{
		get
		{
			return base.width;
		}
		set
		{
			base.width = value;
		}
	}

	[Obsolete("Use 'height' instead")]
	public int lineHeight
	{
		get
		{
			return base.height;
		}
		set
		{
			base.height = value;
		}
	}

	public bool multiLine
	{
		get
		{
			return mMaxLineCount != 1;
		}
		set
		{
			if (mMaxLineCount != 1 != value)
			{
				mMaxLineCount = ((!value) ? 1 : 0);
				hasChanged = true;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			if (hasChanged)
			{
				ProcessText();
			}
			return base.localCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			if (hasChanged)
			{
				ProcessText();
			}
			return base.worldCorners;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			if (hasChanged)
			{
				ProcessText();
			}
			return base.drawingDimensions;
		}
	}

	public int maxLineCount
	{
		get
		{
			return mMaxLineCount;
		}
		set
		{
			if (mMaxLineCount != value)
			{
				mMaxLineCount = Mathf.Max(value, 0);
				hasChanged = true;
				if (overflowMethod == Overflow.ShrinkContent)
				{
					MakePixelPerfect();
				}
			}
		}
	}

	public Effect effectStyle
	{
		get
		{
			return mEffectStyle;
		}
		set
		{
			if (mEffectStyle != value)
			{
				mEffectStyle = value;
				hasChanged = true;
			}
		}
	}

	public Color effectColor
	{
		get
		{
			return mEffectColor;
		}
		set
		{
			if (mEffectColor != value)
			{
				mEffectColor = value;
				if (mEffectStyle != 0)
				{
					hasChanged = true;
				}
			}
		}
	}

	public Vector2 effectDistance
	{
		get
		{
			return mEffectDistance;
		}
		set
		{
			if (mEffectDistance != value)
			{
				mEffectDistance = value;
				hasChanged = true;
			}
		}
	}

	[Obsolete("Use 'overflowMethod == UILabel.Overflow.ShrinkContent' instead")]
	public bool shrinkToFit
	{
		get
		{
			return mOverflow == Overflow.ShrinkContent;
		}
		set
		{
			if (value)
			{
				overflowMethod = Overflow.ShrinkContent;
			}
		}
	}

	public string processedText
	{
		get
		{
			if (mLastWidth != mWidth || mLastHeight != mHeight)
			{
				mLastWidth = mWidth;
				mLastHeight = mHeight;
				mShouldBeProcessed = true;
			}
			if (hasChanged)
			{
				ProcessText();
			}
			return mProcessedText;
		}
	}

	public Vector2 printedSize
	{
		get
		{
			if (hasChanged)
			{
				ProcessText();
			}
			return mCalculatedSize;
		}
	}

	public override Vector2 localSize
	{
		get
		{
			if (hasChanged)
			{
				ProcessText();
			}
			return base.localSize;
		}
	}

	private bool isValid => mFont != null || mTrueTypeFont != null;

	private float pixelSize => (!(mFont != null)) ? 1f : mFont.pixelSize;

	protected override void OnInit()
	{
		base.OnInit();
		if (mTrueTypeFont == null && mFont != null && mFont.isDynamic)
		{
			mTrueTypeFont = mFont.dynamicFont;
			mFontSize = mFont.defaultSize;
			mFontStyle = mFont.dynamicFontStyle;
			mFont = null;
		}
		SetActiveFont(mTrueTypeFont);
	}

	protected override void OnDisable()
	{
		SetActiveFont(null);
		base.OnDisable();
	}

	protected void SetActiveFont(Font fnt)
	{
		if (mActiveTTF != fnt)
		{
			if (mActiveTTF != null)
			{
				Font font = mActiveTTF;
				font.textureRebuildCallback = (Font.FontTextureRebuildCallback)Delegate.Remove(font.textureRebuildCallback, new Font.FontTextureRebuildCallback(((UIWidget)this).MarkAsChanged));
			}
			mActiveTTF = fnt;
			if (mActiveTTF != null)
			{
				Font font2 = mActiveTTF;
				font2.textureRebuildCallback = (Font.FontTextureRebuildCallback)Delegate.Combine(font2.textureRebuildCallback, new Font.FontTextureRebuildCallback(((UIWidget)this).MarkAsChanged));
			}
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		if (hasChanged)
		{
			ProcessText();
		}
		return base.GetSides(relativeTo);
	}

	protected override void UpgradeFrom265()
	{
		ProcessText(legacyMode: true);
		if (mShrinkToFit)
		{
			overflowMethod = Overflow.ShrinkContent;
			mMaxLineCount = 0;
		}
		if (mMaxLineWidth != 0)
		{
			base.width = mMaxLineWidth;
			overflowMethod = ((mMaxLineCount > 0) ? Overflow.ResizeHeight : Overflow.ShrinkContent);
		}
		else
		{
			overflowMethod = Overflow.ResizeFreely;
		}
		if (mMaxLineHeight != 0)
		{
			base.height = mMaxLineHeight;
		}
		if (mFont != null)
		{
			int num = Mathf.RoundToInt((float)mFont.defaultSize * mFont.pixelSize);
			if (base.height < num)
			{
				base.height = num;
			}
		}
		mMaxLineWidth = 0;
		mMaxLineHeight = 0;
		mShrinkToFit = false;
		if (GetComponent<BoxCollider>() != null)
		{
			NGUITools.AddWidgetCollider(base.gameObject, considerInactive: true);
		}
	}

	protected override void OnAnchor()
	{
		if (mOverflow == Overflow.ResizeFreely)
		{
			if (base.isFullyAnchored)
			{
				mOverflow = Overflow.ShrinkContent;
			}
		}
		else if (mOverflow == Overflow.ResizeHeight && topAnchor.target != null && bottomAnchor.target != null)
		{
			mOverflow = Overflow.ShrinkContent;
		}
		base.OnAnchor();
	}

	private void ProcessAndRequest()
	{
		if (ambigiousFont != null)
		{
			ProcessText();
			if (mActiveTTF != null)
			{
				NGUIText.RequestCharactersInTexture(mActiveTTF, mText);
			}
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (mLineWidth > 0f)
		{
			mMaxLineWidth = Mathf.RoundToInt(mLineWidth);
			mLineWidth = 0f;
		}
		if (!mMultiline)
		{
			mMaxLineCount = 1;
			mMultiline = true;
		}
		mPremultiply = (material != null && material.shader != null && material.shader.name.Contains("Premultiplied"));
		ProcessAndRequest();
	}

	public override void MarkAsChanged()
	{
		hasChanged = true;
		base.MarkAsChanged();
	}

	private void ProcessText()
	{
		ProcessText(legacyMode: false);
	}

	private void ProcessText(bool legacyMode)
	{
		if (!isValid)
		{
			return;
		}
		mChanged = true;
		hasChanged = false;
		NGUIText.rectWidth = ((!legacyMode) ? base.width : ((mMaxLineWidth == 0) ? 1000000 : mMaxLineWidth));
		NGUIText.rectHeight = ((!legacyMode) ? base.height : ((mMaxLineHeight == 0) ? 1000000 : mMaxLineHeight));
		int value;
		if (legacyMode)
		{
			Vector3 localScale = base.cachedTransform.localScale;
			value = Mathf.RoundToInt(localScale.x);
		}
		else
		{
			value = fontSize;
		}
		mPrintedSize = Mathf.Abs(value);
		mScale = 1f;
		if (NGUIText.rectWidth < 1 || NGUIText.rectHeight < 0)
		{
			mProcessedText = string.Empty;
			return;
		}
		UpdateNGUIText(mPrintedSize, mWidth, mHeight);
		if (mOverflow == Overflow.ResizeFreely)
		{
			NGUIText.rectWidth = 1000000;
		}
		if (mOverflow == Overflow.ResizeFreely || mOverflow == Overflow.ResizeHeight)
		{
			NGUIText.rectHeight = 1000000;
		}
		if (mPrintedSize > 0)
		{
			bool keepCrisp = this.keepCrisp;
			int num = mPrintedSize;
			while (true)
			{
				if (num > 0)
				{
					if (keepCrisp)
					{
						mPrintedSize = num;
						NGUIText.fontSize = mPrintedSize;
					}
					else
					{
						mScale = (float)num / (float)mPrintedSize;
						NGUIText.fontScale = ((!(bitmapFont != null)) ? mScale : (mScale * bitmapFont.pixelSize));
					}
					NGUIText.Update(request: false);
					bool flag = NGUIText.WrapText(mText, out mProcessedText);
					if (mOverflow != 0 || flag)
					{
						break;
					}
					if (--num <= 1)
					{
						return;
					}
					num--;
					continue;
				}
				return;
			}
			if (mOverflow == Overflow.ResizeFreely)
			{
				mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
				mWidth = Mathf.Max(minWidth, Mathf.RoundToInt(mCalculatedSize.x));
				mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
				if ((mWidth & 1) == 1)
				{
					mWidth++;
				}
				if ((mHeight & 1) == 1)
				{
					mHeight++;
				}
			}
			else if (mOverflow == Overflow.ResizeHeight)
			{
				mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
				mHeight = Mathf.Max(minHeight, Mathf.RoundToInt(mCalculatedSize.y));
				if ((mHeight & 1) == 1)
				{
					mHeight++;
				}
			}
			else
			{
				mCalculatedSize = NGUIText.CalculatePrintedSize(mProcessedText);
			}
			if (legacyMode)
			{
				base.width = Mathf.RoundToInt(mCalculatedSize.x);
				base.height = Mathf.RoundToInt(mCalculatedSize.y);
				base.cachedTransform.localScale = Vector3.one;
			}
		}
		else
		{
			base.cachedTransform.localScale = Vector3.one;
			mProcessedText = string.Empty;
			mScale = 1f;
		}
	}

	public override void MakePixelPerfect()
	{
		if (ambigiousFont != null)
		{
			float num = (!(bitmapFont != null)) ? 1f : bitmapFont.pixelSize;
			Vector3 localPosition = base.cachedTransform.localPosition;
			localPosition.x = Mathf.RoundToInt(localPosition.x);
			localPosition.y = Mathf.RoundToInt(localPosition.y);
			localPosition.z = Mathf.RoundToInt(localPosition.z);
			base.cachedTransform.localPosition = localPosition;
			base.cachedTransform.localScale = Vector3.one;
			if (mOverflow == Overflow.ResizeFreely)
			{
				AssumeNaturalSize();
				return;
			}
			Overflow overflow = mOverflow;
			mOverflow = Overflow.ShrinkContent;
			ProcessText(legacyMode: false);
			mOverflow = overflow;
			int num2 = Mathf.RoundToInt(mCalculatedSize.x * num);
			int num3 = Mathf.RoundToInt(mCalculatedSize.y * num);
			if (bitmapFont != null)
			{
				num2 = Mathf.Max(bitmapFont.defaultSize);
				num3 = Mathf.Max(bitmapFont.defaultSize);
			}
			else
			{
				num2 = Mathf.Max(base.minWidth);
				num3 = Mathf.Max(base.minHeight);
			}
			if (base.width < num2)
			{
				base.width = num2;
			}
			if (base.height < num3)
			{
				base.height = num3;
			}
		}
		else
		{
			base.MakePixelPerfect();
		}
	}

	public void AssumeNaturalSize()
	{
		if (ambigiousFont != null)
		{
			ProcessText(legacyMode: false);
			float num = (!(bitmapFont != null)) ? 1f : bitmapFont.pixelSize;
			base.width = Mathf.RoundToInt(mCalculatedSize.x * num);
			base.height = Mathf.RoundToInt(mCalculatedSize.y * num);
			if ((base.width & 1) == 1)
			{
				base.width++;
			}
			if ((base.height & 1) == 1)
			{
				base.height++;
			}
		}
	}

	public int GetCharacterIndex(Vector3 worldPos)
	{
		Vector2 localPos = base.cachedTransform.InverseTransformPoint(worldPos);
		return GetCharacterIndex(localPos);
	}

	public int GetCharacterIndex(Vector2 localPos)
	{
		if (isValid)
		{
			string processedText = this.processedText;
			if (string.IsNullOrEmpty(processedText))
			{
				return 0;
			}
			UpdateNGUIText(fontSize, mWidth, mHeight);
			NGUIText.PrintCharacterPositions(processedText, mTempVerts, mTempIndices);
			if (mTempVerts.size > 0)
			{
				ApplyOffset(mTempVerts, 0);
				int closestCharacter = NGUIText.GetClosestCharacter(mTempVerts, localPos);
				closestCharacter = mTempIndices[closestCharacter];
				mTempVerts.Clear();
				mTempIndices.Clear();
				return closestCharacter;
			}
		}
		return 0;
	}

	public int GetCharacterIndex(int currentIndex, KeyCode key)
	{
		if (isValid)
		{
			string processedText = this.processedText;
			if (string.IsNullOrEmpty(processedText))
			{
				return 0;
			}
			UpdateNGUIText(fontSize, mWidth, mHeight);
			NGUIText.PrintCharacterPositions(processedText, mTempVerts, mTempIndices);
			if (mTempVerts.size > 0)
			{
				ApplyOffset(mTempVerts, 0);
				for (int i = 0; i < mTempIndices.size; i++)
				{
					if (mTempIndices[i] == currentIndex)
					{
						Vector2 pos = mTempVerts[i];
						switch (key)
						{
						case KeyCode.UpArrow:
							pos.y += fontSize + spacingY;
							break;
						case KeyCode.DownArrow:
							pos.y -= fontSize + spacingY;
							break;
						case KeyCode.Home:
							pos.x -= 1000f;
							break;
						case KeyCode.End:
							pos.x += 1000f;
							break;
						}
						int closestCharacter = NGUIText.GetClosestCharacter(mTempVerts, pos);
						closestCharacter = mTempIndices[closestCharacter];
						if (closestCharacter == currentIndex)
						{
							break;
						}
						mTempVerts.Clear();
						mTempIndices.Clear();
						return closestCharacter;
					}
				}
				mTempVerts.Clear();
				mTempIndices.Clear();
			}
			switch (key)
			{
			case KeyCode.UpArrow:
			case KeyCode.Home:
				return 0;
			case KeyCode.DownArrow:
			case KeyCode.End:
				return processedText.Length;
			}
		}
		return currentIndex;
	}

	public void PrintOverlay(int start, int end, UIGeometry caret, UIGeometry highlight, Color caretColor, Color highlightColor)
	{
		caret?.Clear();
		highlight?.Clear();
		if (!isValid)
		{
			return;
		}
		string processedText = this.processedText;
		UpdateNGUIText(fontSize, mWidth, mHeight);
		int size = caret.verts.size;
		Vector2 item = new Vector2(0.5f, 0.5f);
		float finalAlpha = base.finalAlpha;
		if (highlight != null && start != end)
		{
			int size2 = highlight.verts.size;
			NGUIText.PrintCaretAndSelection(processedText, start, end, caret.verts, highlight.verts);
			if (highlight.verts.size > size2)
			{
				ApplyOffset(highlight.verts, size2);
				Color32 item2 = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * finalAlpha);
				for (int i = size2; i < highlight.verts.size; i++)
				{
					highlight.uvs.Add(item);
					highlight.cols.Add(item2);
				}
			}
		}
		else
		{
			NGUIText.PrintCaretAndSelection(processedText, start, end, caret.verts, null);
		}
		ApplyOffset(caret.verts, size);
		Color32 item3 = new Color(caretColor.r, caretColor.g, caretColor.b, caretColor.a * finalAlpha);
		for (int j = size; j < caret.verts.size; j++)
		{
			caret.uvs.Add(item);
			caret.cols.Add(item3);
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
		if (!isValid)
		{
			return;
		}
		int size = verts.size;
		Color color = base.color;
		color.a = finalAlpha;
		if (mFont != null && mFont.premultipliedAlpha)
		{
			color = NGUITools.ApplyPMA(color);
		}
		string processedText = this.processedText;
		float num = (!(mFont != null)) ? 1f : mFont.pixelSize;
		int size2 = verts.size;
		UpdateNGUIText(fontSize, mWidth, mHeight);
		NGUIText.tint = color;
		NGUIText.Print(processedText, verts, uvs, cols);
		Vector2 vector = ApplyOffset(verts, size2);
		if (effectStyle != 0)
		{
			int size3 = verts.size;
			float num2 = num;
			vector.x = num2 * mEffectDistance.x;
			vector.y = num2 * mEffectDistance.y;
			ApplyShadow(verts, uvs, cols, size, size3, vector.x, 0f - vector.y);
			if (effectStyle == Effect.Outline)
			{
				size = size3;
				size3 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size3, 0f - vector.x, vector.y);
				size = size3;
				size3 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size3, vector.x, vector.y);
				size = size3;
				size3 = verts.size;
				ApplyShadow(verts, uvs, cols, size, size3, 0f - vector.x, 0f - vector.y);
			}
		}
	}

	protected Vector2 ApplyOffset(BetterList<Vector3> verts, int start)
	{
		Vector2 pivotOffset = base.pivotOffset;
		float f = Mathf.Lerp(0f, -mWidth, pivotOffset.x);
		float f2 = Mathf.Lerp(mHeight, 0f, pivotOffset.y) + Mathf.Lerp(mCalculatedSize.y - (float)mHeight, 0f, pivotOffset.y);
		f = Mathf.Round(f);
		f2 = Mathf.Round(f2);
		for (int i = start; i < verts.size; i++)
		{
			verts.buffer[i].x += f;
			verts.buffer[i].y += f2;
		}
		return new Vector2(f, f2);
	}

	private void ApplyShadow(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols, int start, int end, float x, float y)
	{
		Color color = mEffectColor;
		color.a *= finalAlpha;
		Color32 color2 = (!(bitmapFont != null) || !bitmapFont.premultipliedAlpha) ? color : NGUITools.ApplyPMA(color);
		for (int i = start; i < end; i++)
		{
			verts.Add(verts.buffer[i]);
			uvs.Add(uvs.buffer[i]);
			cols.Add(cols.buffer[i]);
			Vector3 vector = verts.buffer[i];
			vector.x += x;
			vector.y += y;
			verts.buffer[i] = vector;
			cols.buffer[i] = color2;
		}
	}

	public int CalculateOffsetToFit(string text)
	{
		UpdateNGUIText();
		NGUIText.encoding = false;
		NGUIText.symbolStyle = NGUIText.SymbolStyle.None;
		return NGUIText.CalculateOffsetToFit(text);
	}

	public void SetCurrentProgress()
	{
		if (UIProgressBar.current != null)
		{
			text = UIProgressBar.current.value.ToString("F");
		}
	}

	public void SetCurrentPercent()
	{
		if (UIProgressBar.current != null)
		{
			text = Mathf.RoundToInt(UIProgressBar.current.value * 100f) + "%";
		}
	}

	public void SetCurrentSelection()
	{
		if (UIPopupList.current != null)
		{
			text = ((!UIPopupList.current.isLocalized) ? UIPopupList.current.value : Localization.Localize(UIPopupList.current.value));
		}
	}

	public bool Wrap(string text, out string final)
	{
		return Wrap(text, out final, 1000000);
	}

	public bool Wrap(string text, out string final, int height)
	{
		UpdateNGUIText(fontSize, mWidth, height);
		return NGUIText.WrapText(text, out final);
	}

	public void UpdateNGUIText()
	{
		UpdateNGUIText(fontSize, mWidth, mHeight);
	}

	public void UpdateNGUIText(int size, int lineWidth, int lineHeight)
	{
		NGUIText.fontSize = mPrintedSize;
		NGUIText.fontStyle = mFontStyle;
		NGUIText.rectWidth = lineWidth;
		NGUIText.rectHeight = lineHeight;
		NGUIText.gradient = mApplyGradient;
		NGUIText.gradientTop = mGradientTop;
		NGUIText.gradientBottom = mGradientBottom;
		NGUIText.encoding = mEncoding;
		NGUIText.premultiply = mPremultiply;
		NGUIText.symbolStyle = mSymbols;
		NGUIText.maxLines = mMaxLineCount;
		NGUIText.spacingX = mSpacingX;
		NGUIText.spacingY = mSpacingY;
		NGUIText.fontScale = ((!(bitmapFont != null)) ? mScale : (mScale * bitmapFont.pixelSize));
		if (mFont != null)
		{
			NGUIText.bitmapFont = mFont;
			while (true)
			{
				UIFont replacement = NGUIText.bitmapFont.replacement;
				if (replacement == null)
				{
					break;
				}
				NGUIText.bitmapFont = replacement;
			}
			if (NGUIText.bitmapFont.isDynamic)
			{
				NGUIText.dynamicFont = NGUIText.bitmapFont.dynamicFont;
				NGUIText.bitmapFont = null;
			}
			else
			{
				NGUIText.dynamicFont = null;
			}
		}
		else
		{
			NGUIText.dynamicFont = mTrueTypeFont;
			NGUIText.bitmapFont = null;
		}
		if (keepCrisp)
		{
			UIRoot root = base.root;
			if (root != null)
			{
				NGUIText.pixelDensity = ((!(root != null)) ? 1f : root.pixelSizeAdjustment);
			}
		}
		switch (base.pivot)
		{
		case Pivot.TopLeft:
		case Pivot.Left:
		case Pivot.BottomLeft:
			NGUIText.alignment = TextAlignment.Left;
			break;
		case Pivot.TopRight:
		case Pivot.Right:
		case Pivot.BottomRight:
			NGUIText.alignment = TextAlignment.Right;
			break;
		default:
			NGUIText.alignment = TextAlignment.Center;
			break;
		}
		NGUIText.Update();
	}
}

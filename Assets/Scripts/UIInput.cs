using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Input Field")]
public class UIInput : MonoBehaviour
{
	public enum InputType
	{
		Standard,
		AutoCorrect,
		Password
	}

	public enum Validation
	{
		None,
		Integer,
		Float,
		Alphanumeric,
		Username,
		Name
	}

	public enum KeyboardType
	{
		Default,
		ASCIICapable,
		NumbersAndPunctuation,
		URL,
		NumberPad,
		PhonePad,
		NamePhonePad,
		EmailAddress
	}

	public delegate char OnValidate(string text, int charIndex, char addedChar);

	public static UIInput current;

	public static UIInput selection;

	public UILabel label;

	public InputType inputType;

	public KeyboardType keyboardType;

	public Validation validation;

	public int characterLimit;

	public string savedAs;

	public GameObject selectOnTab;

	public Color activeTextColor = Color.white;

	public Color caretColor = new Color(1f, 1f, 1f, 0.8f);

	public Color selectionColor = new Color(1f, 223f / 255f, 47f / 85f, 0.5f);

	public List<EventDelegate> onSubmit = new List<EventDelegate>();

	public List<EventDelegate> onChange = new List<EventDelegate>();

	public OnValidate onValidate;

	[SerializeField]
	[HideInInspector]
	protected string mValue;

	protected string mDefaultText = string.Empty;

	protected Color mDefaultColor = Color.white;

	protected float mPosition;

	protected bool mDoInit = true;

	protected UIWidget.Pivot mPivot;

	protected static int mDrawStart;

	protected static TouchScreenKeyboard mKeyboard;

	public string defaultText
	{
		get
		{
			return mDefaultText;
		}
		set
		{
			mDefaultText = value;
		}
	}

	[Obsolete("Use UIInput.value instead")]
	public string text
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
		}
	}

	public string value
	{
		get
		{
			if (mDoInit)
			{
				Init();
			}
			if (isSelected && mKeyboard != null && mKeyboard.active)
			{
				return mKeyboard.text;
			}
			return mValue;
		}
		set
		{
			if (mDoInit)
			{
				Init();
			}
			mDrawStart = 0;
			if (isSelected && mKeyboard != null)
			{
				mKeyboard.text = value;
			}
			if (this.value != value)
			{
				mValue = value;
				if (isSelected && mKeyboard != null)
				{
					mKeyboard.text = value;
				}
				SaveToPlayerPrefs(mValue);
				UpdateLabel();
				ExecuteOnChange();
			}
		}
	}

	[Obsolete("Use UIInput.isSelected instead")]
	public bool selected
	{
		get
		{
			return isSelected;
		}
		set
		{
			isSelected = value;
		}
	}

	public bool isSelected
	{
		get
		{
			return selection == this;
		}
		set
		{
			if (!value)
			{
				if (isSelected)
				{
					UICamera.selectedObject = null;
				}
			}
			else
			{
				UICamera.selectedObject = base.gameObject;
			}
		}
	}

	protected int cursorPosition => value.Length;

	private void Start()
	{
		if (string.IsNullOrEmpty(mValue))
		{
			if (!string.IsNullOrEmpty(savedAs) && PlayerPrefs.HasKey(savedAs))
			{
				value = PlayerPrefs.GetString(savedAs);
			}
		}
		else
		{
			value = mValue.Replace("\\n", "\n");
		}
	}

	protected void Init()
	{
		if (mDoInit && label != null)
		{
			mDoInit = false;
			mDefaultText = label.text;
			mDefaultColor = label.color;
			label.supportEncoding = false;
			mPivot = label.pivot;
			Vector3 localPosition = label.cachedTransform.localPosition;
			mPosition = localPosition.x;
			UpdateLabel();
		}
	}

	protected void SaveToPlayerPrefs(string val)
	{
		if (!string.IsNullOrEmpty(savedAs))
		{
			if (string.IsNullOrEmpty(val))
			{
				PlayerPrefs.DeleteKey(savedAs);
			}
			else
			{
				PlayerPrefs.SetString(savedAs, val);
			}
		}
	}

	protected virtual void OnSelect(bool isSelected)
	{
		if (isSelected)
		{
			OnSelectEvent();
		}
		else
		{
			OnDeselectEvent();
		}
	}

	protected void OnSelectEvent()
	{
		selection = this;
		if (mDoInit)
		{
			Init();
		}
		if (label != null && NGUITools.GetActive(this))
		{
			label.color = activeTextColor;
			if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
			{
				mKeyboard = ((inputType != InputType.Password) ? TouchScreenKeyboard.Open(mValue, (TouchScreenKeyboardType)keyboardType, inputType == InputType.AutoCorrect, label.multiLine, secure: false, alert: false, defaultText) : TouchScreenKeyboard.Open(mValue, TouchScreenKeyboardType.Default, autocorrection: false, multiline: false, secure: true));
			}
			else
			{
				Input.imeCompositionMode = IMECompositionMode.On;
				Input.compositionCursorPos = ((!(UICamera.current != null) || !(UICamera.current.cachedCamera != null)) ? label.worldCorners[0] : UICamera.current.cachedCamera.WorldToScreenPoint(label.worldCorners[0]));
				mDrawStart = 0;
			}
			UpdateLabel();
		}
	}

	protected void OnDeselectEvent()
	{
		if (mDoInit)
		{
			Init();
		}
		if (label != null && NGUITools.GetActive(this))
		{
			mValue = value;
			if (mKeyboard != null)
			{
				mKeyboard.active = false;
				mKeyboard = null;
			}
			if (string.IsNullOrEmpty(mValue))
			{
				label.text = mDefaultText;
				label.color = mDefaultColor;
			}
			else
			{
				label.text = mValue;
			}
			Input.imeCompositionMode = IMECompositionMode.Off;
			RestoreLabelPivot();
		}
		selection = null;
		UpdateLabel();
	}

	private void Update()
	{
		if (mKeyboard == null || !isSelected)
		{
			return;
		}
		string text = mKeyboard.text;
		if (mValue != text)
		{
			mValue = string.Empty;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (onValidate != null)
				{
					c = onValidate(mValue, mValue.Length, c);
				}
				else if (validation != 0)
				{
					c = Validate(mValue, mValue.Length, c);
				}
				if (c != 0)
				{
					mValue += c;
				}
			}
			if (characterLimit > 0 && mValue.Length > characterLimit)
			{
				mValue = mValue.Substring(0, characterLimit);
			}
			UpdateLabel();
			ExecuteOnChange();
			if (mValue != text)
			{
				mKeyboard.text = mValue;
			}
		}
		if (mKeyboard.done)
		{
			if (!mKeyboard.wasCanceled)
			{
				Submit();
			}
			mKeyboard = null;
			isSelected = false;
		}
	}

	protected void Submit()
	{
		if (NGUITools.GetActive(this))
		{
			current = this;
			mValue = value;
			EventDelegate.Execute(onSubmit);
			SaveToPlayerPrefs(mValue);
			current = null;
		}
	}

	protected void UpdateLabel()
	{
		if (!(label != null))
		{
			return;
		}
		if (mDoInit)
		{
			Init();
		}
		bool isSelected = this.isSelected;
		string value = this.value;
		bool flag = string.IsNullOrEmpty(value);
		label.color = ((!flag || isSelected) ? activeTextColor : mDefaultColor);
		string text;
		if (flag)
		{
			text = ((!isSelected) ? mDefaultText : string.Empty);
			RestoreLabelPivot();
		}
		else
		{
			if (inputType == InputType.Password)
			{
				text = string.Empty;
				int i = 0;
				for (int length = value.Length; i < length; i++)
				{
					text += "*";
				}
			}
			else
			{
				text = value;
			}
			int num = isSelected ? Mathf.Min(text.Length, cursorPosition) : 0;
			string str = text.Substring(0, num);
			if (isSelected)
			{
				str += Input.compositionString;
			}
			text = str + text.Substring(num, text.Length - num);
			if (isSelected && label.overflowMethod == UILabel.Overflow.ClampContent)
			{
				int num2 = label.CalculateOffsetToFit(text);
				if (num2 == 0)
				{
					mDrawStart = 0;
					RestoreLabelPivot();
				}
				else if (num < mDrawStart)
				{
					mDrawStart = num;
					SetPivotToLeft();
				}
				else if (num2 < mDrawStart)
				{
					mDrawStart = num2;
					SetPivotToLeft();
				}
				else
				{
					num2 = label.CalculateOffsetToFit(text.Substring(0, num));
					if (num2 > mDrawStart)
					{
						mDrawStart = num2;
						SetPivotToRight();
					}
				}
				if (mDrawStart != 0)
				{
					text = text.Substring(mDrawStart, text.Length - mDrawStart);
				}
			}
			else
			{
				mDrawStart = 0;
				RestoreLabelPivot();
			}
		}
		label.text = text;
	}

	protected void SetPivotToLeft()
	{
		Vector2 pivotOffset = NGUIMath.GetPivotOffset(mPivot);
		pivotOffset.x = 0f;
		label.pivot = NGUIMath.GetPivot(pivotOffset);
	}

	protected void SetPivotToRight()
	{
		Vector2 pivotOffset = NGUIMath.GetPivotOffset(mPivot);
		pivotOffset.x = 1f;
		label.pivot = NGUIMath.GetPivot(pivotOffset);
	}

	protected void RestoreLabelPivot()
	{
		if (label != null && label.pivot != mPivot)
		{
			label.pivot = mPivot;
		}
	}

	protected char Validate(string text, int pos, char ch)
	{
		if (validation == Validation.None || !base.enabled)
		{
			return ch;
		}
		if (validation == Validation.Integer)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && pos == 0 && !text.Contains("-"))
			{
				return ch;
			}
		}
		else if (validation == Validation.Float)
		{
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
			if (ch == '-' && pos == 0 && !text.Contains("-"))
			{
				return ch;
			}
			if (ch == '.' && !text.Contains("."))
			{
				return ch;
			}
		}
		else if (validation == Validation.Alphanumeric)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return ch;
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (validation == Validation.Username)
		{
			if (ch >= 'A' && ch <= 'Z')
			{
				return (char)(ch - 65 + 97);
			}
			if (ch >= 'a' && ch <= 'z')
			{
				return ch;
			}
			if (ch >= '0' && ch <= '9')
			{
				return ch;
			}
		}
		else if (validation == Validation.Name)
		{
			char c = (text.Length <= 0) ? ' ' : text[Mathf.Clamp(pos, 0, text.Length - 1)];
			char c2 = (text.Length <= 0) ? '\n' : text[Mathf.Clamp(pos + 1, 0, text.Length - 1)];
			if (ch >= 'a' && ch <= 'z')
			{
				if (c == ' ')
				{
					return (char)(ch - 97 + 65);
				}
				return ch;
			}
			if (ch >= 'A' && ch <= 'Z')
			{
				if (c != ' ' && c != '\'')
				{
					return (char)(ch - 65 + 97);
				}
				return ch;
			}
			switch (ch)
			{
			case '\'':
				if (c != ' ' && c != '\'' && c2 != '\'' && !text.Contains("'"))
				{
					return ch;
				}
				break;
			case ' ':
				if (c != ' ' && c != '\'' && c2 != ' ' && c2 != '\'')
				{
					return ch;
				}
				break;
			}
		}
		return '\0';
	}

	protected void ExecuteOnChange()
	{
		if (EventDelegate.IsValid(onChange))
		{
			current = this;
			EventDelegate.Execute(onChange);
			current = null;
		}
	}
}

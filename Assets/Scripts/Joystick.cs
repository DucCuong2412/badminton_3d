using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[RequireComponent(typeof(Image))]
public class Joystick : MonoBehaviour
{
    [NonSerialized]
    private static Joystick[] joysticks;

    [NonSerialized]
    private static bool enumeratedJoysticks;

    [NonSerialized]
    private static float tapTimeDelta = 0.3f;

    public bool touchPad;
    public Rect touchZone;
    public Vector2 deadZone;
    public bool normalize;
    public Vector2 position;
    public int tapCount;

    private int lastFingerId;
    private float tapTimeWindow;
    private Vector2 fingerDownPos;
    private float fingerDownTime;
    private float firstDeltaTime;

    private Image gui;
    private RectTransform rectTransform;
    private Boundary guiBoundary;
    private Vector2 guiTouchOffset;
    private Vector2 guiCenter;

    public Joystick()
    {
        deadZone = Vector2.zero;
        lastFingerId = -1;
        firstDeltaTime = 0.5f;
        guiBoundary = new Boundary();
    }

    public void Start()
    {
        gui = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;
        Vector2 anchoredPos = rectTransform.anchoredPosition;

        if (touchPad)
        {
            touchZone = new Rect(anchoredPos - size / 2, size);
            return;
        }

        guiTouchOffset = size * 0.5f;
        guiCenter = anchoredPos;

        guiBoundary.min.x = anchoredPos.x - guiTouchOffset.x;
        guiBoundary.max.x = anchoredPos.x + guiTouchOffset.x;
        guiBoundary.min.y = anchoredPos.y - guiTouchOffset.y;
        guiBoundary.max.y = anchoredPos.y + guiTouchOffset.y;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        enumeratedJoysticks = false;
    }

    public void ResetJoystick()
    {
        rectTransform.anchoredPosition = guiCenter;
        lastFingerId = -1;
        position = Vector2.zero;
        fingerDownPos = Vector2.zero;

        if (touchPad)
        {
            var color = gui.color;
            color.a = 0.025f;
            gui.color = color;
        }
    }

    public bool IsFingerDown()
    {
        return lastFingerId != -1;
    }

    public void LatchedFinger(int fingerId)
    {
        if (lastFingerId == fingerId)
        {
            ResetJoystick();
        }
    }

    public void Update()
    {
        if (!enumeratedJoysticks)
        {
            joysticks = FindObjectsOfType<Joystick>();
            enumeratedJoysticks = true;
        }

        int touchCount = Input.touchCount;

        if (tapTimeWindow > 0f)
        {
            tapTimeWindow -= Time.deltaTime;
        }
        else
        {
            tapCount = 0;
        }

        if (touchCount == 0)
        {
            ResetJoystick();
        }
        else
        {
            for (int i = 0; i < touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 touchPos = touch.position;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, touchPos, null, out localPoint);
                bool validTouch = false;

                if (touchPad)
                {
                    if (touchZone.Contains(touchPos)) validTouch = true;
                }
                else
                {
                    validTouch = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touchPos);
                }

                if (validTouch && (lastFingerId == -1 || lastFingerId != touch.fingerId))
                {
                    if (touchPad)
                    {
                        var color = gui.color;
                        color.a = 0.15f;
                        gui.color = color;
                        lastFingerId = touch.fingerId;
                        fingerDownPos = touch.position;
                        fingerDownTime = Time.time;
                    }
                    lastFingerId = touch.fingerId;
                    tapCount = (tapTimeWindow > 0f) ? tapCount + 1 : 1;
                    tapTimeWindow = tapTimeDelta;

                    foreach (var joy in joysticks)
                    {
                        if (joy != this) joy.LatchedFinger(touch.fingerId);
                    }
                }

                if (lastFingerId == touch.fingerId)
                {
                    if (touch.tapCount > tapCount) tapCount = touch.tapCount;

                    if (touchPad)
                    {
                        position.x = Mathf.Clamp((touch.position.x - fingerDownPos.x) / (touchZone.width / 2f), -1f, 1f);
                        position.y = Mathf.Clamp((touch.position.y - fingerDownPos.y) / (touchZone.height / 2f), -1f, 1f);
                    }
                    else
                    {
                        Vector2 clamped = new Vector2(
                            Mathf.Clamp(localPoint.x, guiBoundary.min.x, guiBoundary.max.x),
                            Mathf.Clamp(localPoint.y, guiBoundary.min.y, guiBoundary.max.y)
                        );
                        rectTransform.anchoredPosition = clamped;
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        ResetJoystick();
                    }
                }
            }
        }

        if (!touchPad)
        {
            position.x = (rectTransform.anchoredPosition.x - guiCenter.x) / guiTouchOffset.x;
            position.y = (rectTransform.anchoredPosition.y - guiCenter.y) / guiTouchOffset.y;
        }

        if (Mathf.Abs(position.x) < deadZone.x) position.x = 0f;
        else if (normalize) position.x = Mathf.Sign(position.x) * (Mathf.Abs(position.x) - deadZone.x) / (1f - deadZone.x);

        if (Mathf.Abs(position.y) < deadZone.y) position.y = 0f;
        else if (normalize) position.y = Mathf.Sign(position.y) * (Mathf.Abs(position.y) - deadZone.y) / (1f - deadZone.y);
    }
}

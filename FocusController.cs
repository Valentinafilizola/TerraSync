using UnityEngine;
using TMPro;

public class FocusController : MonoBehaviour
{
    [Header("References")]
    public SerialArduinoReader serial;   // drag SerialManager (with SerialArduinoReader) here
    public TextMeshProUGUI uiText;       // drag your TMP text here

    [Header("Timing")]
    public float holdToEnableSeconds = 2f;
    public float confirmDisableWindowSeconds = 3f;

    private enum State
    {
        Off,
        On,
        ConfirmDisable
    }

    private State state = State.Off;

    private float holdTimer = 0f;
    private float confirmTimer = 0f;

    private bool lastButtonDown = false;

    void Start()
    {
        Show("Focus Mode Off");
    }

    void Update()
    {
        if (serial == null) return;

        bool buttonDown = serial.ButtonDown;
        bool pressedThisFrame = serial.ButtonPressedThisFrame;

        switch (state)
        {
            case State.Off:
                // hold 2 seconds to enable
                if (buttonDown)
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= holdToEnableSeconds)
                    {
                        state = State.On;
                        holdTimer = 0f;
                        Show("Focus Mode On");
                    }
                    else
                    {
                        // Optional: show hold progress (comment out if you want calmer UI)
                        // Show($"Hold to enable... {holdTimer:0.0}s");
                    }
                }
                else
                {
                    holdTimer = 0f;
                }
                break;

            case State.On:
                // single press starts disable confirmation
                if (pressedThisFrame)
                {
                    state = State.ConfirmDisable;
                    confirmTimer = confirmDisableWindowSeconds;
                    Show("Disable Focus Mode?");
                }
                break;

            case State.ConfirmDisable:
                confirmTimer -= Time.deltaTime;

                // two presses to confirm: (we already used first press to enter ConfirmDisable)
                if (pressedThisFrame)
                {
                    state = State.Off;
                    Show("Focus Mode Disabled");
                }
                else if (confirmTimer <= 0f)
                {
                    // timed out -> go back to On
                    state = State.On;
                    Show("Focus Mode On");
                }
                break;
        }

        lastButtonDown = buttonDown;
    }

    void Show(string msg)
    {
        if (uiText != null)
            uiText.text = msg;
    }

    // Optional helper if other scripts need it later
    public bool IsFocusOn()
    {
        return state == State.On || state == State.ConfirmDisable;
    }
}

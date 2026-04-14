using UnityEngine;
using UnityEngine.InputSystem;

public static class InputManager
{
    private static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        EnsureInitialized();
    }

    public static bool AnyInputStartedThisFrame()
    {
        EnsureInitialized();

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        return TryGetPrimaryPointerDownThisFrame(out _);
    }

    public static bool ResetPressedThisFrame()
    {
        EnsureInitialized();
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
    }

    public static bool TryGetPrimaryPointerDownThisFrame(out Vector2 screenPosition)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Down, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Down, out screenPosition);
    }

    public static bool TryGetPrimaryPointerHeld(out Vector2 screenPosition)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Held, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Held, out screenPosition);
    }

    public static bool TryGetPrimaryPointerUpThisFrame(out Vector2 screenPosition)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Up, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Up, out screenPosition);
    }

    public static Vector2 GetTilt()
    {
        EnsureInitialized();

#if UNITY_EDITOR || UNITY_STANDALONE
        return ReadEditorTilt();
#else
        EnsureAccelerometerEnabled();

        if (Accelerometer.current == null)
            return Vector2.zero;

        Vector3 acceleration = Accelerometer.current.acceleration.ReadValue();
        return new Vector2(acceleration.x, acceleration.y);
#endif
    }

    private static void EnsureInitialized()
    {
        if (initialized)
            return;

        initialized = true;

        if (InputSystem.settings != null)
            InputSystem.settings.compensateForScreenOrientation = true;
    }

    private static void EnsureAccelerometerEnabled()
    {
        if (Accelerometer.current == null || Accelerometer.current.enabled)
            return;

        InputSystem.EnableDevice(Accelerometer.current);
    }

    private static bool TryGetTouchPointer(PointerQuery query, out Vector2 screenPosition)
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            bool isRelevantTouch =
                touch.press.wasPressedThisFrame ||
                touch.press.isPressed ||
                touch.press.wasReleasedThisFrame;

            if (isRelevantTouch)
            {
                screenPosition = touch.position.ReadValue();

                switch (query)
                {
                    case PointerQuery.Down:
                        return touch.press.wasPressedThisFrame;
                    case PointerQuery.Held:
                        return touch.press.isPressed;
                    case PointerQuery.Up:
                        return touch.press.wasReleasedThisFrame;
                }
            }
        }
        screenPosition = Vector2.zero;
        return false;
    }

    private static bool TryGetMousePointer(PointerQuery query, out Vector2 screenPosition)
    {
        if (Mouse.current != null)
        {
            screenPosition = Mouse.current.position.ReadValue();

            switch (query)
            {
                case PointerQuery.Down:
                    return Mouse.current.leftButton.wasPressedThisFrame;
                case PointerQuery.Held:
                    return Mouse.current.leftButton.isPressed;
                case PointerQuery.Up:
                    return Mouse.current.leftButton.wasReleasedThisFrame;
            }
        }

        screenPosition = Vector2.zero;
        return false;
    }

    private static Vector2 ReadEditorTilt()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            x -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            x += 1f;
        if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed)
            y -= 1f;
        if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed)
            y += 1f;

        return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
    }

    private enum PointerQuery
    {
        Down,
        Held,
        Up
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using InputTouchPhase = UnityEngine.InputSystem.TouchPhase;

public readonly struct PrimaryPointerSample
{
    public readonly Vector2 screenPosition;
    public readonly bool isTouch;
    public readonly float pressure;
    public readonly Vector2 radius;
    public readonly InputTouchPhase phase;

    public PrimaryPointerSample(
        Vector2 screenPosition,
        bool isTouch,
        float pressure,
        Vector2 radius,
        InputTouchPhase phase)
    {
        this.screenPosition = screenPosition;
        this.isTouch = isTouch;
        this.pressure = pressure;
        this.radius = radius;
        this.phase = phase;
    }
}

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

        if (TryGetPrimaryPointerDownDetailed(out PrimaryPointerSample sample))
        {
            screenPosition = sample.screenPosition;
            return true;
        }

        screenPosition = Vector2.zero;
        return false;
    }

    public static bool TryGetPrimaryPointerHeld(out Vector2 screenPosition)
    {
        EnsureInitialized();

        if (TryGetPrimaryPointerHeldDetailed(out PrimaryPointerSample sample))
        {
            screenPosition = sample.screenPosition;
            return true;
        }

        screenPosition = Vector2.zero;
        return false;
    }

    public static bool TryGetPrimaryPointerUpThisFrame(out Vector2 screenPosition)
    {
        EnsureInitialized();

        if (TryGetPrimaryPointerUpDetailed(out PrimaryPointerSample sample))
        {
            screenPosition = sample.screenPosition;
            return true;
        }

        screenPosition = Vector2.zero;
        return false;
    }

    public static bool TryGetPrimaryPointerDownDetailed(out PrimaryPointerSample sample)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Down, out sample) ||
               TryGetMousePointer(PointerQuery.Down, out sample);
    }

    public static bool TryGetPrimaryPointerHeldDetailed(out PrimaryPointerSample sample)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Held, out sample) ||
               TryGetMousePointer(PointerQuery.Held, out sample);
    }

    public static bool TryGetPrimaryPointerUpDetailed(out PrimaryPointerSample sample)
    {
        EnsureInitialized();
        return TryGetTouchPointer(PointerQuery.Up, out sample) ||
               TryGetMousePointer(PointerQuery.Up, out sample);
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

    public static bool IsPrimaryPointerOverUI()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
            return false;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            if (eventSystem.IsPointerOverGameObject(touchId))
                return true;
        }

        return eventSystem.IsPointerOverGameObject();
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

    private static bool TryGetTouchPointer(PointerQuery query, out PrimaryPointerSample sample)
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
                sample = new PrimaryPointerSample(
                    touch.position.ReadValue(),
                    true,
                    touch.pressure.ReadValue(),
                    touch.radius.ReadValue(),
                    touch.phase.ReadValue());

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
        sample = default;
        return false;
    }

    private static bool TryGetMousePointer(PointerQuery query, out PrimaryPointerSample sample)
    {
        if (Mouse.current != null)
        {
            sample = new PrimaryPointerSample(
                Mouse.current.position.ReadValue(),
                false,
                0f,
                Vector2.zero,
                GetMousePhase(query));

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

        sample = default;
        return false;
    }

    private static InputTouchPhase GetMousePhase(PointerQuery query)
    {
        switch (query)
        {
            case PointerQuery.Down:
                return InputTouchPhase.Began;
            case PointerQuery.Held:
                return InputTouchPhase.Moved;
            case PointerQuery.Up:
                return InputTouchPhase.Ended;
            default:
                return InputTouchPhase.None;
        }
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

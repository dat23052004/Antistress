using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class InputManager : Singleton<InputManager>
{
    private bool accelerometerEnabledByManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _ = Ins;
    }

    protected override void Initialize()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        ConfigureInputSystem();
        ConfigureEventSystem();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DisableAccelerometerIfNeeded();
    }

    public bool AnyInputStartedThisFrame()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        return TryGetPrimaryPointerDownThisFrame(out _);
    }

    public bool ResetPressedThisFrame()
    {
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
    }

    public bool TryGetPrimaryPointerDownThisFrame(out Vector2 screenPosition)
    {
        return TryGetTouchPointer(PointerQuery.Down, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Down, out screenPosition);
    }

    public bool TryGetPrimaryPointerHeld(out Vector2 screenPosition)
    {
        return TryGetTouchPointer(PointerQuery.Held, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Held, out screenPosition);
    }

    public bool TryGetPrimaryPointerUpThisFrame(out Vector2 screenPosition)
    {
        return TryGetTouchPointer(PointerQuery.Up, out screenPosition) ||
               TryGetMousePointer(PointerQuery.Up, out screenPosition);
    }

    public Vector2 GetTilt()
    {
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureInputSystem();
        ConfigureEventSystem();
    }

    private void ConfigureInputSystem()
    {
        if (InputSystem.settings != null)
            InputSystem.settings.compensateForScreenOrientation = true;

        EnsureAccelerometerEnabled();
    }

    private void EnsureAccelerometerEnabled()
    {
        if (Accelerometer.current == null || Accelerometer.current.enabled)
            return;

        InputSystem.EnableDevice(Accelerometer.current);
        accelerometerEnabledByManager = true;
    }

    private void DisableAccelerometerIfNeeded()
    {
        if (!accelerometerEnabledByManager || Accelerometer.current == null)
            return;

        InputSystem.DisableDevice(Accelerometer.current);
        accelerometerEnabledByManager = false;
    }

    private void ConfigureEventSystem()
    {
        EventSystem eventSystem = EventSystem.current ?? FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
            return;

        InputSystemUIInputModule inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputSystemModule == null)
            inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();

        BaseInputModule[] modules = eventSystem.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in modules)
            module.enabled = module == inputSystemModule;
    }

    private bool TryGetTouchPointer(PointerQuery query, out Vector2 screenPosition)
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

    private bool TryGetMousePointer(PointerQuery query, out Vector2 screenPosition)
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

    private Vector2 ReadEditorTilt()
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

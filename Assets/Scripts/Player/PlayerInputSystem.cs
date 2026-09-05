using UnityEngine;
using UnityEngine.InputSystem;

namespace MagicSchool.Player
{
    /// <summary>
    /// Centralized input for world-space picking/dragging (Bench, Board, etc) and misc key
    /// checks (e.g. GameManager's start-combat trigger).
    /// This project has Active Input Handling set to the New Input System exclusively -
    /// read input through here (Mouse.current/Keyboard.current), never UnityEngine.Input,
    /// which compiles fine but throws InvalidOperationException at runtime under this setting.
    /// </summary>
    internal static class PlayerInputSystem
    {
        public static bool IsPointerDown => Mouse.current.leftButton.isPressed;
        public static bool DragPressedThisFrame => Mouse.current.leftButton.wasPressedThisFrame;
        public static bool DragReleasedThisFrame => Mouse.current.leftButton.wasReleasedThisFrame;
        public static bool SpacePressedThisFrame => Keyboard.current.spaceKey.wasPressedThisFrame;
        public static bool InspectPressedThisFrame => Mouse.current.rightButton.wasPressedThisFrame;
        public static bool RestartPressedThisFrame => Keyboard.current.rKey.wasPressedThisFrame;

        // dev only: raise the scoreboard with made-up numbers, see ScoreboardPreview
        public static bool PreviewScoreboardPressedThisFrame => Keyboard.current.f1Key.wasPressedThisFrame;

        // adjust game's speed using numkey
        public static int SpeedPressedThisFrame()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) return 1;
            if (Keyboard.current.digit2Key.wasPressedThisFrame) return 2;
            if (Keyboard.current.digit3Key.wasPressedThisFrame) return 3;

            return 0;
        }

        public static Vector2 PointerScreenPosition => Mouse.current.position.ReadValue();

        // Converts the current mouse position to a world point on the plane facing the given camera.
        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            Vector3 screenPos = Mouse.current.position.ReadValue();
            screenPos.z = -cam.transform.position.z;
            return cam.ScreenToWorldPoint(screenPos);
        }
    }
}

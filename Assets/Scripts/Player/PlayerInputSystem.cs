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
        public static bool PointerPressedThisFrame => Mouse.current.leftButton.wasPressedThisFrame;
        public static bool PointerReleasedThisFrame => Mouse.current.leftButton.wasReleasedThisFrame;
        public static bool SpacePressedThisFrame => Keyboard.current.spaceKey.wasPressedThisFrame;

        // Converts the current mouse position to a world point on the plane facing the given camera.
        public static Vector3 GetMouseWorldPosition(Camera cam)
        {
            Vector3 screenPos = Mouse.current.position.ReadValue();
            screenPos.z = -cam.transform.position.z;
            return cam.ScreenToWorldPoint(screenPos);
        }
    }
}

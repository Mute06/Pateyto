using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Pateyto
{
    /// <summary>
    /// Toggles a UI RawImage and a secondary GameObject when the player presses the "E" key.
    /// Attach this script to any GameObject in the scene (e.g., the player or a dedicated manager).
    /// </summary>
    public class ToggleRawImage : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("The RawImage component that should be shown/hidden.")]
        public RawImage targetRawImage;

        [Tooltip("The secondary GameObject that should be hidden/shown opposite to the RawImage.")]
        public GameObject otherObject;

        // Internal state tracking
        private bool _isRawImageActive = false;

        private void Awake()
        {
            // Auto‑assign if not set in the Inspector
            if (targetRawImage == null)
            {
                var rawImg = FindObjectOfType<RawImage>();
                if (rawImg != null) targetRawImage = rawImg;
            }

            if (otherObject == null && targetRawImage != null)
            {
                // Try to find a sibling object with a name hint; adjust as needed
                var sibling = targetRawImage.transform.parent?.Find("OtherObject");
                if (sibling != null) otherObject = sibling.gameObject;
            }
        }

        private void Update()
        {
            // Using the new Input System (already integrated in the project)
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        /// <summary>
        /// Switches the visibility of the RawImage and the other object.
        /// </summary>
        private void Toggle()
        {
            _isRawImageActive = !_isRawImageActive;

            if (targetRawImage != null)
                targetRawImage.gameObject.SetActive(_isRawImageActive);

            if (otherObject != null)
                otherObject.SetActive(!_isRawImageActive);
        }
    }
}

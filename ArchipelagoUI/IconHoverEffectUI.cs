using UnityEngine;

namespace OriBFArchipelago.ArchipelagoUI
{
    internal class IconHoverEffectUI : MonoBehaviour
    {
        public RuntimeWorldMapIcon IconType { get; set; }
        public AreaMapUI MapUI { get; set; }
        public RuntimeGameWorldArea Area { get; set; }
        public bool SavePedestalCollected { get; set; }

        private const float DETECTION_RADIUS = 5f;
        private bool isSettingsOpen = false;
        private float hoverTimer = 0f;
        private const float HOVER_DELAY = 0.1f;


        // Track whether this is the closest icon to the cursor
        private bool isClosestIcon = false;

        void Update()
        {
            if (MapUI == null)
                return;

            if (GameSettings.Instance.CurrentControlScheme == ControlScheme.Controller)
            {
                global::Core.Input.CursorPosition = new Vector2(0.5f, 0.5f);
            }

            Vector2 cursorPos = global::Core.Input.CursorPositionUI;
            Vector2 worldCursorPos = MapUI.Navigation.MapToWorldPosition(cursorPos);
            Vector2 iconPos = IconType.Position;
            float cursorDistance = Vector2.Distance(worldCursorPos, iconPos);

            bool isHovered = cursorDistance < DETECTION_RADIUS;

            // Check if this is the closest icon to the cursor
            isClosestIcon = isHovered && IsClosestIcon(worldCursorPos, cursorDistance);

            ToggleLogicScreen(isHovered);
        }

        private void ToggleLogicScreen(bool isHovered)
        {
            if (isHovered && isClosestIcon)
            {
                hoverTimer += Time.deltaTime;
                if (hoverTimer >= HOVER_DELAY && !isSettingsOpen)
                {
                    isSettingsOpen = true;
                    IconHoverUI.ShowMapTrackerSettings = true;
                    IconHoverUI.Icon = IconType;
                    IconHoverUI.Area = Area;
                }
            }
            else
            {
                hoverTimer = 0f;
                if (isSettingsOpen && IconHoverUI.Icon == IconType)
                {
                    isSettingsOpen = false;
                    IconHoverUI.ShowMapTrackerSettings = false;
                    IconHoverUI.Icon = null;
                    IconHoverUI.Area = null;
                }
            }
        }

        private bool IsClosestIcon(Vector2 worldCursorPos, float thisDistance)
        {
            // Find all other icon hover effects in the scene
            IconHoverEffectUI[] allIcons = FindObjectsOfType<IconHoverEffectUI>();

            foreach (IconHoverEffectUI otherIcon in allIcons)
            {
                // Skip this icon
                if (otherIcon == this) continue;

                // Skip icons that don't have a valid MapUI reference
                if (otherIcon.MapUI == null) continue;

                Vector2 otherIconPos = otherIcon.IconType.Position;
                float otherDistance = Vector2.Distance(worldCursorPos, otherIconPos);

                // If we find another icon that's closer to the cursor, this is not the closest
                if (otherDistance < thisDistance)
                {
                    return false;
                }
            }

            // If we get here, no closer icon was found
            return true;
        }
    }
}

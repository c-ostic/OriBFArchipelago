using HarmonyLib;
using OriBFArchipelago.Helper;

namespace OriBFArchipelago.Patches
{
    /// <summary>
    /// Keeps popups tagged with <see cref="NonDismissablePopup"/> (the "connecting" overlay) from
    /// being closed by the player. ConfirmOrCancel.FixedUpdate disables itself whenever A or Cancel
    /// is pressed - so we skip that entirely for tagged popups, while still consuming the button
    /// press so it doesn't leak to anything behind the popup. The popup is removed in code once the
    /// connection succeeds or fails.
    /// </summary>
    [HarmonyPatch(typeof(ConfirmOrCancel), "FixedUpdate")]
    internal class ConfirmOrCancelPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ConfirmOrCancel __instance)
        {
            if (__instance.GetComponent<NonDismissablePopup>() == null)
                return true; // not ours - run the normal open/close handling

            // Consume confirm/cancel so the input is swallowed instead of closing the popup
            // or reaching the menu behind it.
            if (global::Core.Input.ActionButtonA.OnPressed)
                global::Core.Input.ActionButtonA.Used = true;

            if (global::Core.Input.Cancel.OnPressed)
                global::Core.Input.Cancel.Used = true;

            return false; // skip the original FixedUpdate -> the popup never disables itself
        }
    }
}

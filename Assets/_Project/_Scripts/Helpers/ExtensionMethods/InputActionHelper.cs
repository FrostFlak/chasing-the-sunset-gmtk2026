using UnityEngine.InputSystem;

namespace Helpers.ExtMethods {
    public static class InputActionHelper {
        public static string GetActionKey(this InputActionReference inputActionReference) => inputActionReference.action.GetBindingDisplayString(0, out _, out _, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }
}
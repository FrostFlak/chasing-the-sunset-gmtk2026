using UnityEngine.Localization.Components;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace Helpers.ExtMethods {
    public static class LocalizeStringHelper {
        public static void SetString(
            this LocalizeStringEvent stringEvent,
            string localVariable,
            string value
        ) => ((StringVariable)stringEvent.StringReference[localVariable]).Value = value;
        
        public static void SetInt(
            this LocalizeStringEvent stringEvent,
            string localVariable,
            int value
        ) => ((IntVariable)stringEvent.StringReference[localVariable]).Value = value;
        
        public static void SetFloat(
            this LocalizeStringEvent stringEvent,
            string localVariable,
            float value
        ) => ((FloatVariable)stringEvent.StringReference[localVariable]).Value = value;
    }
}
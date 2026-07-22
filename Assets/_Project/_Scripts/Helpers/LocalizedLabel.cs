using System;
using TMPro;
using UnityEngine.Localization.Components;

namespace Helpers {
    [Serializable]
    public struct LocalizedLabel {
        public LocalizeStringEvent StringEvent;
        public TMP_Text Label;
    }
}
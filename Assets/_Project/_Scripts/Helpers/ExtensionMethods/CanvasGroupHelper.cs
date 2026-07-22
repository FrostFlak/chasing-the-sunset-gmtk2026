namespace Helpers {
    public static class CanvasGroupHelper {
        public static void SetState(this UnityEngine.CanvasGroup canvasGroup, bool active) {
            canvasGroup.alpha = active ? 1 : 0;
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
        
        public static void SetStateNoAlpha(this UnityEngine.CanvasGroup canvasGroup, bool active) {
            canvasGroup.interactable = active;
            canvasGroup.blocksRaycasts = active;
        }
        
        public static void SetAlpha(this UnityEngine.CanvasGroup canvasGroup, float alpha) => canvasGroup.alpha = alpha;
        public static void SetAlpha(this UnityEngine.CanvasGroup canvasGroup, bool active) => canvasGroup.alpha = active ? 1 : 0;
    }
}
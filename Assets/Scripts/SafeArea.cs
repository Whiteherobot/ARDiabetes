using UnityEngine;

namespace ARDiabetes
{
    /// <summary>Ajusta el RectTransform al área segura del dispositivo (barra de estado / notch).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        RectTransform rt;
        Rect last;

        void Awake() { rt = GetComponent<RectTransform>(); Apply(); }
        void Update() { if (Screen.safeArea != last) Apply(); }

        void Apply()
        {
            last = Screen.safeArea;
            var sa = Screen.safeArea;
            Vector2 min = sa.position;
            Vector2 max = sa.position + sa.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;
            if (float.IsNaN(min.x) || float.IsNaN(max.x)) return;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}

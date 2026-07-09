using UnityEngine;
using UnityEngine.EventSystems;

namespace ARDiabetes
{
    /// <summary>Feedback táctil: escala el botón al presionar y lo devuelve al soltar.</summary>
    public class PressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        Vector3 baseScale = Vector3.one;
        Vector3 target = Vector3.one;

        void OnEnable() { target = baseScale; transform.localScale = baseScale; }

        public void OnPointerDown(PointerEventData e) { target = baseScale * 0.94f; }
        public void OnPointerUp(PointerEventData e) { target = baseScale; }
        public void OnPointerExit(PointerEventData e) { target = baseScale; }

        void Update()
        {
            if ((transform.localScale - target).sqrMagnitude > 0.00001f)
                transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * 18f);
        }
    }
}

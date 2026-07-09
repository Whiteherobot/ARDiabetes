using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ARDiabetes
{
    /// <summary>
    /// Carrusel horizontal de onboarding: swipe con snap, auto-avance, puntos indicadores
    /// y control externo (Next / GoTo). onComplete se dispara al pulsar "siguiente" en el último slide.
    /// </summary>
    public class Carousel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform content;
        public float slideWidth;
        public int count;
        public Image[] dots;
        public Color dotOn = Color.white, dotOff = Color.gray;
        public System.Action onComplete;
        public System.Action<int> onIndexChanged;

        int index;
        float targetX;
        bool dragging;
        bool userInteracted;
        float autoTimer;
        float scale = 1f;

        void Start()
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) scale = c.scaleFactor > 0 ? c.scaleFactor : 1f;
            GoTo(0);
        }

        void Update()
        {
            if (!dragging)
            {
                float x = Mathf.Lerp(content.anchoredPosition.x, targetX, Time.unscaledDeltaTime * 12f);
                content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
            }
            if (!userInteracted && count > 1)
            {
                autoTimer += Time.unscaledDeltaTime;
                if (autoTimer >= 4.5f) { autoTimer = 0; GoTo((index + 1) % count); }
            }
        }

        public void GoTo(int i)
        {
            index = Mathf.Clamp(i, 0, count - 1);
            targetX = -index * slideWidth;
            UpdateDots();
            onIndexChanged?.Invoke(index);
        }

        public void Next()
        {
            userInteracted = true;
            if (index < count - 1) GoTo(index + 1);
            else onComplete?.Invoke();
        }

        void UpdateDots()
        {
            if (dots == null) return;
            for (int i = 0; i < dots.Length; i++)
                if (dots[i] != null) dots[i].color = i == index ? dotOn : dotOff;
        }

        public void OnBeginDrag(PointerEventData e) { dragging = true; userInteracted = true; }

        public void OnDrag(PointerEventData e)
        {
            float dx = e.delta.x / scale;
            float x = content.anchoredPosition.x + dx;
            float min = -(count - 1) * slideWidth, max = 0f;
            if (x > max) x = max + (x - max) * 0.35f;      // rubber band
            if (x < min) x = min + (x - min) * 0.35f;
            content.anchoredPosition = new Vector2(x, content.anchoredPosition.y);
        }

        public void OnEndDrag(PointerEventData e)
        {
            dragging = false;
            int nearest = Mathf.RoundToInt(-content.anchoredPosition.x / slideWidth);
            // un empujón decidido cambia de slide aunque no llegue a la mitad
            if (Mathf.Abs(e.delta.x) > 6f)
                nearest = e.delta.x < 0 ? index + 1 : index - 1;
            GoTo(nearest);
        }
    }
}

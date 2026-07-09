using System.Collections;
using UnityEngine;

namespace ARDiabetes
{
    /// <summary>Anima la entrada de un elemento (fade + subida) al activarse, con retardo opcional (stagger).</summary>
    public class Appear : MonoBehaviour
    {
        public float delay = 0f;
        public float dur = 0.34f;
        public float yOffset = 46f;

        CanvasGroup cg;

        void OnEnable() { StartCoroutine(Run()); }

        IEnumerator Run()
        {
            var rt = (RectTransform)transform;
            if (cg == null) cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            Vector2 basePos = rt.anchoredPosition;
            Vector2 from = basePos + new Vector2(0, -yOffset);
            cg.alpha = 0f;
            rt.anchoredPosition = from;
            float t = -delay;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = 1f - Mathf.Pow(1f - k, 3f); // ease-out cubic
                cg.alpha = Mathf.Clamp01(k <= 0 ? 0 : e);
                rt.anchoredPosition = Vector2.LerpUnclamped(from, basePos, e);
                yield return null;
            }
            cg.alpha = 1f;
            rt.anchoredPosition = basePos;
        }
    }
}

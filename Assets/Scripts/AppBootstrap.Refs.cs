using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace ARDiabetes
{
    // Pequeños contenedores de referencias para poder reasignar los campos "actuales" al cambiar
    // de libro/pantalla sin duplicar toda la lógica de ARStateChanged/PlayNarration/etc.
    class DetalleRefs : MonoBehaviour
    {
        public Image Band, Img; public TMP_Text Title, Desc;
        public void Set(Image band, Image img, TMP_Text title, TMP_Text desc) { Band = band; Img = img; Title = title; Desc = desc; }
    }

    class ArRefs : MonoBehaviour
    {
        public RawImage Raw; public Image Bg; public TMP_Text Hint, Title, Toast;
        public void Set(RawImage raw, Image bg, TMP_Text hint, TMP_Text title, TMP_Text toast)
        { Raw = raw; Bg = bg; Hint = hint; Title = title; Toast = toast; }
    }

    // Arrastre horizontal sobre la pantalla de AR para rotar la figura (real o de respaldo).
    class DragRotate : MonoBehaviour, IDragHandler
    {
        public Action<float> OnDragX;
        public void OnDrag(PointerEventData e) => OnDragX?.Invoke(e.delta.x);
    }

    class ProgresoRefs : MonoBehaviour
    {
        public TMP_Text Stats; public Image OverallFill; public Image[] BookFill; public TMP_Text[] BookCount;
        public RectTransform PhotoGrid;
        public void Set(TMP_Text stats, Image overallFill, Image[] bookFill, TMP_Text[] bookCount, RectTransform photoGrid)
        { Stats = stats; OverallFill = overallFill; BookFill = bookFill; BookCount = bookCount; PhotoGrid = photoGrid; }
    }

    class InsigniasRefs : MonoBehaviour
    {
        public TMP_Text Subtitle; public Image[] IconBgs; public TMP_Text[] Titles; public TMP_Text[] Status;
        public void Set(TMP_Text subtitle, Image[] iconBgs, TMP_Text[] titles, TMP_Text[] status)
        { Subtitle = subtitle; IconBgs = iconBgs; Titles = titles; Status = status; }
    }

    class ColeccionRefs : MonoBehaviour
    {
        public TMP_Text Subtitle; public Image[] IconBgs; public TMP_Text[] Titles; public TMP_Text[] Status;
        public int[] CardBook; public int[] CardTopic;
        public void Set(TMP_Text subtitle, Image[] iconBgs, TMP_Text[] titles, TMP_Text[] status, int[] cardBook, int[] cardTopic)
        { Subtitle = subtitle; IconBgs = iconBgs; Titles = titles; Status = status; CardBook = cardBook; CardTopic = cardTopic; }
    }

    class ConfigRefs : MonoBehaviour
    {
        public TMP_Text AgeSub, MuteSub;
        public void Set(TMP_Text ageSub, TMP_Text muteSub) { AgeSub = ageSub; MuteSub = muteSub; }
    }

    class JuegosRefs : MonoBehaviour
    {
        public TMP_Text[] Count;
        public void Set(TMP_Text[] count) { Count = count; }
    }

    class QuizRefs : MonoBehaviour
    {
        public TMP_Text Progress, Question, ResultText;
        public RectTransform ResultPanel;

        public RectTransform McGroup;
        public Button[] McButtons; public Image[] McImages; public TMP_Text[] McLabels;

        public RectTransform MatchGroup;
        public Button[] LeftButtons; public Image[] LeftImages; public TMP_Text[] LeftLabels;
        public Button[] RightButtons; public Image[] RightImages; public TMP_Text[] RightLabels;

        public RectTransform MsGroup;
        public Button[] MsButtons; public Image[] MsImages; public TMP_Text[] MsLabels;
        public Button MsConfirm;

        public void Set(TMP_Text progress, TMP_Text question, TMP_Text resultText,
            RectTransform mcGroup, Button[] mcButtons, Image[] mcImages, TMP_Text[] mcLabels,
            RectTransform matchGroup, Button[] leftButtons, Image[] leftImages, TMP_Text[] leftLabels,
            Button[] rightButtons, Image[] rightImages, TMP_Text[] rightLabels,
            RectTransform msGroup, Button[] msButtons, Image[] msImages, TMP_Text[] msLabels,
            Button msConfirm, RectTransform resultPanel)
        {
            Progress = progress; Question = question; ResultText = resultText; ResultPanel = resultPanel;
            McGroup = mcGroup; McButtons = mcButtons; McImages = mcImages; McLabels = mcLabels;
            MatchGroup = matchGroup;
            LeftButtons = leftButtons; LeftImages = leftImages; LeftLabels = leftLabels;
            RightButtons = rightButtons; RightImages = rightImages; RightLabels = rightLabels;
            MsGroup = msGroup; MsButtons = msButtons; MsImages = msImages; MsLabels = msLabels; MsConfirm = msConfirm;
        }
    }
}

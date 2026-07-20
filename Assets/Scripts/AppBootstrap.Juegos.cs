using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Pantalla 6) JUEGOS Y RETOS: hub por libro + quiz de 3-4 preguntas (Choice/Matching/MultiSelect).
    public partial class AppBootstrap
    {
        RectTransform BuildJuegos()
        {
            var p = Panel("6_Juegos");
            bool land = Land;
            HeaderBand(p, UIKit.Juegos, spIconJuegos, "Juegos y Retos", () => ShowOnly(pMenu), showBack: false);
            var sub = UIKit.Text(p, "Responde el quiz de cada libro y gana estrellas", 38, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!land) UIKit.Frac(sub, 0.08f, 0.815f, 0.92f, 0.865f);
            else UIKit.Frac(sub, 0.08f, 0.77f, 0.92f, 0.83f);

            var count = new TMP_Text[3];
            int cols = land ? 3 : 1, rows = land ? 1 : 3;
            float y0 = land ? 0.20f : 0.13f, y1 = land ? 0.72f : 0.79f;
            for (int i = 0; i < 3; i++)
            {
                var b = books[i];
                var rc = UIKit.Cell(i, cols, rows, 0.05f, y0, 0.95f, y1, 0.03f, 0.03f);
                int idx = i;
                var btn = UIKit.Button(p, "", UIKit.Card, UIKit.Navy, 30, () => { quizBook = idx; ShowOnly(bookQuiz[idx]); });
                UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                UIKit.AddShadow(btn.GetComponent<Image>(), 30, 0.12f, -6, 6);
                btn.gameObject.AddComponent<Appear>().delay = 0.05f * i;

                var disc = UIKit.Img(btn.transform, UIKit.Circle(), "Disc"); disc.color = b.Accent;
                var tic = UIKit.Img(btn.transform, b.HeroIcon, "Ic"); tic.color = Color.white;
                var tl = UIKit.Text(btn.transform, "Quiz " + b.Title.Replace("Libro ", ""), 38, UIKit.Navy, TextAlignmentOptions.Left);
                var cnt = UIKit.Text(btn.transform, "", 28, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
                var chev = UIKit.Text(btn.transform, "›", 58, b.Accent, TextAlignmentOptions.Right);
                count[i] = cnt;

                if (!land)
                {
                    UIKit.Frac(disc, 0.04f, 0.18f, 0.26f, 0.82f);
                    UIKit.Frac(tic, 0.095f, 0.33f, 0.205f, 0.67f);
                    UIKit.Frac(tl, 0.31f, 0.46f, 0.87f, 0.82f);
                    UIKit.Frac(cnt, 0.31f, 0.16f, 0.87f, 0.44f);
                    UIKit.Frac(chev, 0.88f, 0.30f, 0.97f, 0.70f);
                }
                else
                {
                    UIKit.Frac(disc, 0.10f, 0.48f, 0.32f, 0.92f);
                    UIKit.Frac(tic, 0.155f, 0.575f, 0.265f, 0.795f);
                    UIKit.Frac(tl, 0.08f, 0.28f, 0.94f, 0.44f);
                    UIKit.Frac(cnt, 0.08f, 0.13f, 0.94f, 0.26f);
                    UIKit.Frac(chev, 0.85f, 0.60f, 0.95f, 0.78f);
                }
            }
            p.gameObject.AddComponent<JuegosRefs>().Set(count);
            BuildBottomNav(p, -1);
            return p;
        }

        void RefreshJuegos()
        {
            var refs = pJuegos.GetComponent<JuegosRefs>();
            if (refs == null) return;
            for (int i = 0; i < 3; i++)
                if (refs.Count[i] != null) refs.Count[i].text = AppState.QuizCorrectCount(i) + "/" + books[i].Quiz.Length + " correctas";
        }

        static readonly Color GreenOk = UIKit.Hex("BBF0C6");
        static readonly Color RedBad = UIKit.Hex("F8C7CE");
        static readonly Color BlueSel = UIKit.Hex("D8E8FB");

        RectTransform BuildQuiz(int book)
        {
            var b = books[book];
            var p = Panel(book + "_Quiz");
            bool land = Land;
            HeaderBand(p, b.Accent, b.HeroIcon, "Quiz: " + b.Title, () => ShowOnly(pJuegos));

            // Cuántas opciones/pares necesita CADA tipo de juego en este libro (0 = ese tipo no
            // aparece en este libro). Se construye solo lo que hace falta, ya posicionado y con
            // su sombra final — así se evita el bug de sombras "flotantes" por reposicionar
            // después de llamar AddShadow.
            int mcCount = 0, pairCount = 0, msCount = 0;
            foreach (var g in b.Quiz)
            {
                if (g.Type == GameType.Choice) mcCount = Mathf.Max(mcCount, g.Options.Length);
                else if (g.Type == GameType.Matching) pairCount = Mathf.Max(pairCount, g.Left.Length);
                else if (g.Type == GameType.MultiSelect) msCount = Mathf.Max(msCount, g.Options.Length);
            }

            var progress = UIKit.Text(p, "", 32, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            var card = UIKit.Box(p, UIKit.Card, 36, "QCard");
            var question = UIKit.Text(card.transform, "", 38, UIKit.Navy, TextAlignmentOptions.Center);
            UIKit.Frac(question, 0.06f, 0.08f, 0.94f, 0.92f);

            float progY0, progY1, cardY0, cardY1, ansY0, ansY1, ax0, ax1;
            if (!land) { progY0 = 0.835f; progY1 = 0.87f; cardY0 = 0.62f; cardY1 = 0.815f; ansY0 = 0.065f; ansY1 = 0.47f; ax0 = 0.08f; ax1 = 0.92f; }
            else { progY0 = 0.77f; progY1 = 0.82f; cardY0 = 0.53f; cardY1 = 0.75f; ansY0 = 0.07f; ansY1 = 0.42f; ax0 = 0.14f; ax1 = 0.86f; }
            UIKit.Frac(progress, ax0, progY0, ax1, progY1);
            UIKit.Frac(card.rectTransform, 0.06f, cardY0, 0.94f, cardY1);

            // ---- Grupo Choice (opción múltiple / verdadero-falso / completar frase) ----
            var mcGroup = UIKit.Node("McGroup", p);
            UIKit.Stretch(mcGroup);
            var mcBtn = new Button[mcCount]; var mcImg = new Image[mcCount]; var mcLbl = new TMP_Text[mcCount];
            if (mcCount > 0)
            {
                float gap = land ? 0.02f : 0.025f;
                float oh = (ansY1 - ansY0 - gap * (mcCount - 1)) / mcCount;
                for (int i = 0; i < mcCount; i++)
                {
                    var btn = UIKit.Button(mcGroup, "", UIKit.Card, UIKit.Navy, 30, null);
                    var lbl = UIKit.Text(btn.transform, "", 32, UIKit.Navy, TextAlignmentOptions.Left);
                    UIKit.Frac(lbl, 0.08f, 0.1f, 0.92f, 0.9f);
                    float top = ansY1 - i * (oh + gap);
                    UIKit.Frac(R(btn), ax0, top - oh, ax1, top);
                    mcBtn[i] = btn; mcImg[i] = btn.GetComponent<Image>(); mcLbl[i] = lbl;
                }
            }

            // ---- Grupo Matching (relacionar, tap-to-pair) ----
            var matchGroup = UIKit.Node("MatchGroup", p);
            UIKit.Stretch(matchGroup);
            var leftBtn = new Button[pairCount]; var leftImg = new Image[pairCount]; var leftLbl = new TMP_Text[pairCount];
            var rightBtn = new Button[pairCount]; var rightImg = new Image[pairCount]; var rightLbl = new TMP_Text[pairCount];
            if (pairCount > 0)
            {
                float gap = 0.018f;
                float rh = (ansY1 - ansY0 - gap * (pairCount - 1)) / pairCount;
                float lx0 = land ? 0.10f : 0.05f, lx1 = 0.47f, rx0 = 0.53f, rx1 = land ? 0.90f : 0.95f;
                for (int i = 0; i < pairCount; i++)
                {
                    float top = ansY1 - i * (rh + gap);
                    var lb = UIKit.Button(matchGroup, "", UIKit.Card, UIKit.Navy, 24, null, 26);
                    var ll = UIKit.Text(lb.transform, "", 26, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Normal);
                    UIKit.Frac(ll, 0.06f, 0.1f, 0.94f, 0.9f);
                    UIKit.Frac(R(lb), lx0, top - rh, lx1, top);
                    leftBtn[i] = lb; leftImg[i] = lb.GetComponent<Image>(); leftLbl[i] = ll;

                    var rb = UIKit.Button(matchGroup, "", UIKit.Card, UIKit.Navy, 24, null, 26);
                    var rl = UIKit.Text(rb.transform, "", 26, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Normal);
                    UIKit.Frac(rl, 0.06f, 0.1f, 0.94f, 0.9f);
                    UIKit.Frac(R(rb), rx0, top - rh, rx1, top);
                    rightBtn[i] = rb; rightImg[i] = rb.GetComponent<Image>(); rightLbl[i] = rl;
                }
            }

            // ---- Grupo MultiSelect (grilla de opciones + botón Confirmar) ----
            var msGroup = UIKit.Node("MsGroup", p);
            UIKit.Stretch(msGroup);
            var msBtn = new Button[msCount]; var msImg = new Image[msCount]; var msLbl = new TMP_Text[msCount];
            Button msConfirm = null; Image msConfirmImg = null;
            if (msCount > 0)
            {
                int cols = 2, rows = Mathf.CeilToInt(msCount / 2f);
                float confirmH = 0.09f, gridY0 = ansY0 + confirmH + 0.03f, gridY1 = ansY1;
                for (int i = 0; i < msCount; i++)
                {
                    var rc = UIKit.Cell(i, cols, rows, ax0, gridY0, ax1, gridY1, 0.03f, 0.02f);
                    var btn = UIKit.Button(msGroup, "", UIKit.Card, UIKit.Navy, 24, null, 28);
                    var lbl = UIKit.Text(btn.transform, "", 28, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Normal);
                    UIKit.Stretch(lbl.rectTransform, 10);
                    UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                    msBtn[i] = btn; msImg[i] = btn.GetComponent<Image>(); msLbl[i] = lbl;
                }
                msConfirm = UIKit.Button(msGroup, "Confirmar", b.Accent, Color.white, 32, null, 34);
                UIKit.Frac(R(msConfirm), ax0, ansY0, ax1, ansY0 + confirmH);
                msConfirmImg = msConfirm.GetComponent<Image>();
            }

            // AddShadow lee el rect ACTUAL del elemento para calcular la sombra: se llama después
            // de fijar la posición final (Frac) de cada grupo, ya construido a su tamaño real.
            UIKit.AddShadow(card, 36, 0.10f, -6, 6);
            for (int i = 0; i < mcCount; i++) UIKit.AddShadow(mcImg[i], 26, 0.10f, -5, 5);
            for (int i = 0; i < pairCount; i++) { UIKit.AddShadow(leftImg[i], 18, 0.10f, -4, 4); UIKit.AddShadow(rightImg[i], 18, 0.10f, -4, 4); }
            for (int i = 0; i < msCount; i++) UIKit.AddShadow(msImg[i], 20, 0.10f, -4, 4);
            if (msConfirmImg != null) UIKit.AddShadow(msConfirmImg, 24, 0.12f, -5, 5);

            // Overlay de resultado final (oculto hasta terminar los juegos del libro)
            var resultPanel = UIKit.Node("Result", p);
            UIKit.Stretch(resultPanel);
            var resBg = UIKit.Box(resultPanel, new Color(0.04f, 0.07f, 0.11f, 0.88f), 0, "Bg");
            UIKit.Stretch(resBg.rectTransform);
            var resultText = UIKit.Text(resultPanel, "", 52, Color.white);
            var retryBtn = UIKit.Button(resultPanel, "Reintentar", b.Accent, Color.white, 36, () => ShowOnly(p, false));
            var backBtn = UIKit.Button(resultPanel, "Volver a Juegos", UIKit.Blue, Color.white, 36, () => ShowOnly(pJuegos));
            if (!land)
            {
                UIKit.Frac(resultText, 0.1f, 0.5f, 0.9f, 0.74f);
                UIKit.Frac(R(retryBtn), 0.15f, 0.32f, 0.85f, 0.39f);
                UIKit.Frac(R(backBtn), 0.15f, 0.22f, 0.85f, 0.29f);
            }
            else
            {
                UIKit.Frac(resultText, 0.1f, 0.48f, 0.9f, 0.76f);
                UIKit.Frac(R(retryBtn), 0.30f, 0.30f, 0.70f, 0.40f);
                UIKit.Frac(R(backBtn), 0.30f, 0.16f, 0.70f, 0.26f);
            }
            resultPanel.gameObject.SetActive(false);

            var refs = p.gameObject.AddComponent<QuizRefs>();
            refs.Set(progress, question, resultText, mcGroup, mcBtn, mcImg, mcLbl,
                matchGroup, leftBtn, leftImg, leftLbl, rightBtn, rightImg, rightLbl,
                msGroup, msBtn, msImg, msLbl, msConfirm, resultPanel);

            for (int i = 0; i < mcCount; i++) { int idx = i; mcBtn[i].onClick.AddListener(() => AnswerChoice(refs, idx)); }
            for (int i = 0; i < pairCount; i++)
            {
                int idx = i;
                leftBtn[i].onClick.AddListener(() => TapLeft(refs, idx));
                rightBtn[i].onClick.AddListener(() => TapRight(refs, idx));
            }
            for (int i = 0; i < msCount; i++) { int idx = i; msBtn[i].onClick.AddListener(() => ToggleMs(refs, idx)); }
            if (msConfirm != null) msConfirm.onClick.AddListener(() => ConfirmMs(refs));
            return p;
        }

        void RefreshQuiz()
        {
            quizQ = 0; quizScore = 0;
            var refs = bookQuiz[quizBook].GetComponent<QuizRefs>();
            if (refs == null) return;
            refs.ResultPanel.gameObject.SetActive(false);
            ShowQuizQuestion(refs);
        }

        void ShowQuizQuestion(QuizRefs refs)
        {
            var qz = books[quizBook].Quiz[quizQ];
            refs.Question.text = qz.Q;
            refs.Progress.text = "Pregunta " + (quizQ + 1) + "/" + books[quizBook].Quiz.Length;
            refs.McGroup.gameObject.SetActive(qz.Type == GameType.Choice);
            refs.MatchGroup.gameObject.SetActive(qz.Type == GameType.Matching);
            refs.MsGroup.gameObject.SetActive(qz.Type == GameType.MultiSelect);

            if (qz.Type == GameType.Choice)
            {
                quizAnswered = false;
                for (int i = 0; i < refs.McButtons.Length; i++)
                {
                    bool active = i < qz.Options.Length;
                    refs.McButtons[i].gameObject.SetActive(active);
                    if (!active) continue;
                    refs.McLabels[i].text = qz.Options[i];
                    refs.McImages[i].color = UIKit.Card;
                    refs.McButtons[i].interactable = true;
                }
            }
            else if (qz.Type == GameType.Matching)
            {
                int n = qz.Left.Length;
                matchRightOrder = new int[n];
                for (int i = 0; i < n; i++) matchRightOrder[i] = i;
                for (int i = n - 1; i > 0; i--) // Fisher-Yates: orden visual de la columna derecha
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    int tmp = matchRightOrder[i]; matchRightOrder[i] = matchRightOrder[j]; matchRightOrder[j] = tmp;
                }
                matchDoneLeft = new bool[n]; matchDoneRight = new bool[n];
                matchSelected = -1; matchCount = 0; matchLocked = false;
                for (int i = 0; i < refs.LeftButtons.Length; i++)
                {
                    bool active = i < n;
                    refs.LeftButtons[i].gameObject.SetActive(active);
                    refs.RightButtons[i].gameObject.SetActive(active);
                    if (!active) continue;
                    refs.LeftLabels[i].text = qz.Left[i];
                    refs.RightLabels[i].text = qz.Right[matchRightOrder[i]];
                    refs.LeftImages[i].color = UIKit.Card;
                    refs.RightImages[i].color = UIKit.Card;
                    refs.LeftButtons[i].interactable = true;
                    refs.RightButtons[i].interactable = true;
                }
            }
            else // MultiSelect
            {
                int n = qz.Options.Length;
                msSelected = new bool[n]; msAnswered = false;
                for (int i = 0; i < refs.MsButtons.Length; i++)
                {
                    bool active = i < n;
                    refs.MsButtons[i].gameObject.SetActive(active);
                    if (!active) continue;
                    refs.MsLabels[i].text = qz.Options[i];
                    refs.MsImages[i].color = UIKit.Card;
                    refs.MsButtons[i].interactable = true;
                }
                if (refs.MsConfirm != null) refs.MsConfirm.interactable = true;
            }
        }

        void AnswerChoice(QuizRefs refs, int optIdx)
        {
            if (quizAnswered) return;
            quizAnswered = true;
            var qz = books[quizBook].Quiz[quizQ];
            bool correct = optIdx == qz.Correct;
            refs.McImages[optIdx].color = correct ? GreenOk : RedBad;
            if (!correct) refs.McImages[qz.Correct].color = GreenOk;
            for (int i = 0; i < qz.Options.Length; i++) refs.McButtons[i].interactable = false;
            RegisterAnswer(correct);
            StartCoroutine(NextQuizStep(refs));
        }

        void TapLeft(QuizRefs refs, int idx)
        {
            if (matchLocked || matchDoneLeft[idx]) return;
            matchSelected = idx;
            for (int i = 0; i < matchDoneLeft.Length; i++)
                if (!matchDoneLeft[i]) refs.LeftImages[i].color = i == idx ? BlueSel : UIKit.Card;
        }

        void TapRight(QuizRefs refs, int row)
        {
            if (matchLocked || matchDoneRight[row] || matchSelected < 0) return;
            var qz = books[quizBook].Quiz[quizQ];
            int leftIdx = matchSelected;
            bool correct = matchRightOrder[row] == leftIdx;
            if (correct)
            {
                matchDoneLeft[leftIdx] = true; matchDoneRight[row] = true;
                refs.LeftImages[leftIdx].color = GreenOk; refs.RightImages[row].color = GreenOk;
                refs.LeftButtons[leftIdx].interactable = false; refs.RightButtons[row].interactable = false;
                matchSelected = -1; matchCount++;
                if (matchCount >= qz.Left.Length)
                {
                    RegisterAnswer(true);
                    StartCoroutine(NextQuizStep(refs));
                }
            }
            else
            {
                StartCoroutine(FlashWrongMatch(refs, leftIdx, row));
            }
        }

        IEnumerator FlashWrongMatch(QuizRefs refs, int leftIdx, int row)
        {
            matchLocked = true;
            refs.LeftImages[leftIdx].color = RedBad;
            refs.RightImages[row].color = RedBad;
            yield return new WaitForSeconds(0.45f);
            if (!matchDoneLeft[leftIdx]) refs.LeftImages[leftIdx].color = UIKit.Card;
            if (!matchDoneRight[row]) refs.RightImages[row].color = UIKit.Card;
            matchSelected = -1;
            matchLocked = false;
        }

        void ToggleMs(QuizRefs refs, int idx)
        {
            if (msAnswered) return;
            msSelected[idx] = !msSelected[idx];
            refs.MsImages[idx].color = msSelected[idx] ? BlueSel : UIKit.Card;
        }

        void ConfirmMs(QuizRefs refs)
        {
            if (msAnswered) return;
            msAnswered = true;
            var qz = books[quizBook].Quiz[quizQ];
            bool allCorrect = true;
            for (int i = 0; i < qz.CorrectMask.Length; i++)
            {
                bool shouldSel = qz.CorrectMask[i];
                refs.MsImages[i].color = msSelected[i] == shouldSel ? GreenOk : RedBad;
                refs.MsButtons[i].interactable = false;
                if (msSelected[i] != shouldSel) allCorrect = false;
            }
            if (refs.MsConfirm != null) refs.MsConfirm.interactable = false;
            RegisterAnswer(allCorrect);
            StartCoroutine(NextQuizStep(refs));
        }

        void RegisterAnswer(bool correct)
        {
            if (!correct) return;
            quizScore++;
            var msgs = AppState.MarkQuizCorrect(quizBook, quizQ);
            if (msgs.Count > 0) ShowReward(string.Join("\n", msgs));
        }

        IEnumerator NextQuizStep(QuizRefs refs)
        {
            yield return new WaitForSeconds(0.9f);
            quizQ++;
            if (quizQ >= books[quizBook].Quiz.Length) ShowQuizResult(refs);
            else ShowQuizQuestion(refs);
        }

        void ShowQuizResult(QuizRefs refs)
        {
            refs.ResultPanel.gameObject.SetActive(true);
            refs.ResultText.text = "¡Terminaste!\n" + quizScore + "/" + books[quizBook].Quiz.Length + " correctas";
        }
    }
}

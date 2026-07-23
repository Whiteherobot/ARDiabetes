using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Pantallas 1) INICIO 2) BIENVENIDA (carrusel) 3) PERFIL 4) MENÚ.
    public partial class AppBootstrap
    {
        // ============================================================
        // 1 - INICIO
        // ============================================================
        RectTransform BuildInicio()
        {
            var p = Panel("1_Inicio");
            var logo = UIKit.Img(p, spLogo, "Logo"); logo.preserveAspect = true;
            var t1 = UIKit.Text(p, "Diabetes Tipo 1", 120, UIKit.Navy);
            var t2 = UIKit.Text(p, "Aprendo, entiendo y me cuido", 48, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            var b = UIKit.Button(p, "COMENZAR", UIKit.Blue, Color.white, 40, () => ShowOnly(pBienvenida));

            if (!Land)
            {
                UIKit.Frac(logo.rectTransform, 0.16f, 0.55f, 0.84f, 0.88f);
                UIKit.Frac(t1.rectTransform, 0.06f, 0.42f, 0.94f, 0.53f);
                UIKit.Frac(t2.rectTransform, 0.10f, 0.36f, 0.90f, 0.41f);
                UIKit.Frac(R(b), 0.18f, 0.08f, 0.82f, 0.155f);
            }
            else
            {
                UIKit.Frac(logo.rectTransform, 0.30f, 0.46f, 0.70f, 0.93f);
                UIKit.Frac(t1.rectTransform, 0.08f, 0.30f, 0.92f, 0.46f);
                UIKit.Frac(t2.rectTransform, 0.14f, 0.22f, 0.86f, 0.30f);
                UIKit.Frac(R(b), 0.34f, 0.07f, 0.66f, 0.18f);
            }
            UIKit.AddShadow(b.GetComponent<Image>(), 40, 0.18f, -10, 6);
            return p;
        }

        // ============================================================
        // 2 - BIENVENIDA (carrusel)
        // ============================================================
        RectTransform BuildBienvenida()
        {
            var p = Panel("2_Bienvenida");
            bool land = Land;

            var card = UIKit.Box(p, UIKit.Card, 48, "Card");
            if (!land) UIKit.Frac(card.rectTransform, 0.06f, 0.27f, 0.94f, 0.91f);
            else UIKit.Frac(card.rectTransform, 0.07f, 0.26f, 0.93f, 0.84f);
            UIKit.AddShadow(card, 48, 0.12f, -10, 10);

            var viewport = UIKit.Node("Viewport", card.transform);
            UIKit.Frac(viewport, 0.04f, 0.14f, 0.96f, 0.97f);
            viewport.gameObject.AddComponent<RectMask2D>();
            var catcher = viewport.gameObject.AddComponent<Image>();
            catcher.color = new Color(1, 1, 1, 0); catcher.raycastTarget = true;

            Canvas.ForceUpdateCanvases();
            float w = viewport.rect.width, h = viewport.rect.height;
            if (w < 10) { w = (land ? Screen.height : Screen.width) * 0.8f; h = w; }

            var content = UIKit.Node("Content", viewport);
            content.anchorMin = content.anchorMax = new Vector2(0, 0.5f);
            content.pivot = new Vector2(0, 0.5f);
            content.sizeDelta = new Vector2(w * 3, h);
            content.anchoredPosition = Vector2.zero;

            string[] titles = { "¡Hola!", "Explora 3 libros", "Escanea y juega" };
            string[] bodies =
            {
                "Esta app usa realidad aumentada para ayudarte a entender la diabetes tipo 1 de una forma divertida e interactiva.",
                "Descubre tu cuerpo, tu alimentación y tu cuidado clínico con modelos 3D que cobran vida en AR.",
                "Apunta la cámara a las páginas del libro, gana estrellas y sube de nivel con juegos y retos."
            };
            Sprite[] imgs = { spWelcome != null ? spWelcome : spLogo, spLogo, spIconScan };
            for (int i = 0; i < 3; i++)
            {
                var slide = UIKit.Node("Slide" + i, content);
                slide.anchorMin = slide.anchorMax = new Vector2(0, 0.5f);
                slide.pivot = new Vector2(0, 0.5f);
                slide.sizeDelta = new Vector2(w, h);
                slide.anchoredPosition = new Vector2(i * w, 0);

                var tt = UIKit.Text(slide, titles[i], 78, UIKit.Navy);
                UIKit.Frac(tt, 0.05f, 0.82f, 0.95f, 0.98f);
                var bd = UIKit.Text(slide, bodies[i], 44, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
                UIKit.Frac(bd, land ? 0.06f : 0.08f, 0.52f, land ? 0.94f : 0.92f, 0.80f);
                var im = UIKit.Img(slide, imgs[i], "Img"); im.preserveAspect = true;
                UIKit.Frac(im, 0.30f, 0.05f, 0.70f, 0.50f);
            }

            var dots = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var d = UIKit.Img(card.transform, UIKit.Circle(), "Dot" + i);
                d.preserveAspect = true;
                d.color = i == 0 ? UIKit.Blue : UIKit.Hex("C4D3E0");
                float cx = 0.5f + (i - 1) * 0.045f;
                UIKit.Frac(d, cx - 0.012f, 0.045f, cx + 0.012f, 0.095f);
                dots[i] = d;
            }

            var siguiente = UIKit.Button(p, "SIGUIENTE", UIKit.Blue, Color.white, 40, null);
            var omitir = UIKit.Button(p, "Omitir", new Color(0, 0, 0, 0), UIKit.Muted, 20, () => ShowOnly(pMenu), 38, false);
            if (!land)
            {
                UIKit.Frac(R(siguiente), 0.18f, 0.10f, 0.82f, 0.175f);
                UIKit.Frac(R(omitir), 0.30f, 0.05f, 0.70f, 0.095f);
            }
            else
            {
                UIKit.Frac(R(siguiente), 0.34f, 0.10f, 0.66f, 0.19f);
                UIKit.Frac(R(omitir), 0.40f, 0.03f, 0.60f, 0.09f);
            }
            UIKit.AddShadow(siguiente.GetComponent<Image>(), 40, 0.18f, -10, 6);
            var sigLabel = siguiente.GetComponentInChildren<TMP_Text>();

            var carousel = viewport.gameObject.AddComponent<Carousel>();
            carousel.content = content;
            carousel.slideWidth = w;
            carousel.count = 3;
            carousel.dots = dots;
            carousel.dotOn = UIKit.Blue;
            carousel.dotOff = UIKit.Hex("C4D3E0");
            carousel.onComplete = () => ShowOnly(pPerfil);
            carousel.onIndexChanged = idx => { if (sigLabel != null) sigLabel.text = idx == 2 ? "EMPEZAR" : "SIGUIENTE"; };
            siguiente.onClick.AddListener(() => carousel.Next());
            return p;
        }

        // ============================================================
        // 3 - PERFIL
        // ============================================================
        RectTransform BuildPerfil()
        {
            var p = Panel("3_Perfil");
            bool land = Land;

            var t = UIKit.Text(p, "¿Cuál es tu edad?", 74, UIKit.Navy);
            var sub = UIKit.Text(p, "Personalizamos la experiencia según tu edad", 42, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!land)
            {
                UIKit.Frac(t, 0.05f, 0.87f, 0.95f, 0.94f);
                UIKit.Frac(sub, 0.08f, 0.82f, 0.92f, 0.865f);
            }
            else
            {
                UIKit.Frac(t, 0.05f, 0.85f, 0.95f, 0.96f);
                UIKit.Frac(sub, 0.08f, 0.77f, 0.92f, 0.84f);
            }

            int cols = land ? 3 : 1, rows = land ? 1 : 3;
            float x0 = 0.05f, x1 = 0.95f, y0 = land ? 0.20f : 0.08f, y1 = land ? 0.70f : 0.78f;
            float gx = 0.03f, gy = 0.035f;
            Sprite[] avs = { spAvatar1, spAvatar2, spAvatar3 };
            string[] labs = { "5 - 9 años", "10 - 12 años", "13 - 15 años" };
            AppState.AgeGroup[] grp = { AppState.AgeGroup.Kids_5_9, AppState.AgeGroup.Kids_10_12, AppState.AgeGroup.Teens_13_15 };
            Color[] acc = { UIKit.Prog, UIKit.Nutri, UIKit.Juegos };
            for (int i = 0; i < 3; i++)
            {
                var rc = UIKit.Cell(i, cols, rows, x0, y0, x1, y1, gx, gy);
                AgeOption(p, rc, avs[i], labs[i], grp[i], acc[i], land);
            }
            return p;
        }

        void AgeOption(RectTransform parent, Rect rc, Sprite avatar, string label, AppState.AgeGroup group, Color accent, bool land)
        {
            var btn = UIKit.Button(parent, "", UIKit.Card, UIKit.Navy, 36, () => { AppState.Age = group; ShowOnly(pMenu); });
            UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
            UIKit.AddShadow(btn.GetComponent<Image>(), 36, 0.10f, -8, 8);

            var disc = UIKit.Img(btn.transform, UIKit.Circle(), "Disc"); disc.color = accent; disc.preserveAspect = true;
            var av = UIKit.Img(btn.transform, avatar, "Avatar"); av.preserveAspect = true;
            var tl = UIKit.Text(btn.transform, label, 54, UIKit.Navy, land ? TextAlignmentOptions.Center : TextAlignmentOptions.Left);

            if (!land)
            {
                UIKit.Frac(disc, 0.04f, 0.16f, 0.28f, 0.84f);
                UIKit.Frac(av, 0.05f, 0.14f, 0.27f, 0.86f);
                UIKit.Frac(tl, 0.32f, 0.2f, 0.88f, 0.8f);
                var chev = UIKit.Text(btn.transform, ">", 60, accent, TextAlignmentOptions.Right);
                UIKit.Frac(chev, 0.88f, 0.3f, 0.97f, 0.7f);
            }
            else
            {
                UIKit.Frac(disc, 0.28f, 0.52f, 0.72f, 0.92f);
                UIKit.Frac(av, 0.29f, 0.51f, 0.71f, 0.93f);
                UIKit.Frac(tl, 0.05f, 0.12f, 0.95f, 0.42f);
            }
        }

        // ============================================================
        // 4 - MENÚ PRINCIPAL
        // ============================================================
        RectTransform BuildMenu()
        {
            var p = Panel("4_Menu");
            bool land = Land;

            // ---- Header rico (saludo + nivel + progreso + estrellas) ----
            var head = UIKit.Box(p, UIKit.Card, 40, "Head");
            if (!land) UIKit.Frac(head.rectTransform, 0.05f, 0.85f, 0.95f, 0.975f);
            else UIKit.Frac(head.rectTransform, 0.04f, 0.80f, 0.96f, 0.97f);
            UIKit.AddShadow(head, 40, 0.10f, -6, 6);
            head.gameObject.AddComponent<Appear>();

            var avBtn = UIKit.Button(head.transform, "", UIKit.Soft, UIKit.Navy, 30, () => ShowOnly(pPerfil), 20, true);
            UIKit.Frac(R(avBtn), 0.025f, 0.14f, 0.20f, 0.86f);
            menuAvatar = UIKit.Img(avBtn.transform, AvatarFor(AppState.Age), "Avatar"); menuAvatar.preserveAspect = true;
            UIKit.Frac(menuAvatar, 0.08f, 0.08f, 0.92f, 0.92f);

            holaText = UIKit.Text(head.transform, AppState.GreetingFor(AppState.Age), 46, UIKit.Navy, TextAlignmentOptions.Left);
            var hola = holaText;
            UIKit.Frac(hola, 0.23f, 0.55f, 0.74f, 0.92f);
            nivelText = UIKit.Text(head.transform, "Nivel " + AppState.Level + " · Racha " + AppState.Streak, 30, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
            UIKit.Frac(nivelText, 0.23f, 0.30f, 0.74f, 0.52f);
            nivelFill = ProgressBar(head.transform, AppState.LevelProgress, 0.23f, 0.12f, 0.74f, 0.24f);

            var pill = UIKit.Box(head.transform, UIKit.Hex("FFF3D6"), 30, "Pill");
            UIKit.Frac(pill.rectTransform, 0.76f, 0.28f, 0.98f, 0.72f);
            var star = UIKit.Img(pill.transform, spStar, "Star"); star.preserveAspect = true;
            UIKit.Frac(star, 0.08f, 0.16f, 0.44f, 0.84f);
            starLabel = UIKit.Text(pill.transform, AppState.Stars.ToString(), 40, UIKit.Navy, TextAlignmentOptions.Left);
            UIKit.Frac(starLabel, 0.46f, 0.12f, 0.96f, 0.88f);

            var titulo = UIKit.Text(p, "Explora los libros", 50, UIKit.Navy);
            if (!land) UIKit.Frac(titulo, 0.08f, 0.79f, 0.92f, 0.84f);
            else UIKit.Frac(titulo, 0.08f, 0.72f, 0.92f, 0.78f);

            // ---- Grid de tiles ----
            string[] labels = { "Libro\nFisiológico", "Libro\nNutricional", "Libro\nClínico", "Escanear\nPágina AR", "Juegos\ny Retos", "Progreso" };
            Color[] colors = { UIKit.Hex("DD5450"), UIKit.Nutri, UIKit.Clin, UIKit.Scan, UIKit.Juegos, UIKit.Prog };
            Sprite[] icons = { spIconFisio, spIconNutri, spIconClinico, spIconScan, spIconJuegos, spIconProgreso };

            float gx = 0.03f, gy = 0.03f;
            float x0 = 0.05f, x1 = 0.95f;
            float y0 = land ? 0.20f : 0.235f, y1 = land ? 0.70f : 0.77f;
            for (int i = 0; i < 6; i++)
            {
                var rc = UIKit.Cell(i, 3, 2, x0, y0, x1, y1, gx, gy);
                string label = labels[i];
                int idx = i;
                var tile = UIKit.Button(p, "", colors[i], Color.white, 34, () => OnMenuTile(idx, label));
                UIKit.Frac(R(tile), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                UIKit.AddShadow(tile.GetComponent<Image>(), 34, 0.16f, -8, 8);
                tile.gameObject.AddComponent<Appear>().delay = 0.05f * i;

                var ic = UIKit.Img(tile.transform, icons[i], "Icon"); ic.preserveAspect = true;
                UIKit.Frac(ic, 0.22f, 0.42f, 0.78f, 0.92f);
                var tl = UIKit.Text(tile.transform, label, 40, Color.white);
                UIKit.Frac(tl, 0.05f, 0.06f, 0.95f, 0.40f);
            }

            BuildBottomNav(p, 0);
            return p;
        }

        void OnMenuTile(int idx, string label)
        {
            if (idx >= 0 && idx <= 2) { currentBook = idx; currentTopic = 0; ShowOnly(bookTemas[idx]); }
            else if (idx == 3) ShowOnly(pScanAR);
            else if (idx == 4) ShowOnly(pJuegos);
            else if (idx == 5) ShowOnly(pProgreso);
        }
    }
}

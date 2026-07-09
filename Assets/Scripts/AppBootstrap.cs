using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    /// <summary>
    /// Pantallas 1-4 con layout responsivo (anclaje por fracciones) que llena la pantalla y se
    /// reconstruye al rotar, soportando portrait y landscape.
    /// 1) INICIO  2) BIENVENIDA (carrusel)  3) PERFIL  4) MENÚ PRINCIPAL.
    /// </summary>
    public class AppBootstrap : MonoBehaviour
    {
        [Header("Sprites (asignados por ProjectSetup)")]
        public Sprite spLogo;
        public Sprite spWelcome;
        public Sprite spAvatar1, spAvatar2, spAvatar3;
        public Sprite spIconFisio, spIconNutri, spIconClinico, spIconScan, spIconJuegos, spIconProgreso;
        public Sprite spStar;
        public Sprite spPancreas;
        public GameObject pancreasModel;
        public TMP_FontAsset font;

        [Header("Iconos UI")]
        public Sprite icInfo, icAudio, icRotate, icClose, icCamera, icChart, icQuestion, icDrop, icGear, icHome, icBook, icPancreas;
        [Header("Audio narración (por tema)")]
        public AudioClip[] narracion;
        AudioSource audioSrc;
        [Header("Marcadores AR (QR)")]
        public Texture2D[] markers;

        Canvas canvas;
        RectTransform root;
        RectTransform pInicio, pBienvenida, pPerfil, pMenu;
        RectTransform pLibroTemas, pLibroDetalle, pLibroAR;
        RectTransform[] panels;
        int shown;
        bool builtLandscape;
        bool captureStarted;

        TMP_Text starLabel, toast;
        Image menuAvatar;

        // --- Libro Fisiológico (5A) ---
        ModelViewer modelViewer;
        ARController arController;
        RawImage arRaw;
        Image arBg;
        TMP_Text arHintText;
        Camera mainCam;
        int currentTopic;
        TMP_Text detTitle, detDesc, arTitle, arInfo, arToast;
        Image detImg, detBand;
        RectTransform arInfoCard;

        static readonly string[] TopicTitles =
        {
            "¿Qué es la diabetes tipo 1?", "El páncreas", "Insulina y glucosa", "Cómo funciona"
        };
        static readonly string[] TopicDesc =
        {
            "La diabetes tipo 1 aparece cuando el cuerpo casi no produce insulina, la hormona que nos da energía. ¡Aprende a conocerla y cuidarte!",
            "El páncreas es el órgano que fabrica la insulina en tu cuerpo. Explóralo en 3D y descubre cómo es por dentro.",
            "La insulina es como una llave que deja entrar la glucosa (energía) a tus células para que funcionen.",
            "Descubre cómo trabajan juntos el páncreas, la insulina y la glucosa para darte energía cada día."
        };
        static readonly string[] TopicSub =
        {
            "Conoce la enfermedad", "El órgano de la insulina", "Cómo obtienes energía", "Todo trabaja en equipo"
        };

        bool Land => Screen.width > Screen.height;
        static RectTransform R(Component c) => (RectTransform)c.transform;

        Sprite AvatarFor(AppState.AgeGroup g)
        {
            switch (g)
            {
                case AppState.AgeGroup.Kids_5_9: return spAvatar1;
                case AppState.AgeGroup.Kids_10_12: return spAvatar2;
                case AppState.AgeGroup.Teens_13_15: return spAvatar3;
                default: return spAvatar2;
            }
        }

        void Start()
        {
            if (font != null) UIKit.Font = font;
            audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.playOnAwake = false;
            if (pancreasModel != null) modelViewer = ModelViewer.Create(pancreasModel, UIKit.Hex("C86B6B"));
            mainCam = Camera.main;
            if (pancreasModel != null)
            {
                arController = ARController.Create(pancreasModel, UIKit.Hex("C86B6B"), markers);
                arController.OnState = ARStateChanged;
            }
            BuildAll();
            if (Environment.GetEnvironmentVariable("ARCAP") == "1" && !captureStarted)
            {
                captureStarted = true;
                StartCoroutine(AutoCapture());
            }
        }

        void Update()
        {
            if (canvas != null && Land != builtLandscape) BuildAll();
        }

        void BuildAll()
        {
            if (canvas != null) Destroy(canvas.gameObject);
            builtLandscape = Land;
            BuildCanvas();
            pInicio = BuildInicio();
            pBienvenida = BuildBienvenida();
            pPerfil = BuildPerfil();
            pMenu = BuildMenu();
            pLibroTemas = BuildLibroTemas();
            pLibroDetalle = BuildLibroDetalle();
            pLibroAR = BuildLibroAR();
            panels = new[] { pInicio, pBienvenida, pPerfil, pMenu, pLibroTemas, pLibroDetalle, pLibroAR };
            ShowOnly(panels[Mathf.Clamp(shown, 0, panels.Length - 1)], false);
        }

        // ---- Preview en el editor (ver las vistas sin darle Play) ----
        public void EditorPreview(int index)
        {
            EditorClearPreview();
            if (font != null) UIKit.Font = font;
            currentTopic = 1; // El páncreas
            shown = Mathf.Clamp(index, 0, 6);
            BuildAll();
        }

        public void EditorClearPreview()
        {
            canvas = null;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                if (go != null && go.GetComponent<Canvas>() != null)
                    DestroyImmediate(go);
        }

        // ============================================================
        void BuildCanvas()
        {
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                                    typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = Land ? new Vector2(1920, 1080) : new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = Land ? 1f : 0f; // portrait: ancho; landscape: alto

            var bg = UIKit.Img(canvas.transform, UIKit.VerticalGradient(UIKit.SkyTop, UIKit.Sky), "Background");
            bg.preserveAspect = false;
            UIKit.Stretch(bg.rectTransform);

            // Blobs decorativos suaves (dan profundidad, menos plano)
            AddBlob(canvas.transform, UIKit.Fisio, 0.05f, 0.86f, 620);
            AddBlob(canvas.transform, UIKit.Nutri, 0.95f, 0.72f, 540);
            AddBlob(canvas.transform, UIKit.Scan, 0.90f, 0.12f, 600);
            AddBlob(canvas.transform, UIKit.Clin, 0.08f, 0.20f, 520);

            root = UIKit.Node("SafeRoot", canvas.transform);
            UIKit.Stretch(root);
            root.gameObject.AddComponent<SafeArea>();

            // Toast global (visible en cualquier pantalla)
            toast = UIKit.Text(canvas.transform, "", 40, UIKit.BlueDark);
            var trt = toast.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0f); trt.pivot = new Vector2(0.5f, 0f);
            trt.anchoredPosition = new Vector2(0, 210); trt.sizeDelta = new Vector2(940, 90);
        }

        RectTransform Panel(string name)
        {
            var rt = UIKit.Node(name, root);
            UIKit.Stretch(rt);
            rt.gameObject.AddComponent<CanvasGroup>();
            return rt;
        }

        // Barra inferior de accesos rápidos (Inicio / Progreso / Config / Ayuda)
        void BuildBottomNav(RectTransform p, int active)
        {
            var bar = UIKit.Box(p, UIKit.Card, 40, "BottomNav");
            var brt = bar.rectTransform;
            brt.anchorMin = new Vector2(0, 0); brt.anchorMax = new Vector2(1, 0); brt.pivot = new Vector2(0.5f, 0);
            float h = 150f;
            brt.offsetMin = new Vector2(40, 22); brt.offsetMax = new Vector2(-40, 22 + h);
            UIKit.AddShadow(bar, 40, 0.12f, -6, 6);

            Sprite[] ic = { icHome, icChart, icGear, icQuestion };
            string[] lb = { "Inicio", "Progreso", "Config", "Ayuda" };
            System.Action[] act =
            {
                () => ShowOnly(pMenu),
                () => Toast("Progreso: próximamente"),
                () => Toast("Configuración: próximamente"),
                () => Toast("Ayuda: próximamente")
            };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                bool on = i == active;
                var b = UIKit.Button(bar.transform, "", new Color(0, 0, 0, 0), UIKit.Navy, 20, () => act[idx](), 20, true);
                var rt = R(b);
                rt.anchorMin = new Vector2(i / 4f, 0); rt.anchorMax = new Vector2((i + 1) / 4f, 1);
                rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
                var g = UIKit.Img(b.transform, ic[i], "Ico"); g.color = on ? UIKit.Blue : UIKit.Muted;
                UIKit.Frac(g, 0.32f, 0.42f, 0.68f, 0.86f);
                var t = UIKit.Text(b.transform, lb[i], 26, on ? UIKit.Blue : UIKit.Muted);
                UIKit.Frac(t, 0.05f, 0.10f, 0.95f, 0.40f);
            }
        }

        void AddBlob(Transform parent, Color c, float ax, float ay, float size)
        {
            var img = UIKit.Img(parent, UIKit.Circle(), "Blob");
            img.color = new Color(c.r, c.g, c.b, 0.10f);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(size, size);
        }

        // ============================================================
        void ShowOnly(RectTransform panel, bool animate = true)
        {
            shown = Array.IndexOf(panels, panel);
            if (shown < 0) shown = 0;
            if (audioSrc != null) audioSrc.Stop();
            foreach (var p in panels) if (p != null) p.gameObject.SetActive(p == panel);

            if (panel == pMenu)
            {
                if (starLabel != null) starLabel.text = AppState.Stars.ToString();
                if (menuAvatar != null) menuAvatar.sprite = AvatarFor(AppState.Age);
            }
            if (panel == pLibroDetalle) RefreshDetalle();
            if (panel == pLibroAR && arTitle != null)
            {
                arTitle.text = TopicTitles[currentTopic];
                if (arInfo != null) arInfo.text = TopicDesc[currentTopic];
                if (arInfoCard != null) arInfoCard.gameObject.SetActive(false);
            }
            if (arController != null)
            {
                if (panel == pLibroAR) arController.Activate();
                else
                {
                    arController.Deactivate();
                    if (mainCam != null) mainCam.enabled = true;
                    if (arRaw != null) arRaw.enabled = modelViewer != null;
                }
            }
            if (animate) { StartCoroutine(FadeIn(panel)); }
        }

        IEnumerator FadeIn(RectTransform panel)
        {
            var cg = panel.GetComponent<CanvasGroup>();
            float t = 0, dur = 0.26f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                float e = 1f - Mathf.Pow(1f - k, 3f); // ease-out
                if (cg != null) cg.alpha = e;
                panel.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, e);
                panel.anchoredPosition = new Vector2(0, Mathf.Lerp(34f, 0f, e));
                yield return null;
            }
            if (cg != null) cg.alpha = 1;
            panel.localScale = Vector3.one;
            panel.anchoredPosition = Vector2.zero;
        }

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

            var hola = UIKit.Text(head.transform, "¡Hola!", 46, UIKit.Navy, TextAlignmentOptions.Left);
            UIKit.Frac(hola, 0.23f, 0.55f, 0.74f, 0.92f);
            var nivel = UIKit.Text(head.transform, "Nivel " + AppState.Level + " · Racha " + AppState.Streak, 30, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
            UIKit.Frac(nivel, 0.23f, 0.30f, 0.74f, 0.52f);
            ProgressBar(head.transform, AppState.LevelProgress, 0.23f, 0.12f, 0.74f, 0.24f);

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
            Color[] colors = { UIKit.Fisio, UIKit.Nutri, UIKit.Clin, UIKit.Scan, UIKit.Juegos, UIKit.Prog };
            Sprite[] icons = { spIconFisio, spIconNutri, spIconClinico, spIconScan, spIconJuegos, spIconProgreso };

            float gx = 0.03f, gy = 0.03f;
            float x0 = 0.05f, x1 = 0.95f;
            float y0 = land ? 0.20f : 0.235f, y1 = land ? 0.70f : 0.77f;
            for (int i = 0; i < 6; i++)
            {
                var rc = UIKit.Cell(i, 3, 2, x0, y0, x1, y1, gx, gy);
                string label = labels[i];
                int idx = i;
                var tile = UIKit.Button(p, "", colors[i], Color.white, 34,
                    () => { if (idx == 0) ShowOnly(pLibroTemas); else Toast("Próximamente: " + label.Replace("\n", " ")); });
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

        void ProgressBar(Transform parent, float value, float x0, float y0, float x1, float y1)
        {
            var track = UIKit.Box(parent, UIKit.Hex("E2ECF5"), 16, "Track");
            UIKit.Frac(track.rectTransform, x0, y0, x1, y1);
            var fill = UIKit.Box(track.transform, UIKit.Nutri, 16, "Fill");
            UIKit.Frac(fill.rectTransform, 0f, 0f, Mathf.Clamp01(value), 1f);
        }

        // ============================================================
        // 5A - LIBRO FISIOLÓGICO (Temas -> Detalle -> Experiencia AR)
        // ============================================================
        static readonly Color[] TopicAccent = { UIKit.Clin, UIKit.Fisio, UIKit.Nutri, UIKit.Prog };

        // Botón circular con icono (para controles del visor AR).
        Button CircleBtn(RectTransform parent, Sprite icon, Color bg, Color iconTint,
                         UnityEngine.Events.UnityAction onClick, float ax, float ay, float size, float offx, float offy)
        {
            var go = new GameObject("CircleBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>(); img.sprite = UIKit.Circle(); img.color = bg;
            var b = go.AddComponent<Button>(); b.targetGraphic = img; b.transition = Selectable.Transition.None;
            go.AddComponent<PressEffect>();
            if (onClick != null) b.onClick.AddListener(onClick);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(offx, offy); rt.sizeDelta = new Vector2(size, size);
            if (icon != null) { var g = UIKit.Img(go.transform, icon, "Icon"); g.color = iconTint; UIKit.Frac(g, 0.26f, 0.26f, 0.74f, 0.74f); }
            return b;
        }

        void HeaderBand(RectTransform p, Sprite icon, string title, UnityEngine.Events.UnityAction onBack, bool showBack = true)
        {
            var band = UIKit.Box(p, UIKit.Fisio, 0, "Band");
            UIKit.Frac(band.rectTransform, 0f, Land ? 0.86f : 0.885f, 1f, 1.01f);
            if (showBack)
            {
                var back = UIKit.Button(band.transform, "‹", new Color(1, 1, 1, 0.22f), Color.white, 26, onBack, 46);
                var brt = R(back); brt.anchorMin = brt.anchorMax = new Vector2(0, 0.5f); brt.pivot = new Vector2(0, 0.5f);
                brt.anchoredPosition = new Vector2(36, 0); brt.sizeDelta = new Vector2(96, 96);
            }
            if (icon != null)
            {
                var g = UIKit.Img(band.transform, icon, "Ico"); g.color = Color.white;
                UIKit.Frac(g, showBack ? 0.135f : 0.05f, 0.24f, showBack ? 0.215f : 0.13f, 0.76f);
            }
            var t = UIKit.Text(band.transform, title, 50, Color.white);
            UIKit.Frac(t, showBack ? 0.24f : 0.16f, 0.1f, 0.94f, 0.9f);
        }

        RectTransform BuildLibroTemas()
        {
            var p = Panel("5A_Temas");
            HeaderBand(p, icBook, "Libro Fisiológico", () => ShowOnly(pMenu), showBack: false);
            var sub = UIKit.Text(p, "Elige un tema para explorar en 3D", 40, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!Land) UIKit.Frac(sub, 0.08f, 0.815f, 0.92f, 0.86f);
            else UIKit.Frac(sub, 0.1f, 0.77f, 0.9f, 0.83f);

            Sprite[] ticons = { icQuestion, icPancreas, icDrop, icGear };
            bool land = Land;
            int cols = land ? 2 : 1, rows = land ? 2 : 4;
            float y0 = land ? 0.22f : 0.13f, y1 = land ? 0.72f : 0.79f;
            for (int i = 0; i < 4; i++)
            {
                var rc = UIKit.Cell(i, cols, rows, 0.05f, y0, 0.95f, y1, 0.03f, 0.03f);
                int idx = i;
                Color acc = TopicAccent[i];
                var btn = UIKit.Button(p, "", UIKit.Card, UIKit.Navy, 30, () => { currentTopic = idx; ShowOnly(pLibroDetalle); });
                UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                UIKit.AddShadow(btn.GetComponent<Image>(), 30, 0.12f, -6, 6);
                btn.gameObject.AddComponent<Appear>().delay = 0.05f * i;

                // Círculo de color con el icono del tema (mismo lenguaje visual que Perfil)
                var disc = UIKit.Img(btn.transform, UIKit.Circle(), "Disc"); disc.color = acc;
                var tic = UIKit.Img(btn.transform, ticons[i], "Ic"); tic.color = Color.white;
                var tl = UIKit.Text(btn.transform, TopicTitles[i], 40, UIKit.Navy, TextAlignmentOptions.Left);
                var sb = UIKit.Text(btn.transform, TopicSub[i], 30, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
                var chev = UIKit.Text(btn.transform, "›", 58, acc, TextAlignmentOptions.Right);

                if (!land)
                {
                    UIKit.Frac(disc, 0.04f, 0.18f, 0.26f, 0.82f);
                    UIKit.Frac(tic, 0.095f, 0.33f, 0.205f, 0.67f);
                    UIKit.Frac(tl, 0.31f, 0.46f, 0.87f, 0.82f);
                    UIKit.Frac(sb, 0.31f, 0.16f, 0.87f, 0.44f);
                    UIKit.Frac(chev, 0.88f, 0.30f, 0.97f, 0.70f);
                }
                else
                {
                    UIKit.Frac(disc, 0.035f, 0.14f, 0.20f, 0.86f);
                    UIKit.Frac(tic, 0.075f, 0.32f, 0.16f, 0.68f);
                    UIKit.Frac(tl, 0.24f, 0.44f, 0.88f, 0.84f);
                    UIKit.Frac(sb, 0.24f, 0.14f, 0.88f, 0.42f);
                    UIKit.Frac(chev, 0.90f, 0.32f, 0.98f, 0.68f);
                }
            }
            BuildBottomNav(p, -1);
            return p;
        }

        void PlaceSquareFrac(Image img, float cx, float cy)
        {
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(cx, cy); rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero; rt.sizeDelta = new Vector2(64, 64);
        }

        RectTransform BuildLibroDetalle()
        {
            var p = Panel("5A_Detalle");
            HeaderBand(p, spPancreas, "Libro Fisiológico", () => ShowOnly(pLibroTemas));
            var card = UIKit.Box(p, UIKit.Card, 44, "Card");
            if (!Land) UIKit.Frac(card.rectTransform, 0.06f, 0.19f, 0.94f, 0.83f);
            else UIKit.Frac(card.rectTransform, 0.06f, 0.13f, 0.94f, 0.81f);
            UIKit.AddShadow(card, 44, 0.10f, -8, 8);

            detBand = UIKit.Box(card.transform, TopicAccent[currentTopic], 40, "Hero");
            detImg = UIKit.Img(detBand.transform, spPancreas, "Img"); detImg.preserveAspect = true;
            detTitle = UIKit.Text(card.transform, "", 54, UIKit.Navy);
            detDesc = UIKit.Text(card.transform, "", 40, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!Land)
            {
                UIKit.Frac(detBand.rectTransform, 0.06f, 0.55f, 0.94f, 0.95f);
                UIKit.Frac(detImg, 0.28f, 0.06f, 0.72f, 0.94f);
                UIKit.Frac(detTitle, 0.06f, 0.44f, 0.94f, 0.53f);
                UIKit.Frac(detDesc, 0.08f, 0.10f, 0.92f, 0.42f);
            }
            else
            {
                UIKit.Frac(detBand.rectTransform, 0.04f, 0.12f, 0.42f, 0.9f);
                UIKit.Frac(detImg, 0.08f, 0.08f, 0.92f, 0.92f);
                UIKit.Frac(detTitle, 0.46f, 0.66f, 0.97f, 0.88f);
                UIKit.Frac(detDesc, 0.46f, 0.2f, 0.97f, 0.62f);
            }

            var escuchar = UIKit.Button(p, "  Escuchar", UIKit.Blue, Color.white, 36, () => PlayNarration(), 38);
            var g = UIKit.Img(R(escuchar), icAudio, "Ico"); g.color = Color.white;
            var scan = UIKit.Button(p, "ESCANEAR PÁGINA", UIKit.Fisio, Color.white, 40, () => ShowOnly(pLibroAR));
            if (!Land)
            {
                UIKit.Frac(R(escuchar), 0.15f, 0.105f, 0.85f, 0.16f);
                UIKit.Frac(g, 0.16f, 0.2f, 0.30f, 0.8f);
                UIKit.Frac(R(scan), 0.15f, 0.04f, 0.85f, 0.095f);
            }
            else
            {
                UIKit.Frac(R(escuchar), 0.08f, 0.03f, 0.48f, 0.10f);
                UIKit.Frac(g, 0.1f, 0.2f, 0.24f, 0.8f);
                UIKit.Frac(R(scan), 0.52f, 0.03f, 0.92f, 0.10f);
            }
            UIKit.AddShadow(scan.GetComponent<Image>(), 40, 0.18f, -8, 6);
            RefreshDetalle();
            return p;
        }

        void RefreshDetalle()
        {
            if (detTitle == null) return;
            detTitle.text = TopicTitles[currentTopic];
            detDesc.text = TopicDesc[currentTopic];
            if (detBand != null) detBand.color = TopicAccent[currentTopic];
        }

        RectTransform BuildLibroAR()
        {
            var p = Panel("5A_AR");
            var bg = UIKit.Box(p, UIKit.Hex("14212E"), 4, "ARbg"); UIKit.Stretch(bg.rectTransform);
            arBg = bg;

            var raw = new GameObject("Model3D", typeof(RectTransform)).AddComponent<RawImage>();
            raw.transform.SetParent(p, false); raw.raycastTarget = false;
            if (modelViewer != null) { raw.texture = modelViewer.Texture; raw.color = Color.white; }
            else raw.color = new Color(1, 1, 1, 0);
            if (!Land) UIKit.Frac(raw.rectTransform, 0.06f, 0.24f, 0.94f, 0.83f);
            else UIKit.Frac(raw.rectTransform, 0.26f, 0.14f, 0.74f, 0.92f);
            arRaw = raw;

            arTitle = UIKit.Text(p, "", 50, Color.white);
            UIKit.Frac(arTitle, 0.10f, 0.905f, 0.90f, 0.97f);
            var pill = UIKit.Box(p, new Color(1, 1, 1, 0.12f), 30, "Hint");
            var hint = UIKit.Text(pill.transform, "Vista 3D · la cámara AR en vivo llega pronto", 30, UIKit.Hex("BFD3E6"), TextAlignmentOptions.Center, FontStyles.Normal);
            UIKit.Stretch(hint.rectTransform, 24);
            arHintText = hint;
            UIKit.Frac(pill.rectTransform, 0.14f, 0.855f, 0.86f, 0.90f);

            // Controles circulares
            CircleBtn(p, icInfo, UIKit.Blue, Color.white, () => { if (arInfoCard != null) arInfoCard.gameObject.SetActive(!arInfoCard.gameObject.activeSelf); }, 1f, 0.68f, 118, -82, 0);
            CircleBtn(p, icAudio, UIKit.Nutri, Color.white, () => PlayNarration(), 1f, 0.57f, 118, -82, 0);
            CircleBtn(p, icRotate, UIKit.Prog, Color.white, () => { if (modelViewer != null) modelViewer.ToggleSpin(); }, 1f, 0.46f, 118, -82, 0);
            CircleBtn(p, icClose, UIKit.Juegos, Color.white, () => ShowOnly(pLibroDetalle), 0.5f, 0f, 130, -190, 150);
            CircleBtn(p, icCamera, UIKit.Clin, Color.white, () => ShowARToast("Captura: próximamente"), 0.5f, 0f, 176, 0, 160);

            arToast = UIKit.Text(p, "", 36, Color.white);
            UIKit.Frac(arToast, 0.05f, 0.135f, 0.95f, 0.185f);

            arInfoCard = UIKit.Node("InfoCard", p);
            var ic = UIKit.Box(arInfoCard, UIKit.Card, 36, "Bg"); UIKit.Stretch(ic.rectTransform);
            UIKit.AddShadow(ic, 36, 0.25f, -8, 10);
            arInfo = UIKit.Text(arInfoCard, "", 40, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Normal);
            UIKit.Stretch(arInfo.rectTransform, 48);
            if (!Land) UIKit.Frac(arInfoCard, 0.10f, 0.34f, 0.90f, 0.64f);
            else UIKit.Frac(arInfoCard, 0.28f, 0.30f, 0.72f, 0.74f);
            arInfoCard.gameObject.SetActive(false);
            return p;
        }

        void ARStateChanged(bool ok)
        {
            if (ok)
            {
                if (arRaw != null) arRaw.enabled = false;
                if (arBg != null) arBg.color = new Color(0, 0, 0, 0); // transparente: se ve la cámara
                if (mainCam != null) mainCam.enabled = false;
                if (arHintText != null) arHintText.text = "Apunta la cámara al QR del tema";
            }
            else
            {
                if (arRaw != null) arRaw.enabled = modelViewer != null;
                if (arBg != null) arBg.color = UIKit.Hex("14212E"); // fondo oscuro para el visor 3D
                if (mainCam != null) mainCam.enabled = true;
                if (arHintText != null) arHintText.text = "Vista 3D interactiva · toca Girar para explorar";
            }
        }

        void PlayNarration()
        {
            if (audioSrc == null || narracion == null || currentTopic >= narracion.Length || narracion[currentTopic] == null)
            { ShowARToast("Sin audio disponible"); return; }
            if (audioSrc.isPlaying) { audioSrc.Stop(); ShowARToast("Audio detenido"); }
            else { audioSrc.clip = narracion[currentTopic]; audioSrc.Play(); ShowARToast("Reproduciendo narración…"); }
        }

        void ShowARToast(string msg)
        {
            if (arToast == null) return;
            arToast.text = msg;
            CancelInvoke(nameof(ClearARToast));
            Invoke(nameof(ClearARToast), 1.6f);
        }
        void ClearARToast() { if (arToast != null) arToast.text = ""; }

        void Toast(string msg)
        {
            if (toast == null) return;
            toast.text = msg;
            CancelInvoke(nameof(ClearToast));
            Invoke(nameof(ClearToast), 2.2f);
        }
        void ClearToast() { if (toast != null) toast.text = ""; }

        // ============================================================
        IEnumerator AutoCapture()
        {
            string dir = Environment.GetEnvironmentVariable("ARCAP_DIR");
            if (string.IsNullOrEmpty(dir)) dir = Application.persistentDataPath;
            string suf = Land ? "_land" : "_port";
            var names = new[] { "1_inicio", "2_bienvenida", "3_perfil", "4_menu", "5a_temas", "5a_detalle", "5a_ar" };
            currentTopic = 1; // El páncreas, para el detalle/AR
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < panels.Length; i++)
            {
                ShowOnly(panels[i], false);
                yield return new WaitForSeconds(0.8f); // dejar asentar animaciones de entrada
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, "cap_" + names[i] + suf + ".png"));
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.4f);
            Application.Quit();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    /// <summary>
    /// Todas las pantallas de la app, construidas por código con anclaje por fracciones
    /// (responsivo portrait/landscape). 1) INICIO 2) BIENVENIDA (carrusel) 3) PERFIL 4) MENÚ
    /// 5A/5B/5C) Temas → Detalle → Experiencia AR de cada libro. 5D) Escaneo genérico (cualquier página).
    ///
    /// Repartida en varios archivos parciales por responsabilidad (ver también
    /// AppBootstrap.BookContent/.UICore/.Screens.*/.AR/.Juegos/.Refs.cs) — este archivo solo
    /// tiene los campos y el ciclo de vida (Start/Update/BuildAll).
    /// </summary>
    public partial class AppBootstrap : MonoBehaviour
    {
        [Header("Sprites (asignados por ProjectSetup)")]
        public Sprite spLogo;
        public Sprite spWelcome;
        public Sprite spAvatar1, spAvatar2, spAvatar3;
        public Sprite spIconFisio, spIconNutri, spIconClinico, spIconScan, spIconJuegos, spIconProgreso;
        public Sprite spStar;
        public Sprite spPancreas;
        public GameObject pancreasModel, platoModel, glucoModel;
        public TMP_FontAsset font;

        [Header("Iconos UI")]
        public Sprite icInfo, icAudio, icRotate, icClose, icCamera, icChart, icQuestion, icDrop, icGear, icHome, icBook, icPancreas, icProfile;
        public Sprite icPlate, icBread, icApple, icClock, icSyringe, icAlert, icCalendar;
        [Header("Audio narración (12: 4 por libro)")]
        public AudioClip[] narracion;
        AudioSource audioSrc;
        [Header("Música de fondo")]
        public AudioClip musicaFondo;
        AudioSource bgAudioSrc;
        [Header("Marcadores AR (QR) — 4 por libro, Fisiológico/Nutricional/Clínico")]
        public Texture2D[] markersFisio, markersNutri, markersClinico;

        Canvas canvas;
        RectTransform root;
        RectTransform globalBg;
        Image flashOverlay;
        RectTransform pInicio, pBienvenida, pPerfil, pMenu, pScanAR, pProgreso, pConfig, pAyuda, pJuegos, pInsignias, pColeccion;
        List<string> pendingStartMsgs; // avisos de bonus (p.ej. racha de 7 días) detectados antes de tener UI armada
        RectTransform photoViewer;
        RawImage photoViewerImg;
        RectTransform rewardBox;
        TMP_Text rewardText;
        Coroutine rewardCo;
        RectTransform[] bookTemas = new RectTransform[3];
        RectTransform[] bookDetalle = new RectTransform[3];
        RectTransform[] bookAR = new RectTransform[3];
        RectTransform[] bookQuiz = new RectTransform[3];
        RectTransform[] panels;
        HashSet<RectTransform> arPanels = new HashSet<RectTransform>();
        HashSet<RectTransform> detallePanels = new HashSet<RectTransform>();
        int shown;
        bool builtLandscape;
        bool captureStarted;
        int quizBook, quizQ, quizScore;
        bool quizAnswered;
        // Estado del juego de Matching (tap-to-pair) para la pregunta actual.
        int[] matchRightOrder;
        bool[] matchDoneLeft, matchDoneRight;
        int matchSelected = -1, matchCount;
        bool matchLocked;
        // Estado del juego de MultiSelect para la pregunta actual.
        bool[] msSelected;
        bool msAnswered;

        TMP_Text starLabel, toast, holaText, nivelText;
        Image menuAvatar, nivelFill;

        // --- Libros (5A/5B/5C) ---
        BookDef[] books;
        ModelViewer[] modelViewers = new ModelViewer[3];

        // "Actuales": se reasignan al entrar a cada pantalla de Detalle/AR (índice 0-2 = libro, 3 = escaneo genérico)
        ModelViewer modelViewer;
        ARController arController;
        RawImage arRaw;
        Image arBg;
        TMP_Text arHintText;
        Camera mainCam;
        int currentBook, currentTopic;
        bool isGenericScan;
        TMP_Text detTitle, detDesc, arTitle, arToast;
        Image detImg, detBand;
        // Texto de info del tema actual: ya no es una tarjeta 2D (ver ARController.BuildInfoPanel,
        // que la ancla en el espacio 3D junto al modelo). Se guarda solo como respaldo para el
        // toast del botón "i" cuando todavía no se escaneó ningún marcador real.
        string currentArInfoText;

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
            // Cama musical de fondo, separada de audioSrc (narración) para que pausar/reanudar una
            // no afecte a la otra. Un solo AudioSource para toda la app (no por pantalla): arranca
            // una vez y sigue sonando en loop mientras se navega, igual que en cualquier app similar.
            bgAudioSrc = gameObject.AddComponent<AudioSource>();
            bgAudioSrc.playOnAwake = false;
            bgAudioSrc.loop = true;
            bgAudioSrc.volume = 0.35f;
            bgAudioSrc.spatialBlend = 0f;
            if (musicaFondo != null)
            {
                bgAudioSrc.clip = musicaFondo;
                if (!AppState.Muted) bgAudioSrc.Play();
            }
            mainCam = Camera.main;
            // Se guarda para recién mostrar el toast cuando exista UI (ver InitialBuild): acá el
            // Canvas todavía no se construyó, así que ShowReward no tendría dónde pintarlo.
            pendingStartMsgs = AppState.TouchDailyStreak();
            BuildBooks();

            for (int b = 0; b < 3; b++)
                if (books[b].Model != null) modelViewers[b] = ModelViewer.Create(books[b].Model, books[b].ModelTint, b);

            // Un único rig de AR (ARSession/cámara) para toda la app: crear uno por libro causaba que
            // ARCore devolviera "camera was passed NULL" al tener varias sesiones nativas compitiendo
            // (AR Foundation solo soporta un ARSession activo por proceso). Se registran los marcadores
            // de los 3 libros de una vez; SetScope() restringe cuáles puede detectar cada pantalla.
            var scanEntries = new List<MarkerEntry>();
            for (int b = 0; b < 3; b++)
            {
                if (books[b].Markers == null) continue;
                for (int i = 0; i < books[b].Markers.Length; i++)
                {
                    var m = books[b].Markers[i];
                    if (m == null) continue;
                    string title = i < books[b].TopicTitle.Length ? books[b].TopicTitle[i] : books[b].Title;
                    scanEntries.Add(new MarkerEntry { Marker = m, Model = books[b].Model, Tint = books[b].ModelTint, Title = title, Book = b, Topic = i });
                }
            }
            arController = ARController.CreateMulti(scanEntries.ToArray());
            arController.OnState = ARStateChanged;
            arController.OnMarkerTitle = t => { if (arTitle != null) arTitle.text = t; };
            arController.OnMarkerSeen = (b, t) =>
            {
                var msgs = AppState.MarkMarkerScanned(b, t);
                if (msgs.Count > 0) ShowReward(string.Join("\n", msgs));
            };
            // El texto de info ahora vive anclado en el espacio 3D junto al modelo (no como
            // tarjeta 2D encima de la pantalla) — ARController lo arma solo, pidiendo acá el
            // texto real (depende de la edad elegida, por eso se resuelve en el momento, no se
            // precalcula una vez).
            arController.GetInfoText = (b, t) => books[b].Desc(t, AppState.Age);

            StartCoroutine(InitialBuild());
        }

        // En un arranque en frío en Android, Screen.width/height a veces reportan un valor
        // transitorio (de la orientación "por defecto") durante los primeros frames, antes de
        // asentarse en el tamaño real de la ventana. Construir de inmediato con ese valor deja
        // toda la UI armada con las fracciones de la orientación equivocada — se ve "desplazada"
        // hasta el próximo cambio real de orientación (que dispara un BuildAll correcto en
        // Update). Se espera a que el tamaño quede igual en 2 frames seguidos antes de construir.
        IEnumerator InitialBuild()
        {
            int w = Screen.width, h = Screen.height;
            for (int i = 0; i < 10; i++)
            {
                yield return null;
                if (Screen.width == w && Screen.height == h) break;
                w = Screen.width; h = Screen.height;
            }
            BuildAll();
            if (pendingStartMsgs != null && pendingStartMsgs.Count > 0)
                ShowReward(string.Join("\n", pendingStartMsgs));
            if (Environment.GetEnvironmentVariable("ARCAP") == "1" && !captureStarted)
            {
                captureStarted = true;
                StartCoroutine(AutoCapture());
            }
        }

        void Update()
        {
            // No reconstruir la UI mientras se está en una Experiencia AR: un giro/tilt de la mano
            // dispara un cambio Land/Portrait que recreaba el Canvas (incluido el fondo global,
            // que vuelve a quedar visible/opaco) tapando el feed real de la cámara a mitad de sesión.
            bool onAr = panels != null && shown >= 0 && shown < panels.Length && arPanels.Contains(panels[shown]);
            if (canvas != null && Land != builtLandscape && !onAr) BuildAll();
        }

        void BuildAll()
        {
            if (canvas != null) Destroy(canvas.gameObject);
            builtLandscape = Land;
            arPanels.Clear(); detallePanels.Clear();
            BuildCanvas();
            pInicio = BuildInicio();
            pBienvenida = BuildBienvenida();
            pPerfil = BuildPerfil();
            pMenu = BuildMenu();
            for (int b = 0; b < 3; b++)
            {
                int bi = b;
                bookTemas[b] = BuildLibroTemas(bi);
                bookDetalle[b] = BuildLibroDetalle(bi);
                bookAR[b] = BuildLibroAR(bi);
                detallePanels.Add(bookDetalle[b]);
                arPanels.Add(bookAR[b]);
            }
            pScanAR = BuildScanAR();
            arPanels.Add(pScanAR);
            pProgreso = BuildProgreso();
            pInsignias = BuildInsignias();
            pColeccion = BuildColeccion();
            pConfig = BuildConfig();
            pAyuda = BuildAyuda();
            pJuegos = BuildJuegos();
            for (int b = 0; b < 3; b++) bookQuiz[b] = BuildQuiz(b);
            panels = new RectTransform[] { pInicio, pBienvenida, pPerfil, pMenu,
                bookTemas[0], bookDetalle[0], bookAR[0],
                bookTemas[1], bookDetalle[1], bookAR[1],
                bookTemas[2], bookDetalle[2], bookAR[2],
                pScanAR, pProgreso, pConfig, pAyuda,
                pJuegos, bookQuiz[0], bookQuiz[1], bookQuiz[2], pInsignias, pColeccion };
            ShowOnly(panels[Mathf.Clamp(shown, 0, panels.Length - 1)], false);
        }

        // ---- Preview en el editor (ver las vistas sin darle Play) ----
        public void EditorPreview(int index)
        {
            EditorClearPreview();
            if (font != null) UIKit.Font = font;
            currentBook = 0; currentTopic = 1;
            shown = Mathf.Clamp(index, 0, 20);
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
    }
}

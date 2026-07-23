using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;
using TMPro;

namespace ARDiabetes
{
    /// <summary>Un marcador (QR) con el modelo/tinte/título que debe mostrar al detectarse.</summary>
    public class MarkerEntry
    {
        public Texture2D Marker;
        public GameObject Model;
        public Color Tint;
        public string Title;
        public int Book = -1, Topic = -1; // para avisar qué tema fue escaneado (progreso/estrellas)
    }

    /// <summary>
    /// Rig de AR (ARCore) con image tracking: monta ARSession + XROrigin + cámara AR + ARTrackedImageManager,
    /// crea una librería de imágenes en runtime desde los QR y coloca el modelo correspondiente al marcador
    /// detectado (soporta varios marcadores/modelos a la vez, para el escaneo genérico de cualquier libro).
    /// Si el dispositivo no soporta AR (p.ej. desktop), avisa por OnState(false) para caer al visor 3D.
    /// </summary>
    public class ARController : MonoBehaviour
    {
        public Action<bool> OnState;      // true = AR activo, false = no soportado
        public Action<string> OnMarkerTitle; // título del marcador actualmente mostrado (para 5D genérico)
        public Action<int, int> OnMarkerSeen; // (libro, tema) la primera vez que se detecta ese marcador
        public Func<int, int, string> GetInfoText; // (libro, tema) -> descripción a mostrar en el panel de info anclado

        ARSession session;
        XROrigin origin;
        Camera arCam;
        ARTrackedImageManager imgMgr;
        Dictionary<string, MarkerEntry> byName = new Dictionary<string, MarkerEntry>();
        Texture2D[] allMarkers;
        GameObject spawned;
        string spawnedMarkerName;
        bool imagesAdded;
        float userYaw;
        public bool autoSpin = true; // gira solo tipo "turntable", igual que el visor de respaldo (Spinner.cs)
        public float autoSpinSpeed = 35f; // grados/seg (antes 25 - poco perceptible con material mate)
        HashSet<string> scope; // null = sin filtro (ve cualquier marcador); usado para restringir el escaneo al libro actual

        // Panel de info anclado en el espacio 3D junto al modelo (no una tarjeta 2D encima de
        // la pantalla): vive y muere con "spawned", del mismo marcador.
        GameObject infoPanel;
        bool infoVisible; // intención del usuario (toggle); se combina con el estado de tracking

        /// <summary>Hay un modelo anclado mostrándose ahora mismo (para que el botón "i" 2D sepa
        /// si puede abrir el panel en AR o si debe caer a un toast, cuando aún no se escaneó nada).</summary>
        public bool HasSpawnedModel => spawned != null;

        /// <summary>Restringe qué marcadores puede detectar (null = todos, usado en el escaneo genérico 5D).
        /// Limpia el modelo mostrado, para no arrastrar el de un libro/tema anterior al cambiar de pantalla.</summary>
        public void SetScope(IEnumerable<string> names)
        {
            scope = names == null ? null : new HashSet<string>(names);
            if (spawned != null) Destroy(spawned);
            if (infoPanel != null) Destroy(infoPanel);
            infoVisible = false;
            spawned = null;
            spawnedMarkerName = null;
        }

        /// <summary>Un solo modelo/tinte para todos los marcadores dados (uso normal: un libro).</summary>
        public static ARController Create(GameObject model, Color tint, Texture2D[] markers, string title = null)
        {
            var entries = new List<MarkerEntry>();
            if (markers != null)
                foreach (var m in markers)
                    if (m != null) entries.Add(new MarkerEntry { Marker = m, Model = model, Tint = tint, Title = title });
            return CreateMulti(entries.ToArray());
        }

        /// <summary>Varios marcadores, cada uno con su propio modelo/tinte/título (escaneo genérico 5D).</summary>
        public static ARController CreateMulti(MarkerEntry[] entries)
        {
            var go = new GameObject("ARRig");
            go.SetActive(false); // INACTIVO antes de construir: difiere los Awake hasta tener todo cableado
            var c = go.AddComponent<ARController>();
            c.allMarkers = new Texture2D[entries.Length];
            for (int i = 0; i < entries.Length; i++)
            {
                c.allMarkers[i] = entries[i].Marker;
                if (entries[i].Marker != null) c.byName[entries[i].Marker.name] = entries[i];
            }
            c.Build();
            return c;
        }

        void Build()
        {
            var sess = new GameObject("ARSession");
            sess.transform.SetParent(transform, false);
            session = sess.AddComponent<ARSession>();
            sess.AddComponent<ARInputManager>();

            var originGo = new GameObject("XROrigin");
            originGo.transform.SetParent(transform, false);
            origin = originGo.AddComponent<XROrigin>();

            var offset = new GameObject("CameraOffset");
            offset.transform.SetParent(originGo.transform, false);

            var camGo = new GameObject("ARCamera");
            camGo.transform.SetParent(offset.transform, false);
            arCam = camGo.AddComponent<Camera>();
            arCam.clearFlags = CameraClearFlags.SolidColor;
            arCam.backgroundColor = Color.black;
            arCam.nearClipPlane = 0.05f;
            arCam.farClipPlane = 20f;
            var camMgr = camGo.AddComponent<ARCameraManager>();
            camMgr.autoFocusRequested = true; // asegura autoenfoque (marcador impreso queda cerca, ~20-30cm)
            camGo.AddComponent<ARCameraBackground>();

            var tpd = camGo.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            tpd.positionInput = new UnityEngine.InputSystem.InputActionProperty(
                new UnityEngine.InputSystem.InputAction("pos", UnityEngine.InputSystem.InputActionType.Value,
                    "<HandheldARInputDevice>/devicePosition", expectedControlType: "Vector3"));
            tpd.rotationInput = new UnityEngine.InputSystem.InputActionProperty(
                new UnityEngine.InputSystem.InputAction("rot", UnityEngine.InputSystem.InputActionType.Value,
                    "<HandheldARInputDevice>/deviceRotation", expectedControlType: "Quaternion"));
            tpd.positionInput.action.Enable();
            tpd.rotationInput.action.Enable();

            origin.Camera = arCam;
            origin.CameraFloorOffsetObject = offset;

            // XROrigin.Awake() puede auto-provisionar una cámara de respaldo (clearFlags=Skybox)
            // ANTES de que le asignemos la nuestra arriba. Esa cámara "fantasma" queda activa con
            // el mismo depth que arCam y su Skybox se mezcla/tapa el feed real de la cámara AR.
            // La eliminamos explícitamente por si acaso.
            foreach (var stray in transform.GetComponentsInChildren<Camera>(true))
            {
                if (stray != arCam) { Debug.Log("[AR] Destruyendo cámara fantasma: " + stray.name); Destroy(stray.gameObject); }
            }

            // El modelo anclado no tenía NINGUNA luz direccional propia (solo el ambiente/skybox
            // por defecto, que es prácticamente plano) — con material mate de un solo color, girar
            // sin sombras/brillos que se muevan es imperceptible a simple vista (reportado como
            // "los modelos no giran" en la predefensa 2026-07-23, aunque la rotación en código sí
            // se aplica cada frame). Se cuelgan de la cámara AR (no del mundo) para que la luz
            // siempre incida desde un ángulo consistente sin importar cómo esté orientado el marcador.
            var keyLightGo = new GameObject("ModelKeyLight");
            keyLightGo.transform.SetParent(camGo.transform, false);
            var keyLight = keyLightGo.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.intensity = 1.3f;
            keyLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);

            var fillLightGo = new GameObject("ModelFillLight");
            fillLightGo.transform.SetParent(camGo.transform, false);
            var fillLight = fillLightGo.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.4f;
            fillLight.transform.rotation = Quaternion.Euler(-25f, 150f, 0f);

            imgMgr = originGo.AddComponent<ARTrackedImageManager>();
            imgMgr.enabled = false;
            imgMgr.trackablesChanged.AddListener(OnChanged);
        }

        public void Activate()
        {
            // NOTA (probado 2026-07-08): intentar mantener ARSession apagado hasta confirmar
            // compatibilidad evita el diálogo nativo de Google en tablets no-ARCore, pero CUELGA
            // ARSession.CheckAvailability() para siempre (nada sondea el resultado nativo async sin
            // el componente activo) — rompería el AR real en dispositivos SÍ compatibles (confirmado
            // funcionando así en el Infinix NOTE 40 Pro). Se prioriza que el AR real funcione:
            // se activa todo junto; en tablets incompatibles puede aparecer una vez el diálogo nativo
            // de ARCore ("requiere Google Play Services for AR") — es comportamiento estándar del SDK,
            // no un bug de la app, y tras cerrarlo el timeout de abajo cae al visor 3D igual.
            gameObject.SetActive(true);
            if (arCam != null) arCam.enabled = true;
            // El auto-giro es un solo ARController compartido por toda la app (no se recrea al
            // cambiar de pantalla) — si se apagó con el botón "girar" en una vista anterior, sin
            // esto quedaba apagado también en la siguiente, silenciosamente. Cada Experiencia AR
            // nueva arranca girando por defecto, como un maniquí en vitrina.
            autoSpin = true;
            StartCoroutine(Run());
        }

        public void Deactivate()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }

        void Fallback()
        {
            if (arCam != null) arCam.enabled = false;
            OnState?.Invoke(false);
        }

        IEnumerator Run()
        {
            // 1) Disponibilidad de ARCore
            if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
                yield return ARSession.CheckAvailability();
            Debug.Log("[AR] tras CheckAvailability state=" + ARSession.state);
            // Sin ARCore (no soportado o falta instalarlo) -> caer al visor 3D.
            if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.NeedsInstall)
            { Debug.Log("[AR] fallback (no soportado/needs install)"); Fallback(); yield break; }

            // 2) Esperar a que la sesión inicialice/trackee
            float t = 0;
            while (t < 12f)
            {
                var s = ARSession.state;
                if (s == ARSessionState.SessionInitializing || s == ARSessionState.SessionTracking) break;
                if (s == ARSessionState.Unsupported) { Fallback(); yield break; }
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (ARSession.state != ARSessionState.SessionInitializing && ARSession.state != ARSessionState.SessionTracking)
            { Debug.Log("[AR] fallback (timeout, state=" + ARSession.state + ")"); Fallback(); yield break; }
            Debug.Log("[AR] AR ACTIVO state=" + ARSession.state);
            OnState?.Invoke(true);

            if (!imagesAdded && imgMgr != null && allMarkers != null)
            {
                var lib = imgMgr.CreateRuntimeLibrary();
                if (lib is MutableRuntimeReferenceImageLibrary mlib)
                {
                    imgMgr.referenceLibrary = mlib;
                    foreach (var tex in allMarkers)
                    {
                        if (tex == null) continue;
                        try { mlib.ScheduleAddImageWithValidationJob(tex, tex.name, 0.12f); }
                        catch (Exception e) { Debug.LogWarning("[AR] addImage " + tex.name + ": " + e.Message); }
                    }
                    imagesAdded = true;
                }
                imgMgr.enabled = true;
            }
        }

        void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
        {
            foreach (var img in args.added) Place(img);
            foreach (var img in args.updated) Place(img);
        }

        void Place(ARTrackedImage img)
        {
            string name = img.referenceImage.name;
            if (scope != null && !scope.Contains(name)) return;
            if (!byName.TryGetValue(name, out var entry) || entry.Model == null) return;

            if (spawnedMarkerName != name)
            {
                if (spawned != null) Destroy(spawned);
                if (infoPanel != null) Destroy(infoPanel);
                infoVisible = false;
                spawned = Instantiate(entry.Model);
                spawned.transform.localScale = Vector3.one; // el prefab puede traer escala no uniforme (se veía "alargado")
                userYaw = 0f;
                ModelViewer.StripEmbeddedCamerasAndLights(spawned); // el FBX trae cámara/luz incrustada
                var sh = Shader.Find("Standard");
                if (sh != null)
                {
                    var mat = new Material(sh) { color = entry.Tint };
                    mat.SetFloat("_Glossiness", 0.35f); // un brillo especular que se mueve al girar hace mucho más notorio el giro
                    foreach (var r in spawned.GetComponentsInChildren<Renderer>())
                    {
                        var mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                        r.sharedMaterials = mats;
                    }
                }
                // Auto-escala a ~9 cm sobre el marcador
                var rends = spawned.GetComponentsInChildren<Renderer>();
                if (rends.Length > 0)
                {
                    Bounds b = rends[0].bounds;
                    foreach (var r in rends) b.Encapsulate(r.bounds);
                    float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
                    if (size > 0.0001f) spawned.transform.localScale *= 0.09f / size;
                }
                spawnedMarkerName = name;
                if (entry.Title != null) OnMarkerTitle?.Invoke(entry.Title);
                if (entry.Book >= 0 && entry.Topic >= 0) OnMarkerSeen?.Invoke(entry.Book, entry.Topic);
                BuildInfoPanel(entry);
            }
            if (spawned == null) return;
            spawned.transform.SetParent(img.transform, false);
            spawned.transform.localPosition = Vector3.zero;
            // De pie sobre la página (perpendicular al marcador), no acostado: antes -90° en X
            // tumbaba el modelo sobre el marcador. La rotación en sí (userYaw) la aplica Update()
            // en cada frame, no acá (ver comentario ahí abajo).
            bool trackingOk = img.trackingState == TrackingState.Tracking || img.trackingState == TrackingState.Limited;
            spawned.SetActive(trackingOk);
            if (infoPanel != null)
            {
                infoPanel.transform.SetParent(img.transform, false);
                infoPanel.SetActive(trackingOk && infoVisible);
            }
        }

        /// <summary>Arma el panel de info anclado en 3D junto al modelo (no una tarjeta 2D de
        /// pantalla): un Canvas World Space con el texto real del tema, con su propio botón —
        /// tocarlo lo cierra, y el botón "i" de la UI 2D lo abre/cierra desde afuera. Empieza
        /// oculto (igual que la tarjeta 2D anterior).</summary>
        void BuildInfoPanel(MarkerEntry entry)
        {
            var go = new GameObject("InfoPanel", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = arCam;
            go.AddComponent<GraphicRaycaster>();
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(520, 300);
            // Escala física del panel: ~8 cm de ancho (antes 15 cm tapaba el modelo — probado en
            // dispositivo real 2026-07-17), independiente del tamaño del modelo (el panel se
            // parentea directo al marcador, no al modelo, para no heredar su escala automática —
            // cada modelo puede traer un factor de escala interno distinto). Más separado del
            // modelo (16 cm de offset en vez de 13) para que no se solapen.
            rt.localScale = Vector3.one * 0.00015f;
            rt.localPosition = new Vector3(0.16f, 0.04f, 0f);

            var bg = UIKit.Box(go.transform, UIKit.Card, 36, "Bg");
            UIKit.Stretch(bg.rectTransform);
            UIKit.AddShadow(bg, 36, 0.3f, -10, 12);
            var btn = bg.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(ToggleInfo);

            string text = (entry.Book >= 0 && entry.Topic >= 0 && GetInfoText != null)
                ? GetInfoText(entry.Book, entry.Topic) : entry.Title;
            var txt = UIKit.Text(go.transform, text ?? "", 34, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Normal);
            UIKit.Stretch(txt.rectTransform, 40);

            infoPanel = go;
            infoPanel.SetActive(false);
        }

        /// <summary>Abre/cierra el panel de info anclado en AR. Lo llama tanto el botón "i" de la
        /// UI 2D como el propio panel al tocarlo (para cerrarse).</summary>
        public void ToggleInfo()
        {
            infoVisible = !infoVisible;
            if (infoPanel != null) infoPanel.SetActive(infoVisible && spawned != null && spawned.activeSelf);
        }

        /// <summary>Rota el modelo anclado (arrastre táctil), sin perder su anclaje sobre el marcador.</summary>
        public void AddYaw(float degrees) { userYaw += degrees; }

        // Auto-giro continuo (además del arrastre manual, que se suma sobre el mismo userYaw).
        // BUG real encontrado en dispositivo (2026-07-17): con un marcador físico QUIETO, ARCore
        // no siempre reporta el tracked image en trackablesChanged.updated cada frame (la pose no
        // cambia lo suficiente) — Place() casi no se llamaba, así que el modelo nunca giraba visualmente
        // aunque userYaw sí se incrementaba puertas adentro. Ahora la rotación se aplica ACÁ, todos los
        // frames, sin depender de que ARFoundation dispare el evento de tracking.
        void Update()
        {
            if (autoSpin) userYaw += autoSpinSpeed * Time.deltaTime;
            if (spawned != null) spawned.transform.localRotation = Quaternion.Euler(0f, userYaw, 0f);

            // El panel de info siempre mira a la cámara ("billboard"): así se lee bien sin
            // importar la orientación real del marcador/celular, en vez de asumir a mano qué eje
            // del marcador es "arriba" (arriesgado de adivinar sin poder probarlo en el momento).
            // Probado en dispositivo real (2026-07-17): con el forward apuntando HACIA la cámara
            // el texto salía espejado (se ve, pero al revés) — el forward de un Canvas debe
            // apuntar en el mismo sentido que mira la cámara (alejándose de ella), no hacia ella.
            if (infoPanel != null && infoPanel.activeInHierarchy && arCam != null)
            {
                Vector3 awayFromCam = infoPanel.transform.position - arCam.transform.position;
                if (awayFromCam.sqrMagnitude > 0.0001f) infoPanel.transform.rotation = Quaternion.LookRotation(awayFromCam);
            }
        }

        public void ToggleSpin() { autoSpin = !autoSpin; }
    }
}

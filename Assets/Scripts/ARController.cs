using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

namespace ARDiabetes
{
    /// <summary>
    /// Rig de AR (ARCore) con image tracking: monta ARSession + XROrigin + cámara AR + ARTrackedImageManager,
    /// crea una librería de imágenes en runtime desde los QR y coloca el modelo sobre el marcador detectado.
    /// Si el dispositivo no soporta AR (p.ej. desktop), avisa por OnState(false) para caer al visor 3D.
    /// </summary>
    public class ARController : MonoBehaviour
    {
        public Action<bool> OnState;   // true = AR activo, false = no soportado

        ARSession session;
        XROrigin origin;
        Camera arCam;
        ARTrackedImageManager imgMgr;
        GameObject modelPrefab;
        Color tint;
        Texture2D[] markers;
        GameObject spawned;
        bool imagesAdded;
        bool notified;

        public static ARController Create(GameObject modelPrefab, Color tint, Texture2D[] markers)
        {
            var go = new GameObject("ARRig");
            go.SetActive(false); // INACTIVO antes de construir: difiere los Awake hasta tener todo cableado
            var c = go.AddComponent<ARController>();
            c.modelPrefab = modelPrefab; c.tint = tint; c.markers = markers;
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
            camGo.AddComponent<ARCameraManager>();
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

            if (!imagesAdded && imgMgr != null && markers != null)
            {
                var lib = imgMgr.CreateRuntimeLibrary();
                if (lib is MutableRuntimeReferenceImageLibrary mlib)
                {
                    imgMgr.referenceLibrary = mlib;
                    foreach (var tex in markers)
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
            if (spawned == null && modelPrefab != null)
            {
                spawned = Instantiate(modelPrefab);
                var sh = Shader.Find("Standard");
                if (sh != null)
                {
                    var mat = new Material(sh) { color = tint };
                    foreach (var r in spawned.GetComponentsInChildren<Renderer>())
                    {
                        var mats = new Material[r.sharedMaterials.Length];
                        for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                        r.sharedMaterials = mats;
                    }
                }
                // Auto-escala a ~8 cm sobre el marcador
                var rends = spawned.GetComponentsInChildren<Renderer>();
                if (rends.Length > 0)
                {
                    Bounds b = rends[0].bounds;
                    foreach (var r in rends) b.Encapsulate(r.bounds);
                    float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
                    if (size > 0.0001f) spawned.transform.localScale *= 0.09f / size;
                }
            }
            if (spawned == null) return;
            spawned.transform.SetParent(img.transform, false);
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            spawned.SetActive(img.trackingState == TrackingState.Tracking || img.trackingState == TrackingState.Limited);
        }
    }
}

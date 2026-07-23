using UnityEngine;

namespace ARDiabetes
{
    /// <summary>
    /// Monta un visor 3D fuera de pantalla: instancia el modelo, lo auto-encuadra y lo renderiza
    /// a una RenderTexture con su propia cámara y luz (aislado por layer). El resultado se muestra
    /// en un RawImage. Sustituye al fondo de cámara real de AR hasta integrar ARCore.
    /// </summary>
    public class ModelViewer : MonoBehaviour
    {
        public RenderTexture Texture { get; private set; }
        Spinner spinner;
        Camera cam;
        Transform holder;
        int Layer; // una layer DISTINTA por instancia (ver Create) — antes era una const compartida

        /// <summary>slot 0/1/2 (uno por libro) -> layer 2/3/4, para que cada visor renderice SOLO su
        /// propio modelo. Los 3 rigs comparten la misma posición en el mundo (todos lejos de la
        /// escena real) — sin layers distintas, la cámara de cada visor terminaba renderizando los
        /// 3 modelos superpuestos (bug real encontrado 2026-07-23: el páncreas se veía "multicolor"
        /// porque en realidad eran el páncreas + el plato + el glucómetro apilados en el mismo punto).</summary>
        public static ModelViewer Create(GameObject modelPrefab, Color tint, int slot = 0)
        {
            var rig = new GameObject("Model3DRig");
            rig.transform.position = new Vector3(0, -500, 0); // lejos, además solo se ve por RT
            var mv = rig.AddComponent<ModelViewer>();
            mv.Layer = 2 + slot;
            mv.Build(modelPrefab, tint);
            return mv;
        }

        void Build(GameObject prefab, Color tint)
        {
            var holderGo = new GameObject("Holder");
            holderGo.transform.SetParent(transform, false);
            holder = holderGo.transform;

            GameObject model = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.transform.SetParent(holder, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = Vector3.one; // el prefab puede traer escala no uniforme; normalizar antes de medir bounds
            SetLayer(model, Layer);
            StripEmbeddedCamerasAndLights(model);

            // Material visible en Built-in (los materiales URP del prototipo se verían rosados).
            var sh = Shader.Find("Standard");
            if (sh != null)
            {
                var mat = new Material(sh) { color = tint };
                mat.SetFloat("_Glossiness", 0.25f);
                foreach (var r in model.GetComponentsInChildren<Renderer>())
                {
                    var mats = new Material[r.sharedMaterials.Length];
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                }
            }

            // Auto-encuadre: centrar y escalar a ~2 unidades (uniforme, ya con localScale=1 arriba).
            var rends = model.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                model.transform.position += holder.position - b.center;
                float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
                if (size > 0.0001f) holder.localScale = Vector3.one * (2f / size);
            }

            spinner = holderGo.AddComponent<Spinner>();

            var camGo = new GameObject("PreviewCam");
            camGo.transform.SetParent(transform, false);
            cam = camGo.AddComponent<Camera>();
            cam.cullingMask = 1 << Layer;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.fieldOfView = 45f;
            cam.nearClipPlane = 0.05f;
            cam.transform.position = transform.position + new Vector3(0, 0.4f, -3.2f);
            cam.transform.LookAt(transform.position + new Vector3(0, 0.1f, 0));

            Texture = new RenderTexture(800, 900, 16) { name = "ModelRT" };
            cam.targetTexture = Texture;

            var lightGo = new GameObject("PreviewLight");
            lightGo.transform.SetParent(transform, false);
            var l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.cullingMask = 1 << Layer;
            l.intensity = 1.1f;
            l.transform.rotation = Quaternion.Euler(40f, -35f, 0f);

            var fill = new GameObject("FillLight").AddComponent<Light>();
            fill.transform.SetParent(transform, false);
            fill.type = LightType.Directional;
            fill.cullingMask = 1 << Layer;
            fill.intensity = 0.5f;
            fill.transform.rotation = Quaternion.Euler(-20f, 140f, 0f);

            // Que la cámara principal NO dibuje el modelo.
            if (Camera.main != null) Camera.main.cullingMask &= ~(1 << Layer);

            // Apagada por defecto: sin esto, cada libro sumaba una cámara renderizando su
            // RenderTexture en TODO momento (incluso en pantallas donde nunca se ve), lo que
            // se notaba como lentitud general de toda la app al pasar de 1 a 3 libros.
            cam.enabled = false;
        }

        public void ToggleSpin() { if (spinner != null) spinner.spinning = !spinner.spinning; }

        /// <summary>Enciende/apaga el render de este visor. Solo debe estar activo el del libro
        /// que se está mostrando en ese momento.</summary>
        public void SetActive(bool on) { if (cam != null) cam.enabled = on; }

        /// <summary>Rota el modelo manualmente (arrastre táctil), independiente del auto-giro.</summary>
        public void AddYaw(float degrees) { if (holder != null) holder.Rotate(Vector3.up, degrees, Space.World); }

        static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayer(t.gameObject, layer);
        }

        // Modelos exportados de Sketchfab/Meshy suelen traer incrustada la cámara (y a veces luces)
        // de su escena de origen. Si se dejan activas, compiten con nuestras propias cámaras
        // (Skybox por defecto tapando el feed real de la cámara AR). Se eliminan al instanciar.
        public static void StripEmbeddedCamerasAndLights(GameObject model)
        {
            foreach (var cam in model.GetComponentsInChildren<Camera>(true)) UnityEngine.Object.Destroy(cam);
            foreach (var lt in model.GetComponentsInChildren<Light>(true)) UnityEngine.Object.Destroy(lt);
            foreach (var al in model.GetComponentsInChildren<AudioListener>(true)) UnityEngine.Object.Destroy(al);
        }
    }
}

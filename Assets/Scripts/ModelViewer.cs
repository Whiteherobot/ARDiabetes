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
        const int Layer = 2; // Ignore Raycast (no se usa para 3D en esta app)

        public static ModelViewer Create(GameObject modelPrefab, Color tint)
        {
            var rig = new GameObject("Model3DRig");
            rig.transform.position = new Vector3(0, -500, 0); // lejos, además solo se ve por RT
            var mv = rig.AddComponent<ModelViewer>();
            mv.Build(modelPrefab, tint);
            return mv;
        }

        void Build(GameObject prefab, Color tint)
        {
            var holder = new GameObject("Holder");
            holder.transform.SetParent(transform, false);

            GameObject model = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            model.transform.SetParent(holder.transform, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
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

            // Auto-encuadre: centrar y escalar a ~2 unidades.
            var rends = model.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                model.transform.position += holder.transform.position - b.center;
                float size = Mathf.Max(b.size.x, b.size.y, b.size.z);
                if (size > 0.0001f) holder.transform.localScale = Vector3.one * (2f / size);
            }

            spinner = holder.AddComponent<Spinner>();

            var camGo = new GameObject("PreviewCam");
            camGo.transform.SetParent(transform, false);
            var cam = camGo.AddComponent<Camera>();
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
        }

        public void ToggleSpin() { if (spinner != null) spinner.spinning = !spinner.spinning; }

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

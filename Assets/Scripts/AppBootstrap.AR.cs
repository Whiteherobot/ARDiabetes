using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Núcleo de la Experiencia AR (compartido por cada libro y por el escaneo genérico),
    // narración de audio y captura de foto.
    public partial class AppBootstrap
    {
        // Núcleo común de una pantalla de Experiencia AR (usada por cada libro y por el escaneo genérico).
        RectTransform BuildArScreenCore(string name, Func<ModelViewer> viewerGetter, UnityEngine.Events.UnityAction onClose)
        {
            var p = Panel(name);
            var bg = UIKit.Box(p, UIKit.Hex("14212E"), 4, "ARbg"); UIKit.Stretch(bg.rectTransform);

            // Arrastrar en cualquier parte de la pantalla (menos los botones, que van encima)
            // rota la figura anclada — antes no se podía interactuar con el modelo real en AR.
            var dragCatcher = UIKit.Box(p, new Color(0, 0, 0, 0), 0, "DragCatcher");
            dragCatcher.raycastTarget = true;
            UIKit.Stretch(dragCatcher.rectTransform);
            var dr = dragCatcher.gameObject.AddComponent<DragRotate>();
            dr.OnDragX = dx => { arController?.AddYaw(-dx * 0.3f); modelViewer?.AddYaw(-dx * 0.3f); };

            var raw = new GameObject("Model3D", typeof(RectTransform)).AddComponent<RawImage>();
            raw.transform.SetParent(p, false); raw.raycastTarget = false;
            var mv = viewerGetter();
            if (mv != null) { raw.texture = mv.Texture; raw.color = Color.white; }
            else raw.color = new Color(1, 1, 1, 0);
            // Portrait: el modelo ocupa la franja superior (0.42-0.83) para dejar libre una franja
            // propia abajo para infoCard, y que ambos convivan sin que la tarjeta tape el modelo.
            // Landscape: el modelo queda igual (columna central) porque infoCard se ubica en el
            // margen lateral libre, no debajo.
            if (!Land) UIKit.Frac(raw.rectTransform, 0.06f, 0.42f, 0.94f, 0.83f);
            else UIKit.Frac(raw.rectTransform, 0.26f, 0.14f, 0.74f, 0.92f);

            var titleTxt = UIKit.Text(p, "", 50, Color.white);
            UIKit.Frac(titleTxt, 0.10f, 0.905f, 0.90f, 0.97f);
            var pill = UIKit.Box(p, new Color(1, 1, 1, 0.12f), 30, "Hint");
            var hint = UIKit.Text(pill.transform, "Vista 3D · la cámara AR en vivo llega pronto", 30, UIKit.Hex("BFD3E6"), TextAlignmentOptions.Center, FontStyles.Normal);
            UIKit.Stretch(hint.rectTransform, 24);
            UIKit.Frac(pill.rectTransform, 0.14f, 0.855f, 0.86f, 0.90f);

            // Controles circulares
            // El texto de info ahora vive anclado en 3D junto al modelo (ARController.ToggleInfo),
            // no como tarjeta 2D encima de la pantalla. Si todavía no se escaneó ningún marcador
            // real (nada anclado en el mundo para mostrar), el botón cae a un toast con el mismo
            // texto en vez de no hacer nada.
            CircleBtn(p, icInfo, UIKit.Blue, Color.white, () =>
            {
                if (arController != null && arController.HasSpawnedModel) arController.ToggleInfo();
                else Toast(currentArInfoText);
            }, 1f, 0.68f, 118, -82, 0);
            CircleBtn(p, icAudio, UIKit.Nutri, Color.white, () => PlayNarration(), 1f, 0.57f, 118, -82, 0);
            CircleBtn(p, icRotate, UIKit.Prog, Color.white, () => { if (modelViewer != null) modelViewer.ToggleSpin(); if (arController != null) arController.ToggleSpin(); }, 1f, 0.46f, 118, -82, 0);
            CircleBtn(p, icClose, UIKit.Juegos, Color.white, onClose, 0.5f, 0f, 130, -190, 150);
            CircleBtn(p, icCamera, UIKit.Clin, Color.white, () => StartCoroutine(FlashAndSave()), 0.5f, 0f, 176, 0, 160);

            var toastTxt = UIKit.Text(p, "", 36, Color.white);
            UIKit.Frac(toastTxt, 0.05f, 0.135f, 0.95f, 0.185f);

            p.gameObject.AddComponent<ArRefs>().Set(raw, bg, hint, titleTxt, toastTxt);
            return p;
        }

        void ARStateChanged(bool ok)
        {
            if (ok)
            {
                if (arRaw != null) arRaw.enabled = false;
                if (arBg != null) arBg.color = new Color(0, 0, 0, 0); // transparente: se ve la cámara
                if (mainCam != null) mainCam.enabled = false;
                if (globalBg != null) globalBg.gameObject.SetActive(false); // deja pasar el feed real
                if (arHintText != null) arHintText.text = "Apunta la cámara al QR del tema";
                // AR real activo: arRaw (el RawImage del visor de respaldo) queda oculto, pero sin
                // esto su cámara offscreen (+ 2 luces, RenderTexture 800x900) seguía renderizando
                // cada frame en paralelo a la cámara AR real — el doble de trabajo de GPU sin nada
                // visible a cambio. Encontrada como causa de lentitud general en la experiencia AR
                // real (reportado 2026-07-17).
                modelViewer?.SetActive(false);
            }
            else
            {
                if (arRaw != null) arRaw.enabled = modelViewer != null;
                if (arBg != null) arBg.color = UIKit.Hex("14212E"); // fondo oscuro para el visor 3D
                if (mainCam != null) mainCam.enabled = true;
                if (globalBg != null) globalBg.gameObject.SetActive(true);
                if (arHintText != null) arHintText.text = modelViewer != null
                    ? "Vista 3D interactiva · toca Girar para explorar"
                    : "AR no disponible en este dispositivo para el escaneo libre";
                modelViewer?.SetActive(true);
            }
        }

        // Nota: usa Toast() (global, visible en cualquier pantalla) en vez de ShowARToast() porque
        // este método lo llaman tanto el botón "Escuchar" de Detalle como el de la Experiencia AR.
        void PlayNarration()
        {
            if (AppState.Muted) { Toast("Audio silenciado (activalo en Configuración)"); return; }
            var clips = isGenericScan ? null : books[currentBook].Narration;
            if (audioSrc == null || clips == null || currentTopic >= clips.Length || clips[currentTopic] == null)
            { Toast("Sin audio disponible"); return; }
            if (audioSrc.isPlaying) { audioSrc.Stop(); Toast("Audio detenido"); }
            else
            {
                audioSrc.clip = clips[currentTopic]; audioSrc.Play();
                var msgs = AppState.MarkAudioHeard(currentBook, currentTopic);
                if (msgs.Count > 0) ShowReward(string.Join("\n", msgs));
                else Toast("Reproduciendo narración…");
            }
        }

        IEnumerator FlashAndSave()
        {
            string dir = System.IO.Path.Combine(Application.persistentDataPath, "Fotos");
            System.IO.Directory.CreateDirectory(dir);
            // ScreenCapture.CaptureScreenshot antepone persistentDataPath incluso si ya le pasamos
            // una ruta absoluta (en este dispositivo terminaba duplicando la ruta y el archivo
            // nunca se guardaba donde la app después buscaba las fotos) — hay que darle una ruta
            // relativa para que la arme bien sola.
            string relFile = "Fotos/foto_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(relFile);
            if (flashOverlay != null)
            {
                float t = 0, dur = 0.22f;
                while (t < dur)
                {
                    t += Time.unscaledDeltaTime;
                    flashOverlay.color = new Color(1, 1, 1, Mathf.Sin(Mathf.Clamp01(t / dur) * Mathf.PI) * 0.85f);
                    yield return null;
                }
                flashOverlay.color = new Color(1, 1, 1, 0);
            }
            yield return new WaitForSeconds(0.25f);
            ShowARToast("Foto guardada");
        }

        // ============================================================
        IEnumerator AutoCapture()
        {
            string dir = Environment.GetEnvironmentVariable("ARCAP_DIR");
            if (string.IsNullOrEmpty(dir)) dir = Application.persistentDataPath;
            string suf = Land ? "_land" : "_port";
            currentBook = 0; currentTopic = 1;
            yield return new WaitForSeconds(1f);
            for (int i = 0; i < panels.Length; i++)
            {
                ShowOnly(panels[i], false);
                yield return new WaitForSeconds(0.8f); // dejar asentar animaciones de entrada
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, "cap_" + panels[i].name + suf + ".png"));
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.4f);
            Application.Quit();
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Pantallas 8) CONFIGURACIÓN 9) AYUDA.
    public partial class AppBootstrap
    {
        // ============================================================
        // 8 - CONFIGURACIÓN
        // ============================================================
        RectTransform BuildConfig()
        {
            var p = Panel("8_Config");
            bool land = Land;
            HeaderBand(p, UIKit.Muted, icGear, "Configuración", () => ShowOnly(pMenu), showBack: false);

            float y0 = land ? 0.30f : 0.44f, y1 = land ? 0.78f : 0.85f;
            var ageSub = ConfigRow(p, 0, 3, y0, y1, "Cambiar perfil de edad", AppState.AgeLabel(AppState.Age),
                icProfile, UIKit.Blue, () => ShowOnly(pPerfil));
            var muteSub = ConfigRow(p, 1, 3, y0, y1, "Sonido y narración",
                AppState.Muted ? "Silenciado · toca para activar" : "Activado · toca para silenciar",
                icAudio, UIKit.Nutri, () => { AppState.Muted = !AppState.Muted; RefreshConfig(); });
            ConfigRow(p, 2, 3, y0, y1, "Reiniciar progreso", "Borra estrellas, temas vistos y racha",
                icRotate, UIKit.Scan, () => { AppState.ResetProgress(); RefreshConfig(); RefreshProgreso(); Toast("Progreso reiniciado"); });

            var version = UIKit.Text(p, "AR Diabetes Tipo 1 · v1.0 · Endify", 24, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!land) UIKit.Frac(version, 0.1f, 0.185f, 0.9f, 0.22f);
            else UIKit.Frac(version, 0.1f, 0.16f, 0.9f, 0.20f);

            BuildBottomNav(p, 2);
            p.gameObject.AddComponent<ConfigRefs>().Set(ageSub, muteSub);
            return p;
        }

        // Fila de Configuración: círculo+icono, título, subtítulo (devuelve el subtítulo para refrescarlo).
        TMP_Text ConfigRow(RectTransform parent, int index, int total, float y0, float y1, string title, string sub,
                           Sprite icon, Color accent, UnityEngine.Events.UnityAction onClick)
        {
            var rc = UIKit.Cell(index, 1, total, 0.05f, y0, 0.95f, y1, 0f, 0.03f);
            var btn = UIKit.Button(parent, "", UIKit.Card, UIKit.Navy, 30, onClick);
            UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
            UIKit.AddShadow(btn.GetComponent<Image>(), 30, 0.08f, -4, 4);
            var disc = UIKit.Img(btn.transform, UIKit.Circle(), "Disc"); disc.color = accent;
            UIKit.Frac(disc, 0.03f, 0.2f, 0.20f, 0.8f);
            var ic = UIKit.Img(btn.transform, icon, "Ic"); ic.color = Color.white;
            UIKit.Frac(ic, 0.065f, 0.32f, 0.16f, 0.68f);
            var tl = UIKit.Text(btn.transform, title, 32, UIKit.Navy, TextAlignmentOptions.Left);
            UIKit.Frac(tl, 0.26f, 0.5f, 0.94f, 0.86f);
            var sb = UIKit.Text(btn.transform, sub, 24, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
            UIKit.Frac(sb, 0.26f, 0.14f, 0.94f, 0.48f);
            return sb;
        }

        void RefreshConfig()
        {
            if (pConfig == null) return;
            var refs = pConfig.GetComponent<ConfigRefs>();
            if (refs == null) return;
            if (refs.AgeSub != null) refs.AgeSub.text = AppState.AgeLabel(AppState.Age);
            if (refs.MuteSub != null) refs.MuteSub.text = AppState.Muted ? "Silenciado · toca para activar" : "Activado · toca para silenciar";
        }

        // ============================================================
        // 9 - AYUDA
        // ============================================================
        RectTransform BuildAyuda()
        {
            var p = Panel("9_Ayuda");
            bool land = Land;
            HeaderBand(p, UIKit.Scan, icQuestion, "Ayuda", () => ShowOnly(pMenu), showBack: false);

            string[] q =
            {
                "¿Cómo escaneo una página?",
                "¿Cómo gano estrellas?",
                "La figura no aparece o se ve rara",
                "¿Se guarda mi progreso?"
            };
            string[] a =
            {
                "Apuntá la cámara del celular a la página impresa del libro, sin taparla, hasta que aparezca la figura en 3D.",
                "Leyendo cada tema, escuchando su narración y escaneando su página en AR. ¡Completar un libro entero te da un bono extra!",
                "Apoyá la hoja impresa sobre una mesa quieta y mové solo el celular despacio alrededor. Si se ve muy grande o rara, alejate un poco de la hoja.",
                "Sí, se guarda solo en este dispositivo. Podés reiniciarlo cuando quieras desde Configuración."
            };
            int cols = land ? 2 : 1, rows = land ? 2 : 4;
            float y0 = land ? 0.20f : 0.135f, y1 = land ? 0.78f : 0.855f;
            for (int i = 0; i < 4; i++)
            {
                var rc = UIKit.Cell(i, cols, rows, 0.05f, y0, 0.95f, y1, 0.03f, 0.03f);
                var card = UIKit.Box(p, UIKit.Card, 30, "Faq" + i);
                UIKit.Frac(card.rectTransform, rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                UIKit.AddShadow(card, 30, 0.08f, -4, 4);
                var qt = UIKit.Text(card.transform, q[i], 30, UIKit.Navy, TextAlignmentOptions.Left);
                UIKit.Frac(qt, 0.06f, 0.56f, 0.94f, 0.92f);
                var at = UIKit.Text(card.transform, a[i], 24, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
                UIKit.Frac(at, 0.06f, 0.06f, 0.94f, 0.52f);
            }
            BuildBottomNav(p, 3);
            return p;
        }
    }
}

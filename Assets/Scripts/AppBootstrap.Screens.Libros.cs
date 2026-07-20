using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Pantallas 5A/5B/5C - LIBROS: Temas -> Detalle (la Experiencia AR en sí vive en AppBootstrap.AR.cs).
    public partial class AppBootstrap
    {
        static readonly Color[] TopicAccent = { UIKit.Clin, UIKit.Fisio, UIKit.Nutri, UIKit.Prog };

        RectTransform BuildLibroTemas(int book)
        {
            var b = books[book];
            var p = Panel(book + "_Temas");
            HeaderBand(p, b.Accent, icBook, b.Title, () => ShowOnly(pMenu), showBack: false);
            var sub = UIKit.Text(p, "Elige un tema para explorar en 3D", 40, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!Land) UIKit.Frac(sub, 0.08f, 0.815f, 0.92f, 0.86f);
            else UIKit.Frac(sub, 0.1f, 0.77f, 0.9f, 0.83f);

            bool land = Land;
            int cols = land ? 2 : 1, rows = land ? 2 : 4;
            float y0 = land ? 0.22f : 0.13f, y1 = land ? 0.72f : 0.79f;
            for (int i = 0; i < 4; i++)
            {
                var rc = UIKit.Cell(i, cols, rows, 0.05f, y0, 0.95f, y1, 0.03f, 0.03f);
                int idx = i;
                Color acc = TopicAccent[i];
                var btn = UIKit.Button(p, "", UIKit.Card, UIKit.Navy, 30, () => { currentTopic = idx; ShowOnly(bookDetalle[book]); });
                UIKit.Frac(R(btn), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                UIKit.AddShadow(btn.GetComponent<Image>(), 30, 0.12f, -6, 6);
                btn.gameObject.AddComponent<Appear>().delay = 0.05f * i;

                // Círculo de color con el icono del tema (mismo lenguaje visual que Perfil)
                var disc = UIKit.Img(btn.transform, UIKit.Circle(), "Disc"); disc.color = acc;
                var tic = UIKit.Img(btn.transform, b.TopicIcon[i], "Ic"); tic.color = Color.white;
                var tl = UIKit.Text(btn.transform, b.TopicTitle[i], 40, UIKit.Navy, TextAlignmentOptions.Left);
                var sb = UIKit.Text(btn.transform, b.TopicSub[i], 30, UIKit.Muted, TextAlignmentOptions.Left, FontStyles.Normal);
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

        RectTransform BuildLibroDetalle(int book)
        {
            var b = books[book];
            var p = Panel(book + "_Detalle");
            HeaderBand(p, b.Accent, b.HeroIcon, b.Title, () => ShowOnly(bookTemas[book]));
            var card = UIKit.Box(p, UIKit.Card, 44, "Card");
            if (!Land) UIKit.Frac(card.rectTransform, 0.06f, 0.19f, 0.94f, 0.83f);
            else UIKit.Frac(card.rectTransform, 0.06f, 0.13f, 0.94f, 0.81f);
            UIKit.AddShadow(card, 44, 0.10f, -8, 8);

            var band = UIKit.Box(card.transform, b.Accent, 40, "Hero");
            var img = UIKit.Img(band.transform, b.HeroIcon, "Img"); img.preserveAspect = true;
            var title = UIKit.Text(card.transform, "", 54, UIKit.Navy);
            var desc = UIKit.Text(card.transform, "", 40, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
            if (!Land)
            {
                UIKit.Frac(band.rectTransform, 0.06f, 0.55f, 0.94f, 0.95f);
                UIKit.Frac(img, 0.28f, 0.06f, 0.72f, 0.94f);
                UIKit.Frac(title, 0.06f, 0.44f, 0.94f, 0.53f);
                UIKit.Frac(desc, 0.08f, 0.10f, 0.92f, 0.42f);
            }
            else
            {
                UIKit.Frac(band.rectTransform, 0.04f, 0.12f, 0.42f, 0.9f);
                UIKit.Frac(img, 0.08f, 0.08f, 0.92f, 0.92f);
                UIKit.Frac(title, 0.46f, 0.66f, 0.97f, 0.88f);
                UIKit.Frac(desc, 0.46f, 0.2f, 0.97f, 0.62f);
            }

            var escuchar = UIKit.Button(p, "  Escuchar", UIKit.Blue, Color.white, 36, () => PlayNarration(), 38);
            var g = UIKit.Img(R(escuchar), icAudio, "Ico"); g.color = Color.white;
            var scan = UIKit.Button(p, "ESCANEAR PÁGINA", b.Accent, Color.white, 40, () => ShowOnly(bookAR[book]));
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

            // Guardar refs para este libro específico (se re-vinculan a los campos "actuales" en ShowOnly)
            p.gameObject.AddComponent<DetalleRefs>().Set(band, img, title, desc);
            return p;
        }

        void RefreshDetalle()
        {
            var p = bookDetalle[currentBook];
            var refs = p.GetComponent<DetalleRefs>();
            if (refs == null) return;
            var b = books[currentBook];
            refs.Title.text = b.TopicTitle[currentTopic];
            refs.Desc.text = b.Desc(currentTopic, AppState.Age);
            refs.Band.color = b.Accent;
            detTitle = refs.Title; detDesc = refs.Desc; detBand = refs.Band; detImg = refs.Img;
            var msgs = AppState.MarkTopicSeen(currentBook, currentTopic);
            if (msgs.Count > 0) ShowReward(string.Join("\n", msgs));
        }

        RectTransform BuildLibroAR(int book)
        {
            var b = books[book];
            var p = BuildArScreenCore(book + "_AR", () => modelViewers[book], () => ShowOnly(bookDetalle[book]));
            return p;
        }

        RectTransform BuildScanAR()
        {
            return BuildArScreenCore("5D_ScanAR", () => null, () => ShowOnly(pMenu));
        }
    }
}

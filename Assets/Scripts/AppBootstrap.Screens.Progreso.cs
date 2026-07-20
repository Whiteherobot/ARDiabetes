using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARDiabetes
{
    // Pantallas 7) PROGRESO 7B) INSIGNIAS 7C) COLECCIÓN AR.
    public partial class AppBootstrap
    {
        // ============================================================
        // 7 - PROGRESO
        // ============================================================
        RectTransform BuildProgreso()
        {
            var p = Panel("7_Progreso");
            bool land = Land;
            HeaderBand(p, UIKit.Prog, icChart, "Tu progreso", () => ShowOnly(pMenu), showBack: false);
            // Acceso a las 2 vitrinas de colección, apiladas en la esquina derecha de la franja.
            // Antes eran círculos blancos translúcidos SIN texto (mismo look que el botón de
            // volver) — el único par de botones de toda la app sin su etiqueta al lado, y por
            // eso costaba notarlos/entender qué eran. Ahora: círculo BLANCO SÓLIDO con el ícono a
            // color (se recorta fuerte contra la franja naranja, como una insignia/sticker) +
            // etiqueta de texto debajo, igual que hace el resto de la app (nav inferior, tiles).
            float vitrinaAy = land ? 0.935f : 0.945f;
            CircleBtn(p, spStar, Color.white, UIKit.Prog, () => ShowOnly(pInsignias), 1f, vitrinaAy, 84, -68, 0);
            var lblIns = UIKit.Text(p, "Insignias", 18, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            var lir = R(lblIns); lir.anchorMin = lir.anchorMax = new Vector2(1f, vitrinaAy);
            lir.pivot = new Vector2(0.5f, 0.5f); lir.anchoredPosition = new Vector2(-68, -58); lir.sizeDelta = new Vector2(150, 28);

            CircleBtn(p, spIconScan, Color.white, UIKit.Scan, () => ShowOnly(pColeccion), 1f, vitrinaAy, 84, -180, 0);
            var lblCol = UIKit.Text(p, "Estampitas", 18, Color.white, TextAlignmentOptions.Center, FontStyles.Bold);
            var lcr = R(lblCol); lcr.anchorMin = lcr.anchorMax = new Vector2(1f, vitrinaAy);
            lcr.pivot = new Vector2(0.5f, 0.5f); lcr.anchoredPosition = new Vector2(-180, -58); lcr.sizeDelta = new Vector2(150, 28);

            var card = UIKit.Box(p, UIKit.Card, 40, "Stats");
            if (!land) UIKit.Frac(card.rectTransform, 0.05f, 0.715f, 0.95f, 0.855f);
            else UIKit.Frac(card.rectTransform, 0.04f, 0.64f, 0.96f, 0.80f);
            UIKit.AddShadow(card, 40, 0.10f, -6, 6);
            var statsTxt = UIKit.Text(card.transform, "", 30, UIKit.Navy, TextAlignmentOptions.Left, FontStyles.Normal);
            UIKit.Frac(statsTxt, 0.06f, 0.5f, 0.94f, 0.92f);
            var overallFill = ProgressBar(card.transform, 0f, 0.06f, 0.16f, 0.94f, 0.40f, UIKit.Prog);

            var booksTitle = UIKit.Text(p, "Tus libros", 34, UIKit.Navy, TextAlignmentOptions.Left);
            if (!land) UIKit.Frac(booksTitle, 0.06f, 0.665f, 0.6f, 0.705f);
            else UIKit.Frac(booksTitle, 0.05f, 0.585f, 0.6f, 0.625f);

            var bookFill = new Image[3];
            var bookCount = new TMP_Text[3];
            string[] bookLabels = { "Libro Fisiológico", "Libro Nutricional", "Libro Clínico" };
            Color[] bookColors = { UIKit.Fisio, UIKit.Nutri, UIKit.Clin };
            float rowsY0 = land ? 0.42f : 0.435f, rowsY1 = land ? 0.575f : 0.655f;
            for (int i = 0; i < 3; i++)
            {
                var rc = UIKit.Cell(i, 1, 3, 0.06f, rowsY0, 0.94f, rowsY1, 0f, 0.03f);
                var lbl = UIKit.Text(p, bookLabels[i], 28, UIKit.Navy, TextAlignmentOptions.Left, FontStyles.Normal);
                UIKit.Frac(lbl, rc.xMin, rc.yMin + rc.height * 0.5f, rc.xMax, rc.yMax);
                bookCount[i] = UIKit.Text(p, "0/4", 26, UIKit.Muted, TextAlignmentOptions.Right, FontStyles.Normal);
                UIKit.Frac(bookCount[i], rc.xMax - 0.14f, rc.yMin + rc.height * 0.5f, rc.xMax, rc.yMax);
                bookFill[i] = ProgressBar(p, 0f, rc.xMin, rc.yMin, rc.xMax, rc.yMin + rc.height * 0.38f, bookColors[i]);
            }

            var photoTitle = UIKit.Text(p, "Tus fotos", 34, UIKit.Navy, TextAlignmentOptions.Left);
            if (!land) UIKit.Frac(photoTitle, 0.06f, 0.375f, 0.6f, 0.415f);
            else UIKit.Frac(photoTitle, 0.05f, 0.35f, 0.6f, 0.39f);

            var photoGrid = UIKit.Node("PhotoGrid", p);
            if (!land) UIKit.Frac(photoGrid, 0.05f, 0.185f, 0.95f, 0.365f);
            else UIKit.Frac(photoGrid, 0.04f, 0.19f, 0.96f, 0.34f);

            BuildBottomNav(p, 1);
            p.gameObject.AddComponent<ProgresoRefs>().Set(statsTxt, overallFill, bookFill, bookCount, photoGrid);
            return p;
        }

        void RefreshProgreso()
        {
            if (pProgreso == null) return;
            var refs = pProgreso.GetComponent<ProgresoRefs>();
            if (refs == null) return;
            int total = AppState.TopicsSeenCount();
            if (refs.Stats != null)
                refs.Stats.text = "Nivel " + AppState.Level + " · Racha " + AppState.Streak + (AppState.Streak == 1 ? " día" : " días")
                    + "\n" + AppState.Stars + " estrellas · " + total + "/12 temas explorados";
            if (refs.OverallFill != null) UIKit.Frac(refs.OverallFill, 0f, 0f, Mathf.Clamp01(total / 12f), 1f);
            for (int i = 0; i < 3; i++)
            {
                int seen = AppState.TopicsSeenCount(i);
                if (refs.BookFill != null && refs.BookFill[i] != null) UIKit.Frac(refs.BookFill[i], 0f, 0f, seen / 4f, 1f);
                if (refs.BookCount != null && refs.BookCount[i] != null) refs.BookCount[i].text = seen + "/4";
            }
            RefreshPhotoGrid(refs.PhotoGrid);
        }

        void RefreshPhotoGrid(RectTransform grid)
        {
            if (grid == null) return;
            for (int i = grid.childCount - 1; i >= 0; i--) Destroy(grid.GetChild(i).gameObject);

            string[] files = new string[0];
            string dir = System.IO.Path.Combine(Application.persistentDataPath, "Fotos");
            if (System.IO.Directory.Exists(dir))
            {
                files = System.IO.Directory.GetFiles(dir, "*.png");
                Array.Sort(files, StringComparer.Ordinal);
                Array.Reverse(files); // más recientes primero (nombre = timestamp)
            }
            int n = Mathf.Min(files.Length, 6);
            if (n == 0)
            {
                var msg = UIKit.Text(grid, "Aún no tomaste fotos. Andá a una Experiencia AR y tocá el botón de cámara.",
                    28, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
                UIKit.Stretch(msg.rectTransform);
                return;
            }
            for (int i = 0; i < n; i++)
            {
                var rc = UIKit.Cell(i, 3, 2, 0f, 0f, 1f, 1f, 0.05f, 0.08f);
                string path = files[i];
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(System.IO.File.ReadAllBytes(path));
                var thumb = UIKit.Button(grid, "", UIKit.Card, UIKit.Navy, 20, () => OpenPhotoViewer(path), 0, false);
                UIKit.Frac(R(thumb), rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                var raw = new GameObject("Thumb", typeof(RectTransform)).AddComponent<RawImage>();
                raw.transform.SetParent(thumb.transform, false); raw.texture = tex; raw.raycastTarget = false;
                UIKit.Stretch(raw.rectTransform, 4);
            }
        }

        void OpenPhotoViewer(string path)
        {
            if (photoViewer == null || photoViewerImg == null) return;
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(System.IO.File.ReadAllBytes(path));
            photoViewerImg.texture = tex;
            photoViewer.gameObject.SetActive(true);
        }

        // ============================================================
        // 7B - INSIGNIAS (vitrina de logros, accedida desde Progreso)
        // ============================================================
        RectTransform BuildInsignias()
        {
            var p = Panel("7B_Insignias");
            bool land = Land;
            HeaderBand(p, UIKit.Prog, spStar, "Insignias", () => ShowOnly(pProgreso));

            var subtitle = UIKit.Text(p, "", 34, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Bold);
            UIKit.Frac(subtitle, 0.06f, land ? 0.795f : 0.825f, 0.94f, land ? 0.850f : 0.870f);

            // Mismo ícono que ya usa cada libro/sección en el resto de la app (nada nuevo que cargar).
            Sprite[] icons = { spIconFisio, spIconNutri, spIconClinico, spStar,
                                spIconJuegos, spIconJuegos, spIconJuegos, spIconScan, icCalendar };

            var badges = AppState.Badges;
            var iconBgs = new Image[badges.Length];
            var titles = new TMP_Text[badges.Length];
            var status = new TMP_Text[badges.Length];

            float x0 = 0.05f, x1 = 0.95f, y0 = 0.03f, y1 = land ? 0.74f : 0.79f;
            for (int i = 0; i < badges.Length; i++)
            {
                var rc = UIKit.Cell(i, 3, 3, x0, y0, x1, y1, 0.03f, 0.04f);
                var b = badges[i];
                var card = UIKit.Box(p, UIKit.Card, 26, "Badge" + i);
                UIKit.Frac(card.rectTransform, rc.xMin, rc.yMin, rc.xMax, rc.yMax);

                // Tamaño fijo en píxeles (no Frac): la tarjeta no es cuadrada, y con preserveAspect
                // un Frac de aspecto no-1:1 encogería el círculo en vez de llenarlo bien.
                var iconBg = UIKit.Img(card.transform, UIKit.Circle(), "IconBg"); iconBg.color = b.Color;
                var ibrt = R(iconBg); ibrt.anchorMin = ibrt.anchorMax = new Vector2(0.5f, 0.7f);
                ibrt.pivot = new Vector2(0.5f, 0.5f); ibrt.anchoredPosition = Vector2.zero; ibrt.sizeDelta = new Vector2(84, 84);
                var iconImg = UIKit.Img(iconBg.transform, icons[i], "Icon"); iconImg.color = Color.white;
                UIKit.Stretch(iconImg.rectTransform, 20);

                var title = UIKit.Text(card.transform, b.Title, 24, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Bold);
                UIKit.Frac(title, 0.05f, 0.34f, 0.95f, 0.48f);
                var desc = UIKit.Text(card.transform, b.Desc, 17, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
                UIKit.Frac(desc, 0.05f, 0.14f, 0.95f, 0.34f);
                var st = UIKit.Text(card.transform, "", 18, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Bold);
                UIKit.Frac(st, 0.05f, 0.02f, 0.95f, 0.14f);

                UIKit.AddShadow(card, 26, 0.10f, -4, 4);

                iconBgs[i] = iconBg; titles[i] = title; status[i] = st;
            }

            p.gameObject.AddComponent<InsigniasRefs>().Set(subtitle, iconBgs, titles, status);
            return p;
        }

        void RefreshInsignias()
        {
            if (pInsignias == null) return;
            var refs = pInsignias.GetComponent<InsigniasRefs>();
            if (refs == null) return;
            var badges = AppState.Badges;
            int got = AppState.BadgesUnlockedCount();
            if (refs.Subtitle != null) refs.Subtitle.text = got + "/" + badges.Length + " insignias conseguidas";
            for (int i = 0; i < badges.Length; i++)
            {
                bool on = AppState.IsBadgeUnlocked(i);
                if (refs.IconBgs != null && refs.IconBgs[i] != null)
                    refs.IconBgs[i].color = on ? badges[i].Color : Color.Lerp(badges[i].Color, UIKit.Muted, 0.75f);
                if (refs.Titles != null && refs.Titles[i] != null)
                    refs.Titles[i].color = on ? UIKit.Navy : UIKit.Muted;
                if (refs.Status != null && refs.Status[i] != null)
                {
                    refs.Status[i].text = on ? "Conseguida" : "Bloqueada";
                    refs.Status[i].color = on ? UIKit.Nutri : UIKit.Muted;
                }
            }
        }

        // ============================================================
        // 7C - COLECCIÓN AR (estampita por cada uno de los 12 marcadores, accedida desde Progreso)
        // ============================================================
        RectTransform BuildColeccion()
        {
            var p = Panel("7C_Coleccion");
            bool land = Land;
            HeaderBand(p, UIKit.Scan, spIconScan, "Colección AR", () => ShowOnly(pProgreso));

            var subtitle = UIKit.Text(p, "", 34, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Bold);
            UIKit.Frac(subtitle, 0.06f, land ? 0.795f : 0.825f, 0.94f, land ? 0.850f : 0.870f);

            // Columna = libro, fila = tema: se lee como un álbum de estampitas, un libro por columna.
            const int cols = 3, rows = 4, n = 12;
            float x0 = 0.05f, x1 = 0.95f, y0 = 0.03f, y1 = land ? 0.74f : 0.79f;
            var iconBgs = new Image[n];
            var titles = new TMP_Text[n];
            var status = new TMP_Text[n];
            var cardBook = new int[n];
            var cardTopic = new int[n];

            for (int topic = 0; topic < 4; topic++)
            {
                for (int book = 0; book < 3; book++)
                {
                    int i = topic * cols + book;
                    var rc = UIKit.Cell(i, cols, rows, x0, y0, x1, y1, 0.03f, 0.035f);
                    var b = books[book];
                    var card = UIKit.Box(p, UIKit.Card, 24, "Stamp" + book + "_" + topic);
                    UIKit.Frac(card.rectTransform, rc.xMin, rc.yMin, rc.xMax, rc.yMax);

                    // Tamaño fijo en píxeles (no Frac), mismo motivo que en Insignias: la tarjeta
                    // no es cuadrada y con preserveAspect un Frac no-1:1 encoge el círculo.
                    var iconBg = UIKit.Img(card.transform, UIKit.Circle(), "IconBg"); iconBg.color = b.Accent;
                    var ibrt = R(iconBg); ibrt.anchorMin = ibrt.anchorMax = new Vector2(0.5f, 0.72f);
                    ibrt.pivot = new Vector2(0.5f, 0.5f); ibrt.anchoredPosition = Vector2.zero; ibrt.sizeDelta = new Vector2(72, 72);
                    Sprite topicIcon = (b.TopicIcon != null && topic < b.TopicIcon.Length) ? b.TopicIcon[topic] : null;
                    var iconImg = UIKit.Img(iconBg.transform, topicIcon, "Icon"); iconImg.color = Color.white;
                    UIKit.Stretch(iconImg.rectTransform, 16);

                    string title = (b.TopicTitle != null && topic < b.TopicTitle.Length) ? b.TopicTitle[topic] : "Tema";
                    var t = UIKit.Text(card.transform, title, 19, UIKit.Navy, TextAlignmentOptions.Center, FontStyles.Bold);
                    UIKit.Frac(t, 0.05f, 0.36f, 0.95f, 0.50f);
                    var sub = UIKit.Text(card.transform, b.Title, 15, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Normal);
                    UIKit.Frac(sub, 0.05f, 0.20f, 0.95f, 0.36f);
                    var st = UIKit.Text(card.transform, "", 16, UIKit.Muted, TextAlignmentOptions.Center, FontStyles.Bold);
                    UIKit.Frac(st, 0.05f, 0.02f, 0.95f, 0.18f);

                    UIKit.AddShadow(card, 24, 0.10f, -4, 4);

                    iconBgs[i] = iconBg; titles[i] = t; status[i] = st; cardBook[i] = book; cardTopic[i] = topic;
                }
            }

            p.gameObject.AddComponent<ColeccionRefs>().Set(subtitle, iconBgs, titles, status, cardBook, cardTopic);
            return p;
        }

        void RefreshColeccion()
        {
            if (pColeccion == null) return;
            var refs = pColeccion.GetComponent<ColeccionRefs>();
            if (refs == null) return;
            if (refs.Subtitle != null) refs.Subtitle.text = AppState.MarkersScannedCount() + "/12 páginas coleccionadas";
            for (int i = 0; i < 12; i++)
            {
                int book = refs.CardBook[i], topic = refs.CardTopic[i];
                bool on = AppState.IsMarkerScanned(book, topic);
                var accent = books[book].Accent;
                if (refs.IconBgs != null && refs.IconBgs[i] != null)
                    refs.IconBgs[i].color = on ? accent : Color.Lerp(accent, UIKit.Muted, 0.75f);
                if (refs.Titles != null && refs.Titles[i] != null)
                    refs.Titles[i].color = on ? UIKit.Navy : UIKit.Muted;
                if (refs.Status != null && refs.Status[i] != null)
                {
                    refs.Status[i].text = on ? "Coleccionada" : "Sin escanear";
                    refs.Status[i].color = on ? UIKit.Nutri : UIKit.Muted;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARDiabetes
{
    /// <summary>
    /// Estado global de la app que persiste entre pantallas (y entre sesiones vía PlayerPrefs).
    /// Edad, estrellas (con varias formas reales de ganarlas), temas vistos, racha diaria y silencio.
    /// </summary>
    public static class AppState
    {
        public enum AgeGroup { None = 0, Kids_5_9 = 1, Kids_10_12 = 2, Teens_13_15 = 3 }

        const string KeyAge = "ar_age_group";
        const string KeyStars = "ar_stars";
        const string KeyTopicsMask = "ar_topics_seen";   // bit i = libro*4 + tema (leer el texto)
        const string KeyAudioMask = "ar_audio_heard";    // bit i = libro*4 + tema (escuchar narración)
        const string KeyScanMask = "ar_marker_scanned";  // bit i = libro*4 + tema (escanear el QR real)
        const string KeyQuizMask = "ar_quiz_correct";    // bit i = libro*4 + pregunta (respuesta correcta, quiz)
        const string KeyBonusMask = "ar_bonus";          // bit 0-2 = libro completo (lectura), bit 3 = los 3 libros,
                                                          // bit 4-6 = quiz de libro completo (Juegos y Retos),
                                                          // bit 7 = explorador AR (12/12 marcadores), bit 8 = racha de 7 días.
                                                          // Cada bit es también, 1 a 1, el índice de una insignia en Badges[].
        const string KeyStreak = "ar_streak";
        const string KeyLastDay = "ar_last_day"; // días desde una fecha fija, para calcular racha
        const string KeyMuted = "ar_muted";
        const int TopicsTotal = 12; // 3 libros x 4 temas
        const int StarsPerLevel = 50;
        static readonly DateTime Epoch = new DateTime(2020, 1, 1);

        public static AgeGroup Age
        {
            get => (AgeGroup)PlayerPrefs.GetInt(KeyAge, 0);
            set { PlayerPrefs.SetInt(KeyAge, (int)value); PlayerPrefs.Save(); }
        }

        public static int Stars
        {
            get => PlayerPrefs.GetInt(KeyStars, 0);
            set { PlayerPrefs.SetInt(KeyStars, Mathf.Max(0, value)); PlayerPrefs.Save(); }
        }

        public static bool Muted
        {
            get => PlayerPrefs.GetInt(KeyMuted, 0) == 1;
            set { PlayerPrefs.SetInt(KeyMuted, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool IsTopicSeen(int book, int topic) => GetBit(KeyTopicsMask, book, topic);
        public static bool IsAudioHeard(int book, int topic) => GetBit(KeyAudioMask, book, topic);
        public static bool IsMarkerScanned(int book, int topic) => GetBit(KeyScanMask, book, topic);

        static bool GetBit(string key, int book, int topic) => (PlayerPrefs.GetInt(key, 0) & (1 << (book * 4 + topic))) != 0;

        /// <summary>Pone en 1 el bit si no lo estaba. Devuelve true si era nuevo.</summary>
        static bool SetBitIfNew(string key, int book, int topic)
        {
            int mask = PlayerPrefs.GetInt(key, 0);
            int flag = 1 << (book * 4 + topic);
            if ((mask & flag) != 0) return false;
            PlayerPrefs.SetInt(key, mask | flag);
            return true;
        }

        /// <summary>Suma estrellas y devuelve un mensaje de "¡Subiste de nivel!" si corresponde (o null).</summary>
        static string AwardStars(int amount)
        {
            int before = Level;
            Stars += amount;
            return Level > before ? "¡Subiste a nivel " + Level + "!" : null;
        }

        /// <summary>Revisa si con el tema recién marcado se completó un libro o los 3 libros,
        /// y otorga el bonus correspondiente la primera vez. Devuelve el mensaje si aplica.</summary>
        static string CheckCompletionBonus(int book)
        {
            int bonus = PlayerPrefs.GetInt(KeyBonusMask, 0);
            string msg = null;
            if (TopicsSeenCount(book) >= 4 && (bonus & (1 << book)) == 0)
            {
                bonus |= 1 << book;
                Stars += 15;
                msg = "¡Libro completo! +15 estrellas";
            }
            if (TopicsSeenCount() >= TopicsTotal && (bonus & (1 << 3)) == 0)
            {
                bonus |= 1 << 3;
                Stars += 25;
                msg = "¡Completaste los 3 libros! +25 estrellas";
            }
            PlayerPrefs.SetInt(KeyBonusMask, bonus);
            return msg;
        }

        /// <summary>Marca un tema como leído la primera vez (+10 estrellas). Devuelve las líneas de
        /// aviso para mostrar en un toast (vacío si ya estaba visto).</summary>
        public static List<string> MarkTopicSeen(int book, int topic)
        {
            var msgs = new List<string>();
            if (!SetBitIfNew(KeyTopicsMask, book, topic)) return msgs;
            msgs.Add("+10 estrellas · nuevo tema explorado");
            string lvl = AwardStars(10);
            string bonus = CheckCompletionBonus(book);
            if (bonus != null) msgs.Add(bonus);
            if (lvl != null) msgs.Add(lvl);
            return msgs;
        }

        /// <summary>Marca la narración de un tema como escuchada la primera vez (+5 estrellas).</summary>
        public static List<string> MarkAudioHeard(int book, int topic)
        {
            var msgs = new List<string>();
            if (!SetBitIfNew(KeyAudioMask, book, topic)) return msgs;
            msgs.Add("+5 estrellas · narración escuchada");
            string lvl = AwardStars(5);
            if (lvl != null) msgs.Add(lvl);
            return msgs;
        }

        /// <summary>Marca el QR real de un tema como escaneado en AR la primera vez (+5 estrellas).</summary>
        public static List<string> MarkMarkerScanned(int book, int topic)
        {
            var msgs = new List<string>();
            if (!SetBitIfNew(KeyScanMask, book, topic)) return msgs;
            msgs.Add("+5 estrellas · página escaneada en AR");
            string lvl = AwardStars(5);
            string bonus = CheckScanAllBonus();
            if (bonus != null) msgs.Add(bonus);
            if (lvl != null) msgs.Add(lvl);
            return msgs;
        }

        /// <summary>Bonus único (+25) al escanear en AR real los 12 marcadores (insignia "Explorador AR").</summary>
        static string CheckScanAllBonus()
        {
            if (PlayerPrefs.GetInt(KeyScanMask, 0) != (1 << TopicsTotal) - 1) return null;
            int bonus = PlayerPrefs.GetInt(KeyBonusMask, 0);
            if ((bonus & (1 << 7)) != 0) return null;
            PlayerPrefs.SetInt(KeyBonusMask, bonus | (1 << 7));
            Stars += 25;
            return "¡Explorador AR! Escaneaste las 12 páginas · +25 estrellas";
        }

        /// <summary>Marca un juego del quiz de un libro como respondido correctamente la primera
        /// vez (+5 estrellas). Si con este ya se respondieron bien los 3 del libro, suma el bonus de
        /// "quiz completo" (+20, una sola vez). Cada libro tiene 3 juegos (no 4); se reutiliza el
        /// multiplicador *4 de SetBitIfNew (compartido con temas/audio/escaneo), solo se usan 3 de
        /// los 4 bits que le corresponden a cada libro.</summary>
        public static List<string> MarkQuizCorrect(int book, int question)
        {
            var msgs = new List<string>();
            if (!SetBitIfNew(KeyQuizMask, book, question)) return msgs;
            msgs.Add("+5 estrellas · respuesta correcta");
            string lvl = AwardStars(5);
            string bonus = CheckQuizBonus(book);
            if (bonus != null) msgs.Add(bonus);
            if (lvl != null) msgs.Add(lvl);
            return msgs;
        }

        static string CheckQuizBonus(int book)
        {
            int mask = PlayerPrefs.GetInt(KeyQuizMask, 0);
            for (int i = 0; i < 3; i++) if ((mask & (1 << (book * 4 + i))) == 0) return null;
            int bonusMask = PlayerPrefs.GetInt(KeyBonusMask, 0);
            int bit = 4 + book;
            if ((bonusMask & (1 << bit)) != 0) return null;
            PlayerPrefs.SetInt(KeyBonusMask, bonusMask | (1 << bit));
            Stars += 20;
            return "¡Quiz completo! +20 estrellas";
        }

        /// <summary>Juegos del quiz de un libro respondidos correctamente (0-3), para mostrar
        /// "2/3 correctas" en el hub de Juegos y Retos.</summary>
        public static int QuizCorrectCount(int book)
        {
            int mask = PlayerPrefs.GetInt(KeyQuizMask, 0);
            int n = 0;
            for (int i = 0; i < 3; i++) if ((mask & (1 << (book * 4 + i))) != 0) n++;
            return n;
        }

        /// <summary>Marcadores reales escaneados en AR (0-12), para el contador de la Colección AR.</summary>
        public static int MarkersScannedCount()
        {
            int mask = PlayerPrefs.GetInt(KeyScanMask, 0);
            int n = 0;
            for (int i = 0; i < TopicsTotal; i++) if ((mask & (1 << i)) != 0) n++;
            return n;
        }

        /// <summary>Temas vistos de un libro (0-2), o el total de los 3 si book &lt; 0.</summary>
        public static int TopicsSeenCount(int book = -1)
        {
            int mask = PlayerPrefs.GetInt(KeyTopicsMask, 0);
            int from = book < 0 ? 0 : book * 4, to = book < 0 ? TopicsTotal : book * 4 + 4;
            int n = 0;
            for (int i = from; i < to; i++) if ((mask & (1 << i)) != 0) n++;
            return n;
        }

        // Nivel real: sube cada StarsPerLevel estrellas ganadas (leer, escuchar, escanear, bonus).
        // Así el nivel responde a CUALQUIER forma de interactuar, no solo a abrir un tema.
        public static int Level => 1 + Stars / StarsPerLevel;

        // Progreso dentro del nivel actual (0..1), para la barra del menú/Progreso.
        public static float LevelProgress => (Stars % StarsPerLevel) / (float)StarsPerLevel;

        public static int Streak => PlayerPrefs.GetInt(KeyStreak, 0);

        /// <summary>Actualiza la racha de días consecutivos. Llamar una vez al iniciar la app.
        /// Devuelve el aviso de bonus si con esto se alcanzó la racha de 7 días (o lista vacía).</summary>
        public static List<string> TouchDailyStreak()
        {
            var msgs = new List<string>();
            int today = (int)(DateTime.Now.Date - Epoch).TotalDays;
            int last = PlayerPrefs.GetInt(KeyLastDay, -1);
            if (last == today) return msgs; // ya contado hoy
            int streak = last == today - 1 ? PlayerPrefs.GetInt(KeyStreak, 0) + 1 : 1;
            PlayerPrefs.SetInt(KeyStreak, streak);
            PlayerPrefs.SetInt(KeyLastDay, today);
            PlayerPrefs.Save();
            string bonus = CheckStreakBonus();
            if (bonus != null) msgs.Add(bonus);
            return msgs;
        }

        /// <summary>Bonus único (+20) al alcanzar una racha de 7 días seguidos (insignia "Racha de fuego").</summary>
        static string CheckStreakBonus()
        {
            if (Streak < 7) return null;
            int bonus = PlayerPrefs.GetInt(KeyBonusMask, 0);
            if ((bonus & (1 << 8)) != 0) return null;
            PlayerPrefs.SetInt(KeyBonusMask, bonus | (1 << 8));
            Stars += 20;
            return "¡Racha de 7 días! +20 estrellas";
        }

        /// <summary>Una insignia coleccionable: título, descripción de cómo se gana y color de marca
        /// (reusa los colores de libro/sección ya definidos en UIKit, no duplica hex nuevos).</summary>
        public struct BadgeDef { public string Title; public string Desc; public Color Color; }

        // Índice = bit en KeyBonusMask (ver comentario arriba). Coleccionable desde la pantalla Insignias.
        public static readonly BadgeDef[] Badges = new[]
        {
            new BadgeDef{ Title = "Corazón sabio",     Desc = "Completa el Libro Fisiológico",                   Color = UIKit.Hex("DD5450")  },
            new BadgeDef{ Title = "Plato saludable",    Desc = "Completa el Libro Nutricional",                   Color = UIKit.Nutri  },
            new BadgeDef{ Title = "Doctor junior",      Desc = "Completa el Libro Clínico",                       Color = UIKit.Clin   },
            new BadgeDef{ Title = "Diabetes Experto",   Desc = "Completa los 3 libros",                           Color = UIKit.Prog   },
            new BadgeDef{ Title = "Genio Fisiológico",  Desc = "Responde bien los 3 juegos del Libro Fisiológico", Color = UIKit.Hex("DD5450")  },
            new BadgeDef{ Title = "Genio Nutricional",  Desc = "Responde bien los 3 juegos del Libro Nutricional", Color = UIKit.Nutri  },
            new BadgeDef{ Title = "Genio Clínico",      Desc = "Responde bien los 3 juegos del Libro Clínico",     Color = UIKit.Clin   },
            new BadgeDef{ Title = "Explorador AR",      Desc = "Escanea las 12 páginas en realidad aumentada",     Color = UIKit.Scan   },
            new BadgeDef{ Title = "Racha de fuego",     Desc = "Usa la app 7 días seguidos",                       Color = UIKit.Juegos },
        };

        /// <summary>Cuántas insignias ya se ganaron, para el resumen "X/9" de la pantalla Insignias.</summary>
        public static int BadgesUnlockedCount()
        {
            int n = 0;
            for (int i = 0; i < Badges.Length; i++) if (IsBadgeUnlocked(i)) n++;
            return n;
        }

        public static bool IsBadgeUnlocked(int i) => (PlayerPrefs.GetInt(KeyBonusMask, 0) & (1 << i)) != 0;

        /// <summary>Reinicia estrellas, temas vistos/escuchados/escaneados, bonus y racha (no toca la edad).</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.SetInt(KeyStars, 0);
            PlayerPrefs.SetInt(KeyTopicsMask, 0);
            PlayerPrefs.SetInt(KeyAudioMask, 0);
            PlayerPrefs.SetInt(KeyScanMask, 0);
            PlayerPrefs.SetInt(KeyQuizMask, 0);
            PlayerPrefs.SetInt(KeyBonusMask, 0);
            PlayerPrefs.SetInt(KeyStreak, 0);
            PlayerPrefs.DeleteKey(KeyLastDay);
            PlayerPrefs.Save();
        }

        public static string AgeLabel(AgeGroup g)
        {
            switch (g)
            {
                case AgeGroup.Kids_5_9: return "5 - 9 años";
                case AgeGroup.Kids_10_12: return "10 - 12 años";
                case AgeGroup.Teens_13_15: return "13 - 15 años";
                default: return "Sin perfil";
            }
        }

        /// <summary>Saludo del menú, con tono distinto según el perfil de edad elegido.</summary>
        public static string GreetingFor(AgeGroup g)
        {
            switch (g)
            {
                case AgeGroup.Kids_5_9: return "¡Hola, campeón!";
                case AgeGroup.Kids_10_12: return "¡Hola, explorador!";
                case AgeGroup.Teens_13_15: return "¡Hola de nuevo!";
                default: return "¡Hola!";
            }
        }
    }
}

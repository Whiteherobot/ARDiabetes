using System;
using UnityEngine;

namespace ARDiabetes
{
    /// <summary>
    /// Estado global de la app que persiste entre pantallas (y entre sesiones vía PlayerPrefs).
    /// Edad, estrellas, temas vistos (progreso real), racha diaria y silencio de audio.
    /// </summary>
    public static class AppState
    {
        public enum AgeGroup { None = 0, Kids_5_9 = 1, Kids_10_12 = 2, Teens_13_15 = 3 }

        const string KeyAge = "ar_age_group";
        const string KeyStars = "ar_stars";
        const string KeyTopicsMask = "ar_topics_seen"; // bit i = libro*4 + tema
        const string KeyStreak = "ar_streak";
        const string KeyLastDay = "ar_last_day"; // días desde una fecha fija, para calcular racha
        const string KeyMuted = "ar_muted";
        const int TopicsTotal = 12; // 3 libros x 4 temas
        static readonly DateTime Epoch = new DateTime(2020, 1, 1);

        public static AgeGroup Age
        {
            get => (AgeGroup)PlayerPrefs.GetInt(KeyAge, 0);
            set { PlayerPrefs.SetInt(KeyAge, (int)value); PlayerPrefs.Save(); }
        }

        public static int Stars
        {
            get => PlayerPrefs.GetInt(KeyStars, 120); // arranca en 120 (valor base del diagrama)
            set { PlayerPrefs.SetInt(KeyStars, value); PlayerPrefs.Save(); }
        }

        public static bool Muted
        {
            get => PlayerPrefs.GetInt(KeyMuted, 0) == 1;
            set { PlayerPrefs.SetInt(KeyMuted, value ? 1 : 0); PlayerPrefs.Save(); }
        }

        public static bool IsTopicSeen(int book, int topic)
            => (PlayerPrefs.GetInt(KeyTopicsMask, 0) & (1 << (book * 4 + topic))) != 0;

        /// <summary>Marca un tema como explorado la primera vez y otorga estrellas.
        /// Devuelve true si era la primera vez (para avisar al usuario).</summary>
        public static bool MarkTopicSeen(int book, int topic)
        {
            int mask = PlayerPrefs.GetInt(KeyTopicsMask, 0);
            int flag = 1 << (book * 4 + topic);
            if ((mask & flag) != 0) return false;
            PlayerPrefs.SetInt(KeyTopicsMask, mask | flag);
            Stars += 10;
            return true;
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

        // Nivel: sube cada 4 temas explorados (uno por libro completo). Máximo 4 (los 3 libros).
        public static int Level => Mathf.Clamp(1 + TopicsSeenCount() / 4, 1, 4);

        // Progreso dentro del nivel actual (0..1); 1 si ya se completó todo.
        public static float LevelProgress
        {
            get
            {
                int seen = TopicsSeenCount();
                return seen >= TopicsTotal ? 1f : (seen % 4) / 4f;
            }
        }

        public static int Streak => PlayerPrefs.GetInt(KeyStreak, 0);

        /// <summary>Actualiza la racha de días consecutivos. Llamar una vez al iniciar la app.</summary>
        public static void TouchDailyStreak()
        {
            int today = (int)(DateTime.UtcNow.Date - Epoch).TotalDays;
            int last = PlayerPrefs.GetInt(KeyLastDay, -1);
            if (last == today) return; // ya contado hoy
            int streak = last == today - 1 ? PlayerPrefs.GetInt(KeyStreak, 0) + 1 : 1;
            PlayerPrefs.SetInt(KeyStreak, streak);
            PlayerPrefs.SetInt(KeyLastDay, today);
            PlayerPrefs.Save();
        }

        /// <summary>Reinicia estrellas, temas vistos y racha (no toca la edad elegida).</summary>
        public static void ResetProgress()
        {
            PlayerPrefs.SetInt(KeyStars, 120);
            PlayerPrefs.SetInt(KeyTopicsMask, 0);
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
    }
}

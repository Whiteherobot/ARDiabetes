using UnityEngine;

namespace ARDiabetes
{
    /// <summary>
    /// Estado global de la app que persiste entre pantallas (y entre sesiones vía PlayerPrefs).
    /// Pantallas 1-4: guarda el grupo de edad elegido y los puntos (estrellas).
    /// </summary>
    public static class AppState
    {
        public enum AgeGroup { None = 0, Kids_5_9 = 1, Kids_10_12 = 2, Teens_13_15 = 3 }

        const string KeyAge = "ar_age_group";
        const string KeyStars = "ar_stars";

        public static AgeGroup Age
        {
            get => (AgeGroup)PlayerPrefs.GetInt(KeyAge, 0);
            set { PlayerPrefs.SetInt(KeyAge, (int)value); PlayerPrefs.Save(); }
        }

        public static int Stars
        {
            get => PlayerPrefs.GetInt(KeyStars, 120); // el diagrama arranca en 120
            set { PlayerPrefs.SetInt(KeyStars, value); PlayerPrefs.Save(); }
        }

        // Nivel / progreso / racha (valores del diagrama por ahora)
        public static int Level => 3;
        public static float LevelProgress => 0.75f; // 0..1
        public static int Streak => 5;

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

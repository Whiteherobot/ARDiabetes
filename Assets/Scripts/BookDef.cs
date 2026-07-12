using UnityEngine;

namespace ARDiabetes
{
    /// <summary>Contenido de un libro (Fisiológico/Nutricional/Clínico): temas, modelo 3D y marcadores.</summary>
    public class BookDef
    {
        public string Title;
        public Color Accent;
        public Sprite HeroIcon;
        public GameObject Model;
        public Color ModelTint;
        public string[] TopicTitle;
        public string[] TopicSub;
        public Sprite[] TopicIcon;
        public Texture2D[] Markers;
        public AudioClip[] Narration;

        // Misma info, 3 niveles de lenguaje/detalle ([0]=5-9 años, [1]=10-12, [2]=13-15) — el
        // perfil de edad no es solo cosmético (avatar/saludo): cambia el contenido que se lee.
        public string[] DescKids;
        public string[] DescPre;
        public string[] DescTeens;

        public string Desc(int topic, AppState.AgeGroup age)
        {
            var arr = age == AppState.AgeGroup.Kids_5_9 ? DescKids
                    : age == AppState.AgeGroup.Teens_13_15 ? DescTeens
                    : DescPre; // Kids_10_12 y None (sin perfil) caen al nivel intermedio
            return arr[topic];
        }

        // Quiz del libro (Juegos y Retos): 4 preguntas, una por tema.
        public QuizQuestion[] Quiz;
    }

    /// <summary>Una pregunta de opción múltiple (3 opciones) del quiz de un libro.</summary>
    public class QuizQuestion
    {
        public string Q;
        public string[] Options;
        public int Correct;
    }
}

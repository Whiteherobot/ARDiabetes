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

        // Juegos y Retos del libro: 3 juegos de tipos distintos (no todos son opción múltiple).
        public GameItem[] Quiz;
    }

    public enum GameType { Choice, Matching, MultiSelect }

    /// <summary>
    /// Un juego/pregunta de "Juegos y Retos". Choice cubre opción múltiple, verdadero/falso
    /// y completar-la-frase por igual (Options + Correct); Matching y MultiSelect usan sus
    /// propios campos.
    /// </summary>
    public class GameItem
    {
        public GameType Type = GameType.Choice;
        public string Q;

        // Choice (MC / Verdadero-Falso / Completar frase): hasta 4 opciones.
        public string[] Options;
        public int Correct;

        // Matching: Left[i] se relaciona correctamente con Right[i]. Right se muestra
        // desordenado en pantalla; el emparejamiento correcto se valida por índice original.
        public string[] Left;
        public string[] Right;

        // MultiSelect: Options[] + máscara de cuáles deben quedar seleccionadas.
        public bool[] CorrectMask;
    }
}

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
        public string[] TopicDesc;
        public Sprite[] TopicIcon;
        public Texture2D[] Markers;
        public AudioClip[] Narration;
    }
}

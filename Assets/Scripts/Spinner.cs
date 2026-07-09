using UnityEngine;

namespace ARDiabetes
{
    /// <summary>Gira un objeto sobre su eje Y (para el visor 3D del modelo).</summary>
    public class Spinner : MonoBehaviour
    {
        public float speed = 35f;
        public bool spinning = true;
        void Update()
        {
            if (spinning)
                transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.World);
        }
    }
}

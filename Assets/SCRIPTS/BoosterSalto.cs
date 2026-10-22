using UnityEngine;

public class BoosterSalto : MonoBehaviour
{
    [Header("Configuración del Impulso")]
    public float fuerzaImpulso = 16f; // Fuerza para elevar al personaje
    public AudioClip sonidoBooster;   // Arrastra aquí el efecto de sonido del salto

    private AudioSource audioSource;

    private void Awake()
    {
        // Obtener el componente AudioSource asignado al mismo objeto
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verifica si lo que pisó el cubo es el jugador
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("Jugador"))
        {
            Rigidbody2D rbJugador = collision.gameObject.GetComponent<Rigidbody2D>();
            
            if (rbJugador != null)
            {
                // Resetea la velocidad vertical previa para un impulso limpio
                rbJugador.linearVelocity = new Vector2(rbJugador.linearVelocity.x, 0f);
                rbJugador.AddForce(Vector2.up * fuerzaImpulso, ForceMode2D.Impulse);

                // Reproducir el sonido del booster
                if (audioSource != null && sonidoBooster != null)
                {
                    audioSource.PlayOneShot(sonidoBooster);
                }
            }
        }
    }
}
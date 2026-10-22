using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float speed = 5f;
    public float jumpForce = 8f;
    public float rollBoost = 12f;
    public float duracionRoll = 0.4f;

    [Header("Límite de Caída al Vacío")]
    public float limiteCaidaY = -10f; // Si cae por debajo de este valor Y, muere

    [Header("Interfaz y Contador")]
    public TextMeshProUGUI textoMonedas;
    public GameObject textoVictoria;
    public int totalMonedasEnNivel = 5;

    [Header("Sistema de Vidas y Game Over")]
    public int vidas = 3;
    public Image[] corazonesUI;        // Arrastra los 3 corazones aquí
    public GameObject panelGameOver;  // Arrastra el PanelGameOver aquí

    [Header("Efectos de Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoMoneda;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoGameOver;   // Arrastra la canción de Game Over aquí

    private int contadorMonedas = 0;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool enElAire = false;
    private bool estaRodando = false;
    private float tiempoRodada = 0f;

    private bool estaApareciendo = true;
    public float duracionAparicion = 1.0f; 
    private float tiempoAparicion = 0f;

    private Vector3 posicionInicial;
    private bool estaMuerto = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        posicionInicial = transform.position;
        estaApareciendo = true;
        tiempoAparicion = duracionAparicion;

        if (textoVictoria != null) textoVictoria.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);

        ActualizarTextoMonedas();
    }

    void Update()
    {
        // Si está muerto, bloquea las acciones
        if (estaMuerto) return;

        // Detección de Caída al Vacío (Muerte automática si cae del mapa)
        if (transform.position.y < limiteCaidaY && !estaMuerto)
        {
            StartCoroutine(ProcesoMuerte());
            return;
        }

        // 0. Bloqueo durante la animación de aparición
        if (estaApareciendo)
        {
            tiempoAparicion -= Time.deltaTime;
            if (tiempoAparicion <= 0f) estaApareciendo = false;
            return;
        }

        // 1. Movimiento Horizontal (A/D o Flechas)
        float moveInput = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        }

        bool estaEnSuelo = rb != null && Mathf.Abs(rb.linearVelocity.y) < 0.05f;

        // 2. Rodar con tecla "C"
        if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame && estaEnSuelo && !estaRodando)
        {
            estaRodando = true;
            tiempoRodada = duracionRoll;
            if (animator != null) animator.SetTrigger("Roll");
        }

        float velocidadActual = speed;
        if (estaRodando)
        {
            velocidadActual = rollBoost;
            tiempoRodada -= Time.deltaTime;
            if (tiempoRodada <= 0f) estaRodando = false;

            if (moveInput == 0f && spriteRenderer != null)
            {
                moveInput = spriteRenderer.flipX ? -1f : 1f;
            }
        }

        transform.Translate(new Vector3(moveInput * velocidadActual * Time.deltaTime, 0, 0));

        // 3. Animación de movimiento
        if (animator != null && !estaRodando) animator.SetFloat("Speed", Mathf.Abs(moveInput));

        // 4. Voltear el personaje
        if (spriteRenderer != null && moveInput != 0f)
        {
            if (moveInput > 0) spriteRenderer.flipX = false;
            else if (moveInput < 0) spriteRenderer.flipX = true;
        }

        // 5. Salto con Espacio
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && estaEnSuelo && !estaRodando)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            enElAire = true;
        }

        // 6. Caída y Aterrizaje
        if (rb != null && animator != null)
        {
            bool estaCayendo = rb.linearVelocity.y < -0.2f;
            animator.SetBool("IsFalling", estaCayendo);

            if (!estaEnSuelo) enElAire = true;
            else if (enElAire && estaEnSuelo)
            {
                animator.SetTrigger("Land");
                enElAire = false;
            }
        }
    }

    // 7. Detección de Colisiones (Monedas y Obstáculos)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (estaMuerto) return;

        // A) Recolección de Monedas
        if (collision.CompareTag("Coin"))
        {
            contadorMonedas++;
            ActualizarTextoMonedas();

            if (audioSource != null && sonidoMoneda != null)
            {
                audioSource.PlayOneShot(sonidoMoneda);
            }

            if (contadorMonedas >= totalMonedasEnNivel)
            {
                if (textoVictoria != null) textoVictoria.SetActive(true);
                if (audioSource != null && sonidoVictoria != null)
                {
                    audioSource.PlayOneShot(sonidoVictoria);
                }
            }

            Destroy(collision.gameObject);
        }

        // B) Pinchos u Obstáculos (Pérdida de Vida / Muerte)
        if (collision.CompareTag("Obstacle"))
        {
            StartCoroutine(ProcesoMuerte());
        }
    }

    IEnumerator ProcesoMuerte()
    {
        estaMuerto = true;
        vidas--;

        // Desactivar el corazón que corresponde
        if (vidas >= 0 && vidas < corazonesUI.Length)
        {
            corazonesUI[vidas].gameObject.SetActive(false);
        }

        // Frenar al personaje y reproducir la animación de morir
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger("player_die");

        // Esperar 0.3 segundos para una animación de respawn rápida
        yield return new WaitForSeconds(0.3f);

        if (vidas > 0)
        {
            // AÚN TIENE VIDAS: Vuelve al inicio
            transform.position = posicionInicial;

            // Resetear el trigger de muerte para no entrar en bucle
            if (animator != null) animator.ResetTrigger("player_die");

            estaApareciendo = true;
            tiempoAparicion = duracionAparicion;
            estaMuerto = false;
        }
        else
        {
            // PERDIÓ LAS 3 VIDAS: Game Over
            if (panelGameOver != null) panelGameOver.SetActive(true);

            // 1. Detener toda la música de fondo activa de la escena
            AudioSource[] todosLosAudios = Object.FindObjectsOfType<AudioSource>();
            foreach (AudioSource audio in todosLosAudios)
            {
                audio.Stop();
            }

            // 2. Reproducir la canción de Game Over
            if (audioSource != null && sonidoGameOver != null)
            {
                audioSource.PlayOneShot(sonidoGameOver);
            }

            // Esperar 3.5 segundos en la pantalla de Game Over antes de reiniciar el nivel completo
            yield return new WaitForSeconds(3.5f);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void ActualizarTextoMonedas()
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "Monedas: " + contadorMonedas + " / " + totalMonedasEnNivel;
        }
    }
}
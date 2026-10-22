using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoriaMenu : MonoBehaviour
{
    // Función para el botón de repetir el nivel actual
    public void ReiniciarNivel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Función para el botón de avanzar
    public void SiguienteNivel()
    {
        Debug.Log("Próximo nivel para el Avance 2");
        // Cuando tengas el Nivel 2, descomentas la siguiente línea:
        // SceneManager.LoadScene("Nivel2");
    }
}
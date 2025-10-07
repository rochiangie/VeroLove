using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void IrAClientes()
    {
        SceneManager.LoadScene("ClientesScene");
    }

    public void IrAServicios()
    {
        SceneManager.LoadScene("ServiciosScene");
    }

    public void IrATurnos()
    {
        SceneManager.LoadScene("TurnosScene");
    }

    public void IrAHistorial()
    {
        SceneManager.LoadScene("HistorialScene");
    }

    public void CerrarSesion()
    {
        PlayerPrefs.DeleteKey("accessToken");
        SceneManager.LoadScene("LoginScene");
    }
}

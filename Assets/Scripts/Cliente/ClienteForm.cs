using UnityEngine;
using TMPro;

public class ClienteForm : MonoBehaviour
{
    public TMP_InputField inputNombre;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputEmail;
    public ClienteUI clienteUI;       // Arrastrar en el Inspector
    public GameObject panelFormulario;

    public void GuardarCliente()
    {
        if (clienteUI == null)
        {
            Debug.LogError("[ClienteForm] Falta referencia a ClienteUI");
            return;
        }

        string n = (inputNombre?.text ?? "").Trim();
        string t = (inputTelefono?.text ?? "").Trim();
        string e = (inputEmail?.text ?? "").Trim();

        if (string.IsNullOrEmpty(n))
        {
            Debug.LogWarning("[ClienteForm] El nombre es requerido");
            return;
        }

        var nuevoCliente = new Cliente(n, t, e);
        clienteUI.AgregarCliente(nuevoCliente);

        // Limpiar formulario
        if (inputNombre) inputNombre.text = "";
        if (inputTelefono) inputTelefono.text = "";
        if (inputEmail) inputEmail.text = "";

        // Ocultar el panel si existe
        if (panelFormulario) panelFormulario.SetActive(false);
    }

    public void MostrarFormulario()
    {
        if (panelFormulario) panelFormulario.SetActive(true);
    }
}

using UnityEngine;
using TMPro;
using SQLite;

public class ClienteForm : MonoBehaviour
{
    // Dependencias de UI
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellido; // Asegúrate de tener este campo en la UI
    public TMP_InputField inputTelefono;
    public TMP_InputField inputMail; // Corresponde al campo Email en el UI/DB
    public ClienteUI clienteUI; // Referencia para recargar la lista visual
    public GameObject panelFormulario;

    // Conexión a la DB
    private SQLiteConnection db;

    void Awake()
    {
        // Obtener la conexión a la base de datos usando el Singleton
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
        else
        {
            Debug.LogError("DBConnection no está inicializado. No se puede guardar clientes.");
        }
    }

    public void GuardarCliente()
    {
        if (db == null)
        {
            Debug.LogError("Conexión a DB no disponible.");
            return;
        }

        // 1. Recolección y saneamiento de datos
        string nombre = (inputNombre?.text ?? "").Trim();
        string apellido = (inputApellido?.text ?? "").Trim();
        string telefono = (inputTelefono?.text ?? "").Trim();
        string mail = (inputMail?.text ?? "").Trim();

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            // Error que viste: [ClienteForm] Nombre y Apellido son requeridos.
            Debug.LogWarning("[ClienteForm] Nombre y Apellido son requeridos.");
            return;
        }

        // 2. Instancia el Cliente usando inicializador de objeto
        // ESTO RESUELVE el error CS1729
        Cliente nuevoCliente = new Cliente
        {
            Nombre = nombre,
            Apellido = apellido,
            Telefono = telefono,
            Mail = mail
        };

        // 3. Guarda el cliente en la base de datos SQLite
        try
        {
            db.Insert(nuevoCliente);
            Debug.Log($"✅ Cliente '{nombre} {apellido}' guardado con ID: {nuevoCliente.Id}");

            // 4. Recargar la lista visual de clientes
            if (clienteUI != null)
            {
                clienteUI.RecargarListaClientes();
            }
        }
        catch (System.Exception e)
        {
            // Útil para detectar correos duplicados si el campo [Unique] está activado
            Debug.LogError($"❌ Error al guardar cliente: {e.Message}");
        }

        // 5. Limpiar y Ocultar Formulario
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        if (inputNombre) inputNombre.text = "";
        if (inputApellido) inputApellido.text = "";
        if (inputTelefono) inputTelefono.text = "";
        if (inputMail) inputMail.text = "";

        if (panelFormulario) panelFormulario.SetActive(false);
    }

    public void MostrarFormulario()
    {
        if (panelFormulario) panelFormulario.SetActive(true);
    }

    public void CancelarFormulario()
    {
        LimpiarFormulario();
    }
}
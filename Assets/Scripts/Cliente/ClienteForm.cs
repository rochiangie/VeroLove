using UnityEngine;
using TMPro;
using SQLite;

// Añadimos el alias necesario para la conexión a SQLite
using SQLiteConnectionCustom = SQLite.SQLiteConnection;

public class ClienteForm : MonoBehaviour
{
    // Dependencias de UI
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellido; // Asumo que tienes este campo para el modelo completo
    public TMP_InputField inputTelefono;
    public TMP_InputField inputMail;
    public GameObject panelFormulario;

    // Referencia a la conexión de la DB
    private DBConnection dbConnection;
    private SQLiteConnectionCustom db;

    void Awake()
    {
        // Obtener la conexión a la base de datos al inicio
        dbConnection = FindObjectOfType<DBConnection>();
        if (dbConnection == null)
        {
            Debug.LogError("DBConnection no encontrado. Asegúrate de que está en la escena.");
            return;
        }
        db = dbConnection.GetConnection();
    }

    public void GuardarCliente()
    {
        // 1. Recolección y saneamiento de datos
        string nombre = (inputNombre?.text ?? "").Trim();
        string apellido = (inputApellido?.text ?? "").Trim(); // Asumiendo que existe en la UI
        string telefono = (inputTelefono?.text ?? "").Trim();
        string mail = (inputMail?.text ?? "").Trim();

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            Debug.LogWarning("[ClienteForm] Nombre y Apellido son requeridos.");
            // Aquí deberías mostrar una alerta en la UI
            return;
        }

        // 2. CORRECCIÓN: Instancia el Cliente usando inicializador de objeto
        // Esto resuelve el error CS1729
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
        }
        catch (System.Exception e)
        {
            // Error común: Correo duplicado si pusiste [Unique]
            Debug.LogError($"❌ Error al guardar cliente: {e.Message}");
            return;
        }

        // 4. Limpiar y Ocultar Formulario
        LimpiarFormulario();

        // Si tienes una lista de clientes en el ClienteUI, necesitarás un método para refrescarla.
        // Asumiendo que ClienteUI tiene ahora un método de recarga:
        // clienteUI.RecargarListaClientes(); 
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
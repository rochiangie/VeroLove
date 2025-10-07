using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.Linq;

// Añadimos el alias necesario para la conexión a SQLite
using SQLiteConnectionCustom = SQLite.SQLiteConnection;

public class ClienteUI : MonoBehaviour
{
    [Header("UI")]
    public Transform contenedorClientes;      // Content del ScrollView
    public GameObject prefabClienteItem;      // Prefab con hijos: Nombre, Telefono, Email (TMP_Text)
    public TMP_InputField inputBusqueda;      // Campo de búsqueda
    public Button botonBuscar;                // (Opcional) botón Buscar

    // Referencia a la conexión de la DB
    private DBConnection dbConnection;
    private SQLiteConnectionCustom db;
    private List<Cliente> listaClientes = new List<Cliente>();

    void Awake()
    {
        // Obtener la conexión a la base de datos
        dbConnection = FindObjectOfType<DBConnection>();
        if (dbConnection == null)
        {
            Debug.LogError("DBConnection no encontrado. Asegúrate de que está en la escena.");
            return;
        }
        db = dbConnection.GetConnection();
    }

    void Start()
    {
        CargarClientesDesdeDB();
        MostrarClientes();

        // Filtro en vivo
        if (inputBusqueda != null)
            inputBusqueda.onValueChanged.AddListener(_ => BuscarClienteVisual());

        // Botón Buscar (si existe en la escena)
        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarClienteVisual);
    }

    // --- Lógica de Carga de Datos (SQLite) ---

    public void CargarClientesDesdeDB()
    {
        try
        {
            // Cargar todos los clientes de la tabla
            listaClientes = db.Table<Cliente>().ToList();
            listaClientes = listaClientes.OrderBy(c => c.Apellido).ToList(); // Opcional: ordenar por apellido
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar clientes desde DB: {e.Message}");
            listaClientes = new List<Cliente>(); // Inicializar vacía en caso de error
        }
        MostrarClientes();
    }

    // El método AgregarCliente ya no se necesita, el Guardar ocurre en ClienteForm.cs
    // Ahora solo necesitamos recargar la lista si ClienteForm guarda algo nuevo.
    public void RecargarListaClientes()
    {
        CargarClientesDesdeDB();
    }

    // --- Lógica de Interfaz Visual ---

    private void MostrarClientes()
    {
        foreach (Transform hijo in contenedorClientes)
            Destroy(hijo.gameObject);

        foreach (Cliente cliente in listaClientes)
            CrearItemVisual(cliente);
    }

    private void CrearItemVisual(Cliente cliente)
    {
        GameObject item = Instantiate(prefabClienteItem, contenedorClientes);

        // CORRECCIÓN: Usamos la capitalización correcta (PascalCase) de las propiedades
        // Esto resuelve los errores CS1061

        // Aquí podrías usar una subclase para manejar el Item, pero por simplicidad:

        TMP_Text nombreTMP = item.transform.Find("Nombre")?.GetComponent<TMP_Text>();
        TMP_Text telefonoTMP = item.transform.Find("Telefono")?.GetComponent<TMP_Text>();
        TMP_Text emailTMP = item.transform.Find("Email")?.GetComponent<TMP_Text>();

        // Usamos las propiedades correctas: Nombre, Apellido, Telefono, Mail (del modelo Cliente.cs)
        if (nombreTMP != null) nombreTMP.text = $"{cliente.Nombre} {cliente.Apellido}"; // Concatenamos Nombre y Apellido
        if (telefonoTMP != null) telefonoTMP.text = cliente.Telefono;
        if (emailTMP != null) emailTMP.text = cliente.Mail;

        // Opcional: Guardar el ID en el item para usarlo al seleccionarlo
        // item.GetComponent<ClienteItem>()?.SetClienteId(cliente.Id); 
    }

    // --- Lógica de Búsqueda ---

    // Cambiamos el nombre para que refleje que solo busca en la lista visual actual
    public void BuscarClienteVisual()
    {
        string filtro = (inputBusqueda != null ? inputBusqueda.text : "").Trim().ToLower();

        foreach (Transform hijo in contenedorClientes)
        {
            // Suponemos que el texto visible del cliente está en el hijo llamado "Nombre"
            TMP_Text textoNombreCompleto = hijo.Find("Nombre")?.GetComponent<TMP_Text>();
            string nombreCompleto = (textoNombreCompleto != null ? textoNombreCompleto.text : "").ToLower();

            bool coincide = nombreCompleto.Contains(filtro);
            hijo.gameObject.SetActive(coincide);
        }
    }

    // Método para ser usado por el TurnoManager
    public List<Cliente> ObtenerClientes() => listaClientes;
}
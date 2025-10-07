using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SQLite;
using System.Linq;


public class ClienteUI : MonoBehaviour
{
    [Header("UI")]
    public Transform contenedorClientes;      // Content del ScrollView
    public GameObject prefabClienteItem;      // Prefab del ítem de la lista
    public TMP_InputField inputBusqueda;      // Campo de búsqueda
    public Button botonBuscar;                // Botón Buscar (opcional)

    // Conexión a la DB
    private SQLiteConnection db;
    private List<Cliente> listaClientes = new List<Cliente>();

    void Awake()
    {
        // 1. Obtener la conexión a la base de datos usando el Singleton
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
        else
        {
            Debug.LogError("DBConnection no está inicializado. No se puede cargar clientes.");
        }
    }

    void Start()
    {
        CargarClientesDesdeDB(); // Carga inicial

        // Configuración de listeners
        if (inputBusqueda != null)
            // Usamos BuscarClienteVisual para filtrar en la lista visual cargada
            inputBusqueda.onValueChanged.AddListener(_ => BuscarClienteVisual());

        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarClienteVisual);
    }

    // --- Lógica de Carga y Recarga de Datos ---

    // Este método es llamado por ClienteForm.cs después de guardar un nuevo cliente
    public void RecargarListaClientes()
    {
        CargarClientesDesdeDB();
    }

    private void CargarClientesDesdeDB()
    {
        if (db == null) return;

        try
        {
            // Cargar todos los clientes de la tabla, ordenados por Apellido
            listaClientes = db.Table<Cliente>()
                              .OrderBy(c => c.Apellido)
                              .ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar clientes desde DB: {e.Message}");
            listaClientes = new List<Cliente>();
        }
        MostrarClientes();
    }

    // --- Lógica de Interfaz Visual ---

    private void MostrarClientes()
    {
        // Limpiar contenedor
        foreach (Transform hijo in contenedorClientes)
            Destroy(hijo.gameObject);

        // Crear ítems para cada cliente
        foreach (Cliente cliente in listaClientes)
            CrearItemVisual(cliente);
    }

    private void CrearItemVisual(Cliente cliente)
    {
        GameObject item = Instantiate(prefabClienteItem, contenedorClientes);

        // CORRECCIÓN CS1061: Usamos la capitalización correcta (PascalCase) de las propiedades
        // El error CS1061 se resuelve aquí

        TMP_Text nombreTMP = item.transform.Find("Nombre")?.GetComponent<TMP_Text>();
        TMP_Text telefonoTMP = item.transform.Find("Telefono")?.GetComponent<TMP_Text>();
        // Usamos Mail en lugar del antiguo Email si seguiste el modelo
        TMP_Text emailTMP = item.transform.Find("Email")?.GetComponent<TMP_Text>();

        // Asignamos el texto usando las propiedades correctas del modelo Cliente.cs
        if (nombreTMP != null) nombreTMP.text = $"{cliente.Nombre} {cliente.Apellido}";
        if (telefonoTMP != null) telefonoTMP.text = cliente.Telefono;
        if (emailTMP != null) emailTMP.text = cliente.Mail; // Usamos Mail (corregido)
    }

    // --- Lógica de Búsqueda ---

    public void BuscarClienteVisual()
    {
        string filtro = (inputBusqueda != null ? inputBusqueda.text : "").Trim().ToLower();

        // Filtra los ítems visuales que ya están cargados
        foreach (Transform hijo in contenedorClientes)
        {
            // Asume que el texto principal para buscar está en el hijo llamado "Nombre"
            TMP_Text textoNombreCompleto = hijo.Find("Nombre")?.GetComponent<TMP_Text>();
            string nombreCompleto = (textoNombreCompleto != null ? textoNombreCompleto.text : "").ToLower();

            bool coincide = nombreCompleto.Contains(filtro);
            hijo.gameObject.SetActive(coincide);
        }
    }

    // Método para ser usado por otros Managers (ej. TurnoManager para poblar el dropdown)
    public List<Cliente> ObtenerClientes() => listaClientes;
}
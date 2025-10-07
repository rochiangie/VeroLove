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
    public GameObject prefabClienteItem;      // Prefab del �tem de la lista
    public TMP_InputField inputBusqueda;      // Campo de b�squeda
    public Button botonBuscar;                // Bot�n Buscar (opcional)

    // Conexi�n a la DB
    private SQLiteConnection db;
    private List<Cliente> listaClientes = new List<Cliente>();

    void Awake()
    {
        // 1. Obtener la conexi�n a la base de datos usando el Singleton
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
        else
        {
            Debug.LogError("DBConnection no est� inicializado. No se puede cargar clientes.");
        }
    }

    void Start()
    {
        CargarClientesDesdeDB(); // Carga inicial

        // Configuraci�n de listeners
        if (inputBusqueda != null)
            // Usamos BuscarClienteVisual para filtrar en la lista visual cargada
            inputBusqueda.onValueChanged.AddListener(_ => BuscarClienteVisual());

        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarClienteVisual);
    }

    // --- L�gica de Carga y Recarga de Datos ---

    // Este m�todo es llamado por ClienteForm.cs despu�s de guardar un nuevo cliente
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

    // --- L�gica de Interfaz Visual ---

    private void MostrarClientes()
    {
        // Limpiar contenedor
        foreach (Transform hijo in contenedorClientes)
            Destroy(hijo.gameObject);

        // Crear �tems para cada cliente
        foreach (Cliente cliente in listaClientes)
            CrearItemVisual(cliente);
    }

    private void CrearItemVisual(Cliente cliente)
    {
        GameObject item = Instantiate(prefabClienteItem, contenedorClientes);

        // CORRECCI�N CS1061: Usamos la capitalizaci�n correcta (PascalCase) de las propiedades
        // El error CS1061 se resuelve aqu�

        TMP_Text nombreTMP = item.transform.Find("Nombre")?.GetComponent<TMP_Text>();
        TMP_Text telefonoTMP = item.transform.Find("Telefono")?.GetComponent<TMP_Text>();
        // Usamos Mail en lugar del antiguo Email si seguiste el modelo
        TMP_Text emailTMP = item.transform.Find("Email")?.GetComponent<TMP_Text>();

        // Asignamos el texto usando las propiedades correctas del modelo Cliente.cs
        if (nombreTMP != null) nombreTMP.text = $"{cliente.Nombre} {cliente.Apellido}";
        if (telefonoTMP != null) telefonoTMP.text = cliente.Telefono;
        if (emailTMP != null) emailTMP.text = cliente.Mail; // Usamos Mail (corregido)
    }

    // --- L�gica de B�squeda ---

    public void BuscarClienteVisual()
    {
        string filtro = (inputBusqueda != null ? inputBusqueda.text : "").Trim().ToLower();

        // Filtra los �tems visuales que ya est�n cargados
        foreach (Transform hijo in contenedorClientes)
        {
            // Asume que el texto principal para buscar est� en el hijo llamado "Nombre"
            TMP_Text textoNombreCompleto = hijo.Find("Nombre")?.GetComponent<TMP_Text>();
            string nombreCompleto = (textoNombreCompleto != null ? textoNombreCompleto.text : "").ToLower();

            bool coincide = nombreCompleto.Contains(filtro);
            hijo.gameObject.SetActive(coincide);
        }
    }

    // M�todo para ser usado por otros Managers (ej. TurnoManager para poblar el dropdown)
    public List<Cliente> ObtenerClientes() => listaClientes;
}
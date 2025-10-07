using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Microsoft.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;


public class ClienteUI : MonoBehaviour
{
    [Header("UI")]
    public Transform contenedorClientes;
    public GameObject prefabClienteItem;
    public TMP_InputField inputBusqueda;
    public Button botonBuscar;

    private SqliteConnection db;
    private List<Cliente> listaClientes = new List<Cliente>();

    void Awake()
    {
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
    }

    void Start()
    {
        CargarClientesDesdeDB();

        if (inputBusqueda != null)
            inputBusqueda.onValueChanged.AddListener(_ => BuscarClienteVisual());

        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarClienteVisual);
    }

    // --- Lógica de Carga y Recarga de Datos ---

    public void RecargarListaClientes()
    {
        CargarClientesDesdeDB();
    }

    private void CargarClientesDesdeDB()
    {
        if (db == null) return;
        listaClientes.Clear();

        try
        {
            db.Open();

            using (var command = db.CreateCommand())
            {
                command.CommandText = "SELECT Id, Nombre, Apellido, Mail, Telefono FROM Cliente ORDER BY Apellido";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        listaClientes.Add(new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Apellido = reader.GetString(2),
                            Mail = reader.GetString(3),
                            Telefono = reader.GetString(4)
                        });
                    }
                }
            }

            db.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar clientes desde DB: {e.Message}");
        }
        finally
        {
            if (db.State == System.Data.ConnectionState.Open) db.Close();
        }
        MostrarClientes();
    }

    // --- Lógica de Interfaz Visual y Búsqueda (Usa el mismo código de la última versión) ---
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

        TMP_Text nombreTMP = item.transform.Find("Nombre")?.GetComponent<TMP_Text>();
        TMP_Text telefonoTMP = item.transform.Find("Telefono")?.GetComponent<TMP_Text>();
        TMP_Text emailTMP = item.transform.Find("Email")?.GetComponent<TMP_Text>();

        if (nombreTMP != null) nombreTMP.text = $"{cliente.Nombre} {cliente.Apellido}";
        if (telefonoTMP != null) telefonoTMP.text = cliente.Telefono;
        if (emailTMP != null) emailTMP.text = cliente.Mail;
    }

    public void BuscarClienteVisual()
    {
        string filtro = (inputBusqueda != null ? inputBusqueda.text : "").Trim().ToLower();

        foreach (Transform hijo in contenedorClientes)
        {
            TMP_Text textoNombreCompleto = hijo.Find("Nombre")?.GetComponent<TMP_Text>();
            string nombreCompleto = (textoNombreCompleto != null ? textoNombreCompleto.text : "").ToLower();

            bool coincide = nombreCompleto.Contains(filtro);
            hijo.gameObject.SetActive(coincide);
        }
    }

    public List<Cliente> ObtenerClientes() => listaClientes;
}
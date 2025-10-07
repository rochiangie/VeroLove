using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClienteUI : MonoBehaviour
{
    [Header("UI")]
    public Transform contenedorClientes;      // Content del ScrollView
    public GameObject prefabClienteItem;      // Prefab con hijos: Nombre, Telefono, Email (TMP_Text)
    public TMP_InputField inputBusqueda;      // Campo de búsqueda
    public Button botonBuscar;                // (Opcional) botón Buscar

    private List<Cliente> listaClientes = new List<Cliente>();

    void Start()
    {
        listaClientes = ClienteStorage.CargarClientes();
        MostrarClientes();

        // Filtro en vivo
        if (inputBusqueda != null)
            inputBusqueda.onValueChanged.AddListener(_ => BuscarCliente());

        // Botón Buscar (si existe en la escena)
        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(BuscarCliente);
    }

    public void AgregarCliente(Cliente cliente)
    {
        if (cliente == null) return;
        listaClientes.Add(cliente);
        ClienteStorage.GuardarClientes(listaClientes);
        // Redibujar para mantener orden/consistencia
        MostrarClientes();
        // Reaplicar filtro si hay texto
        BuscarCliente();
    }

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

        // Tus nombres de hijos en el prefab:
        // "Nombre", "Telefono", "Email"
        TMP_Text nombreTMP = item.transform.Find("Nombre")?.GetComponent<TMP_Text>();
        TMP_Text telefonoTMP = item.transform.Find("Telefono")?.GetComponent<TMP_Text>();
        TMP_Text emailTMP = item.transform.Find("Email")?.GetComponent<TMP_Text>();

        if (nombreTMP != null) nombreTMP.text = cliente.nombre;
        if (telefonoTMP != null) telefonoTMP.text = cliente.telefono;
        if (emailTMP != null) emailTMP.text = cliente.email;
    }

    public void BuscarCliente()
    {
        string filtro = (inputBusqueda != null ? inputBusqueda.text : "").Trim().ToLower();

        // Si no hay filtro, mostrar todo
        if (string.IsNullOrEmpty(filtro))
        {
            foreach (Transform hijo in contenedorClientes)
                hijo.gameObject.SetActive(true);
            return;
        }

        // Buscar por el hijo "Nombre" (coincide con el que usamos al crear el ítem)
        foreach (Transform hijo in contenedorClientes)
        {
            TMP_Text textoNombre = hijo.Find("Nombre")?.GetComponent<TMP_Text>();
            string nombre = (textoNombre != null ? textoNombre.text : "").ToLower();
            bool coincide = nombre.Contains(filtro);
            hijo.gameObject.SetActive(coincide);
        }
    }

    public List<Cliente> ObtenerClientes() => listaClientes;
}

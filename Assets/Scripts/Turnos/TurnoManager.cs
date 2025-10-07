using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SQLite;
using System.Linq;

// Nota: Si tu plugin Simple Database utiliza un namespace diferente, cambia 'using SQLite;' 
// por el que te corresponda (ej. using SimpleDatabase;). 

public class TurnoManager : MonoBehaviour
{
    // Dependencias de UI
    public GameObject turnoItemPrefab;
    public Transform contentContainer;

    [Header("Formulario de Turnos")]
    public TMP_InputField inputFecha;
    public TMP_InputField inputHora;
    public TMP_Dropdown dropdownCliente;
    public TMP_Dropdown dropdownServicio;
    public TMP_Dropdown dropdownProfesional;
    public GameObject panelFormulario;

    // Conexión a la DB
    private SQLiteConnection db;

    void Awake()
    {
        // Usamos el patrón Singleton para obtener la única instancia de la DB
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
        else
        {
            Debug.LogError("DBConnection no está inicializado. ¡Asegúrate de que está en la escena y tiene orden de ejecución anticipado!");
        }
    }

    void Start()
    {
        CargarDatosInicialesUI();
        // Mostrar turnos del día actual al iniciar
        MostrarTurnosDelDia(DateTime.Today);
    }

    // ----------------------------------------------------------------------
    // 1. LÓGICA DE CARGA DE DATOS EN LA INTERFAZ
    // ----------------------------------------------------------------------

    void CargarDatosInicialesUI()
    {
        // Limpiar Dropdowns
        dropdownCliente.ClearOptions();
        dropdownServicio.ClearOptions();
        dropdownProfesional.ClearOptions();

        // Cargar Profesionales
        var profesionales = db.Table<Profesional>().ToList();
        List<string> nombresProfesionales = profesionales.Select(p => p.Nombre + " " + p.Apellido).ToList();
        dropdownProfesional.AddOptions(nombresProfesionales);

        // Cargar Servicios
        var servicios = db.Table<Servicio>().ToList();
        List<string> nombresServicios = servicios.Select(s => s.Nombre).ToList();
        dropdownServicio.AddOptions(nombresServicios);

        // Cargar Clientes
        var clientes = db.Table<Cliente>().ToList();
        List<string> nombresClientes = clientes.Select(c => c.Nombre + " " + c.Apellido).ToList();
        dropdownCliente.AddOptions(nombresClientes);

        // Nota: Esta es una simplificación. Para la vida real, es mejor guardar 
        // una lista de los IDs para mapear la selección con el objeto DB.
    }

    public void MostrarTurnosDelDia(DateTime fecha)
    {
        // Limpiar lista visual
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Obtener turnos de la DB para la fecha seleccionada
        var turnosDelDia = db.Table<Turno>()
            .ToList() // Se recomienda usar AsParallel() para consultas grandes, pero .ToList() funciona para la mayoría de apps pequeñas
            .Where(t => t.FechaHoraInicio.Date == fecha.Date)
            .OrderBy(t => t.FechaHoraInicio)
            .ToList();

        // Mostrar en la lista
        foreach (var turno in turnosDelDia)
        {
            // Consultamos los nombres completos para la UI (JOIN manual)
            var cliente = db.Table<Cliente>().Where(c => c.Id == turno.IdCliente).FirstOrDefault();
            var profesional = db.Table<Profesional>().Where(p => p.Id == turno.IdProfesional).FirstOrDefault();
            var servicio = db.Table<Servicio>().Where(s => s.Id == turno.IdServicio).FirstOrDefault();

            if (cliente != null && profesional != null && servicio != null)
            {
                GameObject item = Instantiate(turnoItemPrefab, contentContainer);

                // Asignamos la información en el Item Visual
                string horaInicio = turno.FechaHoraInicio.ToString("HH:mm");

                item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = horaInicio;
                item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{cliente.Nombre} {cliente.Apellido}";
                item.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{servicio.Nombre} ({profesional.Nombre})";
            }
        }
    }

    // ----------------------------------------------------------------------
    // 2. LÓGICA DE AGENDAMIENTO Y VALIDACIÓN
    // ----------------------------------------------------------------------

    public void GuardarTurno()
    {
        // 1. Parseo y Validación Básica de Fecha/Hora
        if (!DateTime.TryParse(inputFecha.text + " " + inputHora.text, out DateTime fechaHoraInicio))
        {
            Debug.LogError("Error: Fecha u hora no válidas.");
            return;
        }

        // 2. Obtener IDs (Simplificado: ¡Deberías usar los IDs reales de la DB!)
        int idProfesional = GetIdProfesionalByName(dropdownProfesional.options[dropdownProfesional.value].text);
        int idServicio = GetIdServicioByName(dropdownServicio.options[dropdownServicio.value].text);
        int idCliente = GetIdClienteByName(dropdownCliente.options[dropdownCliente.value].text);

        if (idProfesional == -1 || idServicio == -1 || idCliente == -1)
        {
            Debug.LogError("Error al obtener ID del Profesional, Servicio o Cliente.");
            return;
        }

        // 3. Obtener la duración del servicio (Necesaria para validar solapamiento)
        var servicioElegido = db.Table<Servicio>().Where(s => s.Id == idServicio).FirstOrDefault();
        if (servicioElegido == null) return;
        int duracionMinutos = servicioElegido.DuracionMinutos;

        // 4. VALIDACIÓN DE SOLAPAMIENTO (La lógica de negocio)
        if (ExisteSolapamiento(idProfesional, fechaHoraInicio, duracionMinutos))
        {
            Debug.LogWarning("❌ ERROR: El profesional NO está disponible en ese horario. ¡Solapamiento detectado!");
            return;
        }

        // 5. Creación y Guardado
        Turno nuevoTurno = new Turno
        {
            IdCliente = idCliente,
            IdServicio = idServicio,
            IdProfesional = idProfesional,
            FechaHoraInicio = fechaHoraInicio,
            Estado = "Confirmado"
        };

        db.Insert(nuevoTurno);

        Debug.Log("✅ Turno agendado exitosamente.");

        MostrarTurnosDelDia(fechaHoraInicio.Date);
        CancelarFormulario();
    }

    private bool ExisteSolapamiento(int idProfesional, DateTime inicioNuevoTurno, int duracionNuevoTurno)
    {
        DateTime finNuevoTurno = inicioNuevoTurno.AddMinutes(duracionNuevoTurno);

        // Consulta todos los turnos confirmados del día para ese profesional
        var turnosExistentes = db.Table<Turno>()
            .ToList()
            .Where(t => t.IdProfesional == idProfesional &&
                        t.FechaHoraInicio.Date == inicioNuevoTurno.Date &&
                        t.Estado == "Confirmado")
            .ToList();

        foreach (var turnoExistente in turnosExistentes)
        {
            // Necesitamos la duración del turno existente para calcular su fin
            var servicioExistente = db.Table<Servicio>().Where(s => s.Id == turnoExistente.IdServicio).FirstOrDefault();
            if (servicioExistente == null) continue;

            DateTime finTurnoExistente = turnoExistente.FechaHoraInicio.AddMinutes(servicioExistente.DuracionMinutos);

            // Regla de Solapamiento:
            // Dos intervalos [A, B) y [C, D) se solapan si A < D Y C < B
            if (inicioNuevoTurno < finTurnoExistente && finNuevoTurno > turnoExistente.FechaHoraInicio)
            {
                return true; // Hay solapamiento
            }
        }
        return false;
    }

    // ----------------------------------------------------------------------
    // 3. MÉTODOS DE AYUDA (Para mapear Dropdown Text a DB ID)
    // ----------------------------------------------------------------------

    // Mapea el texto del Dropdown (Nombre Apellido) al ID del profesional en la DB
    private int GetIdProfesionalByName(string nombreCompleto)
    {
        var partes = nombreCompleto.Split(' ');
        if (partes.Length < 2) return -1;

        string nombre = partes[0];
        string apellido = partes[1]; // Simplificado

        var prof = db.Table<Profesional>()
                     .Where(p => p.Nombre == nombre && p.Apellido == apellido)
                     .FirstOrDefault();

        return prof != null ? prof.Id : -1;
    }

    // Mapea el texto del Dropdown (Nombre del Servicio) al ID del servicio en la DB
    private int GetIdServicioByName(string nombreServicio)
    {
        var serv = db.Table<Servicio>().Where(s => s.Nombre == nombreServicio).FirstOrDefault();
        return serv != null ? serv.Id : -1;
    }

    // Mapea el texto del Dropdown (Nombre Apellido) al ID del cliente en la DB
    private int GetIdClienteByName(string nombreCompleto)
    {
        var partes = nombreCompleto.Split(' ');
        if (partes.Length < 2) return -1;

        string nombre = partes[0];
        string apellido = partes[1];

        var cliente = db.Table<Cliente>()
                        .Where(c => c.Nombre == nombre && c.Apellido == apellido)
                        .FirstOrDefault();

        return cliente != null ? cliente.Id : -1;
    }


    public void AbrirFormulario()
    {
        panelFormulario.SetActive(true);
    }

    public void CancelarFormulario()
    {
        panelFormulario.SetActive(false);
    }
}
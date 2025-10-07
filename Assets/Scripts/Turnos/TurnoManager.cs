using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SQLite;
using System.Linq; // Necesario para LINQ (consultas SQL en C#)

// Alias para evitar el error de ambigüedad
using SQLiteConnectionCustom = SQLite.SQLiteConnection;

public class TurnoManager : MonoBehaviour
{
    // Dependencias de UI
    public GameObject turnoItemPrefab;
    public Transform contentContainer;
    public TMP_InputField inputFecha;
    public TMP_InputField inputHora;
    public TMP_Dropdown dropdownCliente;
    public TMP_Dropdown dropdownServicio;
    public TMP_Dropdown dropdownProfesional; // ¡Añadido! Necesario para la lógica
    public GameObject panelFormulario;

    // Referencia al gestor de la base de datos
    private DBConnection dbConnection;
    private SQLiteConnectionCustom db;

    void Awake()
    {
        // Encontrar la instancia de DBConnection que pusimos en la escena
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
        CargarDatosInicialesUI();
        MostrarTurnosDelDia(DateTime.Today); // Mostrar los turnos de hoy al iniciar
    }

    // --- Métodos de Carga de Datos y UI ---

    void CargarDatosInicialesUI()
    {
        // Cargar Clientes, Servicios y Profesionales en los Dropdowns
        // (La lógica detallada de carga de dropdowns es extensa, se omite aquí 
        // pero requiere usar db.Table<Cliente>().ToList() y poblar las opciones.)

        // EJEMPLO: Carga de Profesionales (CRUCIAL)
        var profesionales = db.Table<Profesional>().ToList();
        dropdownProfesional.ClearOptions();
        List<string> nombresProfesionales = profesionales.Select(p => p.Nombre).ToList();
        dropdownProfesional.AddOptions(nombresProfesionales);
    }

    public void MostrarTurnosDelDia(DateTime fecha)
    {
        // 1. Limpiar lista
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Consulta a la DB para obtener los turnos del día
        // Nota: Tienes que asegurarte que la fecha de la DB se guarde en formato ISO
        var turnosDelDia = db.Table<Turno>()
            .ToList() // Traer todos los turnos (simplificación)
            .Where(t => t.FechaHoraInicio.Date == fecha.Date) // Filtrar por la fecha
            .OrderBy(t => t.FechaHoraInicio)
            .ToList();

        // 3. Mostrar en la lista
        foreach (var turno in turnosDelDia)
        {
            // Necesitarás consultar los nombres de Cliente y Profesional para la UI
            var cliente = db.Table<Cliente>().Where(c => c.Id == turno.IdCliente).FirstOrDefault();
            var profesional = db.Table<Profesional>().Where(p => p.Id == turno.IdProfesional).FirstOrDefault();
            var servicio = db.Table<Servicio>().Where(s => s.Id == turno.IdServicio).FirstOrDefault();

            if (cliente != null && profesional != null && servicio != null)
            {
                GameObject item = Instantiate(turnoItemPrefab, contentContainer);
                // ESTO REEMPLAZA LOS NOMBRES VIEJOS (clienteNombre, servicio, fecha, hora)
                item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{turno.FechaHoraInicio:HH:mm} - {profesional.Nombre}";
                item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{cliente.Nombre} {cliente.Apellido}";
                item.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = servicio.Nombre;
            }
        }
    }

    // --- Lógica de Agendamiento ---

    public void GuardarTurno()
    {
        // 1. Obtener los IDs y datos de los Dropdowns (Necesitas una lógica para guardar los IDs, 
        // pero por ahora, simplificamos la obtención de texto)
        string fechaStr = inputFecha.text;
        string horaStr = inputHora.text;

        if (!DateTime.TryParse(fechaStr + " " + horaStr, out DateTime fechaHoraInicio))
        {
            Debug.LogError("Fecha u hora no válidas.");
            // Aquí iría una alerta al usuario
            return;
        }

        // Necesitas obtener los IDs correctos basados en la selección.
        // Simulamos la obtención de IDs:
        int idProfesionalSeleccionado = GetIdProfesional(dropdownProfesional.options[dropdownProfesional.value].text);
        int idServicioSeleccionado = GetIdServicio(dropdownServicio.options[dropdownServicio.value].text);
        int idClienteSeleccionado = GetIdCliente(dropdownCliente.options[dropdownCliente.value].text); // Suponiendo que guardas el nombre y necesitas el ID

        // Obtener la duración del servicio (CRUCIAL)
        var servicioElegido = db.Table<Servicio>().Where(s => s.Id == idServicioSeleccionado).FirstOrDefault();
        if (servicioElegido == null)
        {
            Debug.LogError("Servicio no encontrado.");
            return;
        }
        int duracionMinutos = servicioElegido.DuracionMinutos;

        // 2. VALIDACIÓN DE SOLAPAMIENTO (La lógica de negocio principal)
        if (ExisteSolapamiento(idProfesionalSeleccionado, fechaHoraInicio, duracionMinutos))
        {
            Debug.LogError("❌ ERROR: El profesional ya tiene un turno agendado en ese horario.");
            // Aquí iría una alerta de UI al usuario
            return;
        }

        // 3. CREAR Y GUARDAR el nuevo Turno
        Turno nuevoTurno = new Turno
        {
            IdCliente = idClienteSeleccionado,
            IdServicio = idServicioSeleccionado,
            IdProfesional = idProfesionalSeleccionado,
            FechaHoraInicio = fechaHoraInicio,
            Estado = "Confirmado" // Estado inicial
        };

        db.Insert(nuevoTurno);

        Debug.Log("✅ Turno agendado exitosamente para el profesional ID: " + idProfesionalSeleccionado);

        MostrarTurnosDelDia(fechaHoraInicio.Date);
        CancelarFormulario();
    }

    // --- Lógica de Validación de Solapamiento ---

    private bool ExisteSolapamiento(int idProfesional, DateTime inicioNuevoTurno, int duracionNuevoTurno)
    {
        DateTime finNuevoTurno = inicioNuevoTurno.AddMinutes(duracionNuevoTurno);

        // Buscar turnos existentes para ese profesional en ese día
        var turnosExistentes = db.Table<Turno>()
            .ToList()
            .Where(t => t.IdProfesional == idProfesional && t.FechaHoraInicio.Date == inicioNuevoTurno.Date)
            .ToList();

        foreach (var turnoExistente in turnosExistentes)
        {
            // Necesitamos la duración del servicio del turno existente
            var servicioExistente = db.Table<Servicio>().Where(s => s.Id == turnoExistente.IdServicio).FirstOrDefault();
            if (servicioExistente == null) continue; // Si no encuentra el servicio, lo salta

            DateTime finTurnoExistente = turnoExistente.FechaHoraInicio.AddMinutes(servicioExistente.DuracionMinutos);

            // Criterio de Solapamiento:
            // Un solapamiento ocurre si:
            // 1. El inicio del nuevo turno está entre el inicio y el fin del existente.
            // O
            // 2. El fin del nuevo turno está entre el inicio y el fin del existente.
            // O
            // 3. El nuevo turno envuelve completamente al existente.

            bool inicioSolapa = inicioNuevoTurno >= turnoExistente.FechaHoraInicio && inicioNuevoTurno < finTurnoExistente;
            bool finSolapa = finNuevoTurno > turnoExistente.FechaHoraInicio && finNuevoTurno <= finTurnoExistente;
            bool envuelve = inicioNuevoTurno <= turnoExistente.FechaHoraInicio && finNuevoTurno >= finTurnoExistente;

            if (inicioSolapa || finSolapa || envuelve)
            {
                return true; // ¡Hay solapamiento!
            }
        }
        return false; // No hay solapamiento, se puede agendar
    }

    // --- Métodos de Ayuda (Debes implementarlos completamente) ---
    // Estos métodos deberían consultar la DB y devolver el ID basado en la selección del Dropdown
    private int GetIdProfesional(string nombreProfesional)
    {
        var prof = db.Table<Profesional>().Where(p => p.Nombre == nombreProfesional).FirstOrDefault();
        return prof != null ? prof.Id : -1;
    }
    private int GetIdServicio(string nombreServicio) { /* Lógica similar a GetIdProfesional */ return 1; }
    private int GetIdCliente(string nombreCliente) { /* Lógica similar a GetIdProfesional */ return 1; }

    public void AbrirFormulario()
    {
        // (Tu código original)
        panelFormulario.SetActive(true);
    }

    public void CancelarFormulario()
    {
        // (Tu código original)
        panelFormulario.SetActive(false);
    }
}
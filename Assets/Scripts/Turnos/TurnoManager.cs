using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.Data.Sqlite;
using System.Linq;
using Unity.VisualScripting.Dependencies.Sqlite;

public class TurnoManager : MonoBehaviour
{
    // ... (UI Dependencies, quedan igual) ...
    public GameObject turnoItemPrefab;
    public Transform contentContainer;
    public TMP_InputField inputFecha;
    public TMP_InputField inputHora;
    public TMP_Dropdown dropdownCliente;
    public TMP_Dropdown dropdownServicio;
    public TMP_Dropdown dropdownProfesional;
    public GameObject panelFormulario;

    private SqliteConnection db;

    void Awake()
    {
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
    }

    void Start()
    {
        // NOTA: Debes implementar CargarDatosInicialesUI para la nueva librería,
        // usando el mismo patrón de ExecuteReader que ClienteUI.cs
        // CargarDatosInicialesUI(); 
        MostrarTurnosDelDia(DateTime.Today);
    }

    // --- Lógica de Agendamiento y Validación ---

    public void GuardarTurno()
    {
        if (db == null) return;

        if (!DateTime.TryParse(inputFecha.text + " " + inputHora.text, out DateTime fechaHoraInicio))
        {
            Debug.LogError("Error: Fecha u hora no válidas.");
            return;
        }

        // **IMPORTANTE:** Estos métodos GetId... deben ser funcionales
        int idProfesional = GetIdProfesionalByName(dropdownProfesional.options[dropdownProfesional.value].text);
        int idServicio = GetIdServicioByName(dropdownServicio.options[dropdownServicio.value].text);
        int idCliente = GetIdClienteByName(dropdownCliente.options[dropdownCliente.value].text);

        int duracionMinutos = GetDuracionServicio(idServicio);
        if (duracionMinutos <= 0) return; // Validación

        if (ExisteSolapamiento(idProfesional, fechaHoraInicio, duracionMinutos))
        {
            Debug.LogWarning("❌ ERROR: El profesional NO está disponible en ese horario.");
            return;
        }

        // 5. Creación y Guardado (Usando comando SQL)
        try
        {
            db.Open();
            using (var command = db.CreateCommand())
            {
                command.CommandText = "INSERT INTO Turno (IdCliente, IdServicio, IdProfesional, FechaHoraInicio, Estado) VALUES (@c, @s, @p, @fecha, @estado)";
                command.Parameters.AddWithValue("@c", idCliente);
                command.Parameters.AddWithValue("@s", idServicio);
                command.Parameters.AddWithValue("@p", idProfesional);
                command.Parameters.AddWithValue("@fecha", fechaHoraInicio.ToString("yyyy-MM-dd HH:mm:ss")); // Formato ISO
                command.Parameters.AddWithValue("@estado", "Confirmado");
                command.ExecuteNonQuery();
            }
            db.Close();

            Debug.Log("✅ Turno agendado exitosamente.");
            MostrarTurnosDelDia(fechaHoraInicio.Date);
            CancelarFormulario();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar turno: {e.Message}");
        }
        finally
        {
            if (db.State == System.Data.ConnectionState.Open) db.Close();
        }
    }

    // --- Lógica de Validación de Solapamiento (Adaptada para comandos SQL) ---
    private bool ExisteSolapamiento(int idProfesional, DateTime inicioNuevoTurno, int duracionNuevoTurno)
    {
        if (db == null) return true;

        DateTime finNuevoTurno = inicioNuevoTurno.AddMinutes(duracionNuevoTurno);
        bool solapamiento = false;

        // Formato para comparación de fechas en la DB
        string inicioNuevoStr = inicioNuevoTurno.ToString("yyyy-MM-dd HH:mm:ss");
        string finNuevoStr = finNuevoTurno.ToString("yyyy-MM-dd HH:mm:ss");
        string fechaDiaStr = inicioNuevoTurno.Date.ToString("yyyy-MM-dd");

        try
        {
            db.Open();
            using (var command = db.CreateCommand())
            {
                // CONSULTA SQL para obtener turnos confirmados del día para ese profesional
                command.CommandText = @"
                    SELECT t.FechaHoraInicio, s.DuracionMinutos 
                    FROM Turno t 
                    JOIN Servicio s ON t.IdServicio = s.Id 
                    WHERE t.IdProfesional = @idp AND t.Estado = 'Confirmado' 
                    AND CAST(strftime('%Y-%m-%d', t.FechaHoraInicio) AS TEXT) = @dia";

                command.Parameters.AddWithValue("@idp", idProfesional);
                command.Parameters.AddWithValue("@dia", fechaDiaStr);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime inicioExistente = DateTime.Parse(reader.GetString(0));
                        int duracionExistente = reader.GetInt32(1);
                        DateTime finExistente = inicioExistente.AddMinutes(duracionExistente);

                        // Regla de Solapamiento: [A, B) y [C, D) se solapan si A < D Y C < B
                        if (inicioNuevoTurno < finExistente && finNuevoTurno > inicioExistente)
                        {
                            solapamiento = true;
                            break;
                        }
                    }
                }
            }
            db.Close();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error de validación SQL: {e.Message}");
            solapamiento = true; // Fallo en la validación por seguridad
        }
        finally
        {
            if (db.State == System.Data.ConnectionState.Open) db.Close();
        }
        return solapamiento;
    }

    // --- Métodos Auxiliares ---
    // NOTA: Implementación básica de búsqueda por nombre. Usa un patrón ExecuteReader similar a ClienteUI.cs
    private int GetIdProfesionalByName(string nombreCompleto) { /* Implementar lógica de búsqueda*/ return 1; }
    private int GetIdServicioByName(string nombreServicio) { /* Implementar lógica de búsqueda*/ return 1; }
    private int GetIdClienteByName(string nombreCliente) { /* Implementar lógica de búsqueda*/ return 1; }
    private int GetDuracionServicio(int idServicio) { /* Implementar lógica de búsqueda*/ return 60; }
    private void CargarDatosInicialesUI() { /* Implementar lógica para llenar dropdowns*/ }
    public void MostrarTurnosDelDia(DateTime fecha) { /* Implementar lógica para mostrar turnos*/ }
    public void AbrirFormulario() { /* Tu código aquí */ }
    public void CancelarFormulario() { /* Tu código aquí */ }
}
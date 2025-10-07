using SQLite;
using System;

public class Turno
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Claves Foráneas (FK). SQLite-net las maneja como campos normales
    // pero la lógica de relación la defines en el código.
    public int IdCliente { get; set; }
    public int IdServicio { get; set; }
    public int IdProfesional { get; set; }

    // Hora exacta de inicio (fecha y hora)
    public DateTime FechaHoraInicio { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } // Ej: "Confirmado", "Cancelado"

    public string Notas { get; set; }

    // Propiedad calculada: No se guarda en la DB, pero es útil en el código.
    // NECESITA ACCESO AL SERVICIO para calcularse correctamente.
    /*
    [Ignore] // Indica a SQLite-net que ignore este campo
    public DateTime FechaHoraFin 
    {
        get {
            // Lógica compleja: necesita buscar el Servicio asociado para obtener DuracionMinutos
            // Se calculará en el Manager para mayor seguridad y simplicidad aquí.
            return FechaHoraInicio.AddMinutes(DuracionMinutos_DEL_SERVICIO);
        }
    }
    */
}
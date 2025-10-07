using SQLite;
using System;

public class Turno
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Claves For�neas (FK). SQLite-net las maneja como campos normales
    // pero la l�gica de relaci�n la defines en el c�digo.
    public int IdCliente { get; set; }
    public int IdServicio { get; set; }
    public int IdProfesional { get; set; }

    // Hora exacta de inicio (fecha y hora)
    public DateTime FechaHoraInicio { get; set; }

    [MaxLength(20)]
    public string Estado { get; set; } // Ej: "Confirmado", "Cancelado"

    public string Notas { get; set; }

    // Propiedad calculada: No se guarda en la DB, pero es �til en el c�digo.
    // NECESITA ACCESO AL SERVICIO para calcularse correctamente.
    /*
    [Ignore] // Indica a SQLite-net que ignore este campo
    public DateTime FechaHoraFin 
    {
        get {
            // L�gica compleja: necesita buscar el Servicio asociado para obtener DuracionMinutos
            // Se calcular� en el Manager para mayor seguridad y simplicidad aqu�.
            return FechaHoraInicio.AddMinutes(DuracionMinutos_DEL_SERVICIO);
        }
    }
    */
}
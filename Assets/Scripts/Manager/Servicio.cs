using SQLite;
using System;

public class Servicio
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Nombre { get; set; }

    public string Descripcion { get; set; }

    // Duración en minutos: CRUCIAL para la lógica de turnos
    public int DuracionMinutos { get; set; }

    // Usamos 'decimal' para precisión en precios
    public decimal Precio { get; set; }
}
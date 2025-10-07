using SQLite;
using System;

public class Servicio
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Nombre { get; set; }

    public string Descripcion { get; set; }

    // Duraci�n en minutos: CRUCIAL para la l�gica de turnos
    public int DuracionMinutos { get; set; }

    // Usamos 'decimal' para precisi�n en precios
    public decimal Precio { get; set; }
}
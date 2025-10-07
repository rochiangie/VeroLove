using SQLite;
using System;

// Indica que esta clase es una tabla en la DB
public class Profesional
{
    // [PrimaryKey] y [AutoIncrement] son atributos de SQLite-net para la PK
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // [MaxLength] es opcional pero útil para la validación
    [MaxLength(100)]
    public string Nombre { get; set; }

    [MaxLength(100)]
    public string Apellido { get; set; }

    [MaxLength(15)]
    public string Telefono { get; set; }

    [MaxLength(50)]
    public string Especialidad { get; set; } // Ejemplo: "Peluquería", "Manicura"
}
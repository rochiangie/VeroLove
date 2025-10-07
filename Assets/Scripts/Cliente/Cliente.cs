using SQLite;
using System;

public class Cliente
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Nombre { get; set; }

    [MaxLength(100)]
    public string Apellido { get; set; }

    [Unique] // Opcional: Asegura que cada cliente tenga un correo único
    [MaxLength(100)]
    public string Mail { get; set; }

    [MaxLength(15)]
    public string Telefono { get; set; }
}
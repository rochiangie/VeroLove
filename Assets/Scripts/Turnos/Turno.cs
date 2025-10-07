using System;
// No requiere using para este script (solo System)

public class Turno
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public int IdServicio { get; set; }
    public int IdProfesional { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public string Estado { get; set; }
    public string Notas { get; set; }
}
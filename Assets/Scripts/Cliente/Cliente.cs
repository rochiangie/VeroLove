[System.Serializable]
public class Cliente
{
    public string nombre;
    public string telefono;
    public string email;

    public Cliente(string nombre, string telefono, string email)
    {
        this.nombre = nombre;
        this.telefono = telefono;
        this.email = email;
    }
}

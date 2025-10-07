using SQLite;
using System.IO;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine; // Para acceder a Application.persistentDataPath

public class DBConnection : MonoBehaviour
{
    private SQLiteConnection db;

    void Start()
    {
        // Ruta donde se guardará el archivo .db en el dispositivo/PC
        string dbPath = Path.Combine(Application.persistentDataPath, "VeroLoveDB.sqlite");
        Debug.Log("Ruta de la DB: " + dbPath);

        try
        {
            // Inicializa la conexión
            db = new SQLiteConnection(dbPath);

            // Crea las tablas si no existen, usando tus clases
            db.CreateTable<Cliente>();
            db.CreateTable<Servicio>();
            db.CreateTable<Profesional>();
            db.CreateTable<Turno>();

            Debug.Log("Conexión a DB y tablas creadas exitosamente.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al conectar o crear tablas: " + e.Message);
        }
    }

    // Método para obtener la conexión y hacer consultas
    public SQLiteConnection GetConnection()
    {
        return db;
    }
}
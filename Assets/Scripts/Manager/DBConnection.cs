using UnityEngine;
using System.IO;

// 1. Definimos un alias para la clase SQLiteConnection
// Esto le dice al compilador: "Cuando veas 'SQLiteConnectionCustom', usa la clase que está en el namespace global."
using SQLite;
using SQLiteConnectionCustom = SQLite.SQLiteConnection;


public class DBConnection : MonoBehaviour
{
    // 2. Usamos el alias para declarar la variable
    private SQLiteConnectionCustom db;

    void Awake()
    {
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Define la ruta donde se guardará el archivo .db
        string dbFileName = "VeroLoveDB.sqlite";
        // Application.persistentDataPath es la ruta segura para guardar datos en el dispositivo
        string dbPath = Path.Combine(Application.persistentDataPath, dbFileName);

        Debug.Log("Intentando conectar/crear DB en: " + dbPath);

        try
        {
            // 3. Usamos el alias para instanciar la conexión
            db = new SQLiteConnectionCustom(dbPath);

            // Creación de Tablas (si no existen)
            db.CreateTable<Cliente>();
            db.CreateTable<Servicio>();
            db.CreateTable<Profesional>();
            db.CreateTable<Turno>();

            Debug.Log("✅ Conexión a DB y tablas verificadas/creadas exitosamente.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR FATAL al inicializar la base de datos: " + e.Message);
        }
    }

    // 4. Método para que otros scripts obtengan la conexión y hagan consultas
    public SQLiteConnectionCustom GetConnection()
    {
        if (db == null)
        {
            // Esto solo debería ocurrir si la inicialización falló o fue accedida muy rápido
            Debug.LogError("Se intentó acceder a la DB antes de que se inicializara. Reintentando...");
            InitializeDatabase();
        }
        return db;
    }
}
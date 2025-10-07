using UnityEngine;
using System.IO;

// 1. Reintroducimos el alias para forzar a usar la librería que quedó.
// Si esta línea causa error, significa que SQLite.SQLiteConnection no existe 
// y tendrías que usar el namespace completo de la versión de Visual Scripting.
// Asumiremos que la que dejaste es la correcta.
using SQLiteConnectionCustom = SQLite.SQLiteConnection;

public class DBConnection : MonoBehaviour
{
    // Usamos el alias para declarar la variable
    private SQLiteConnectionCustom db;

    // Singleton
    public static DBConnection Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        string dbFileName = "VeroLoveDB.sqlite";
        string dbPath = Path.Combine(Application.persistentDataPath, dbFileName);

        Debug.Log("Intentando conectar/crear DB en: " + dbPath);

        try
        {
            // Usamos el alias para instanciar la conexión
            db = new SQLiteConnectionCustom(dbPath);

            // Creación de Tablas
            db.CreateTable<Cliente>();
            db.CreateTable<Servicio>();
            db.CreateTable<Profesional>();
            db.CreateTable<Turno>();

            Debug.Log("✅ Conexión a DB y tablas verificadas/creadas exitosamente.");
        }
        catch (System.Exception e)
        {
            // Nota: Si este error vuelve a salir, el problema es que el binario nativo (sqlite3.dll) sigue faltando.
            Debug.LogError("❌ ERROR FATAL al inicializar la base de datos: " + e.Message);
        }
    }

    // El método GetConnection() necesario para el Singleton
    public SQLiteConnectionCustom GetConnection()
    {
        if (db == null)
        {
            Debug.LogError("La conexión a la DB es nula. Reintentando la inicialización...");
            InitializeDatabase();
        }
        return db;
    }
}
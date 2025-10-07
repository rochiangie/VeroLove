using UnityEngine;
using System.IO;
using Microsoft.Data.Sqlite;
using System.Data; // Necesario para ConnectionState

public class DBConnection : MonoBehaviour
{
    private SqliteConnection db;
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

        try
        {
            // Construye el string de conexión
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath
            };

            db = new SqliteConnection(connectionStringBuilder.ToString());
            db.Open();

            // Creamos las tablas usando comandos SQL puros
            using (var command = db.CreateCommand())
            {
                // CLIENTE
                command.CommandText = "CREATE TABLE IF NOT EXISTS Cliente (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT, Apellido TEXT, Mail TEXT UNIQUE, Telefono TEXT)";
                command.ExecuteNonQuery();

                // SERVICIO
                command.CommandText = "CREATE TABLE IF NOT EXISTS Servicio (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT, Descripcion TEXT, DuracionMinutos INTEGER, Precio REAL)";
                command.ExecuteNonQuery();

                // PROFESIONAL
                command.CommandText = "CREATE TABLE IF NOT EXISTS Profesional (Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT, Apellido TEXT, Especialidad TEXT, Telefono TEXT)";
                command.ExecuteNonQuery();

                // TURNO
                command.CommandText = "CREATE TABLE IF NOT EXISTS Turno (Id INTEGER PRIMARY KEY AUTOINCREMENT, IdCliente INTEGER, IdServicio INTEGER, IdProfesional INTEGER, FechaHoraInicio TEXT, Estado TEXT, Notas TEXT)";
                command.ExecuteNonQuery();
            }

            db.Close();
            Debug.Log("✅ Conexión a DB y tablas verificadas/creadas exitosamente.");
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ ERROR FATAL al inicializar la base de datos: " + e.Message);
        }
    }

    public SqliteConnection GetConnection()
    {
        if (db == null)
        {
            Debug.LogError("La conexión a la DB es nula. Reiniciando la inicialización...");
            InitializeDatabase();
        }
        return db;
    }
}
using UnityEngine;
using TMPro;
using Microsoft.Data.Sqlite;
using Unity.VisualScripting.Dependencies.Sqlite;

public class ClienteForm : MonoBehaviour
{
    public TMP_InputField inputNombre;
    public TMP_InputField inputApellido;
    public TMP_InputField inputTelefono;
    public TMP_InputField inputMail;
    public ClienteUI clienteUI;
    public GameObject panelFormulario;

    private SqliteConnection db;

    void Awake()
    {
        if (DBConnection.Instance != null)
        {
            db = DBConnection.Instance.GetConnection();
        }
    }

    public void GuardarCliente()
    {
        if (db == null) return;

        string nombre = (inputNombre?.text ?? "").Trim();
        string apellido = (inputApellido?.text ?? "").Trim();
        string telefono = (inputTelefono?.text ?? "").Trim();
        string mail = (inputMail?.text ?? "").Trim();

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido))
        {
            Debug.LogWarning("[ClienteForm] Nombre y Apellido son requeridos.");
            return;
        }

        try
        {
            db.Open();

            using (var command = db.CreateCommand())
            {
                // Usamos parámetros para prevenir inyección SQL
                command.CommandText = "INSERT INTO Cliente (Nombre, Apellido, Telefono, Mail) VALUES (@nombre, @apellido, @telefono, @mail)";
                command.Parameters.AddWithValue("@nombre", nombre);
                command.Parameters.AddWithValue("@apellido", apellido);
                command.Parameters.AddWithValue("@telefono", telefono);
                command.Parameters.AddWithValue("@mail", mail);
                command.ExecuteNonQuery();
            }

            db.Close();

            Debug.Log($"✅ Cliente '{nombre} {apellido}' guardado.");

            if (clienteUI != null) clienteUI.RecargarListaClientes();

        }
        catch (SqliteException e)
        {
            // Error si el correo (Mail) es duplicado (debido a UNIQUE en la tabla)
            if (e.SqliteErrorCode == 19) // SQLite error code for constraint violation
            {
                Debug.LogError("El correo electrónico ya está registrado.");
            }
            else
            {
                Debug.LogError($"❌ Error al guardar cliente: {e.Message}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error general al guardar cliente: {e.Message}");
        }
        finally
        {
            if (db.State == System.Data.ConnectionState.Open) db.Close();
        }

        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        if (inputNombre) inputNombre.text = "";
        if (inputApellido) inputApellido.text = "";
        if (inputTelefono) inputTelefono.text = "";
        if (inputMail) inputMail.text = "";

        if (panelFormulario) panelFormulario.SetActive(false);
    }

    public void MostrarFormulario()
    {
        if (panelFormulario) panelFormulario.SetActive(true);
    }

    public void CancelarFormulario()
    {
        LimpiarFormulario();
    }
}
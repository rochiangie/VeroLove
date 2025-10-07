using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ClienteStorage
{
    private static readonly string path = Path.Combine(Application.persistentDataPath, "clientes.json");

    [System.Serializable]
    private class ClientesWrapper { public List<Cliente> clientes = new List<Cliente>(); }

    public static List<Cliente> CargarClientes()
    {
        try
        {
            if (!File.Exists(path)) return new List<Cliente>();

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new List<Cliente>();

            var wrapper = JsonUtility.FromJson<ClientesWrapper>(json);
            return wrapper?.clientes ?? new List<Cliente>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ClienteStorage] Error al leer {path}: {e}");
            return new List<Cliente>();
        }
    }

    public static void GuardarClientes(List<Cliente> clientes)
    {
        try
        {
            var wrapper = new ClientesWrapper { clientes = clientes ?? new List<Cliente>() };
            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(path, json);
#if UNITY_EDITOR
            Debug.Log($"[ClienteStorage] Guardado en {path}");
#endif
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ClienteStorage] Error al guardar {path}: {e}");
        }
    }
}

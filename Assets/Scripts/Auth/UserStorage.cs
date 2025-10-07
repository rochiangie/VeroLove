using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class UserStorage
{
    private static string path = Application.persistentDataPath + "/usuarios.json";

    public static List<Usuario> CargarUsuarios()
    {
        if (!File.Exists(path)) return new List<Usuario>();
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<UsuariosWrapper>(json).usuarios;
    }

    public static void GuardarUsuarios(List<Usuario> usuarios)
    {
        UsuariosWrapper wrapper = new UsuariosWrapper { usuarios = usuarios };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
    }

    [System.Serializable]
    private class UsuariosWrapper
    {
        public List<Usuario> usuarios;
    }
    [System.Serializable]
    public class Cliente
    {
        public string id;
        public string nombre;
        public string telefono;
        public string email;
        public string notas;
        public string fechaAlta;
    }

}

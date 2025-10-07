using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class TurnoStorage
{
    private static string path = Application.persistentDataPath + "/turnos.json";

    public static List<Turno> CargarTurnos()
    {
        if (!File.Exists(path)) return new List<Turno>();
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<TurnosWrapper>(json).turnos;
    }

    public static void GuardarTurnos(List<Turno> turnos)
    {
        TurnosWrapper wrapper = new TurnosWrapper { turnos = turnos };
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(path, json);
    }

    [System.Serializable]
    private class TurnosWrapper
    {
        public List<Turno> turnos;
    }
}

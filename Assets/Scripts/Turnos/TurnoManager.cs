using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnoManager : MonoBehaviour
{
    public GameObject turnoItemPrefab;
    public Transform contentContainer;

    public TMP_InputField inputFecha;
    public TMP_InputField inputHora;
    public TMP_Dropdown dropdownCliente;
    public TMP_Dropdown dropdownServicio;

    public GameObject panelFormulario;

    private List<Turno> turnos = new List<Turno>();

    void Start()
    {
        turnos = TurnoStorage.CargarTurnos();
        MostrarTurnosEnLista();
    }

    void MostrarTurnosEnLista()
    {
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject); // limpiar primero
        }

        foreach (var turno in turnos)
        {
            GameObject item = Instantiate(turnoItemPrefab, contentContainer);
            item.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = turno.clienteNombre;
            item.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = turno.servicio;
            item.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = $"{turno.fecha} {turno.hora}";
        }
    }

    public void AbrirFormulario()
    {
        panelFormulario.SetActive(true);
    }

    public void CancelarFormulario()
    {
        panelFormulario.SetActive(false);
    }

    public void GuardarTurno()
    {
        Turno nuevo = new Turno
        {
            id = System.Guid.NewGuid().ToString(),
            clienteId = dropdownCliente.options[dropdownCliente.value].text, // opcional
            clienteNombre = dropdownCliente.options[dropdownCliente.value].text,
            servicio = dropdownServicio.options[dropdownServicio.value].text,
            fecha = inputFecha.text,
            hora = inputHora.text
        };

        turnos.Add(nuevo);
        TurnoStorage.GuardarTurnos(turnos);
        MostrarTurnosEnLista();
        CancelarFormulario();
    }
}

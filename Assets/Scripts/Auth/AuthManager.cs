using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI submitButtonText;
    public TextMeshProUGUI toggleButtonText;

    public bool isRegistering = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        isRegistering = false;
        ToggleRegisterButton(); // para forzar la interfaz al estado correcto
    }

    public void OnSubmit()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("⚠️ Ingresá un email y una contraseña.");
            return;
        }

        List<Usuario> usuarios = UserStorage.CargarUsuarios();

        if (isRegistering)
        {
            if (usuarios.Exists(u => u.email == email))
            {
                Debug.LogError("⚠️ Usuario ya registrado");
                return;
            }

            usuarios.Add(new Usuario { email = email, password = password });
            UserStorage.GuardarUsuarios(usuarios);
            Debug.Log("✅ Usuario registrado correctamente");
        }
        else
        {
            if (!usuarios.Exists(u => u.email == email && u.password == password))
            {
                Debug.LogError("❌ Credenciales incorrectas");
                return;
            }

            Debug.Log("✅ Login exitoso");
            SceneManager.LoadScene("HomeScene");
        }
    }

    public void ToggleRegisterButton()
    {
        isRegistering = !isRegistering;

        submitButtonText.text = isRegistering ? "Registrarse" : "Ingresar";
        toggleButtonText.text = isRegistering ? "¿Ya tenés cuenta? Ingresar" : "¿No tenés cuenta? Registrate";
    }
}

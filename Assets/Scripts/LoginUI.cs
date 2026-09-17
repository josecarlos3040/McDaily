using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class LoginUI : MonoBehaviour
{
    [Header("Telas")]
    [SerializeField] private GameObject loginButtonScreen;
    [SerializeField] private GameObject registerButtonScreen;

    [Header("Slider Login / Cadastro")]
    [SerializeField] private Slider loginRegisterSlider;

    [Header("Login")]
    [SerializeField] private TMP_InputField emailInputLogin;
    [SerializeField] private TMP_InputField passwordInputLogin;

    [Header("Cadastro")]
    [SerializeField] private TMP_InputField usernameInputRegister;
    [SerializeField] private TMP_InputField emailInputRegister;
    [SerializeField] private TMP_InputField passwordInputRegister;
    [SerializeField] private TMP_InputField confirmPasswordInputRegister;

    [Header("Mensagem")]
    [SerializeField] private TMP_Text messageText;


    private void Start()
    {
        ClearMessage();

        if (loginRegisterSlider != null)
        {
            loginRegisterSlider.onValueChanged.AddListener(
                OnLoginRegisterChanged
            );
        }

        UpdateLoginRegisterScreen();
    }


    private void OnDestroy()
    {
        if (loginRegisterSlider != null)
        {
            loginRegisterSlider.onValueChanged.RemoveListener(
                OnLoginRegisterChanged
            );
        }
    }


    // =========================================================
    // TROCA LOGIN / CADASTRO
    // =========================================================

    private void OnLoginRegisterChanged(float value)
    {
        ClearMessage();

        UpdateLoginRegisterScreen();
    }


    private void UpdateLoginRegisterScreen()
    {
        if (loginRegisterSlider == null)
        {
            return;
        }

        if (loginButtonScreen == null)
        {
            return;
        }

        if (registerButtonScreen == null)
        {
            return;
        }


        if (loginRegisterSlider.value < 0.5f)
        {
            loginButtonScreen.SetActive(true);
            registerButtonScreen.SetActive(false);
        }
        else
        {
            loginButtonScreen.SetActive(false);
            registerButtonScreen.SetActive(true);
        }
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public void Login()
    {
        ClearMessage();

        string email =
            emailInputLogin.text.Trim();

        string password =
            passwordInputLogin.text;


        // EMAIL VAZIO
        if (string.IsNullOrEmpty(email))
        {
            ShowMessage(
                "Digite seu email."
            );

            return;
        }


        // EMAIL INVÁLIDO
        if (!IsValidEmail(email))
        {
            ShowMessage(
                "Digite um email válido."
            );

            return;
        }


        // SENHA VAZIA
        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Digite sua senha."
            );

            return;
        }


        // FIREBASE
        if (FireBaseManager.Instance == null)
        {
            ShowMessage(
                "Firebase Manager não encontrado."
            );

            return;
        }


        FireBaseManager.Instance.Login(
            email,
            password,
            OnLoginResult
        );
    }


    private void OnLoginResult(
        bool success,
        string message
    )
    {
        if (!success)
        {
            ShowMessage(
                message
            );

            return;
        }


        Debug.Log(
            "Login realizado com sucesso."
        );


        SceneManager.LoadScene(
            "InitialScreen"
        );
    }


    // =========================================================
    // CADASTRO
    // =========================================================

    public void Register()
    {
        ClearMessage();


        string username =
            usernameInputRegister.text.Trim();


        string email =
            emailInputRegister.text.Trim();


        string password =
            passwordInputRegister.text;


        string confirmation =
            confirmPasswordInputRegister.text;


        // 1 - USUÁRIO
        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(
                "Digite seu nome de usuário."
            );

            return;
        }


        // 2 - EMAIL VAZIO
        if (string.IsNullOrEmpty(email))
        {
            ShowMessage(
                "Digite seu email."
            );

            return;
        }


        // 3 - EMAIL INVÁLIDO
        if (!IsValidEmail(email))
        {
            ShowMessage(
                "Digite um email válido."
            );

            return;
        }


        // 4 - SENHA VAZIA
        if (string.IsNullOrEmpty(password))
        {
            ShowMessage(
                "Digite uma senha."
            );

            return;
        }


        // 5 - SENHA FORTE
        string passwordError =
            GetPasswordError(
                password
            );


        if (
            !string.IsNullOrEmpty(
                passwordError
            )
        )
        {
            ShowMessage(
                passwordError
            );

            return;
        }


        // 6 - REPETIR SENHA
        if (
            string.IsNullOrEmpty(
                confirmation
            )
        )
        {
            ShowMessage(
                "Repita sua senha."
            );

            return;
        }


        // 7 - SENHAS DIFERENTES
        if (
            password !=
            confirmation
        )
        {
            ShowMessage(
                "As senhas não coincidem."
            );

            return;
        }


        // 8 - FIREBASE
        if (
            FireBaseManager.Instance ==
            null
        )
        {
            ShowMessage(
                "Firebase Manager não encontrado."
            );

            return;
        }


        FireBaseManager.Instance.Register(
            email,
            password,
            username,
            OnRegisterResult
        );
    }


    private void OnRegisterResult(
        bool success,
        string message
    )
    {
        if (!success)
        {
            ShowMessage(
                message
            );

            return;
        }


        Debug.Log(
            "Cadastro realizado com sucesso."
        );


        ShowMessage(
            "Cadastro realizado com sucesso."
        );
    }


    // =========================================================
    // VALIDAR EMAIL
    // =========================================================

    private bool IsValidEmail(
        string email
    )
    {
        if (
            string.IsNullOrEmpty(
                email
            )
        )
        {
            return false;
        }


        return Regex.IsMatch(
            email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
        );
    }


    // =========================================================
    // VALIDAR SENHA
    // =========================================================

    private string GetPasswordError(
        string password
    )
    {
        List<string> missingRequirements =
            new List<string>();


        // 8 CARACTERES
        if (password.Length < 8)
        {
            missingRequirements.Add(
                "pelo menos 8 caracteres"
            );
        }


        bool hasUpper = false;
        bool hasLower = false;
        bool hasNumber = false;
        bool hasSpecial = false;


        foreach (
            char character
            in password
        )
        {
            if (
                char.IsUpper(
                    character
                )
            )
            {
                hasUpper = true;
            }
            else if (
                char.IsLower(
                    character
                )
            )
            {
                hasLower = true;
            }
            else if (
                char.IsDigit(
                    character
                )
            )
            {
                hasNumber = true;
            }
            else
            {
                hasSpecial = true;
            }
        }


        // MAIÚSCULA
        if (!hasUpper)
        {
            missingRequirements.Add(
                "uma letra maiúscula"
            );
        }


        // MINÚSCULA
        if (!hasLower)
        {
            missingRequirements.Add(
                "uma letra minúscula"
            );
        }


        // NÚMERO
        if (!hasNumber)
        {
            missingRequirements.Add(
                "um número"
            );
        }


        // CARACTERE ESPECIAL
        if (!hasSpecial)
        {
            missingRequirements.Add(
                "um caractere especial"
            );
        }


        // SENHA VÁLIDA
        if (
            missingRequirements.Count ==
            0
        )
        {
            return "";
        }


        // SÓ FALTA UMA COISA
        if (
            missingRequirements.Count ==
            1
        )
        {
            return
                "Sua senha precisa ter " +
                missingRequirements[0] +
                ".";
        }


        // MAIS DE UMA COISA FALTANDO
        string message =
            "Sua senha precisa ter ";


        for (
            int i = 0;
            i <
            missingRequirements.Count;
            i++
        )
        {
            bool isLast =
                i ==
                missingRequirements.Count - 1;


            bool isSecondLast =
                i ==
                missingRequirements.Count - 2;


            if (isLast)
            {
                message +=
                    missingRequirements[i];
            }
            else if (isSecondLast)
            {
                message +=
                    missingRequirements[i] +
                    " e ";
            }
            else
            {
                message +=
                    missingRequirements[i] +
                    ", ";
            }
        }


        message += ".";


        return message;
    }


    // =========================================================
    // MENSAGEM
    // =========================================================

    private void ShowMessage(
        string message
    )
    {
        if (messageText != null)
        {
            messageText.text =
                message;
        }


        Debug.LogWarning(
            message
        );
    }


    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text =
                "";
        }
    }
}
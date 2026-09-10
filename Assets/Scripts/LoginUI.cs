using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [SerializeField] GameObject loginButtonScreen;
    [SerializeField] GameObject registerButtonScreen;

    [SerializeField] Slider loginRegisterSlider;

    [Header("Mensagem")]
    [SerializeField] TMP_Text messageText;

    [Header("Login")]
    public TMP_InputField emailInputLogin;
    public TMP_InputField passwordInputLogin;

    [Header("Register")]
    public TMP_InputField usernameInputRegister;
    public TMP_InputField emailInputRegister;
    public TMP_InputField passwordInputRegister;
    public TMP_InputField confirmPasswordInputRegister;


    // =========================================================
    // MENSAGEM
    // =========================================================

    private void ShowMessage(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
    }

    private void HideMessage()
    {
        messageText.text = "";
        messageText.gameObject.SetActive(false);
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public void Login()
    {
        string email = emailInputLogin.text;
        string password = passwordInputLogin.text;

        ShowMessage("Entrando...");

        FireBaseManager.Instance.Login(
            email,
            password,
            success =>
            {
                if (success)
                {
                    // Login deu certo
                    HideMessage();

                    // Limpa os campos
                    emailInputLogin.text = "";
                    passwordInputLogin.text = "";

                    // Vai para a tela inicial
                    SceneManager.LoadScene("InitialScreen");
                }
                else
                {
                    // Login deu errado
                    ShowMessage(
                        "E-mail ou senha incorretos."
                    );

                    // Limpa os campos
                    emailInputLogin.text = "";
                    passwordInputLogin.text = "";
                }
            }
        );
    }


    // =========================================================
    // REGISTER
    // =========================================================

    public void Register()
    {
        string username =
            usernameInputRegister.text;

        string email =
            emailInputRegister.text;

        string password =
            passwordInputRegister.text;

        string confirmPassword =
            confirmPasswordInputRegister.text;


        // =====================================================
        // USERNAME
        // =====================================================

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowMessage(
                "Digite um nome de usuário."
            );

            return;
        }


        // =====================================================
        // EMAIL
        // =====================================================

        if (string.IsNullOrWhiteSpace(email))
        {
            ShowMessage(
                "Digite um email."
            );

            return;
        }


        // =====================================================
        // SENHA
        // =====================================================

        if (string.IsNullOrWhiteSpace(password))
        {
            ShowMessage(
                "Digite uma senha."
            );

            return;
        }


        // =====================================================
        // CONFIRMAR SENHA
        // =====================================================

        if (password != confirmPassword)
        {
            ShowMessage(
                "As senhas não são iguais!"
            );

            return;
        }


        // =====================================================
        // CRIAR CONTA
        // =====================================================

        ShowMessage("Criando conta...");

        FireBaseManager.Instance.Register(
            username,
            email,
            password,
            success =>
            {
                if (success)
                {
                    // Cadastro deu certo
                    ShowMessage(
                        "Conta criada com sucesso!"
                    );

                    // Limpa todos os campos
                    usernameInputRegister.text = "";
                    emailInputRegister.text = "";
                    passwordInputRegister.text = "";
                    confirmPasswordInputRegister.text = "";
                }
                else
                {
                    // Cadastro deu errado
                    ShowMessage(
                        "Não foi possível criar a conta."
                    );

                    // Limpa todos os campos
                    usernameInputRegister.text = "";
                    emailInputRegister.text = "";
                    passwordInputRegister.text = "";
                    confirmPasswordInputRegister.text = "";
                }
            }
        );
    }


    // =========================================================
    // SLIDER
    // =========================================================

    private void Update()
    {
        if (loginRegisterSlider.value == 0)
        {
            loginButtonScreen.SetActive(true);
            registerButtonScreen.SetActive(false);
        }
        else if (loginRegisterSlider.value == 1)
        {
            loginButtonScreen.SetActive(false);
            registerButtonScreen.SetActive(true);
        }
    }
}
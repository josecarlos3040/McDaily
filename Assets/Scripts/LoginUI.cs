using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUI : MonoBehaviour
{
    [SerializeField] GameObject loginButtonScreen;
    [SerializeField] GameObject registerButtonScreen;

    [SerializeField] Slider loginRegisterSlider;


    public TMP_InputField emailInputLogin;
    public TMP_InputField passwordInputLogin;

    public TMP_InputField emailInputRegister;
    public TMP_InputField passwordInputRegister;

    public void Login()
    {
        FireBaseManager.Instance.Login(
            emailInputLogin.text,
            passwordInputLogin.text
        );

        SceneManager.LoadScene("Questionario");
    }

    public void Register()
    {
        FireBaseManager.Instance.Register(
            emailInputRegister.text,
            passwordInputRegister.text
        );

        
    }

    void Update()
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
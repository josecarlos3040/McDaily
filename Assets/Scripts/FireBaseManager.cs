using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
#endif


public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;


#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private FirebaseUser currentUser;

#endif


    private Action<bool, string> loginCallback;
    private Action<bool, string> registerCallback;


    // =========================================================
    // WEBGL
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Initialize(
        string gameObjectName
    );


    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Login(
        string gameObjectName,
        string email,
        string password
    );


    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Register(
        string gameObjectName,
        string email,
        string password,
        string username
    );


    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Logout(
        string gameObjectName
    );

#endif


    // =========================================================
    // RESULTADO WEBGL
    // =========================================================

    [Serializable]
    private class WebResult
    {
        public bool success;

        public string error;
        public string message;

        public string uid;
        public string email;
        public string username;
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }


    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    private void InitializeFirebase()
    {

#if UNITY_WEBGL && !UNITY_EDITOR

    Debug.Log("Inicializando Firebase WebGL...");

    FirebaseWeb_Initialize(
        gameObject.name
    );

#else

        FirebaseApp.LogLevel = LogLevel.Warning;

        Debug.Log("Inicializando Firebase...");

        FirebaseApp
            .CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError(
                        "Inicialização do Firebase cancelada."
                    );

                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError(
                        "Erro ao inicializar Firebase: "
                        + task.Exception
                    );

                    return;
                }

                DependencyStatus status = task.Result;

                if (status != DependencyStatus.Available)
                {
                    Debug.LogError(
                        "Firebase não está disponível: "
                        + status
                    );

                    return;
                }

                auth = FirebaseAuth.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                currentUser = auth.CurrentUser;

                Debug.Log(
                    "Firebase inicializado com sucesso."
                );
            });

#endif
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public void Login(
        string email,
        string password,
        Action<bool, string> callback
    )
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        loginCallback = callback;


        Debug.Log(
            "Tentando login WebGL..."
        );


        FirebaseWeb_Login(
            gameObject.name,
            email,
            password
        );

#else

        if (auth == null)
        {
            callback?.Invoke(
                false,
                "Firebase ainda não foi inicializado."
            );

            return;
        }


        auth
            .SignInWithEmailAndPasswordAsync(
                email,
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    callback?.Invoke(
                        false,
                        "Login cancelado."
                    );

                    return;
                }


                if (task.IsFaulted)
                {
                    string error =
                        GetFirebaseError(
                            task.Exception
                        );


                    callback?.Invoke(
                        false,
                        error
                    );

                    return;
                }


                AuthResult result =
                    task.Result;


                currentUser =
                    result.User;


                if (currentUser == null)
                {
                    callback?.Invoke(
                        false,
                        "Não foi possível obter o usuário."
                    );

                    return;
                }


                Debug.Log(
                    "Login realizado com sucesso: "
                    + currentUser.Email
                );


                callback?.Invoke(
                    true,
                    ""
                );
            });

#endif
    }


    // =========================================================
    // CADASTRO
    // =========================================================

    public void Register(
        string email,
        string password,
        string username,
        Action<bool, string> callback
    )
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        registerCallback = callback;


        Debug.Log(
            "Tentando cadastro WebGL..."
        );


        FirebaseWeb_Register(
            gameObject.name,
            email,
            password,
            username
        );

#else

        if (auth == null)
        {
            callback?.Invoke(
                false,
                "Firebase ainda não foi inicializado."
            );

            return;
        }


        if (firestore == null)
        {
            callback?.Invoke(
                false,
                "Firestore ainda não foi inicializado."
            );

            return;
        }


        auth
            .CreateUserWithEmailAndPasswordAsync(
                email,
                password
            )
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    callback?.Invoke(
                        false,
                        "Cadastro cancelado."
                    );

                    return;
                }


                if (task.IsFaulted)
                {
                    string error =
                        GetFirebaseError(
                            task.Exception
                        );


                    callback?.Invoke(
                        false,
                        error
                    );

                    return;
                }


                AuthResult result =
                    task.Result;


                currentUser =
                    result.User;


                if (currentUser == null)
                {
                    callback?.Invoke(
                        false,
                        "Não foi possível criar o usuário."
                    );

                    return;
                }


                Debug.Log(
                    "Usuário criado no Authentication: "
                    + currentUser.UserId
                );


                SaveUserData(
                    currentUser.UserId,
                    username,
                    callback
                );
            });

#endif
    }


#if !UNITY_WEBGL || UNITY_EDITOR

    // =========================================================
    // SALVAR USUÁRIO NO FIRESTORE
    // =========================================================

    private void SaveUserData(
        string userId,
        string username,
        Action<bool, string> callback
    )
    {
        DocumentReference userDocument =
            firestore
                .Collection("users")
                .Document(userId);


        Dictionary<string, object> userData =
            new Dictionary<string, object>();


        userData.Add(
            "username",
            username
        );


        userData.Add(
            "points",
            0
        );


        userData.Add(
            "streak",
            0
        );


        userDocument
            .SetAsync(userData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    callback?.Invoke(
                        false,
                        "Não foi possível salvar os dados do usuário."
                    );

                    return;
                }


                if (task.IsFaulted)
                {
                    string error =
                        GetFirebaseError(
                            task.Exception
                        );


                    Debug.LogError(
                        "Erro ao salvar no Firestore: "
                        + task.Exception
                    );


                    callback?.Invoke(
                        false,
                        error
                    );

                    return;
                }


                Debug.Log(
                    "Dados do usuário salvos no Firestore."
                );


                callback?.Invoke(
                    true,
                    ""
                );
            });
    }

#endif


    // =========================================================
    // LOGOUT
    // =========================================================

    public void Logout()
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_Logout(
            gameObject.name
        );

#else

        if (auth != null)
        {
            auth.SignOut();
        }


        currentUser = null;

        Debug.Log(
            "Logout realizado."
        );

#endif


        PlayerPrefs.DeleteKey(
            "LoggedIn"
        );


        PlayerPrefs.DeleteKey(
            "UserId"
        );


        PlayerPrefs.DeleteKey(
            "UserEmail"
        );


        PlayerPrefs.DeleteKey(
            "Username"
        );


        PlayerPrefs.Save();
    }


    // =========================================================
    // ESTÁ LOGADO?
    // =========================================================

    public bool IsLoggedIn()
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        if (
            PlayerPrefs.GetInt(
                "LoggedIn",
                0
            ) == 1
        )
        {
            return true;
        }


        return false;

#else

        if (auth == null)
        {
            return false;
        }


        if (auth.CurrentUser == null)
        {
            return false;
        }


        return true;

#endif
    }


    // =========================================================
    // TRATAR ERROS FIREBASE NATIVO
    // =========================================================

#if !UNITY_WEBGL || UNITY_EDITOR

    private string GetFirebaseError(Exception exception)
    {
        if (exception == null)
        {
            return "Ocorreu um erro desconhecido.";
        }

        FirebaseException firebaseException = null;

        // O Firebase normalmente joga a exceção dentro de AggregateException
        AggregateException aggregateException =
            exception as AggregateException;

        if (aggregateException != null)
        {
            AggregateException flattened =
                aggregateException.Flatten();

            foreach (Exception innerException in flattened.InnerExceptions)
            {
                FirebaseException firebaseInner =
                    innerException as FirebaseException;

                if (firebaseInner != null)
                {
                    firebaseException = firebaseInner;
                    break;
                }
            }
        }

        // Caso a própria exceção já seja FirebaseException
        if (firebaseException == null)
        {
            firebaseException =
                exception as FirebaseException;
        }

        if (firebaseException != null)
        {
            AuthError authError =
                (AuthError)firebaseException.ErrorCode;

            Debug.LogWarning(
                "Firebase Auth Error: "
                + authError.ToString()
                + " | "
                + firebaseException.Message
            );

            return TranslateFirebaseError(
                authError.ToString()
            );
        }

        Debug.LogWarning(
            "Erro Firebase desconhecido: "
            + exception
        );

        return TranslateFirebaseError(
            exception.Message
        );
    }

#endif


    // =========================================================
    // TRADUZIR ERROS
    // =========================================================

    private string TranslateFirebaseError(string error)
    {
        if (string.IsNullOrEmpty(error))
        {
            return "Ocorreu um erro.";
        }

        string lowerError =
            error.ToLower();


        // LOGIN - SENHA OU CREDENCIAL ERRADA
        if (
            lowerError.Contains("wrongpassword") ||
            lowerError.Contains("wrong-password") ||
            lowerError.Contains("invalidcredential") ||
            lowerError.Contains("invalid-credential")
        )
        {
            return "Email ou senha incorretos.";
        }


        // USUÁRIO NÃO EXISTE
        if (
            lowerError.Contains("usernotfound") ||
            lowerError.Contains("user-not-found")
        )
        {
            return "Email ou senha incorretos.";
        }


        // EMAIL INVÁLIDO
        if (
            lowerError.Contains("invalidemail") ||
            lowerError.Contains("invalid-email")
        )
        {
            return "O email informado é inválido.";
        }


        // EMAIL JÁ CADASTRADO
        if (
            lowerError.Contains("emailalreadyinuse") ||
            lowerError.Contains("email-already-in-use")
        )
        {
            return "Este email já está cadastrado.";
        }


        // SENHA FRACA
        if (
            lowerError.Contains("weakpassword") ||
            lowerError.Contains("weak-password")
        )
        {
            return "A senha é muito fraca.";
        }


        // MUITAS TENTATIVAS
        if (
            lowerError.Contains("toomanyrequests") ||
            lowerError.Contains("too-many-requests")
        )
        {
            return "Muitas tentativas. Tente novamente mais tarde.";
        }


        // SEM INTERNET
        if (
            lowerError.Contains("networkrequestfailed") ||
            lowerError.Contains("network-request-failed")
        )
        {
            return "Erro de conexão com a internet.";
        }


        // LOGIN POR EMAIL DESATIVADO
        if (
            lowerError.Contains("operationnotallowed") ||
            lowerError.Contains("operation-not-allowed")
        )
        {
            return "Login por email e senha não está habilitado.";
        }


        return "Não foi possível realizar a operação.";
    }


    // =========================================================
    // CALLBACK INICIALIZAÇÃO WEBGL
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebInitializeResult(
        string json
    )
    {
        Debug.Log(
            "Firebase WebGL Initialize: "
            + json
        );


        WebResult result =
            JsonUtility.FromJson<WebResult>(
                json
            );


        if (result == null)
        {
            Debug.LogError(
                "Resposta inválida do Firebase WebGL."
            );

            return;
        }


        if (result.success)
        {
            Debug.Log(
                "Firebase WebGL inicializado com sucesso."
            );
        }
        else
        {
            Debug.LogError(
                "Erro ao inicializar Firebase WebGL: "
                + result.error
            );
        }
    }


    // =========================================================
    // CALLBACK LOGIN WEBGL
    // =========================================================

    public void OnWebLoginResult(
        string json
    )
    {
        Debug.Log(
            "Firebase WebGL Login: "
            + json
        );


        WebResult result =
            JsonUtility.FromJson<WebResult>(
                json
            );


        if (result == null)
        {
            if (loginCallback != null)
            {
                loginCallback.Invoke(
                    false,
                    "Resposta inválida do Firebase."
                );
            }


            loginCallback = null;

            return;
        }


        if (result.success)
        {
            PlayerPrefs.SetInt(
                "LoggedIn",
                1
            );


            PlayerPrefs.SetString(
                "UserId",
                result.uid
            );


            PlayerPrefs.SetString(
                "UserEmail",
                result.email
            );


            PlayerPrefs.Save();


            if (loginCallback != null)
            {
                loginCallback.Invoke(
                    true,
                    ""
                );
            }


            loginCallback = null;

            return;
        }


        string error =
            result.error;


        if (
            !string.IsNullOrEmpty(
                result.message
            )
        )
        {
            error =
                result.message
                + " "
                + result.error;
        }


        error =
            TranslateFirebaseError(
                error
            );


        if (loginCallback != null)
        {
            loginCallback.Invoke(
                false,
                error
            );
        }


        loginCallback = null;
    }


    // =========================================================
    // CALLBACK CADASTRO WEBGL
    // =========================================================

    public void OnWebRegisterResult(
        string json
    )
    {
        Debug.Log(
            "Firebase WebGL Register: "
            + json
        );


        WebResult result =
            JsonUtility.FromJson<WebResult>(
                json
            );


        if (result == null)
        {
            if (registerCallback != null)
            {
                registerCallback.Invoke(
                    false,
                    "Resposta inválida do Firebase."
                );
            }


            registerCallback = null;

            return;
        }


        if (result.success)
        {
            PlayerPrefs.SetInt(
                "LoggedIn",
                1
            );


            PlayerPrefs.SetString(
                "UserId",
                result.uid
            );


            PlayerPrefs.SetString(
                "UserEmail",
                result.email
            );


            PlayerPrefs.SetString(
                "Username",
                result.username
            );


            PlayerPrefs.Save();


            if (registerCallback != null)
            {
                registerCallback.Invoke(
                    true,
                    ""
                );
            }


            registerCallback = null;

            return;
        }


        string error =
            result.error;


        if (
            !string.IsNullOrEmpty(
                result.message
            )
        )
        {
            error =
                result.message
                + " "
                + result.error;
        }


        error =
            TranslateFirebaseError(
                error
            );


        if (registerCallback != null)
        {
            registerCallback.Invoke(
                false,
                error
            );
        }


        registerCallback = null;
    }


    // =========================================================
    // CALLBACK LOGOUT WEBGL
    // =========================================================

    public void OnWebLogoutResult(
        string json
    )
    {
        Debug.Log(
            "Firebase WebGL Logout: "
            + json
        );


        PlayerPrefs.DeleteKey(
            "LoggedIn"
        );


        PlayerPrefs.DeleteKey(
            "UserId"
        );


        PlayerPrefs.DeleteKey(
            "UserEmail"
        );


        PlayerPrefs.DeleteKey(
            "Username"
        );


        PlayerPrefs.Save();
    }

#endif
}
using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
#endif

public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;

    // =========================================================
    // FIREBASE WEB
    // =========================================================

    [Header("Firebase Web")]
    [SerializeField] private string apiKey;
    [SerializeField] private string authDomain;
    [SerializeField] private string projectId;
    [SerializeField] private string storageBucket;
    [SerializeField] private string messagingSenderId;
    [SerializeField] private string appId;


#if !UNITY_WEBGL || UNITY_EDITOR

    // =========================================================
    // FIREBASE UNITY
    // =========================================================

    private FirebaseAuth auth;
    private FirebaseFirestore db;

#endif


#if UNITY_WEBGL && !UNITY_EDITOR

    // =========================================================
    // FIREBASE WEBGL
    // =========================================================

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Initialize(
        string apiKey,
        string authDomain,
        string projectId,
        string storageBucket,
        string messagingSenderId,
        string appId
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Register(
        string username,
        string email,
        string password,
        string gameObjectName
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Login(
        string email,
        string password,
        string gameObjectName,
        string callbackMethod
    );

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_Logout();

#endif


    // =========================================================
    // CALLBACKS
    // =========================================================

    private Action<bool> loginCallback;
    private Action<bool> registerCallback;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


#if UNITY_WEBGL && !UNITY_EDITOR

        // Inicializa Firebase pelo JavaScript
        FirebaseWeb_Initialize(
            apiKey,
            authDomain,
            projectId,
            storageBucket,
            messagingSenderId,
            appId
        );

#else

        // Firebase Unity SDK
        InitializeFirebase();

#endif
    }


#if !UNITY_WEBGL || UNITY_EDITOR

    // =========================================================
    // FIREBASE UNITY - INITIALIZE
    // =========================================================

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                Firebase.DependencyStatus dependencyStatus =
                    task.Result;

                if (dependencyStatus ==
                    Firebase.DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    db = FirebaseFirestore.DefaultInstance;

                    Debug.Log("Firebase Unity inicializado!");
                }
                else
                {
                    Debug.LogError(
                        "Não foi possível inicializar o Firebase. " +
                        "Dependências: " + dependencyStatus
                    );
                }
            });
    }

#endif


    // =========================================================
    // REGISTER
    // =========================================================

    public void Register(
        string username,
        string email,
        string password,
        Action<bool> callback = null
    )
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        registerCallback = callback;

        FirebaseWeb_Register(
            username,
            email,
            password,
            gameObject.name
        );

#else

        if (auth == null)
        {
            Debug.LogError("Firebase ainda não foi inicializado.");
            callback?.Invoke(false);
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(
            email,
            password
        )
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError(
                    "Erro ao criar usuário: " +
                    task.Exception
                );

                callback?.Invoke(false);
                return;
            }

            FirebaseUser user = task.Result.User;

            if (user == null)
            {
                Debug.LogError(
                    "Usuário não foi criado."
                );

                callback?.Invoke(false);
                return;
            }

            string userId = user.UserId;

            DocumentReference userDocument =
                db.Collection("users").Document(userId);

            userDocument.SetAsync(new
            {
                username = username,
                points = 0,
                streak = 0
            })
            .ContinueWithOnMainThread(saveTask =>
            {
                if (saveTask.IsCanceled ||
                    saveTask.IsFaulted)
                {
                    Debug.LogError(
                        "Erro ao salvar usuário: " +
                        saveTask.Exception
                    );

                    callback?.Invoke(false);
                    return;
                }

                Debug.Log(
                    "Usuário criado com sucesso!"
                );

                callback?.Invoke(true);
            });
        });

#endif
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public void Login(
        string email,
        string password,
        Action<bool> callback = null
    )
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        loginCallback = callback;

        FirebaseWeb_Login(
            email,
            password,
            gameObject.name,
            "OnWebLoginResult"
        );

#else

        if (auth == null)
        {
            Debug.LogError(
                "Firebase ainda não foi inicializado."
            );

            callback?.Invoke(false);
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(
            email,
            password
        )
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled ||
                task.IsFaulted)
            {
                Debug.LogError(
                    "Erro ao fazer login: " +
                    task.Exception
                );

                callback?.Invoke(false);
                return;
            }

            FirebaseUser user = task.Result.User;

            if (user != null)
            {
                Debug.Log(
                    "Login realizado com sucesso!"
                );

                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        });

#endif
    }


    // =========================================================
    // WEBGL - LOGIN CALLBACK
    // =========================================================

#if UNITY_WEBGL && !UNITY_EDITOR

    public void OnWebLoginResult(string result)
    {
        Debug.Log(
            "Resultado login WebGL: " + result
        );

        bool success = result == "success";

        loginCallback?.Invoke(success);

        loginCallback = null;
    }


    // =========================================================
    // WEBGL - REGISTER CALLBACK
    // =========================================================

    public void OnWebRegisterResult(string result)
    {
        Debug.Log(
            "Resultado registro WebGL: " + result
        );

        bool success = result == "success";

        registerCallback?.Invoke(success);

        registerCallback = null;
    }

#endif


    // =========================================================
    // LOGOUT
    // =========================================================

    public void Logout()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_Logout();

#else

        if (auth == null)
        {
            Debug.LogWarning(
                "Firebase ainda não foi inicializado."
            );

            return;
        }

        auth.SignOut();

        Debug.Log(
            "Logout realizado!"
        );

#endif
    }


    // =========================================================
    // IS LOGGED IN
    // =========================================================

    public bool IsLoggedIn()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        // Por enquanto o estado do login WebGL
        // é controlado pelo Firebase Web.
        return false;

#else

        return auth != null &&
               auth.CurrentUser != null;

#endif
    }


    // =========================================================
    // CURRENT USER ID
    // =========================================================

    public string GetCurrentUserId()
    {
#if UNITY_WEBGL && !UNITY_EDITOR

        return "";

#else

        if (auth == null ||
            auth.CurrentUser == null)
        {
            return "";
        }

        return auth.CurrentUser.UserId;

#endif
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
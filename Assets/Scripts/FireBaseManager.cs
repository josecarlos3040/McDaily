using Firebase;
using Firebase.Auth;
using UnityEngine;

public class FireBaseManager : MonoBehaviour
{
    public static FireBaseManager Instance;

    private FirebaseAuth auth;

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
        }
    }

    private void Start()
    {
        InitializeFirebase();
    }

    private void CheckUser()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            Debug.Log("Usuário já está logado!");
            Debug.Log(user.Email);
        }
        else
        {
            Debug.Log("Nenhum usuário logado.");
        }
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            Firebase.DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;

                Debug.Log("Firebase conectado!");

                CheckUser();
            }
            else
            {
                Debug.LogError(
                    "Não foi possível inicializar o Firebase: " +
                    dependencyStatus
                );
            }
        });
    }

    public bool IsLoggedIn()
    {
        return auth != null && auth.CurrentUser != null;
    }
    public void Register(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Registro cancelado.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("Erro no registro: " + task.Exception);
                    return;
                }

                FirebaseUser user = task.Result.User;

                Debug.Log("Conta criada!");
                Debug.Log("UID: " + user.UserId);
                Debug.Log("Email: " + user.Email);
            });
    }

    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Login cancelado.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("Erro no login: " + task.Exception);
                    return;
                }

                FirebaseUser user = task.Result.User;

                Debug.Log("Login realizado!");
                Debug.Log("Usuário: " + user.Email);
                Debug.Log("UID: " + user.UserId);
            });
    }

    public void Logout()
    {
        auth.SignOut();

        Debug.Log("Logout realizado.");
    }
}
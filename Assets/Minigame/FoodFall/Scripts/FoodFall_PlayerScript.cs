using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#if !UNITY_WEBGL || UNITY_EDITOR
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class FoodFall_PlayerScript : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Score")]
    public int score;
    public int life;

    private int maxlife;

    [SerializeField] private GameObject particle;

    [Header("HUD")]
    [SerializeField] private Text pontostxt;

    public Image[] heartImages;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [SerializeField] private GameObject Impactparticle;
    [SerializeField] private GameObject GameOverScreen;
    [SerializeField] private Text EndScore;

    [Header("Recompensa")]
    [SerializeField] private int pointsReward = 5;

    private bool gameEnded = false;


#if !UNITY_WEBGL || UNITY_EDITOR

    private FirebaseAuth auth;
    private FirebaseFirestore db;

#else

    [DllImport("__Internal")]
    private static extern void FirebaseWeb_AddPoints(
        int points
    );

#endif


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        mainCamera = Camera.main;

        maxlife = life;

        UpdateHeartsUI();

        UpdateScore();

        GameOverScreen.SetActive(false);


#if !UNITY_WEBGL || UNITY_EDITOR

        auth =
            FirebaseAuth.DefaultInstance;

        db =
            FirebaseFirestore.DefaultInstance;

#endif
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (gameEnded)
        {
            return;
        }

        Movimento();
    }


    // =========================================================
    // MOVIMENTO
    // =========================================================

    private void Movimento()
    {
        if (
            Pointer.current != null &&
            Pointer.current.press.isPressed
        )
        {
            Vector2 screenPosition =
                Pointer.current.position.ReadValue();

            Vector3 worldPosition =
                mainCamera.ScreenToWorldPoint(
                    screenPosition
                );

            transform.position =
                new Vector3(
                    worldPosition.x,
                    transform.position.y,
                    0f
                );
        }
    }


    // =========================================================
    // COLISÕES
    // =========================================================

    private void OnTriggerEnter2D(
        Collider2D other
    )
    {
        if (gameEnded)
        {
            return;
        }


        // =====================================================
        // COMIDA
        // =====================================================

        if (other.CompareTag("Food"))
        {
            score += 10;


            Debug.Log(
                "Pontuação Atual: " +
                score
            );


            Instantiate(
                particle,
                new Vector3(
                    transform.position.x,
                    transform.position.y - 0.2f,
                    transform.position.z
                ),
                Quaternion.identity
            );


            Destroy(
                other.gameObject
            );


            UpdateScore();
        }


        // =====================================================
        // NÃO COMIDA
        // =====================================================

        else if (
            other.CompareTag("NotFood")
        )
        {
            life -= 1;


            Instantiate(
                Impactparticle,
                transform.position,
                Quaternion.identity
            );


            Debug.Log(
                "Perdeu vida"
            );


            Destroy(
                other.gameObject
            );


            UpdateHeartsUI();


            if (life <= 0)
            {
                GameOver();
            }
        }
    }


    // =========================================================
    // SCORE
    // =========================================================

    public void UpdateScore()
    {
        pontostxt.text =
            score.ToString();
    }


    // =========================================================
    // CORAÇÕES
    // =========================================================

    private void UpdateHeartsUI()
    {
        for (
            int i = 0;
            i < heartImages.Length;
            i++
        )
        {
            if (i < life)
            {
                heartImages[i].sprite =
                    fullHeart;
            }
            else
            {
                heartImages[i].sprite =
                    emptyHeart;
            }
        }
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    private void GameOver()
    {
        // Impede dar os 5 pontos mais de uma vez
        if (gameEnded)
        {
            return;
        }


        gameEnded = true;


        GameOverScreen.SetActive(
            true
        );


        EndScore.text =
            score.ToString();


        // Dá 5 pontos para a conta
        AddRewardPoints();
    }


    // =========================================================
    // ADICIONAR PONTOS NO FIREBASE
    // =========================================================

    private void AddRewardPoints()
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        FirebaseWeb_AddPoints(
            pointsReward
        );

        Debug.Log(
            "Recompensa enviada: +" +
            pointsReward +
            " pontos."
        );

#else

        if (
            auth == null ||
            auth.CurrentUser == null
        )
        {
            Debug.LogError(
                "Nenhum usuário está logado."
            );

            return;
        }


        if (db == null)
        {
            Debug.LogError(
                "Firestore não inicializado."
            );

            return;
        }


        string userId =
            auth.CurrentUser.UserId;


        DocumentReference userDocument =
            db
                .Collection("users")
                .Document(userId);


        Dictionary<string, object> update =
            new Dictionary<string, object>();


        update.Add(
            "points",
            FieldValue.Increment(
                pointsReward
            )
        );


        userDocument
            .UpdateAsync(
                update
            );


        Debug.Log(
            "Recompensa enviada: +" +
            pointsReward +
            " pontos."
        );

#endif
    }
}
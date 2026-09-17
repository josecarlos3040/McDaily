using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FoodFall_PlayerScript : MonoBehaviour
{

    private Camera mainCamera;

    [Header("Score")]
    public int score;
    public int life;
    private int maxlife;
    [SerializeField] GameObject particle;

    [Header("HUD")]
    [SerializeField] Text pontostxt;
    public Image[] heartImages; 
    public Sprite fullHeart;   
    public Sprite emptyHeart;
    [SerializeField] GameObject Impactparticle;
    [SerializeField] GameObject GameOverScreen;
    [SerializeField] Text EndScore;


    void Start()
    {
        mainCamera = Camera.main;
        maxlife = life;
        UpdateHeartsUI();
        UpdateScore();
        GameOverScreen.SetActive(false);
    }

    void Update()
    {
        Movimento();
        
    }

    private void Movimento()
    {
        {
            if (Pointer.current != null && Pointer.current.press.isPressed)
            {
                Vector2 screenPosition = Pointer.current.position.ReadValue();
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);

                transform.position = new Vector3(worldPosition.x, transform.position.y, 0f);
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            // Adiciona os pontos
            score += 10;
            Debug.Log("Pontuação Atual: " + score);
            Instantiate(particle, new Vector3(transform.position.x, (transform.position.y - 0.2f), transform.position.z), Quaternion.identity);

            // Destrói o objeto coletado a partir do Player
            Destroy(other.gameObject);
            UpdateScore();
        }
        else if (other.CompareTag("NotFood"))
        {
            life -= 1;
            Instantiate(Impactparticle, transform.position, Quaternion.identity);
            Debug.Log("Perdeu vida");

            Destroy(other.gameObject);
            UpdateHeartsUI();

            if(life <= 0)
            {
                GameOver();
            }
        }



    }

    public void UpdateScore()
    {
        pontostxt.text = score.ToString();
    }

    private void UpdateHeartsUI()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < life)
            {
                heartImages[i].sprite = fullHeart; 
            }
            else
            {
                heartImages[i].sprite = emptyHeart; 
            }
        }
    }

    private void GameOver()
    {
        GameOverScreen.SetActive(true);
        EndScore.text = score.ToString();
    }
}

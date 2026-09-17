using UnityEngine;

public class FoodFall_FoodScript : MonoBehaviour
{
    [SerializeField] float fallSpeed = 5.0f;
    [SerializeField] float destroyY = -6.0f; 

    [Header("FoodVariations")]
    public Sprite Hamb;
    public Sprite Refri;
    public Sprite Fries;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int imagem = Random.Range(1, 3);
        if (imagem == 1)
        {
            spriteRenderer.sprite = Hamb;
        }
        else if (imagem == 2)
        {
            spriteRenderer.sprite = Refri;
        }
        else if (imagem == 3) 
        {
            spriteRenderer.sprite = Fries;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}

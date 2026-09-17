using UnityEngine;

public class FoodFall_NotFoodScript : MonoBehaviour
{
    [SerializeField] float fallSpeed = 5.5f;
    [SerializeField] float destroyY = -6.0f; 


    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}

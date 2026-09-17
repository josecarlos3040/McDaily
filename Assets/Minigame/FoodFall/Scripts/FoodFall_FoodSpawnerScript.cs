using UnityEngine;
using System.Collections;

public class FoodFall_FoodSpawnerScript : MonoBehaviour
{
    [SerializeField] float spawnInterval = 2.0f;
    [SerializeField] int chance = 5;
    [SerializeField] float xMin = -2.2f; // Limite esquerdo da tela
    [SerializeField] float xMax = 2.2f;  // Limite direito da tela
    public float spawnY = 5.5f; // Altura no topo da tela

    [Header("Itens")]
    [SerializeField] GameObject foodPrefab;
    [SerializeField] GameObject notfoodPrefab;

    private GameObject itemPrefab;

    void Start()
    {
        InvokeRepeating(nameof(SpawnItem), 0.5f, spawnInterval);
        StartCoroutine(Aceleracao());
    }

    void SpawnItem()
    {
        float randomX = Random.Range(xMin, xMax);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);
        int randomitem = Random.Range(1, chance);
        if (randomitem == 1)
        {
            itemPrefab = notfoodPrefab;
        }
        else
        {
            itemPrefab = foodPrefab;
        }
        Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
    }

    IEnumerator Aceleracao()
    {
        yield return new WaitForSeconds(1f);
        if (spawnInterval >= 0.8f)
        {
            spawnInterval -= 0.1f;
        }
        Aceleracao();
    }
}

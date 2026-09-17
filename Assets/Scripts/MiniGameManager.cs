using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameManager : MonoBehaviour
{
    


    public void ChangeSceneGame(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Call this method and pass the scene number
    public void SwitchScene(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }
}

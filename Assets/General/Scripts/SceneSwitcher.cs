using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // This is for when calling LoadSceneDefault
    public string sceneNameToSwitchTo;

    public void LoadSceneCustom(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneDefault()
    {
        SceneManager.LoadScene(sceneNameToSwitchTo);
    }
}

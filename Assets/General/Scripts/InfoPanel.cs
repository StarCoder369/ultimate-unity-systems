using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InfoPanel : MonoBehaviour
{
    public TMP_Text nameTxt;
    public TMP_Text descriptionTxt;
    public TMP_Text buttonTxt;
    public string sceneName;
    public bool isFolder;

    GameObject screenToClose;
    GameObject screenToOpen;

    public void UpdateFields(Folder folder)
    {
        nameTxt.text = folder.folderName;
        descriptionTxt.text = folder.description;

        isFolder = folder.folder;

        if (isFolder)
        {
            buttonTxt.text = "Open Folder";
            screenToClose = folder.currentScreen;
            screenToOpen = folder.screenToOpen;
        }
        else
        {
            buttonTxt.text = "Load System";
            screenToClose = null;
            screenToOpen = null;
        }
    }

    public void ClearFields()
    {
        nameTxt.text = "";
        descriptionTxt.text = "";
        buttonTxt.text = "";
        gameObject.SetActive(false);
    }

    public void LoadScene()
    {
        Debug.Log($"LOADING SCENE {sceneName}!");
        SceneManager.LoadScene(sceneName);
    }

    public void ButtonClick()
    {
        if (isFolder)
        {
            screenToOpen.SetActive(true);
            screenToClose.SetActive(false);
        }
        else
        {
            LoadScene();
        }
    }
}

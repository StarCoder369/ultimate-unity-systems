using UnityEngine;

public class Folder : MonoBehaviour
{
    public GameObject infoPanel;

    public GameObject currentScreen;
    public GameObject screenToOpen;

    public string folderName;
    public string description;

    public string sceneName;

    public bool folder;

    public void OpenInfoPanel()
    {
        infoPanel.SetActive(true);
        infoPanel.GetComponent<InfoPanel>().sceneName = sceneName;
        infoPanel.GetComponent<InfoPanel>().UpdateFields(this);
        infoPanel.GetComponent<InfoPanel>().isFolder = folder;
    }
}

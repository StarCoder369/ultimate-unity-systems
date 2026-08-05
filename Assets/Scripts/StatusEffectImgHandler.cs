using UnityEngine;
using UnityEngine.UI;

public class StatusEffectImgHandler : MonoBehaviour
{
    public StatusData statusData;

    public Image fillImg;
    public Image iconImg;

    public float fillProgress;

    void Start()
    {
        UpdateIcons();
    }

    public void UpdateIcons()
    {
        if (statusData.icon != null)
        {
            iconImg.sprite = statusData.icon;
        }
    }

    void Update()
    {
        fillImg.fillAmount = fillProgress / 1;
    }
}

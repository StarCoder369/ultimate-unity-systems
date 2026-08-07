using System;
using TMPro;
using UnityEngine;

public class TextDisplay : MonoBehaviour
{
    public TMP_Text displayText;

    int intNumToDisplay = 0;

    float floatNumToDisplay = 0;

    public enum NumTypes
    {
        Integer,
        Float
    }

    public NumTypes numType;

    void Awake()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (displayText != null)
        {
            switch (numType)
            {
                case NumTypes.Integer:
                    displayText.text = $"{intNumToDisplay}";
                    break;

                case NumTypes.Float:
                    displayText.text = $"{floatNumToDisplay}";
                    break;

            }

        }
    }

    public void UpdateNumFloat(float num)
    {
        switch (numType)
        {
            case NumTypes.Integer:
                intNumToDisplay = Mathf.RoundToInt(num);
                break;

            case NumTypes.Float:
                floatNumToDisplay = (float)Math.Round(num, 2, MidpointRounding.AwayFromZero);
                break;

        }

        UpdateUI();
    }

    public void UpdateNum(int num)
    {
        switch (numType)
        {
            case NumTypes.Integer:
                intNumToDisplay = num;
                break;

            case NumTypes.Float:
                floatNumToDisplay = num;
                break;

        }

        UpdateUI();
    }
}

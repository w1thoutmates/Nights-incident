using System;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    [Header("Main objective")] 
    [SerializeField]
    public TextMeshProUGUI bigRedText;
    
    [SerializeField]
    public TextMeshProUGUI[] smallYellowText;
    
    public void SetBigRedText(string text)
    {
        if (bigRedText)
            bigRedText.text = text;
    }

    public void SetSmallYellowText(int index, string text)
    {
        if (smallYellowText != null && index >= 0 && index < smallYellowText.Length)
        {
            if (smallYellowText[index])
                smallYellowText[index].text = text;
        }
    }

    public void ResetBigRedText()
    {
        if (bigRedText)
            bigRedText.text = "";
    }

    public void ResetSmallYellowText(int index)
    {
        if (smallYellowText != null && index >= 0 && index < smallYellowText.Length)
        {
            if (smallYellowText[index])
                smallYellowText[index].text = "";
        }
    }

    public void ResetAll()
    {
        ResetBigRedText();
        
        for (int i = 0; i < smallYellowText.Length; i++)
        {
            if (smallYellowText[i])
            {
                smallYellowText[i].text = "";
            }
        }
    }
    
    
}

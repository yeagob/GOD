using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PatoCienciaPopup : MonoBehaviour
{
    public TextMeshProUGUI TittleText;

    public void Hide()
    {
        
    }

    public void Show(string tittle)
    {
        gameObject.SetActive(true);
        TittleText.text = tittle;
    }
}

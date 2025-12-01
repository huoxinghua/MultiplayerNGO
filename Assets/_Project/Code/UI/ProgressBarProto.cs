using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   
public class ProgressBarProto : MonoBehaviour
{
    public Image progressBar;
    public float progressAmount = 100f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            IncreaseProgress(10);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            DecreaseProgress(10);
        }
        
    }

    public void DecreaseProgress(float reduceAmount)
    {
        progressAmount -= reduceAmount;
        progressBar.fillAmount = progressAmount / 100f;
    }

    public void IncreaseProgress(float addAmount)
    {
        progressAmount += addAmount;
        progressAmount = Mathf.Clamp(progressAmount, 0, 100);
        progressBar.fillAmount = progressAmount / 100f;
    } 

}

using UnityEngine;
using UnityEngine.UI;

public class FlashLightCharge : MonoBehaviour
{
    public Slider slider;

    public void SetMaxCharge(float charge)
    {
        slider.maxValue = charge;
        slider.value = charge;
    }

    public void SetCharge(float charge)
    {
        slider.value = charge;
    }
}

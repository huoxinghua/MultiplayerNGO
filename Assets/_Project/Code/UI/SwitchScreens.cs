using UnityEngine;

public class SwitchScreens : MonoBehaviour
{

    public GameObject StorePage;
    public GameObject MissionPage;
    
    public void SwitchToStore()
    {
        StorePage.SetActive(true);
        MissionPage.SetActive(false);
    }
    public void SwitchToMission()
    {
        StorePage.SetActive(false);
        MissionPage.SetActive(true);
    }

}

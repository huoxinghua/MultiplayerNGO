using UnityEngine;

public class IKInteractable : MonoBehaviour
{
    [SerializeField] Transform handR;
    [SerializeField] Transform handL;
    [SerializeField] FingerPoseSO fingerSO;

    public void PickupAnimation(PlayerIKController ikController)
    {
        ikController.IKPos(handL, handR, fingerSO);
        ikController.IkActive = true;
    }

    public void DropAnimation(PlayerIKController ikController)
    {
        ikController.IkActive = false;
        ikController.IKPos(null, null, null);
    }
}

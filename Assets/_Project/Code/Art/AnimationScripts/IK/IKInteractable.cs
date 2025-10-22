using UnityEngine;

public class IKInteractable : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Transform handR;
    [SerializeField] Transform handL;

    public void Pickup(PlayerIKController ikController)
    {
        ikController.IKPos(handL, handR);
        ikController.IkActive = true;
    }

    public void Drop(PlayerIKController ikController)
    {
        ikController.IkActive = false;
        ikController.IKPos(null, null);

    }
}

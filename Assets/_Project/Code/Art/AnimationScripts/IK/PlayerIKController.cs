using UnityEngine;

public class PlayerIKController : MonoBehaviour
{
    public IKInteractable interactable;

    [SerializeField] private Animator fpsAnim;
    [SerializeField] private IKInteractableSO ItemSO;
    [SerializeField] private bool ikActive;

    private Transform handL;
    private Transform handR;

    public bool IkActive
    {
        get { return ikActive; }
        set { ikActive = value; }
    }

    private void OnAnimatorIK()
    {
        if (IkActive)
        {
            if (handR != null)
            {

                fpsAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                fpsAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);


                fpsAnim.SetIKPosition(AvatarIKGoal.RightHand, handR.position);
                fpsAnim.SetIKRotation(AvatarIKGoal.RightHand, handR.rotation);
            }
            if (handL != null)
            {

                fpsAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                fpsAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);

                fpsAnim.SetIKPosition(AvatarIKGoal.LeftHand, handL.position);
                fpsAnim.SetIKRotation(AvatarIKGoal.LeftHand, handL.rotation);
            }
        }
        else
        {
            fpsAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
            fpsAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

            fpsAnim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            fpsAnim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

        }
    }

    public void IKPos(Transform handLPos, Transform handRPos)
    {
        handL = handLPos;
        handR = handRPos;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            interactable.Pickup(this);
        }
        if (Input.GetMouseButtonDown(1))
        {
            interactable.Drop(this);
        }
    }
}

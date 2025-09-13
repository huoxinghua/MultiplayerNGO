using UnityEngine;

public class BaseballBatController : MonoBehaviour ,IHeldItem,IInteractable
{
    private BaseballBatModel model;
    private IView view;

    private void Awake()
    {
        model = new BaseballBatModel();
        view = GetComponent<IView>();
    }
    public void Start()
    {
      
    }
    public void OnInteract(GameObject interactingPlayer)
    {
        var inventory = interactingPlayer.GetComponent<Inventory>();
        if (inventory != null && inventory.PickUpItem(gameObject))
        {
            model.SetOwner(interactingPlayer);

            Pickup();
            if (inventory.currentItem == this.gameObject)
            {
                SwapTo();
            }
            else
            {
                SwapOff();
            }
        }
    }
    //for debug
 /*   private void OnDrawGizmosSelected()
    {
        // Set gizmo color
        Gizmos.color = Color.red;

        // Compute attack point same as in PerformMeleeAttack
        Vector3 attackPoint = transform.position + transform.forward * model.GetAttackRange() * 0.5f;

        // Draw sphere where the OverlapSphere would hit
        Gizmos.DrawWireSphere(attackPoint, model.GetAttackRadius());
    }*/
 


    //casts sphere to detect hit collision -> might replace with standard raycast so you have to look at what you hit. May also use a collider instead
void PerformMeleeAttack()
    {
        LayerMask enemyLayer = LayerMask.GetMask("Enemy");

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * model.GetAttackRange() * 0.5f, model.GetAttackRadius(), enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log(enemy.gameObject.name);
          //  enemy.GetComponent<EnemyHealth>().TakeDamage(attackDamage);
        }
    }
    public void Use()
    {
        if (!model.HasOwner || !model.IsInHand) return;
       PerformMeleeAttack();
    }
    public void SwapOff()
    {


        if (!model.HasOwner && view.GetCurrentVisual() == null) return;

        //probably a swap animation here?

        view.DestroyHeldVisual();
        model.InHand(false);
    }
    public void SwapTo()
    {
        if (!model.HasOwner && view.GetCurrentVisual() != null) return;
        //probably a swap animation here?
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        model.InHand(true);
    }
    public void Drop()
    {
        if (!model.HasOwner || !model.IsInHand) return;

        Transform dropPoint = model.Owner.transform.GetChild(3); // or some drop reference
        view.MoveToPosition(dropPoint.position);
        view.DestroyHeldVisual();
        view.SetVisible(true);
        view.SetPhysicsEnabled(true);
        //   view.SetLightEnabled(false); // turn off when dropped. maybe. Might be funnier if they can stay on

        model.ClearOwner();
    }

    public void Pickup()
    {
        view.SetVisible(false);
        view.SetPhysicsEnabled(false);
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        transform.parent = model.Owner.transform.GetChild(0).GetChild(0);
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}

using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //need array to story held item information
    //need to know what is stored and if any are currently in player hand
    //need access to the held items functions
    //likely going to use interfaces and SOs to store relevant information

    //need to add to stored items and the ability to drop them
    //must figure out how to have them in hand for animation/visuals. Ask if it is best to instantiate, have them set active or not, etc
    
        [SerializeField] Transform handTransform;
        public GameObject[] heldItems = new GameObject[5];
        public GameObject currentItem;
        private int currentSlot = 0;

        GameObject twoHandedView;
        GameObject twoHandedObject;


    public void Awake()
    {
        for(int i = 0; i < heldItems.Length; i++)
        {
            heldItems[i] = null;
        }
    }

    //need to fix this a lot. Needs to store all the items, but only show one. The data on the items need to remain somehow. Like if it has been used and cannot be used, ammo, etc? Need this info to stay after being 
    //dropped, swapped, or more. Need to ask Sean if I should have the dropped items be the same as the held items. And with regards to animation of the players hands, should I not actually set the object to be seen, and set the animatin 
    //based on the held object.
    public void EquipItem(GameObject itemPrefab, int slotIndex)
        {
            ClearHand();
        

        //no more instantiate as I need the data to persist on items
     /*       GameObject instance = Instantiate(itemPrefab, handTransform);
            heldItems[slotIndex] = instance;*/
            currentSlot = slotIndex;
        if (heldItems[slotIndex] != null )
        {
            currentItem = heldItems[slotIndex];

        }
        else
        {
            currentItem = null;
        }
        }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseItem();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            DropTwoHanded();
            DropHeldItem();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActivateSelectedSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActivateSelectedSlot(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActivateSelectedSlot(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActivateSelectedSlot(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ActivateSelectedSlot(4);
        }
    }
    public void ActivateSelectedSlot(int index)
    {
        if (twoHandedObject != null || twoHandedView != null) return;
        if (currentSlot >= 0 && heldItems[currentSlot] != null) 
        {
            if (heldItems[currentSlot].GetComponent<IHeldItem>() != null)
            {
                heldItems[currentSlot].GetComponent<IHeldItem>().SwapOff();
            }
        }
      
        currentSlot = index;
        if (heldItems[index] != null)
        {
            if (heldItems[currentSlot].GetComponent<IHeldItem>() != null)
            {
                heldItems[currentSlot].GetComponent<IHeldItem>().SwapTo();
            }
        }
        EquipItem(heldItems[currentSlot], currentSlot);
    }
  
    public bool PickUpTwoHanded(GameObject heldView, GameObject pickedUpObject)
    {
        if(twoHandedObject != null || twoHandedView != null)
        {
            return false;
        }
        if (currentSlot >= 0)
        {
            heldItems[currentSlot]?.GetComponent<IHeldItem>()?.SwapOff();
        }
        currentItem = null;
        twoHandedView = Instantiate(heldView,transform.GetChild(0).GetChild(1));
        twoHandedObject = pickedUpObject;
        currentSlot = -1;
        return true;
    }
    public void DropTwoHanded()
    {
        if (twoHandedObject == null || twoHandedView == null)return;
        
        Destroy(twoHandedView);
        twoHandedView = null;
        twoHandedObject.GetComponent<ITwoHandItem>()?.OnDrop();
        twoHandedObject.transform.position = transform.GetChild(3).position;
        
        twoHandedObject = null;
    }
    public void UseItem()
        {

        //need to add punching here if this is where it is from
            if (currentSlot < 0 || heldItems[currentSlot] == null) return;
      //  Debug.Log("AttemptUse");
            IHeldItem usable = heldItems[currentSlot].GetComponent<IHeldItem>();
            usable?.Use();
        }
    public void DropHeldItem()
    {
        if (currentSlot != -1)
        {
            heldItems[currentSlot]?.GetComponent<IHeldItem>()?.Drop();
            heldItems[currentSlot]?.GetComponent<IHeldItem>()?.SwapOff();
            heldItems[currentSlot] = null;
            currentItem = null;
        }
    }
        private void ClearHand()
        {
            if (currentSlot >= 0 && heldItems[currentSlot] != null)
            {

            //need to switch out hand held model. Still unsure of execution for this

               // Destroy(heldItems[currentSlot]);
            }
        }
        public bool PickUpItem(GameObject itemPrefab)
        {
            if (currentSlot >= 0 && heldItems[currentSlot] == null)
            {
            heldItems[currentSlot] = itemPrefab;
            EquipItem(itemPrefab, currentSlot);
            Debug.Log("Should be");
                return true;
            }

            for (int i = 0; i < heldItems.Length; i++)
            {
                if (heldItems[i] == null)
                {
                heldItems[i] = itemPrefab;
                
                    return true;
                }
            }

           
            Debug.Log("Inventory full");
            return false;
        }
    }


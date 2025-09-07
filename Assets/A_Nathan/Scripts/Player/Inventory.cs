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
    
        public Transform handTransform;
        public GameObject[] heldItems = new GameObject[5];
        private int currentSlot = 0;




    //need to fix this a lot. Needs to store all the items, but only show one. The data on the items need to remain somehow. Like if it has been used and cannot be used, ammo, etc? Need this info to stay after being 
    //dropped, swapped, or more. Need to ask Sean if I should have the dropped items be the same as the held items. And with regards to animation of the players hands, should I not actually set the object to be seen, and set the animatin 
    //based on the held object.
        public void EquipItem(GameObject itemPrefab, int slotIndex)
        {
            ClearHand();

            GameObject instance = Instantiate(itemPrefab, handTransform);
            heldItems[slotIndex] = instance;
            currentSlot = slotIndex;
        }
    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseItem();
        }
    }
    public void UseItem()
        {

        //need to add punching here if this is where it is from
            if (currentSlot < 0 || heldItems[currentSlot] == null) return;
        Debug.Log("AttemptUse");
            var usable = heldItems[currentSlot].GetComponent<IHeldItem>();
            usable?.Use();
        }

        private void ClearHand()
        {
            if (currentSlot >= 0 && heldItems[currentSlot] != null)
            {
                Destroy(heldItems[currentSlot]);
            }
        }
        public bool PickUpItem(GameObject itemPrefab)
        {
            if (currentSlot >= 0 && heldItems[currentSlot] == null)
            {
                EquipItem(itemPrefab, currentSlot);
            heldItems[currentSlot] = itemPrefab;
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


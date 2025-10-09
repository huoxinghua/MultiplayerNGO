using System.Collections.Generic;
using UnityEngine;

public class SampleJarController : MonoBehaviour, IHeldItem, IInteractable
{
    private SampleJarModel model;
    private IView view;
    private Dictionary<string, List<SampleData>> samplesContainer = new Dictionary<string, List<SampleData>>();
    private bool isGetSample = false;
    private float detectDistance = 50f;
    // [SerializeField] private LayerMask lm;

    private void Awake()
    {
        model = new SampleJarModel();
        view = GetComponent<IView>();
    }
    public void Drop()
    {
        if (!model.HasOwner || !model.IsInHand) return;

        Transform dropPoint = model.Owner.transform.GetChild(1);    
        view.MoveToPosition(dropPoint.position);
        view.DestroyHeldVisual();
        view.SetVisible(true);
        view.SetPhysicsEnabled(true);


        model.ClearOwner();
    }
    public void Use()
    {
        if (!model.HasOwner || !model.IsInHand) return;
        Debug.Log("use Sample jar");
        model.Toggle();
        DetectSample();
    }

    private void DetectSample()
    {
        RaycastHit hit;
        Debug.Log("DetectSample");
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, detectDistance))
        {
            Debug.Log("DetectSample inside");
            Debug.Log("hit" + hit.transform.ToString());
            var sample = hit.transform.GetComponent<SampleObjTest>();
            if (sample != null)
            {
                var currentSample = sample.GetSample();
                SaveSample(currentSample);
                isGetSample = true;
            }
            if (sample.gameObject != null)
            {
                Destroy(sample.gameObject);
            }

        }

    }

    public void SaveSample(SampleSO value)
    {
        Debug.Log("Save sample:" + value.name + "money:" + value.GetRandomMoneyValue() + "research" + value.GetRandomResearchValue());
        SampleData data = new SampleData(value.GetRandomResearchValue(),
        value.GetRandomMoneyValue());
        if (!samplesContainer.ContainsKey(value.SampleType))
        {
            samplesContainer[value.SampleType] = new List<SampleData>();
        }
        samplesContainer[value.SampleType].Add(data);
        Debug.Log("sample container:" + samplesContainer.Count);
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

    public void Pickup()
    {
        view.SetVisible(true);
        view.SetPhysicsEnabled(false);
        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        transform.parent = model.Owner.transform.GetChild(0).GetChild(0);
        transform.localPosition = new Vector3(0, 0, 0);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
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

        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        model.InHand(true);
    }



}

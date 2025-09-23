using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SampleJarController : MonoBehaviour, IHeldItem, IInteractable
{
    private SampleJarModel model;
    private IView view;
    private Dictionary<string, List<SampleData>> samplesContainer = new Dictionary<string, List<SampleData>>();
    private bool isGetSample = false;
    private void Awake()
    {
        model = new SampleJarModel();
        view = GetComponent<IView>();
    }
    public void Drop()
    {
        if (!model.HasOwner || !model.IsInHand) return;

        Transform dropPoint = model.Owner.transform.GetChild(3);
        view.MoveToPosition(dropPoint.position);
        view.DestroyHeldVisual();
        view.SetVisible(true);
        view.SetPhysicsEnabled(true);


        model.ClearOwner();
    }
    public void Use()
    {
        view.SetPhysicsEnabled(true);
        if (!model.HasOwner || !model.IsInHand) return;
        Debug.Log("use Sample jar");
        model.Toggle();

    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(" sample jar trigger sth");
        var sampleObj = other.GetComponent<ISampleable>();
        if (sampleObj != null)
        {
            Debug.Log(" sample jar trigger sample");
            var currentSample = sampleObj.GetSample();
            SaveSample(currentSample);
            isGetSample = true;
            Destroy(other.gameObject, 2f);
        }
    }
  
    public void SaveSample(SampleSO value)
    {
        Debug.Log("Save sample:"+ value.name);
        SampleData data = new SampleData(value.GetRandomResearchValue(),
        value.GetRandomMoneyValue());
        if (!samplesContainer.ContainsKey(value.sampleType))
        {
            samplesContainer[value.sampleType] = new List<SampleData>();
        }
        samplesContainer[value.sampleType].Add(data);
        Debug.Log("sample container:"+ samplesContainer.Count);
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
        throw new System.NotImplementedException();
    }

    public void SwapTo()
    {
        if (!model.HasOwner && view.GetCurrentVisual() != null) return;

        view.DisplayHeld(model.Owner.transform.GetChild(0).GetChild(0));
        model.InHand(true);
    }



}

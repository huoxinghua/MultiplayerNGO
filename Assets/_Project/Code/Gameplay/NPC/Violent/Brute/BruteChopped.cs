using UnityEngine;

public class BruteChopped : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        foreach(Collider children in gameObject.GetComponentsInChildren<Collider>())
        {
            children.transform.parent = null;
        }
        Destroy(gameObject);
    }
}

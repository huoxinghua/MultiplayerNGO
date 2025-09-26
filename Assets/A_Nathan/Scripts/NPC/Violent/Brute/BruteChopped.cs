using UnityEngine;

public class BruteChopped : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Awake()
    {
        foreach(GameObject children in gameObject.GetComponentsInChildren<GameObject>())
        {
            children.transform.parent = null;
        }
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

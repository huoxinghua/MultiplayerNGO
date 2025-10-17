using UnityEngine;

public class BeetleTest : MonoBehaviour
{
    [SerializeField] private BeetleAnimation beetleAnim;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            beetleAnim.PlayWalk(10, 10);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            beetleAnim.PlayRun(10, 10);
        }
    }
}

using UnityEngine;

public class WorldParentController : MonoBehaviour
{
    void OnEnable()
    {
        var movers = GetComponentsInChildren<WorldChildMover>(true);

        foreach (var mover in movers)
        {
            mover.StartMoving();
        }
    }
}
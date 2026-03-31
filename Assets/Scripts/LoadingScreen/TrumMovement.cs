using UnityEngine;

public class TramMovement : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float speed = 2f;

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, endPoint.position) < 0.01f)
        {
            transform.position = startPoint.position; 
        }
    }
}
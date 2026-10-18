using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0, 4, -7);
    [SerializeField] float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 wanted = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position, wanted, smoothSpeed * Time.deltaTime);
        transform.LookAt(target);
    }
}
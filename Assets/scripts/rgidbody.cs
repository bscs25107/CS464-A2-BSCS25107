using UnityEngine;


public class rgidbody: MonoBehaviour
{
    [SerializeField] float speed = 5f;
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        rb.linearVelocity = new Vector3(
            h * speed, rb.linearVelocity.y, v * speed);
    }
}
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class player : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpForce = 6f;
    Rigidbody rb;
    bool isGrounded, jumpQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;
    }
    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0, v);
        move = Vector3.ClampMagnitude(move, 1f) * speed;
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);
        if (jumpQueued)
        {
            rb.AddForce(Vector3.up * jumpForce,
                        ForceMode.Impulse);
            jumpQueued = false;
        }
    }

    void OnCollisionStay(Collision c)
    {
        if (c.GetContact(0).normal.y > 0.5f)
            isGrounded = true;
    }

    void OnCollisionExit(Collision c) => isGrounded = false;
}
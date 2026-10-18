using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class player : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;      

    [Header("Jump feel (Part B)")]
    [SerializeField] float coyoteTime = 0.1f;      
    [SerializeField] float jumpBuffer = 0.12f;     
    [SerializeField] float fallMultiplier = 2.2f;

    [Header("Ground check")]
    [SerializeField] float checkRadius = 0.3f;
    [SerializeField] LayerMask groundMask;         

    [Header("Juice: squash on landing")]
    [SerializeField] Transform body;               
    [SerializeField] float squashDuration = 0.2f; 
    [SerializeField] float squashY = 0.7f;

    Rigidbody rb;
    Transform cam;
    Vector3 move;
    bool isGrounded, wasGrounded, jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f;
    float airTime;
    Coroutine squashRoutine;

    Vector3 FeetPos => feet != null ? feet.position : transform.position + Vector3.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        cam = Camera.main.transform;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 f = cam.forward; f.y = 0f; f.Normalize();
        Vector3 r = cam.right; r.y = 0f; r.Normalize();
        move = Vector3.ClampMagnitude(f * v + r * h, 1f);

        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;
        if (canJump && wantJump && !jumpQueued)
        {
            jumpQueued = true;
            lastGroundedTime = lastPressedTime = -1f; 
        }

        if (!isGrounded) airTime += Time.deltaTime;
        if (isGrounded && !wasGrounded)
        {
            if (airTime > 0.1f) StartSquash();
            airTime = 0f;
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(FeetPos, checkRadius, groundMask);

        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);  
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
            jumpQueued = false;
        }

        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
    }

    void StartSquash()
    {
        if (body == null) return;
        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(Squash());
    }

    IEnumerator Squash()
    {
        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);
            float y = Mathf.LerpUnclamped(squashY, 1f, p);   
            float xz = 1f / Mathf.Sqrt(y);                 
            body.localScale = new Vector3(xz, y, xz);
            yield return null;
        }
        body.localScale = Vector3.one;                    
        squashRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Application.isPlaying && isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(FeetPos, checkRadius);
    }
}

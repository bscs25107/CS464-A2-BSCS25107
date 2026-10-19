using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpHeight = 1.8f;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform feet;
    [SerializeField] private float checkRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;

    [Header("Jump Assist Settings (Part B)")]
    [SerializeField] private float coyoteTime = 0.15f;    // Time window after leaving ground
    [SerializeField] private float jumpBufferTime = 0.15f; // Time window before landing

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded;
    private Transform camTransform;

    // Timers
    private float coyoteTimer;
    private float jumpBufferTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        // 1. Movement Inputs
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // 2. Camera-Relative Direction
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (camTransform != null)
        {
            forward = camTransform.forward;
            right = camTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        moveInput = Vector3.ClampMagnitude(forward * vertical + right * horizontal, 1f);

        // 3. Ground Detection & Coyote Time Timer
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime; // Reset grace period while grounded
        }
        else
        {
            coyoteTimer -= Time.deltaTime; // Count down when airborne
        }

        // 4. Jump Input & Buffer Timer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime; // Register input intent
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime; // Count down buffer
        }
    }

    private void FixedUpdate()
    {
        // Maintain horizontal control while retaining vertical physics
        rb.linearVelocity = new Vector3(moveInput.x * speed, rb.linearVelocity.y, moveInput.z * speed);

        // Execute Jump if BOTH coyote time and jump buffer are valid
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            ExecuteJump();
        }
    }

    private void ExecuteJump()
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float v0 = Mathf.Sqrt(2f * g * jumpHeight);

        // Reset vertical velocity prior to impulse
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * (rb.mass * v0), ForceMode.Impulse);

        // Consume timers to prevent double-jumping
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (feet != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(feet.position, checkRadius);
        }
    }
}
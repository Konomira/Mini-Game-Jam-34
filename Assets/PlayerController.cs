using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement parameters
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float wallCheckDistance = 0.5f;
    public float groundCheckDistance = 1f;

    // Jump control parameters
    public float lowJumpMultiplier = 2f;
    public float fallMultiplier = 2.5f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isDashing, canDashJump;
    private bool canDash = true;
    private float dashTime;
    private Vector2 dashDirection;
    private bool isTouchingWall;

    // Coyote time parameters
    public float coyoteTime = 0.2f;  // Time allowed to jump after leaving a platform
    private float coyoteTimeCounter;

    // Layers
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Check if grounded
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (isGrounded)
            canDashJump = false;
        // Check if touching a wall
        isTouchingWall = Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, wallCheckDistance, wallLayer);

        // Handle Coyote Time (Grace Period for Jumping)
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;  // Reset coyote time when grounded
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;  // Count down coyote time
        }


        // Movement input
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (!isDashing)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            // Jumping
            if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }

            // Start dash
            if (canDash && Input.GetButtonDown("Dash"))
            {
                canDashJump = true;
                isDashing = true;
                dashTime = dashDuration;
                dashDirection = new Vector2(moveInput, 0).normalized;

                if (dashDirection == Vector2.zero)
                {
                    dashDirection = new Vector2(transform.localScale.x, 0);
                }

                rb.velocity = dashDirection * dashSpeed;
            }
        }
        else
        {
            if (dashTime > 0)
            {
                rb.velocity = dashDirection * dashSpeed;
                dashTime -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
                canDash = false;
            }
        }

        if(canDashJump)
        {
            if (Input.GetButtonDown("Jump"))
            {
                canDashJump = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                dashTime = 0;
            }
        }

        // Apply jump control for better feel
        if (rb.velocity.y < 0)
        {
            // Apply extra gravity when falling
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            // Apply less gravity when the player releases the jump button mid-jump
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Reset dash after landing
        if (isGrounded)
        {
            canDash = true;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("KillPlane"))
            transform.position = Vector3.zero;

        // Prevent wall sticking by adjusting velocity
        if (isTouchingWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }
}

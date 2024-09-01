using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Movement parameters
    public float acceleration = 5f;
    public float maxMoveSpeed = 5f;
    private float currentSpeed = 0f;
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

    AudioSource source;
    MusicManager manager;

    void Start()
    {
        if(source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.volume = 0.45f;
        manager = FindObjectOfType<MusicManager>();
        rb = GetComponent<Rigidbody2D>();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Droplets"), true);

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
            if (moveInput != 0)
            {
                // Accelerate towards max speed
                currentSpeed = maxMoveSpeed;// Mathf.MoveTowards(currentSpeed, maxMoveSpeed * moveInput, acceleration * Time.deltaTime);
            }
            else
            {
                // Decelerate to zero at twice the acceleration rate
                currentSpeed = 0;// Mathf.MoveTowards(currentSpeed, 0, acceleration * 4f * Time.deltaTime);
            }

            rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

            // Jumping
            if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                source.PlayOneShot(manager.jump);
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

        if (canDashJump)
        {
            if (Input.GetButtonDown("Jump"))
            {
                canDashJump = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                source.pitch = Random.Range(0.95f, 1.05f);
                source.PlayOneShot(manager.jump);

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
            Respawn();

        // Prevent wall sticking by adjusting velocity
        if (isTouchingWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void Respawn()
    {
        // Example respawn logic
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //KetchupHealthController healthController = GetComponent<KetchupHealthController>();
        //if (healthController != null)
        //{
        //    healthController.currentHealth = healthController.maxHealth;
        //    healthController.UpdateSprite();
        //}

        //// Enable collection for all droplets
        //KetchupDroplet[] droplets = FindObjectsOfType<KetchupDroplet>();
        //foreach (KetchupDroplet droplet in droplets)
        //{
        //    droplet.EnableCollection();
        //}
    }

    // Public getters for the KetchupHealthController
    public bool IsDashing
    {
        get { return isDashing; }
    }

    public float DashTime
    {
        get { return dashTime; }
    }

    public float DashDuration
    {
        get { return dashDuration; }
    }

    public Rigidbody2D PlayerRigidbody
    {
        get { return rb; }
    }

    public float CurrentSpeed
    {
        get { return currentSpeed; }
    }
}

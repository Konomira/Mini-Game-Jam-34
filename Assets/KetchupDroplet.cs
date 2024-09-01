using UnityEngine;

public class KetchupDroplet : MonoBehaviour
{
    public float size;
    public float deformAmount = 0.8f;  // Amount to flatten the droplet upon impact
    public float combineDistance = 1.0f;  // Max distance within which droplets should gravitate together
    public float gravityForce = 2f;  // Force used to pull droplets together

    private bool isMerging = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        DisableCollection();  // Initially disable interaction with the player
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            StickToSurface(collision.contacts[0].point);
            StopMovement();  // Stop any residual movement to ensure the droplet sticks
        }
    }

    void Update()
    {
        if (!isMerging)
        {
            FindAndCombineNearbyDroplets();
        }
    }

    void StickToSurface(Vector2 contactPoint)
    {
        // Flatten the droplet when it sticks to a surface
        Vector3 scale = transform.localScale;
        scale.y *= deformAmount;  // Flatten vertically
        scale.x *= 1.0f + (1.0f - deformAmount);  // Slightly widen horizontally to conserve volume
        transform.localScale = scale;

        // Adjust the droplet's position to better align with the contact point
        transform.position = new Vector3(transform.position.x, contactPoint.y + (scale.y * 0.5f), transform.position.z);

        StopMovement();
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;  // Stop any rotational movement
        }
    }

    void FindAndCombineNearbyDroplets()
    {
        Collider2D[] nearbyDroplets = Physics2D.OverlapCircleAll(transform.position, combineDistance);

        foreach (Collider2D collider in nearbyDroplets)
        {
            KetchupDroplet otherDroplet = collider.GetComponent<KetchupDroplet>();

            if (otherDroplet != null && otherDroplet != this && !otherDroplet.isMerging)
            {
                float distance = Vector2.Distance(otherDroplet.transform.position, transform.position);

                // Check if close enough to combine
                if (distance < 0.2f)  // Adjust this threshold as needed
                {
                    CombineDroplets(otherDroplet);
                }
                else
                {
                    GravitateTowards(otherDroplet);  // Apply gravitational pull if not close enough
                }
            }
        }
    }

    void CombineDroplets(KetchupDroplet otherDroplet)
    {
        isMerging = true;
        otherDroplet.isMerging = true;

        // Calculate the combined size based on area
        float combinedArea = size + otherDroplet.size;
        float newRadius = Mathf.Sqrt(combinedArea / Mathf.PI);

        // Scale this droplet to the new size
        transform.localScale = new Vector3(newRadius, newRadius, 1f);
        size = combinedArea;

        // Optionally, adjust the mass of the combined droplet if using mass-based physics
        if (rb != null)
        {
            rb.mass += otherDroplet.rb.mass;  // Add masses together
        }

        Destroy(otherDroplet.gameObject);

        isMerging = false;
    }

    void GravitateTowards(KetchupDroplet otherDroplet)
    {
        Vector2 direction = (otherDroplet.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(otherDroplet.transform.position, transform.position);

        if (distance > 0 && distance < 20)
        {
            // Increase the pull force to make it stronger
            float pullStrength = gravityForce * (1 / distance);
            Vector2 pullForce = direction * pullStrength;

            // Apply the force to move the droplet towards the other
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(pullForce);
            }

            // Optional: Slow down as they get very close to reduce overshooting
            if (distance < 0.1f)
            {
                rb.velocity *= 0.5f;
            }
        }
    }

    public void Initialize(float initialSize)
    {
        size = initialSize;
        float radius = Mathf.Sqrt(initialSize / Mathf.PI);
        transform.localScale = new Vector3(radius, radius, 1f);
    }

    public void DisableCollection()
    {
        gameObject.layer = LayerMask.NameToLayer("Droplets");
        if (rb != null)
        {
            rb.isKinematic = true;  // Disable physics interaction initially
        }
    }

    public void EnableCollection()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        if (rb != null)
        {
            rb.isKinematic = false;  // Re-enable physics interaction when necessary
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            KetchupHealthController healthController = other.GetComponent<KetchupHealthController>();
            if (healthController != null)
            {
                float healthRestored = size * 0.5f;  // Example: Half the droplet's size is restored as health
                healthController.RestoreHealth(healthRestored);
            }

            Destroy(gameObject);
        }
    }
}

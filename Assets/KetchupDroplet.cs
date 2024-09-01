using UnityEngine;

public class KetchupDroplet : MonoBehaviour
{
    public float size;
    public float deformAmount = 0.8f;
    public float combineDistance = 1.0f;

    private bool isMerging = false;

    void Start()
    {
        DisableCollection();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            DeformDroplet(collision.contacts[0].point);
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = true;  // Ensure physics is disabled after hitting the ground
            }
        }
    }

    void Update()
    {
        if (!isMerging)
        {
            FindAndCombineNearbyDroplets();
        }
    }

    void DeformDroplet(Vector2 contactPoint)
    {
        Vector3 scale = transform.localScale;
        scale.y *= deformAmount;
        scale.x *= 1.0f + (1.0f - deformAmount);
        transform.localScale = scale;
        transform.position = new Vector3(transform.position.x, contactPoint.y + (scale.y * 0.5f), transform.position.z);
    }

    void FindAndCombineNearbyDroplets()
    {
        Collider2D[] nearbyDroplets = Physics2D.OverlapCircleAll(transform.position, combineDistance);

        foreach (Collider2D collider in nearbyDroplets)
        {
            KetchupDroplet otherDroplet = collider.GetComponent<KetchupDroplet>();

            if (otherDroplet != null && otherDroplet != this && !otherDroplet.isMerging)
            {
                CombineDroplets(otherDroplet);
                break;
            }
        }
    }

    void CombineDroplets(KetchupDroplet otherDroplet)
    {
        isMerging = true;
        otherDroplet.isMerging = true;

        float combinedArea = size + otherDroplet.size;
        float newRadius = Mathf.Sqrt(combinedArea / Mathf.PI);

        transform.localScale = new Vector3(newRadius, newRadius, 1f);
        size = combinedArea;

        Destroy(otherDroplet.gameObject);

        isMerging = false;
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
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public void EnableCollection()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;  // Re-enable physics interaction
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            KetchupHealthController healthController = other.GetComponent<KetchupHealthController>();
            if (healthController != null)
            {
                float healthRestored = size * 0.5f; // Example: Half the droplet's size is restored as health
                healthController.RestoreHealth(healthRestored);
            }

            Destroy(gameObject);
        }
    }
}

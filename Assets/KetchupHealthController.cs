using UnityEngine;
using UnityEngine.SceneManagement;

public class KetchupHealthController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public string spriteSheetName = "Art/Sprites/Ketchup-Talking-Red-Layer-sheet";
    public PlayerController playerController;
    public DropletSpawner dropletSpawner;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float distancePerHealth = 10f;   // Distance needed to decrease health by 1 unit
    public float jumpDashPenalty = 20f;     // Penalty for jumping or dashing (in health units)

    private Sprite[] healthSprites;
    private Vector3 lastPosition;
    private float distanceTraveled;         // Accumulate distance traveled

    void Start()
    {
        healthSprites = Resources.LoadAll<Sprite>(spriteSheetName);

        if (healthSprites == null || healthSprites.Length == 0)
        {
            Debug.LogError($"No sprites found in the sprite sheet named '{spriteSheetName}'. Please ensure the path is correct and the sprite sheet is sliced properly.");
            return;
        }

        currentHealth = maxHealth;
        lastPosition = playerController.transform.position;
        UpdateSprite();  // Call UpdateSprite to set the initial sprite based on full health
    }

    void Update()
    {
        // Calculate distance traveled since last frame
        distanceTraveled += Vector3.Distance(playerController.transform.position, lastPosition);
        lastPosition = playerController.transform.position;

        // Decrease health by 1 unit for every 'distancePerHealth' traveled
        if (distanceTraveled >= distancePerHealth)
        {
            int healthLoss = Mathf.FloorToInt(distanceTraveled / distancePerHealth);
            distanceTraveled -= healthLoss * distancePerHealth;  // Reset the accumulated distance
            DecreaseHealth(healthLoss);
        }

        // Check for Jump and Dash actions
        if (Input.GetButtonDown("Jump"))
        {
            DecreaseHealth((int)jumpDashPenalty);
        }

        if (playerController.IsDashing && playerController.DashTime == playerController.DashDuration)
        {
            DecreaseHealth((int)jumpDashPenalty);
        }
    }

    public void DecreaseHealth(int amount)
    {
        // Decrease health and spawn droplets
        for (int i = 0; i < amount; i++)
        {
            if (currentHealth > 0)
            {
                currentHealth -= 1;
                SpawnDroplets(1);
                UpdateSprite();  // Update the sprite whenever health changes
            }
        }
    }

    public void SpawnDroplets(float healthLoss)
    {
        dropletSpawner.SpawnDroplet(Vector2.zero, healthLoss);  // Pass the health loss and let the spawner handle the rest
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateSprite();  // Update the sprite whenever health is restored
    }

    public void Respawn()
    {
        transform.position = Vector3.zero; // Reset position
        currentHealth = maxHealth;
        UpdateSprite();

        // Enable collection for the droplets created during the last cycle
        dropletSpawner.EnableCollectionForCurrentCycleDroplets();

        // Re-enable collisions between the Player and Droplets layers
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Droplets"), false);
    }

    public void UpdateSprite()
    {
        float healthPercentage = currentHealth / maxHealth;
        healthPercentage = Mathf.Clamp01(healthPercentage);

        int spriteIndex = Mathf.RoundToInt((healthSprites.Length - 1) * (1 - healthPercentage));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, healthSprites.Length - 1);

        spriteRenderer.sprite = healthSprites[spriteIndex];

        if (healthPercentage < 0.1f)
            FindObjectOfType<PlayerController>().Respawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentHealth = maxHealth;
        if(collision.CompareTag("Goal"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        Destroy(collision.gameObject);
        UpdateSprite();


    }
}

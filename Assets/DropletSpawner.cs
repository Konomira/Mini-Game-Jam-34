using UnityEngine;
using System.Collections.Generic;

public class DropletSpawner : MonoBehaviour
{
    public GameObject ketchupDropletPrefab;   // Reference to the droplet prefab
    public Transform spawnPoint;              // The point on the player where droplets are fired from
    public float launchForce = 2f;            // Base force with which droplets are fired
    public float viscosityFactor = 0.5f;      // Factor to reduce the force for viscosity effect
    public float angleRange = 45f;            // Range for random angles (-45 to 45 degrees)

    private List<KetchupDroplet> currentCycleDroplets = new List<KetchupDroplet>(); // List to track current cycle droplets

    public void SpawnDroplet(Vector2 spawnPosition, float healthLoss)
    {
        // Instantiate a new droplet at the spawn point
        GameObject droplet = Instantiate(ketchupDropletPrefab, spawnPoint.position, Quaternion.identity);

        // Initialize the droplet with the size based on the health lost
        float sizeFactor = Mathf.Sqrt(healthLoss);  // Square root to relate area to diameter
        KetchupDroplet dropletScript = droplet.GetComponent<KetchupDroplet>();
        if (dropletScript != null)
        {
            dropletScript.Initialize(sizeFactor);
            currentCycleDroplets.Add(dropletScript);  // Track this droplet
        }

        // Apply an upward force with a random angle, reduced by viscosityFactor to simulate thickness
        Rigidbody2D rb = droplet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Calculate a random angle between -angleRange and +angleRange degrees
            float randomAngle = Random.Range(-angleRange, angleRange);
            Vector2 launchDirection = Quaternion.Euler(0, 0, randomAngle) * Vector2.up; // Rotate the up vector by the random angle
            rb.AddForce(launchDirection * launchForce * viscosityFactor / sizeFactor, ForceMode2D.Impulse);
        }
    }

    public void EnableCollectionForCurrentCycleDroplets()
    {
        foreach (KetchupDroplet droplet in currentCycleDroplets)
        {
            if (droplet != null)
            {
                droplet.EnableCollection();
            }
        }
        currentCycleDroplets.Clear(); // Clear the list after enabling collection
    }
}

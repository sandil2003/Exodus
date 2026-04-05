using UnityEngine;

public class DroneCollision : MonoBehaviour
{
    public int damage = 10;
    public float damageCooldown = 1.5f;
    private float nextDamageTime = 0f;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();

                if (health != null)
                {
                    health.TakeDamage(damage);
                    nextDamageTime = Time.time + damageCooldown;
                }
            }
        }
    }
}
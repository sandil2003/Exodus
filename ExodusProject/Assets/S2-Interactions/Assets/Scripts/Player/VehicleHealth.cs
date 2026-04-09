using UnityEngine;

public class VehicleHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    private HUDManager _hud;
    private bool _dead = false;

    void Start()
    {
        currentHealth = maxHealth;
        _hud = FindObjectOfType<HUDManager>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("DroneNPC") && !_dead)
        {
            TakeDamage(25);
        }
    }

    void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        _hud.UpdateHealth(currentHealth, maxHealth);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        _dead = true;
        FindObjectOfType<WinLoseManager>().TriggerLose("health");
    }
}
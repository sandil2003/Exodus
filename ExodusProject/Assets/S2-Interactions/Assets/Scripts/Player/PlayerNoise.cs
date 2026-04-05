using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    public float walkNoise = 2f;
    public float sprintNoise = 6f;

    public LayerMask enemyLayer;

    void Update()
    {
        float noiseRadius = 0f;

        if (Input.GetKey(KeyCode.LeftShift))
            noiseRadius = sprintNoise;
        else
            noiseRadius = walkNoise;

        Collider[] enemies = Physics.OverlapSphere(transform.position, noiseRadius, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            // future: alert hover car
            Debug.Log("Enemy hears player!");
        }
    }
}
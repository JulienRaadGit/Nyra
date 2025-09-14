using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamage : MonoBehaviour
{
    public int touchDamage = 5;
    public float tickInterval = 0.5f;
    float nextTick;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time < nextTick) return;

        var stats = other.GetComponent<PlayerStats>();
        if (stats) stats.TakeDamage(touchDamage);

        nextTick = Time.time + tickInterval;
    }
}

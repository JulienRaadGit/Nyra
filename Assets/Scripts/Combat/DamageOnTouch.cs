using UnityEngine;

/// <summary>
/// Generic component that applies damage to objects implementing the Damageable class
/// when this collider enters their trigger. It is designed to be attached to any
/// weapon sub-object (e.g. orbiting orbs) that uses a trigger collider. Damage
/// values assigned to this component will be scaled by PlayerStats.damageMult
/// automatically on hit. This script ignores collisions with the player and its
/// children when an owner is specified.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [Tooltip("Base damage dealt when colliding. This value will be multiplied by the player's damage multiplier if available.")]
    public float damage = 1f;

    [Tooltip("Reference to the Orbit weapon that owns this orb. Used to ignore the player.")]
    public Orbit owner;

    void Reset()
    {
        // Ensure collider is a trigger and has a kinematic rigidbody for trigger events
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.isKinematic = true;
        rb.simulated = true;
        rb.gravityScale = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore collisions with triggers or self
        if (!other || other.isTrigger) return;
        // If an owner is set, ignore collisions with the player or any of its children
        if (owner != null && owner.player != null)
        {
            if (other.transform.IsChildOf(owner.player)) return;
        }
        // Try to apply damage via Damageable component
        if (other.TryGetComponent<Damageable>(out var dmg))
        {
            float mult = PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f;
            dmg.Take(damage * mult);
        }
    }
}
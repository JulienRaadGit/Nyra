using UnityEngine;

/// À mettre sur le prefab "Orbits".
/// Requis : CircleCollider2D (isTrigger=true), Rigidbody2D (Kinematic, Gravity=0).
[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    public float damage = 10f;

    [Tooltip("Référence à l’arme (OrbitWeapon) pour ignorer le joueur.")]
    public Orbit owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other || other.isTrigger) return;

        // ignorer le joueur et ses enfants
        if (owner != null && owner.player != null && other.transform.IsChildOf(owner.player))
            return;

        // --- même esprit que tes autres armes : on essaye les méthodes courantes sans interface ---

        // 1) EnemyHealth.TakeDamage(float)
        var enemyHealth = other.GetComponent("EnemyHealth");
        if (enemyHealth != null)
        {
            var mi = enemyHealth.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(float) });
            if (mi != null) { mi.Invoke(enemyHealth, new object[] { damage }); return; }
        }

        // 2) Health.ApplyDamage(float)
        var health = other.GetComponent("Health");
        if (health != null)
        {
            var mi = health.GetType().GetMethod("ApplyDamage", new System.Type[] { typeof(float) });
            if (mi != null) { mi.Invoke(health, new object[] { damage }); return; }
        }

        // 3) Enemy/Unit/Whatever.Hit(float)
        var enemy = other.GetComponent("Enemy");
        if (enemy != null)
        {
            var mi = enemy.GetType().GetMethod("Hit", new System.Type[] { typeof(float) });
            if (mi != null) { mi.Invoke(enemy, new object[] { damage }); return; }
        }

        // 4) Dernier recours : cherche n’importe quel composant avec TakeDamage(float)
        var comps = other.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            var mi = c.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(float) });
            if (mi != null) { mi.Invoke(c, new object[] { damage }); return; }
        }
    }
}

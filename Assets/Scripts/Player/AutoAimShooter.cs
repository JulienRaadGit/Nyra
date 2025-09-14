using UnityEngine;

public class AutoAimShooter : MonoBehaviour {
    public Projectile projectilePrefab;
    public float fireRate = 2f;   // tirs/seconde (base)
    public float range = 10f;

    float cooldown;

    void Update() {
        cooldown -= Time.deltaTime;
        if (cooldown > 0) return;

        Transform target = FindClosestEnemy();
        if (!target) return;

        Vector2 dir = (target.position - transform.position).normalized;
        var proj = Instantiate(projectilePrefab);
        proj.Fire(transform.position, dir);

        // cooldown effectif avec stats
        float cdMult = PlayerStats.Instance ? PlayerStats.Instance.cooldownMult : 1f;
        float effectiveRate = fireRate / Mathf.Max(0.05f, cdMult); // cdMult<1 => plus rapide
        cooldown = 1f / Mathf.Max(0.01f, effectiveRate);
    }

    Transform FindClosestEnemy() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float min = Mathf.Infinity, r2 = range * range;

        foreach (var e in enemies) {
            float d = (e.transform.position - transform.position).sqrMagnitude;
            if (d < r2 && d < min) { min = d; closest = e.transform; }
        }
        return closest;
    }
}

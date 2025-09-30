using System.Collections;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    [Header("Stats par niveau (index = lvl-1)")]
    public float[] intervalPerLevel = { 2.0f, 1.8f, 1.6f, 1.4f, 1.2f, 1.0f };
    public int[]   targetsPerLevel  = { 1, 1, 2, 2, 3, 4 };
    public int[]   damagePerLevel   = { 20, 28, 36, 45, 55, 70 };

    [Header("Portée & Layer")]
    public float radius = 12f;
    public LayerMask enemyMask;

    [Header("Visuel optionnel")]
    public GameObject lightningVfxPrefab;   // <-- NOM aligné avec WeaponManager

    [Header("Etat")]
    public bool IsEvolved = false;          // <-- utilisé par WeaponManager

    private int level = 0;
    private Coroutine loop;

    public void SetLevel(int newLevel)
    {
        int max = Mathf.Min(intervalPerLevel.Length, targetsPerLevel.Length, damagePerLevel.Length);
        level = Mathf.Clamp(newLevel, 1, Mathf.Max(1, max));
        RestartLoop();
    }

    public void Upgrade() => SetLevel(level + 1);

    private void OnEnable()  => RestartLoop();
    private void OnDisable() { if (loop != null) StopCoroutine(loop); loop = null; }

    private void RestartLoop()
    {
        if (!isActiveAndEnabled) return;
        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(StrikeLoop());
    }

    private IEnumerator StrikeLoop()
    {
        while (true)
        {
            int idx = Mathf.Clamp(level - 1, 0, intervalPerLevel.Length - 1);
            float interval = intervalPerLevel[idx];
            yield return new WaitForSeconds(interval);

            int dmg     = damagePerLevel[Mathf.Clamp(level - 1, 0, damagePerLevel.Length - 1)];

            var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
            if (hits == null || hits.Length == 0) continue;

            // Choisir la cible la plus proche
            Collider2D best = null;
            float bestDist = float.MaxValue;
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (!h) continue;
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = h;
                }
            }

            if (best == null) continue;

            // Appliquer les dégâts UNIQUEMENT à cette cible
            ApplyDirectDamage(best.gameObject, dmg);

            if (lightningVfxPrefab)
            {
                var vfx = Instantiate(lightningVfxPrefab, best.transform.position, Quaternion.identity);
                Destroy(vfx, 0.25f);
            }
        }
    }

    private void ApplyDirectDamage(GameObject target, int dmg)
    {
        // Damageable support
        if (target.TryGetComponent<Damageable>(out var damageable))
        {
            float mult = PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f;
            damageable.Take(dmg * mult);
            return;
        }

        // EnemyHealth support
        if (target.TryGetComponent<EnemyHealth>(out var eh))
        {
            eh.TakeDamage(dmg);
            return;
        }

        // Fallback: tenter sur les parents
        var parentDamageable = target.GetComponentInParent<Damageable>();
        if (parentDamageable != null)
        {
            float mult = PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f;
            parentDamageable.Take(dmg * mult);
            return;
        }
        var parentEh = target.GetComponentInParent<EnemyHealth>();
        if (parentEh != null)
        {
            parentEh.TakeDamage(dmg);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}

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

            int targets = targetsPerLevel[Mathf.Clamp(level - 1, 0, targetsPerLevel.Length - 1)];
            int dmg     = damagePerLevel[Mathf.Clamp(level - 1, 0, damagePerLevel.Length - 1)];

            var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyMask);
            if (hits == null || hits.Length == 0) continue;

            // shuffle
            for (int i = 0; i < hits.Length; i++)
            {
                int j = Random.Range(i, hits.Length);
                (hits[i], hits[j]) = (hits[j], hits[i]);
            }

            int count = Mathf.Min(targets, hits.Length);
            for (int i = 0; i < count; i++)
            {
                var h = hits[i];
                if (!h) continue;

                // Dégâts : tente EnemyHealth puis SendMessage
                var eh = h.GetComponentInParent<EnemyHealth>();
                if (eh) eh.TakeDamage(dmg);
                else h.gameObject.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);

                if (lightningVfxPrefab)
                    Instantiate(lightningVfxPrefab, h.transform.position, Quaternion.identity);
            }
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

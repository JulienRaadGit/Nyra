using UnityEngine;

public class DropOnDeath : MonoBehaviour
{
    [Header("XP")]
    public GameObject xpOrbPrefab;
    public int xpMin = 1, xpMax = 1;
    public float xpScatterRadius = 0.3f;
    [Range(0f,1f)] public float xpDropChance = 1f;

    [Header("Gold")]
    public GameObject goldCoinPrefab;     // Assigne le prefab de pièce
    public int goldMin = 0, goldMax = 2;  // Nombre de pièces à drop (range)
    public float goldScatterRadius = 0.6f;
    [Range(0f,1f)] public float goldDropChance = 1f; // 1 = 100%

    void Start()
    {
        if (TryGetComponent<Damageable>(out var d))
            d.OnDeath += OnDied;
    }

    void OnDied(Damageable d)
    {
        DropXP(d);
        DropGold(d);
    }

    void DropXP(Damageable d)
    {
        if (!xpOrbPrefab) return;
        if (Random.value > xpDropChance) return;

        int count = Random.Range(xpMin, xpMax + 1);
        for (int i = 0; i < count; i++)
        {
            var offset = Random.insideUnitCircle * xpScatterRadius;
            // Utilise la rotation du prefab pour respecter un éventuel visuel
            Instantiate(
                xpOrbPrefab,
                d.transform.position + (Vector3)offset,
                xpOrbPrefab.transform.rotation
            );
        }
    }

    void DropGold(Damageable d)
    {
        if (!goldCoinPrefab) return;
        if (Random.value > goldDropChance) return;

        int count = Random.Range(goldMin, goldMax + 1);
        for (int i = 0; i < count; i++)
        {
            var offset = Random.insideUnitCircle * goldScatterRadius;
            Instantiate(
                goldCoinPrefab,
                d.transform.position + (Vector3)offset,
                goldCoinPrefab.transform.rotation
            );
        }
    }
}

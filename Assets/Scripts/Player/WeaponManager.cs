using UnityEngine;

public class WeaponManager : MonoBehaviour {
    [Header("Parents/Prefabs")]
    public Transform weaponParent;          // crée un Empty "Weapons" sous le Player et assigne-le
    public GameObject auraPrefab;           // Prefabs/Weapons/AuraPrefab
    public GameObject starfallPrefab;       // (optionnel si tu veux un visuel par étoile)

    Aura auraInstance;
    Starfall starfallInstance;

    void EnsureParent(){
        if (!weaponParent){
            var t = transform.Find("Weapons");
            if (!t){
                var go = new GameObject("Weapons");
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                t = go.transform;
            }
            weaponParent = t;
        }
    }

    // ---------- Aura ----------
    public bool HasAura() => auraInstance != null;

    public void AddAura(){
        EnsureParent();
        if (auraInstance) return;

        if (!auraPrefab){
            // Aura sans visuel : on crée tout par code
            var go = new GameObject("Aura");
            go.transform.SetParent(weaponParent);
            go.transform.localPosition = Vector3.zero;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 2f;

            auraInstance = go.AddComponent<Aura>();
            auraInstance.damagePerSecond = 3f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 0f, 0f, 0.2f);
        } else {
            var go = Instantiate(auraPrefab, weaponParent);
            go.transform.localPosition = Vector3.zero;
            auraInstance = go.GetComponent<Aura>();
        }

        // Sync visuelle avec le collider
        if (auraInstance != null) auraInstance.SyncVisualToCollider();
    }

    public void UpgradeAura(){
        if (!auraInstance) return;

        auraInstance.damagePerSecond += 2f;
        var c = auraInstance.GetComponent<CircleCollider2D>();
        if (c) c.radius += 0.5f;

        // Sync visuelle avec le collider
        auraInstance.SyncVisualToCollider();
    }

    // ---------- Starfall ----------
    public bool HasStarfall() => starfallInstance != null;

    public void AddStarfall(){
        EnsureParent();
        if (starfallInstance) return;

        var go = new GameObject("Starfall");
        go.transform.SetParent(weaponParent);
        go.transform.localPosition = Vector3.zero;

        starfallInstance = go.AddComponent<Starfall>();
        if (starfallPrefab) starfallInstance.starVfxPrefab = starfallPrefab;
    }

    public void UpgradeStarfall(){
        if (!starfallInstance) return;
        starfallInstance.Upgrade();
    }

    // ---------- Evolutions (niv 6) ----------
    public void EvolveAura(){
        if (!auraInstance || auraInstance.isEvolved) return;

        auraInstance.damagePerSecond *= 2.0f; // x2 dégâts
        var c = auraInstance.GetComponent<CircleCollider2D>();
        if (c) c.radius += 1.0f;              // + portée

        var sr = auraInstance.GetComponent<SpriteRenderer>();
        if (sr) sr.color = new Color(1f, 0f, 0f, 0.8f); // doré léger

        // Sync visuelle après changement de radius
        auraInstance.SyncVisualToCollider();

        auraInstance.isEvolved = true;
    }

    public void EvolveStarfall(){
        if (!starfallInstance || starfallInstance.isEvolved) return;

        starfallInstance.interval = Mathf.Max(0.6f, starfallInstance.interval * 0.6f); // +40% cadence
        starfallInstance.damage   *= 2.0f;                                             // x2 dégâts
        starfallInstance.starsPerWave += 2;                                            // +2 étoiles
        starfallInstance.radius   += 0.4f;                                             // + zone
        starfallInstance.isEvolved = true;
    }
}

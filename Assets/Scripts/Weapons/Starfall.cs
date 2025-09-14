using UnityEngine;

public class Starfall : MonoBehaviour {
    [Header("Config")]
    public float interval = 4f;   // temps entre vagues
    public int starsPerWave = 1;    // augmente avec l'upgrade
    public float damage = 4f;
    public float radius = 1.6f;     // zone d'impact
    public float spawnRange = 8f;   // distance autour du joueur pour le point de chute
    public GameObject starVfxPrefab; // optionnel : un visuel (sprite/particules)

    [HideInInspector] public bool isEvolved = false;

    float t;
    Transform player;

    void Start(){ player = GameObject.FindGameObjectWithTag("Player")?.transform; }

    void Update(){
        t += Time.deltaTime;
        float realInterval = interval * (PlayerStats.Instance ? PlayerStats.Instance.cooldownMult : 1f);
        if (t >= realInterval){
            t = 0f;
            DoWave();
        }
    }


    void DoWave(){
        if (!player) return;
        for (int i=0;i<starsPerWave;i++){
            Vector2 pos = (Vector2)player.position + Random.insideUnitCircle * spawnRange;
            // petit délai pour simuler "la chute"
            StartCoroutine(ImpactAfterDelay(pos, 0.25f + 0.05f*i));
        }
    }

    System.Collections.IEnumerator ImpactAfterDelay(Vector2 pos, float delay){
        if (starVfxPrefab){
            // Option : prévisualisation (spawn un marqueur)
            var mark = GameObject.Instantiate(starVfxPrefab, pos, Quaternion.identity);
            mark.transform.localScale = Vector3.one * (radius*0.8f);
            GameObject.Destroy(mark, delay + 0.2f);
        }
        yield return new WaitForSeconds(delay);
        Impact(pos);
    }

    void Impact(Vector2 pos){
        // dégâts en zone (multipliés par stats)
        float finalDamage = damage * (PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f);
        var hits = Physics2D.OverlapCircleAll(pos, radius);
        foreach (var h in hits){
            if (h.TryGetComponent<Damageable>(out var d)){
                d.Take(finalDamage);
            }
        }
        // (Option) Effet visuel d’explosion ici
    }

    // Appelée par l’Upgrade
    public void Upgrade(){
        if (starsPerWave < 2) starsPerWave = 2;     // Niveau 2 : 2 étoiles
        else if (starsPerWave < 3) starsPerWave = 3;// Niveau 3 : 3 étoiles
        else radius += 0.5f;                        // Niveau 4 : +zone
        // Niveau 5 (avant évolution) : on raccourcit l’intervalle
        interval = Mathf.Max(0.9f, interval - 0.3f);
        damage += 1.5f;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected(){
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRange);
    }
#endif
}

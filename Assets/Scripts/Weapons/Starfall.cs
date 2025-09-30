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

        // Générer des positions optimisées pour toucher un maximum d'ennemis
        var positions = ComputeGreedyImpactPositions((Vector2)player.position);
        for (int i=0;i<positions.Count;i++){
            Vector2 pos = positions[i];
            // petit délai pour simuler "la chute"
            StartCoroutine(ImpactAfterDelay(pos, 0.25f + 0.05f*i));
        }
    }

    System.Collections.IEnumerator ImpactAfterDelay(Vector2 pos, float delay){
        if (starVfxPrefab){
            // Option : prévisualisation (spawn un marqueur)
            var mark = GameObject.Instantiate(starVfxPrefab, pos, Quaternion.identity);
            mark.transform.localScale = Vector3.one * (radius*0.8f);
            // Le marqueur ne reste affiché qu'un court instant (~0.25s)
            GameObject.Destroy(mark, 0.25f);
        }
        yield return new WaitForSeconds(delay);
        Impact(pos);
    }

    void Impact(Vector2 pos){
        // Appliquer les dégâts directement à tous les ennemis dans la zone
        int mask = LayerMask.GetMask("Enemy");
        float mult = PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f;
        int dmg = Mathf.RoundToInt(damage * mult);
        var hits = Physics2D.OverlapCircleAll(pos, radius, mask);
        if (hits != null){
            foreach (var h in hits){
                if (!h) continue;
                if (h.TryGetComponent<Damageable>(out var d)) d.Take(dmg);
                else if (h.TryGetComponent<EnemyHealth>(out var eh)) eh.TakeDamage(dmg);
                else {
                    var pd = h.GetComponentInParent<Damageable>();
                    if (pd) pd.Take(dmg); else {
                        var peh = h.GetComponentInParent<EnemyHealth>();
                        if (peh) peh.TakeDamage(dmg);
                    }
                }
            }
        }

        // (Option) Effet visuel d'explosion ici
    }

    // Génère des positions espacées pour éviter les chevauchements entre impacts
    System.Collections.Generic.List<Vector2> GenerateSpacedPositions(Vector2 center, float range, int count, float minSpacing){
        var list = new System.Collections.Generic.List<Vector2>(count);
        int attempts = 0;
        while (list.Count < count && attempts < count * 20){
            attempts++;
            Vector2 candidate = center + Random.insideUnitCircle * range;
            bool ok = true;
            for (int i = 0; i < list.Count; i++){
                if ((candidate - list[i]).sqrMagnitude < (minSpacing * minSpacing)) { ok = false; break; }
            }
            if (ok) list.Add(candidate);
        }
        // Fallback si pas assez d'espace: compléter sans contrainte
        while (list.Count < count) list.Add(center + Random.insideUnitCircle * range);
        return list;
    }

    // Sélection gloutonne: à chaque étoile, choisir le centre qui couvre le plus d'ennemis non encore couverts
    System.Collections.Generic.List<Vector2> ComputeGreedyImpactPositions(Vector2 center){
        var result = new System.Collections.Generic.List<Vector2>(starsPerWave);
        int enemyMask = LayerMask.GetMask("Enemy");

        // Récupérer tous les ennemis dans la zone de spawn
        var enemies = Physics2D.OverlapCircleAll(center, spawnRange, enemyMask);
        if (enemies == null || enemies.Length == 0){
            // fallback: espacés si aucun ennemi détecté
            return GenerateSpacedPositions(center, spawnRange, starsPerWave, radius * 1.8f);
        }

        // Ensemble des cibles déjà couvertes
        var covered = new System.Collections.Generic.HashSet<Transform>();
        float minSpacing = radius * 1.5f; // garder un peu d'espace

        // Points candidats = positions d'ennemis et quelques échantillons aléatoires autour
        var candidates = new System.Collections.Generic.List<Vector2>(enemies.Length * 3);
        foreach (var e in enemies){ if (e) candidates.Add(e.transform.position); }
        // Échantillons supplémentaires pour trouver de bons centres entre ennemis
        for (int i=0;i<enemies.Length;i++){
            var e = enemies[i]; if (!e) continue;
            candidates.Add((Vector2)e.transform.position + Random.insideUnitCircle * (radius * 0.5f));
        }
        // Quelques candidats radiaux autour du joueur
        int radial = Mathf.Clamp(8, 8, 24);
        for (int i=0;i<radial;i++){
            float ang = (360f / radial) * i * Mathf.Deg2Rad;
            candidates.Add(center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * (spawnRange * 0.8f));
        }

        // Boucle gloutonne
        for (int k=0; k<starsPerWave; k++){
            Vector2 bestPos = center;
            int bestGain = -1;

            for (int i=0;i<candidates.Count;i++){
                Vector2 c = candidates[i];

                // Respecter l'espacement minimal avec les positions déjà choisies
                bool tooClose = false;
                for (int j=0;j<result.Count;j++){
                    if ((c - result[j]).sqrMagnitude < (minSpacing * minSpacing)) { tooClose = true; break; }
                }
                if (tooClose) continue;

                // Compter le nombre d'ennemis non couverts à portée
                int gain = 0;
                for (int ei=0; ei<enemies.Length; ei++){
                    var en = enemies[ei]; if (!en) continue;
                    if (covered.Contains(en.transform)) continue;
                    if (((Vector2)en.transform.position - c).sqrMagnitude <= radius * radius) gain++;
                }

                if (gain > bestGain){ bestGain = gain; bestPos = c; }
            }

            // Si aucun gain trouvé (ex: très peu d'ennemis), prendre un point aléatoire espacé
            if (bestGain <= 0){
                var fallback = GenerateSpacedPositions(center, spawnRange, 1, minSpacing);
                bestPos = fallback[0];
            }

            result.Add(bestPos);

            // Marquer couverts les ennemis touchés par cette position
            for (int ei=0; ei<enemies.Length; ei++){
                var en = enemies[ei]; if (!en) continue;
                if (((Vector2)en.transform.position - bestPos).sqrMagnitude <= radius * radius){
                    covered.Add(en.transform);
                }
            }
        }

        return result;
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

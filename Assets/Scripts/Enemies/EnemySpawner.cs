using UnityEngine;

public class EnemySpawner : MonoBehaviour {
    public Damageable enemyPrefab;     // mets ton prefab Enemy ici
    public float spawnEvery = 1.5f;    // secondes entre spawns
    public float radius = 12f;         // distance autour du joueur
    public int maxAlive = 30;          // limite simple

    float t; Transform player;

    void Awake(){
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update(){
        if (!player) return;
        t += Time.deltaTime;
        if (t < spawnEvery) return;
        t = 0f;

        if (GameObject.FindGameObjectsWithTag("Enemy").Length >= maxAlive) return;

        float ang = Random.value * Mathf.PI * 2f;
        Vector2 pos = (Vector2)player.position + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
        var e = Instantiate(enemyPrefab, pos, Quaternion.identity);
        e.gameObject.tag = "Enemy";
    }
}

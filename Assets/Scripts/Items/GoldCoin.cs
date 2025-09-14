using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoldCoin : MonoBehaviour
{
    [Header("Coin")]
    public int amount = 1;
    public float lifeTime = 12f;

    [Header("Magnet")]
    public float attractDistance = 2.5f; // distance à partir de laquelle la pièce vient vers le joueur
    public float flySpeed = 7f;

    Transform player;
    PlayerStats playerStats;
    float t;

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            player = playerObj.transform;
            playerStats = playerObj.GetComponent<PlayerStats>();
        }

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= lifeTime) Destroy(gameObject);

        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attractDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, flySpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!playerStats) return;
        if (!other.CompareTag("Player")) return;

        playerStats.AddGold(amount);
        // TODO: jouer un petit son / particule ici si tu veux
        Destroy(gameObject);
    }
}

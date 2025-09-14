using UnityEngine;

public class Projectile : MonoBehaviour {
    public float speed = 15f;
    public float damage = 1f;
    public float lifetime = 3f;

    float timer;
    Vector2 direction;

    public void Fire(Vector2 start, Vector2 dir) {
        transform.position = start;
        direction = dir.normalized;
        timer = 0f;
        gameObject.SetActive(true);
    }

    void Update() {
        transform.Translate(direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime) gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.TryGetComponent<Damageable>(out var dmg)) {
            float mult = PlayerStats.Instance ? PlayerStats.Instance.damageMult : 1f;
            dmg.Take(damage * mult);
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 30;
    public int hp;

    [Header("Popups")]
    public bool showPopup = true;
    public Transform popupAnchor; // optionnel

    void Awake()
    {
        hp = Mathf.Max(1, maxHP);
    }

    public void TakeDamage(int amount, bool crit = false)
    {
        if (amount <= 0 || hp <= 0) return;

        hp = Mathf.Max(0, hp - amount);

        if (showPopup)
        {
            Vector3 pos = GetPopupPos();
            SimpleDamagePopup.Show(pos, amount, crit); // <-- PAS .Spawn sur le pool
        }

        if (hp <= 0) Die();
    }

    Vector3 GetPopupPos()
    {
        if (popupAnchor) return new Vector3(popupAnchor.position.x, popupAnchor.position.y, 0f);

        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr) return sr.bounds.center + Vector3.up * (sr.bounds.extents.y * 0.6f);

        var col = GetComponentInChildren<Collider2D>();
        if (col) return col.bounds.center + Vector3.up * (col.bounds.extents.y * 0.6f);

        return transform.position + Vector3.up * 0.6f;
    }

    void Die()
    {
        Destroy(gameObject);
    }
}

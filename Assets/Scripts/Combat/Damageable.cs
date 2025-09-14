using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Health")]
    public float maxHP = 3f;
    float hp;

    public System.Action<Damageable> OnDeath;

    [Header("Popups")]
    public bool showPopup = true;
    [Tooltip("Point précis au-dessus de la tête (optionnel).")]
    public Transform popupAnchor;

    [SerializeField] float popupMinInterval = 0.06f; // throttle pour dps continus
    float popupCarry;
    float nextPopupTime;

    void Awake() { hp = Mathf.Max(0.01f, maxHP); }

    public void Take(float dmg, bool crit = false)
    {
        if (hp <= 0f || dmg <= 0f) return;

        hp -= dmg;

        if (showPopup) TrySpawnPopup(dmg, crit, GetPopupPos());

        if (hp <= 0f) Die();
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

    void TrySpawnPopup(float dmg, bool crit, Vector3 pos)
    {
        popupCarry += dmg;
        int whole = Mathf.FloorToInt(popupCarry);
        if (whole >= 1 && Time.time >= nextPopupTime)
        {
            SimpleDamagePopup.Show(new Vector3(pos.x, pos.y, 0f), whole, crit);
            popupCarry -= whole;
            nextPopupTime = Time.time + popupMinInterval;
        }
    }

    void Die()
    {
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }
}

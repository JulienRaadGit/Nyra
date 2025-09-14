using UnityEngine;
using System.Collections.Generic;

public class SimpleDamagePopupPool : MonoBehaviour
{
    public static SimpleDamagePopupPool I { get; private set; }

    [Header("Prefab (obligatoire)")]
    public SimpleDamagePopup prefab;

    [Header("Préwarm (optionnel)")]
    public int prewarmCount = 10;

    readonly Queue<SimpleDamagePopup> pool = new Queue<SimpleDamagePopup>();

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        if (!prefab) Debug.LogWarning("[SimpleDamagePopupPool] Prefab non assigné.");

        // Pré-allocation
        for (int i = 0; i < prewarmCount; i++)
        {
            var p = Instantiate(prefab, transform);
            p.gameObject.SetActive(false);
            pool.Enqueue(p);
        }
    }

    public SimpleDamagePopup Get(Vector3 worldPos)
    {
        var p = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
        p.transform.position = worldPos;
        p.SetOwner(this);
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(SimpleDamagePopup p)
    {
        if (!p) return;
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
    }
}

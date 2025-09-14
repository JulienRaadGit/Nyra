using UnityEngine;

public class SimpleDamagePopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] float lifetime = 0.6f;
    [SerializeField] float floatSpeed = 1.6f;
    [SerializeField] Vector2 randomSpread = new Vector2(0.18f, 0.12f);

    [Header("Rendering")]
    [SerializeField] string sortingLayerName = "UI-World";
    [SerializeField] int sortingOrder = 200;

    [Header("Texte")]
    [SerializeField] int fontSize = 36;         // 36 normal, 48 crit
    [SerializeField] float characterSize = 0.08f;

    // Runtime
    SimpleDamagePopupPool owner;
    TextMesh tm;
    float t;
    Color baseColor;
    Vector3 startScale;

    public void SetOwner(SimpleDamagePopupPool o) { owner = o; }

    /// <summary>API statique : récupère une instance dans le pool et l'initialise.</summary>
    public static void Show(Vector3 worldPos, int amount, bool crit = false)
    {
        var p = SimpleDamagePopupPool.I.Get(worldPos);
        p.Init(amount, crit);
    }

    void EnsureTextMesh()
    {
        if (!tm) tm = GetComponent<TextMesh>();
        if (!tm) tm = gameObject.AddComponent<TextMesh>();

        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.richText = false;

        var r = GetComponent<MeshRenderer>();
        if (r)
        {
            int id = SortingLayer.NameToID(sortingLayerName);
            if (id != 0) r.sortingLayerID = id;   // si la layer existe
            r.sortingOrder = sortingOrder;
            r.enabled = true;
        }
    }

    public void Init(int amount, bool crit)
    {
        // Position de départ (léger aléa) + Z=0 (caméra ortho)
        Vector3 p = transform.position + (Vector3)new Vector2(
            Random.Range(-randomSpread.x, randomSpread.x),
            Random.Range(0f, randomSpread.y)
        );
        transform.position = new Vector3(p.x, p.y, 0f);

        EnsureTextMesh();

        tm.text = amount.ToString();
        tm.fontSize = crit ? Mathf.Max(fontSize, 48) : fontSize;
        tm.characterSize = characterSize;
        tm.color = crit ? new Color(1f, 0.85f, 0.25f, 1f) : Color.white;

        baseColor = tm.color;
        startScale = Vector3.one * (crit ? 1.2f : 1f);
        transform.localScale = startScale * 1.15f;

        t = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        t += Time.deltaTime;

        // Monter
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Ease scale vers 1
        float s = Mathf.Lerp(1.15f, 1f, t / 0.15f);
        transform.localScale = startScale * s;

        // Fade
        float a = Mathf.Clamp01(1f - (t / lifetime));
        var c = baseColor; c.a = a; tm.color = c;

        if (t >= lifetime)
        {
            if (owner) owner.Return(this);
            else Destroy(gameObject); // fallback
        }
    }
}

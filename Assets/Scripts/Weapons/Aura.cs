using UnityEngine;

public class Aura : MonoBehaviour {
    public float damagePerSecond = 3f;

    [HideInInspector] public bool isEvolved = false;

    SpriteRenderer sr;
    CircleCollider2D col;
    Vector3 baseSpriteScale;
    float baseVisualRadius;

    [Header("Animation")]
    public float rotationSpeed = 25f; // degrés par seconde

    void Awake(){
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();
        if (sr){
            baseSpriteScale = sr.transform.localScale;
            baseVisualRadius = Mathf.Max(0.0001f, sr.bounds.extents.x);
        }
    }

    void Start(){
        SyncVisualToCollider();
    }

    void Update(){
        // fait tourner seulement le visuel (SpriteRenderer)
        if (sr){
            sr.transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    public void SyncVisualToCollider(){
        if (!sr || !col) return;
        float scaleFactor = col.radius / baseVisualRadius;
        sr.transform.localScale = baseSpriteScale * scaleFactor;
    }

    void OnTriggerStay2D(Collider2D colli){
        if (colli.TryGetComponent<Damageable>(out var d)){
            d.Take(damagePerSecond * Time.deltaTime);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimation2D : MonoBehaviour {
    [Header("Sprites")]
    public Sprite idle;    // image à l'arrêt
    public Sprite move1;   // image 1 en mouvement
    public Sprite move2;   // image 2 en mouvement

    [Header("Réglages")]
    public float frameDuration = 0.5f; // alternance 2 images
    public float moveThreshold = 0.01f; // seuil pour considérer qu'on bouge

    SpriteRenderer sr;
    Vector3 lastPos;
    float t;
    bool toggle;

    void Awake(){
        sr = GetComponent<SpriteRenderer>();
    }
    void Start(){
        lastPos = transform.position;
        if (idle) sr.sprite = idle;
    }

    void Update(){
        Vector3 delta = transform.position - lastPos;

        // Flip gauche/droite en fonction du mouvement horizontal
        if (delta.x > moveThreshold)      sr.flipX = true; // regarde à droite
        else if (delta.x < -moveThreshold)sr.flipX = false;  // regarde à gauche

        bool isMoving = delta.sqrMagnitude > (moveThreshold * moveThreshold);

        if (!isMoving){
            t = 0f; toggle = false;
            if (idle) sr.sprite = idle;
        } else {
            t += Time.deltaTime;
            if (t >= frameDuration){ t = 0f; toggle = !toggle; }
            sr.sprite = toggle ? move1 : move2;
        }

        lastPos = transform.position;
    }
}

using UnityEngine;

public class XpOrb : MonoBehaviour {
    public int xpValue = 1;
    public float attractRange = 3f, speed = 6f;
    Transform player;

    void Start(){ player = GameObject.FindGameObjectWithTag("Player")?.transform; }

    void Update(){
        if (!player) return;
        float d = Vector2.Distance(transform.position, player.position);
        if (d < 0.5f){ LevelSystem.Instance?.Gain(xpValue); Destroy(gameObject); return; }
        if (d < attractRange){ transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime); }
    }
}

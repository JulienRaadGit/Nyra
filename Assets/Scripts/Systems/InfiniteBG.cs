using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class InfiniteBG : MonoBehaviour
{
    public Camera cam;                 // ta caméra orthographique
    public Texture2D texture;          // le même PNG (importé en Default + Repeat)
    public float pixelsPerUnit = 100f; // ton PPU (100 par défaut)
    public float parallax = 1f;        // 1 = colle au monde, <1 = plus lent (parallax)
    public float depth = 50f;          // distance derrière la scène

    Material _mat;
    float _tileWorldSize;              // largeur monde d'un “tile” (1 répétition)

    void Awake()
    {
        if (!cam) cam = Camera.main;
        _mat = GetComponent<MeshRenderer>().material;    // instance
        if (texture == null)
        {
            // essaie de récupérer celle du material si pas assignée
            if (_mat.HasProperty("_BaseMap")) texture = (Texture2D)_mat.GetTexture("_BaseMap");
            else                               texture = (Texture2D)_mat.mainTexture;
        }

        _tileWorldSize = texture.width / pixelsPerUnit;
        FitQuadToScreen();
        UpdateTiling(); // initialise le tiling
    }

    void LateUpdate()
    {
        // centre le quad sur la caméra (ne pas le mettre enfant de la cam)
        Vector3 cp = cam.transform.position;
        transform.position = new Vector3(cp.x, cp.y, depth);

        // offset UV = position monde de la caméra -> évite l'effet "nage"
        Vector2 uv = (new Vector2(cp.x, cp.y) * parallax) / _tileWorldSize;

        if (_mat.HasProperty("_BaseMap")) _mat.SetTextureOffset("_BaseMap", uv);
        else                              _mat.mainTextureOffset = uv;
    }

    void FitQuadToScreen()
    {
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        transform.localScale = new Vector3(w, h, 1f);
    }

    void UpdateTiling()
    {
        float h = cam.orthographicSize * 2f;
        float w = h * cam.aspect;
        Vector2 tiling = new Vector2(w / _tileWorldSize, h / _tileWorldSize);

        if (_mat.HasProperty("_BaseMap")) _mat.SetTextureScale("_BaseMap", tiling);
        else                              _mat.mainTextureScale = tiling;
    }
}

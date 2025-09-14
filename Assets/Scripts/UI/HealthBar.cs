using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HealthBar : MonoBehaviour
{
    [SerializeField] Image fillImage;  // Assigne l'image "Fill" (Type=Filled)

    // Auto-assign si l'enfant s'appelle "Fill"
    void Reset()
    {
        if (!fillImage)
            fillImage = transform.Find("Fill")?.GetComponent<Image>();
    }

    // Sécurise au runtime
    void Awake()
    {
        if (!fillImage)
            fillImage = GetComponentInChildren<Image>(true);
        if (!fillImage)
            Debug.LogError("[HealthBar] 'fillImage' non assigné. Glisse l'Image Fill (Type=Filled).");
    }

    public void SetRatio(float ratio)
    {
        if (!fillImage) return;
        fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}

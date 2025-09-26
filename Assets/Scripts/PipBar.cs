using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PipBar : MonoBehaviour
{
    [SerializeField] private Image[] pips; // Laisse vide : auto-bind
    [SerializeField] private Color onColor = Color.white; // Couleur phase 2 (ex: niveaux 4-6)
    [SerializeField] private Color offColor = Color.gray; // Couleur phase 1 (ex: niveaux 1-3)
    [SerializeField] private Color baseColor = Color.black; // Couleur au repos (niveau 0)
    [SerializeField] private bool limitToThree = true; // Limite visuelle à 3 pips

    void Reset()        { AutoBind(); }
    void OnValidate()   { if (pips == null || pips.Length == 0) AutoBind(); }

    void AutoBind(){
        var list = new List<Image>();
        for (int i = 0; i < transform.childCount; i++){
            var img = transform.GetChild(i).GetComponent<Image>();
            if (img) list.Add(img);
        }
        pips = list.ToArray();
        
        // Initialiser toutes les bougies comme visibles mais noires (repos)
        InitializeAllPips();
    }
    
    /// <summary>
    /// Initialise toutes les bougies comme visibles mais noires (niveau 0)
    /// </summary>
    public void InitializeAllPips(){
        if (pips == null || pips.Length == 0) return;
        
        for (int i = 0; i < pips.Length; i++){
            var img = pips[i]; if (!img) continue;
            
            // Rendre toutes les bougies visibles mais noires
            img.color = baseColor;
            if (img.type == Image.Type.Filled) img.fillAmount = 1f;

            // Optionnel: masquer au-delà de 3
            if (limitToThree) img.gameObject.SetActive(i < 3);
        }
    }

    /// 1 niveau = 1 forme pleine (max = pips.Length, donc 5).
    public void SetStep(int level){
        if (pips == null || pips.Length == 0) return;
        int filled = Mathf.Clamp(level, 0, pips.Length);

        for (int i = 0; i < pips.Length; i++){
            var img = pips[i]; if (!img) continue;
            bool on = i < filled;

            // IMPORTANT : chaque Pip doit être Image -> Type = Filled -> Horizontal -> Origin = Left
            if (img.type == Image.Type.Filled) img.fillAmount = on ? 1f : 0f;

            // Utiliser la couleur au lieu de l'alpha pour montrer l'état
            img.color = on ? onColor : offColor;
        }
    }

    /// Option si tu veux remplir proportionnellement au max (utilise 5 pips même si max=6).
    public void SetProportional(int level, int max){
        if (pips == null || pips.Length == 0) return;
        max = Mathf.Max(1, max);
        level = Mathf.Clamp(level, 0, max);

        float fill = (float)level / max * pips.Length; // 0..5
        for (int i = 0; i < pips.Length; i++){
            var img = pips[i]; if (!img) continue;
            float pipFill = Mathf.Clamp01(fill - i);
            if (img.type == Image.Type.Filled) img.fillAmount = pipFill;

            // Utiliser la couleur au lieu de l'alpha pour montrer l'état
            img.color = pipFill > 0.05f ? onColor : offColor;
        }
    }

    /// <summary>
    /// Mode deux phases sur 3 pips pour 6 niveaux:
    /// 0: noir noir noir
    /// 1..3: gris progressif
    /// 4..6: blanc progressif
    /// </summary>
    public void SetTwoPhase(int level){
        if (pips == null || pips.Length == 0) return;
        level = Mathf.Clamp(level, 0, 6);

        // Phase 1: niveaux 1-3 -> gris
        int phase1 = Mathf.Clamp(level, 0, 3);
        // Phase 2: niveaux 4-6 -> blanc
        int phase2 = Mathf.Clamp(level - 3, 0, 3);

        for (int i = 0; i < pips.Length; i++){
            var img = pips[i]; if (!img) continue;
            bool within = !limitToThree || i < 3;
            img.gameObject.SetActive(within);
            if (!within) continue;

            // Toujours pleines; on code l'état par la couleur
            if (img.type == Image.Type.Filled) img.fillAmount = 1f;

            if (i < phase2){
                img.color = onColor;      // blanc
            } else if (i < phase1){
                img.color = offColor;     // gris
            } else {
                img.color = baseColor;    // noir
            }
        }
    }
}

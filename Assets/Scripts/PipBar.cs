using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PipBar : MonoBehaviour
{
    [SerializeField] private Image[] pips; // Laisse vide : auto-bind
    [SerializeField] private float onAlpha = 1f;
    [SerializeField] private float offAlpha = 0.3f;

    void Reset()        { AutoBind(); }
    void OnValidate()   { if (pips == null || pips.Length == 0) AutoBind(); }

    void AutoBind(){
        var list = new List<Image>();
        for (int i = 0; i < transform.childCount; i++){
            var img = transform.GetChild(i).GetComponent<Image>();
            if (img) list.Add(img);
        }
        pips = list.ToArray();
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

            var c = img.color;
            c.a = on ? onAlpha : offAlpha;
            img.color = c;
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

            var c = img.color;
            c.a = pipFill > 0.05f ? onAlpha : offAlpha;
            img.color = c;
        }
    }
}

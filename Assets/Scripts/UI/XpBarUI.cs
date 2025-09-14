using UnityEngine; using UnityEngine.UI;
public class XpBarUI : MonoBehaviour {
    public Image fill;
    void Awake(){ if(!fill) fill = GetComponent<Image>(); fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = 0; }
    void OnEnable(){ if(LevelSystem.Instance) LevelSystem.Instance.OnXpChanged += Refresh; Refresh(); }
    void OnDisable(){ if(LevelSystem.Instance) LevelSystem.Instance.OnXpChanged -= Refresh; }
    void Refresh(){ var ls = LevelSystem.Instance; float r = ls ? (float)ls.xp/ls.xpToNext : 0f; fill.fillAmount = Mathf.Max(0.0f, r); } // laisse ~3% visible
}

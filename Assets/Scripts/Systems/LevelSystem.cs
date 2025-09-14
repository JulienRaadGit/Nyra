using UnityEngine;

public class LevelSystem : MonoBehaviour {
    public static LevelSystem Instance;
    public int level = 1, xp = 0, xpToNext = 5;
    public System.Action OnLevelUp, OnXpChanged;

    void Awake(){ if(Instance && Instance!=this){ Destroy(gameObject); return; } Instance=this; }

    public void Gain(int amount){
        float mult = PlayerStats.Instance ? PlayerStats.Instance.xpMult : 1f;
        xp += Mathf.RoundToInt(amount * mult);
        OnXpChanged?.Invoke();
        while (xp >= xpToNext){
            xp -= xpToNext;
            level++;
            xpToNext = Mathf.RoundToInt(xpToNext * 1.4f);
            OnLevelUp?.Invoke();
            OnXpChanged?.Invoke();
        }
    }
}

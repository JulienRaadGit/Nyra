using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("PV")]
    public int maxHP = 100;           // base HP (avant multiplicateur)
    public int hp;                    // HP courants (sera écrasé à l'Awake)
    public HealthBar healthBar;       // assigne l'objet HealthBar dans l'Inspector

    [Header("Multiplicateurs (1 = neutre)")]
    public float xpMult = 1f;
    public float goldMult = 1f;
    public float damageMult = 1f;
    public float cooldownMult = 1f;
    public float moveSpeedMult = 1f;
    public float maxHpMult = 1f;
    public float regenPerSec = 0f;

    [Header("Sécurité")]
    [Tooltip("Invulnérabilité au spawn pour éviter de mourir instant si on pop dans un ennemi")]
    public float spawnIFrames = 0.25f;
    float spawnSafeUntil = -999f;

    float regenCarry = 0f;

    // ---------- OR (run courant) ----------
    [Header("Or (run)")]
    [Tooltip("Quantité d'or ramassée pendant la partie en cours")]
    [SerializeField] int gold = 0;
    public int Gold => gold;
    public event Action<int> OnGoldChanged;

    public int EffectiveMaxHP => Mathf.Max(1, Mathf.RoundToInt(maxHP * Mathf.Max(0.0001f, maxHpMult)));

    void Awake()
    {
        if (Instance && Instance != this) { Debug.LogWarning("[PlayerStats] Duplicate -> destroying this"); Destroy(gameObject); return; }
        Instance = this;

        // Init HP
        hp = EffectiveMaxHP;
        spawnSafeUntil = Time.time + spawnIFrames;
        UpdateBar();

        // Init Gold (run)
        SetGold(0); // démarre la run à 0

        Debug.Log($"[PlayerStats] Awake -> maxHP(base)={maxHP}, maxHpMult={maxHpMult}, EffectiveMaxHP={EffectiveMaxHP}, hp={hp}");
    }

    void Start()
    {
        // Double-check visuel
        UpdateBar();
        OnGoldChanged?.Invoke(gold);
    }

    void Update()
    {
        // Régénération
        if (regenPerSec > 0f && hp > 0 && hp < EffectiveMaxHP)
        {
            regenCarry += regenPerSec * Time.deltaTime;
            if (regenCarry >= 1f)
            {
                int add = Mathf.FloorToInt(regenCarry);
                regenCarry -= add;
                Heal(add);
            }
        }
    }

    // ---------- OR : API ----------
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        int add = Mathf.RoundToInt(amount * Mathf.Max(0f, goldMult));
        SetGold(gold + add);
    }

    public bool TrySpendGold(int amount)
    {
        if (amount <= 0) return true;
        if (gold < amount) return false;
        SetGold(gold - amount);
        return true;
    }

    void SetGold(int value)
    {
        gold = Mathf.Max(0, value);
        OnGoldChanged?.Invoke(gold);
        // Debug.Log($"[PlayerStats] Gold -> {gold}");
    }

    // À appeler quand la partie se termine (victoire/défaite) AVANT de recharger une scène.
    public void CommitGoldToMetaAndResetRun()
    {
        MetaGold.Add(gold);
        Debug.Log($"[PlayerStats] Commit gold to meta: +{gold}, total={MetaGold.Get()}");
        SetGold(0);
    }

    // ---------- Dégâts / Soin ----------
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || hp <= 0) return;

        // I-frames de spawn
        if (Time.time < spawnSafeUntil)
        {
            Debug.Log($"[PlayerStats] DMG ignoré (i-frames spawn {spawnSafeUntil - Time.time:0.00}s)");
            return;
        }

        int old = hp;
        hp = Mathf.Max(0, hp - amount);
        UpdateBar();

        Debug.Log($"[PlayerStats] TakeDamage {amount} -> {old} -> {hp}/{EffectiveMaxHP}");

        if (hp <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || hp <= 0) return; // on ne ressuscite pas ici
        int old = hp;
        hp = Mathf.Min(EffectiveMaxHP, hp + amount);
        UpdateBar();
        Debug.Log($"[PlayerStats] Heal {amount} -> {old} -> {hp}/{EffectiveMaxHP}");
    }

    public void ApplyStat(StatId stat)
    {
        switch (stat)
        {
            case StatId.XpPlus:        xpMult       *= 1.10f; break;
            case StatId.GoldPlus:      goldMult     *= 1.10f; break;
            case StatId.DamagePlus:    damageMult   *= 1.05f; break;
            case StatId.CooldownMinus: cooldownMult *= 0.95f; break;
            case StatId.MoveSpeed:     moveSpeedMult*= 1.10f; break;

            case StatId.MaxHpPlus:
            {
                int prevMax = EffectiveMaxHP;
                float prevRatio = prevMax > 0 ? (float)hp / prevMax : 1f;

                maxHpMult *= 1.20f;

                int newMax = EffectiveMaxHP;
                hp = Mathf.Clamp(Mathf.RoundToInt(newMax * prevRatio), 1, newMax);
                UpdateBar();

                Debug.Log($"[PlayerStats] MaxHpPlus -> prevMax={prevMax}, newMax={newMax}, hp={hp}");
                break;
            }

            case StatId.Regen:
                regenPerSec += 0.2f;
                Debug.Log($"[PlayerStats] Regen -> regenPerSec={regenPerSec}");
                break;
        }
    }

    void UpdateBar()
    {
        if (healthBar)
        {
            float ratio = (float)hp / Mathf.Max(1, EffectiveMaxHP);
            healthBar.SetRatio(ratio);
        }
        else
        {
            Debug.LogWarning("[PlayerStats] HealthBar référence manquante !");
        }
    }

    void Die()
    {
        Debug.Log("[PlayerStats] DIE()");
        // TODO: game over / respawn
        // -> Appelle le flow de fin de partie (GameManager etc.)
        // Le GameManager devra ensuite appeler Instance.CommitGoldToMetaAndResetRun();
    }

    // Sécurise les valeurs si modifiées dans l'Inspector
    void OnValidate()
    {
        if (maxHP < 1) maxHP = 1;
        if (maxHpMult <= 0f) maxHpMult = 1f;
        if (goldMult < 0f) goldMult = 0f;
    }
}

public enum StatId
{
    XpPlus, GoldPlus, DamagePlus, CooldownMinus, MoveSpeed, MaxHpPlus, Regen
}

// ---------- MetaGold (persistance simple via PlayerPrefs) ----------
public static class MetaGold
{
    const string KEY = "NYRA_META_GOLD";

    public static int Get() => PlayerPrefs.GetInt(KEY, 0);

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        int total = Mathf.Max(0, Get() + amount);
        PlayerPrefs.SetInt(KEY, total);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}

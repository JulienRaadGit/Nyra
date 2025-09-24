using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Nyra.Upgrades;

// Enum legacy - sera remplacé par celui dans le namespace Nyra.Upgrades
public enum UpgradeId {
    // Stats
    XpPlus, GoldPlus, DamagePlus, CooldownMinus, MoveSpeed, MaxHpPlus, Regen,
    // Armes
    Aura, Starfall, Orbit, Lightning
}

public class UpgradeSystem : MonoBehaviour {
    public static UpgradeSystem Instance;
    public LevelUpUI ui;
    
    [Header("Upgrade Database")]
    [Tooltip("Base de données des upgrades (ScriptableObject)")]
    public UpgradeDatabase upgradeDatabase;
    
    [Header("UI Legacy")]
    [Tooltip("Icône par défaut si aucune n'est trouvée dans la DB")]
    public Sprite defaultIcon;
    [Tooltip("Associe chaque UpgradeId à son Sprite d'icône (legacy - sera remplacé par la DB)")] 
    public List<UpgradeIcon> iconList = new();

    [Header("Options")]
    [Tooltip("Empêche d'ouvrir une nouvelle offre si le panneau est déjà affiché.")]
    public bool preventDoubleOpen = true;

    [Tooltip("Ignore les offres pendant la pause (Time.timeScale == 0).")]
    public bool blockDuringPause = true;

    // Pools de base (types)
    List<UpgradeId> statPool = new() {
        UpgradeId.XpPlus, UpgradeId.GoldPlus, UpgradeId.DamagePlus,
        UpgradeId.CooldownMinus, UpgradeId.MoveSpeed, UpgradeId.MaxHpPlus, UpgradeId.Regen
    };
    List<UpgradeId> weaponPool = new() {
        UpgradeId.Aura, UpgradeId.Starfall, UpgradeId.Orbit, UpgradeId.Lightning
    };

    // Suivi des niveaux pris (0 = pas encore pris)
    Dictionary<UpgradeId,int> levels = new();

    // Compteur d’offres générées (pour “les 3 premiers choix = armes”)
    int offerCount = 0;

    const int STAT_CAP = 5;
    const int WEAPON_CAP = 6; // arme peut évoluer au niv 6

    void Awake(){
        Instance = this;

        // Fallback: retrouve l'UI si non assignée dans l'Inspector
        if (!ui) ui = FindFirstObjectByType<LevelUpUI>();
        if (!ui) Debug.LogError("[UpgradeSystem] Assigne 'ui' (LevelUpUI) dans l’Inspector.");

        foreach (var id in statPool) levels[id] = 0;
        foreach (var id in weaponPool) levels[id] = 0;

        if (LevelSystem.Instance) LevelSystem.Instance.OnLevelUp += Offer;
    }

    void OnDestroy(){
        if (LevelSystem.Instance) LevelSystem.Instance.OnLevelUp -= Offer;
    }

    // ---------- Helpers ----------
    bool IsStat(UpgradeId id)   => statPool.Contains(id);
    bool IsWeapon(UpgradeId id) => weaponPool.Contains(id);

    int  LevelOf(UpgradeId id)  => levels.TryGetValue(id, out var lv) ? lv : 0;
    int  Cap(UpgradeId id)      => IsWeapon(id) ? WEAPON_CAP : STAT_CAP;

    bool IsMaxed(UpgradeId id)  => LevelOf(id) >= Cap(id);
    bool IsOwned(UpgradeId id)  => LevelOf(id) > 0;

    IEnumerable<UpgradeId> AllIds() => statPool.Concat(weaponPool);

    // >>> Exposition publique pour l'UI (pips etc.)
    public int Level(UpgradeId id)    => LevelOf(id);
    public int MaxLevel(UpgradeId id) => Cap(id);

    // ---------- Icônes ----------
    [System.Serializable]
    public class UpgradeIcon {
        public UpgradeId id;
        public Sprite icon;
    }

    Dictionary<UpgradeId, Sprite> iconMap;
    void EnsureIconMap(){
        if (iconMap != null) return;
        iconMap = new Dictionary<UpgradeId, Sprite>();
        foreach (var e in iconList){
            if (!iconMap.ContainsKey(e.id)) iconMap.Add(e.id, e.icon);
            else iconMap[e.id] = e.icon;
        }
    }

    public Sprite Icon(UpgradeId id){
        // Priorité à la nouvelle base de données
        if (upgradeDatabase != null)
        {
            var newId = MapToNewUpgradeId(id);
            if (newId.HasValue)
            {
                var definition = upgradeDatabase.Get(newId.Value);
                if (definition != null && definition.icon != null)
                {
                    Debug.Log($"[UpgradeSystem] Icône trouvée pour {id} -> {newId.Value}: {definition.icon.name}");
                    return definition.icon;
                }
                else
                {
                    Debug.LogWarning($"[UpgradeSystem] Aucune icône trouvée pour {id} -> {newId.Value} dans la DB");
                }
            }
            else
            {
                Debug.LogWarning($"[UpgradeSystem] Pas de mapping pour {id} vers le nouvel enum");
            }
        }
        else
        {
            Debug.LogWarning("[UpgradeSystem] UpgradeDatabase est null");
        }
        
        // Fallback legacy
        EnsureIconMap();
        if (iconMap.TryGetValue(id, out var sp) && sp != null) 
        {
            Debug.Log($"[UpgradeSystem] Utilisation de l'icône legacy pour {id}: {sp.name}");
            return sp;
        }
        
        Debug.LogWarning($"[UpgradeSystem] Aucune icône trouvée pour {id}, utilisation de l'icône par défaut");
        return defaultIcon;
    }

    // ---------- Mapping entre les enums ----------
    /// <summary>
    /// Mappe l'ancien enum UpgradeId vers le nouveau Nyra.Upgrades.UpgradeId
    /// </summary>
    Nyra.Upgrades.UpgradeId? MapToNewUpgradeId(UpgradeId legacyId)
    {
        return legacyId switch
        {
            UpgradeId.MaxHpPlus => Nyra.Upgrades.UpgradeId.HP,
            UpgradeId.DamagePlus => Nyra.Upgrades.UpgradeId.Damage,
            UpgradeId.MoveSpeed => Nyra.Upgrades.UpgradeId.MoveSpeed,
            UpgradeId.Aura => Nyra.Upgrades.UpgradeId.Aura,
            UpgradeId.Starfall => Nyra.Upgrades.UpgradeId.Starfall,
            UpgradeId.Orbit => Nyra.Upgrades.UpgradeId.Orbit,
            UpgradeId.Lightning => Nyra.Upgrades.UpgradeId.Lightning,
            _ => null // Pas de mapping pour les autres (XpPlus, GoldPlus, etc.)
        };
    }

    // Libellé pour l'UI (titre + niveau)
    public string Label(UpgradeId id){
        int lv = LevelOf(id);
        string baseTitle = Title(id);

        // Priorité au label de la DB
        if (upgradeDatabase != null)
        {
            var newId = MapToNewUpgradeId(id);
            if (newId.HasValue)
            {
                var definition = upgradeDatabase.Get(newId.Value);
                if (definition != null && !string.IsNullOrEmpty(definition.label))
                    baseTitle = definition.label;
            }
        }

        if (IsWeapon(id) && lv >= STAT_CAP) {
            // 5/5 -> prochaine = ÉVO (niv 6)
            return $"{baseTitle}  (niv {Mathf.Min(lv,5)}/5 → ÉVO)";
        }
        return $"{baseTitle}  (niv {Mathf.Min(lv,5)}/5)";
    }

    // ---------- Offre ----------
    void Offer(){
        // Guards
        if (!ui) { ui = FindFirstObjectByType<LevelUpUI>(); if (!ui) { Debug.LogError("[UpgradeSystem] ui NULL → impossible d'afficher les cartes."); return; } }
        if (blockDuringPause && Time.timeScale == 0f) return;
        if (preventDoubleOpen) {
            bool alreadyOpen = false;
            try { alreadyOpen = ui.IsOpen; } catch { /* LevelUpUI sans IsOpen */ }
            if (alreadyOpen || (ui.panel && ui.panel.activeSelf)) return;
        }

        offerCount++;

        // Pools filtrés
        var remainingStats =  statPool.Where(id => !IsMaxed(id)).ToList();
        var remainingWeaps =  weaponPool.Where(id => !IsMaxed(id)).ToList();
        var offer = new List<UpgradeId>();

        // 1) Les 3 premières offres : armes uniquement (avec fallback)
        if (offerCount <= 3){
            var pool = new List<UpgradeId>(remainingWeaps);
            FillDistinctRandom(offer, pool, 3);

            if (offer.Count < 3){
                var ownedUpgradableWeapons = weaponPool.Where(id => !IsMaxed(id) && IsOwned(id) && !offer.Contains(id)).ToList();
                FillDistinctRandom(offer, ownedUpgradableWeapons, 3 - offer.Count);
            }
            if (offer.Count < 3){
                var fallback = remainingStats.Where(id => !offer.Contains(id)).ToList();
                FillDistinctRandom(offer, fallback, 3 - offer.Count);
            }
            ui.Show(offer);
            return;
        }

        // 2) À partir de la 4e offre : forcer au moins 1 amélioration possédée
        var ownedUpgradables = AllIds().Where(id => IsOwned(id) && !IsMaxed(id)).ToList();

        if (ownedUpgradables.Count == 0){
            var pool = AllIds().Where(id => !IsMaxed(id)).ToList();
            FillDistinctRandom(offer, pool, 3);
            ui.Show(offer);
            return;
        }

        // a) 1 pick depuis le possédé
        var oneOwned = ownedUpgradables[Random.Range(0, ownedUpgradables.Count)];
        offer.Add(oneOwned);

        // b) compléter depuis le pool général
        var generalPool = AllIds().Where(id => !IsMaxed(id) && !offer.Contains(id)).ToList();
        FillDistinctRandom(offer, generalPool, 3 - offer.Count);

        // c) si manque encore, autoriser doublons
        if (offer.Count < 3){
            var dupes = AllIds().Where(id => !IsMaxed(id)).ToList();
            FillAllowDuplicates(offer, dupes, 3 - offer.Count);
        }

        ui.Show(offer);
    }

    void FillDistinctRandom(List<UpgradeId> outList, List<UpgradeId> pool, int needed){
        for (int i = 0; i < needed && pool.Count > 0; i++){
            int k = Random.Range(0, pool.Count);
            outList.Add(pool[k]);
            pool.RemoveAt(k);
        }
    }
    void FillAllowDuplicates(List<UpgradeId> outList, List<UpgradeId> pool, int needed){
        for (int i = 0; i < needed && pool.Count > 0; i++){
            outList.Add(pool[Random.Range(0, pool.Count)]);
        }
    }

    // ---------- Pick ----------
    public void Pick(UpgradeId id){
        if (IsMaxed(id)) return; // sécurité

        int prev = LevelOf(id);
        int next = Mathf.Min(prev + 1, Cap(id));
        levels[id] = next;

        string upgradeName = GetUpgradeDisplayName(id);
        Debug.Log($"[UpgradeSystem] Upgrade pické: {upgradeName} (niveau {prev} → {next})");

        Apply(id, prev, next);
    }
    
    /// Nom d'affichage (priorité DB)
    string GetUpgradeDisplayName(UpgradeId id)
    {
        if (upgradeDatabase != null)
        {
            var newId = MapToNewUpgradeId(id);
            if (newId.HasValue)
            {
                var definition = upgradeDatabase.Get(newId.Value);
                if (definition != null && !string.IsNullOrEmpty(definition.label))
                    return definition.label;
            }
        }
        return Title(id);
    }

    // ---------- Application des effets ----------
    void Apply(UpgradeId id, int prevLevel, int newLevel){
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var stats = PlayerStats.Instance ? PlayerStats.Instance : FindFirstObjectByType<PlayerStats>();
        if (IsStat(id)){
            // StatId doit matcher les noms (XpPlus, GoldPlus, etc.)
            stats?.ApplyStat((StatId)System.Enum.Parse(typeof(StatId), id.ToString()));
            return;
        }

        var wm = player.GetComponent<WeaponManager>();
        if (!wm) wm = player.AddComponent<WeaponManager>();

        switch(id){
            case UpgradeId.Aura:
                if (prevLevel == 0) wm.AddAura();
                else if (newLevel <= 5) wm.UpgradeAura();
                else wm.EvolveAura(); // niv 6
                break;

            case UpgradeId.Starfall:
                if (prevLevel == 0) wm.AddStarfall();
                else if (newLevel <= 5) wm.UpgradeStarfall();
                else wm.EvolveStarfall();
                break;

            case UpgradeId.Orbit:
                if (prevLevel == 0) wm.AddOrbit();
                else if (newLevel <= 5) wm.UpgradeOrbit();
                else wm.EvolveOrbit();
                break;

            case UpgradeId.Lightning:
                if (prevLevel == 0) wm.AddLightning();
                else if (newLevel <= 5) wm.UpgradeLightning();
                else wm.EvolveLightning();
                break;
        }
    }

    // ---------- Titres ----------
    public static string Title(UpgradeId id) => id switch {
        UpgradeId.XpPlus        => "XP +10% (cumul)",
        UpgradeId.GoldPlus      => "Or +10% (cumul)",
        UpgradeId.DamagePlus    => "Dégâts +5% (cumul)",
        UpgradeId.CooldownMinus => "Cooldown −5% (cumul)",
        UpgradeId.MoveSpeed     => "Vitesse +10%",
        UpgradeId.MaxHpPlus     => "PV max +20%",
        UpgradeId.Regen         => "Régén +0.2 PV/s",
        UpgradeId.Aura          => "Aura cauchemardesque",
        UpgradeId.Starfall      => "Chute d'étoiles",
        UpgradeId.Orbit         => "Orbes en orbite",
        UpgradeId.Lightning     => "Éclair en plein air",
        _ => id.ToString()
    };
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum UpgradeId {
    // Stats
    XpPlus, GoldPlus, DamagePlus, CooldownMinus, MoveSpeed, MaxHpPlus, Regen,
    // Armes
    Aura, Starfall, Orbit, Lightning
}

public class UpgradeSystem : MonoBehaviour {
    public static UpgradeSystem Instance;
    public LevelUpUI ui;

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
        UpgradeId.Aura, UpgradeId.Starfall, UpgradeId.Orbit
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
        if (!ui) ui = FindObjectOfType<LevelUpUI>(true);
        if (!ui) Debug.LogError("[UpgradeSystem] Assigne 'ui' (LevelUpUI) dans l’Inspector.");

        foreach (var id in statPool) levels[id] = 0;
        foreach (var id in weaponPool) levels[id] = 0;

        if (LevelSystem.Instance) LevelSystem.Instance.OnLevelUp += Offer;
    }

    void OnDestroy(){
        if (LevelSystem.Instance) LevelSystem.Instance.OnLevelUp -= Offer;
    }

    // ---------- Helpers ----------
    bool IsStat(UpgradeId id) => statPool.Contains(id);
    bool IsWeapon(UpgradeId id) => weaponPool.Contains(id);

    int Cap(UpgradeId id) => IsWeapon(id) ? WEAPON_CAP : STAT_CAP;

    bool IsMaxed(UpgradeId id) => levels.GetValueOrDefault(id,0) >= Cap(id);

    bool IsOwned(UpgradeId id) => levels.GetValueOrDefault(id,0) > 0;

    IEnumerable<UpgradeId> AllIds() => statPool.Cast<UpgradeId>().Concat(weaponPool);

    // Libellé pour l’UI (titre + niveau)
    public string Label(UpgradeId id){
        int lv = levels.GetValueOrDefault(id,0);
        string baseTitle = Title(id);
        if (IsWeapon(id) && lv >= STAT_CAP) {
            // 5/5 -> prochaine = EVO (niv 6)
            return $"{baseTitle}  (niv {Mathf.Min(lv,5)}/5 → ÉVO)";
        }
        return $"{baseTitle}  (niv {Mathf.Min(lv,5)}/5)";
    }

    // ---------- Offre ----------
    void Offer(){
        // Optionnels/guards
        if (!ui) { ui = FindObjectOfType<LevelUpUI>(true); if (!ui) { Debug.LogError("[UpgradeSystem] ui NULL → impossible d’afficher les cartes."); return; } }
        if (blockDuringPause && Time.timeScale == 0f) return; // déjà en pause avec un panel ouvert
        if (preventDoubleOpen) {
            // Si LevelUpUI expose IsOpen -> on l'utilise, sinon on check activeSelf
            bool alreadyOpen = false;
            try { alreadyOpen = ui.IsOpen; } catch { /* LevelUpUI sans IsOpen */ }
            if (alreadyOpen || (ui.panel && ui.panel.activeSelf)) return;
        }

        offerCount++;

        // Pool filtré des éléments encore améliorables
        List<UpgradeId> remainingStats  = statPool.Where(id => !IsMaxed(id)).ToList();
        List<UpgradeId> remainingWeaps  = weaponPool.Where(id => !IsMaxed(id)).ToList();

        var offer = new List<UpgradeId>();

        // 1) Les 3 premières offres : armes uniquement
        if (offerCount <= 3){
            var pool = new List<UpgradeId>(remainingWeaps);
            FillDistinctRandom(offer, pool, 3);
            // Si plus assez d’armes dispo, on complète par armes possédées non maxées
            if (offer.Count < 3){
                var ownedUpgradableWeapons = weaponPool.Where(id => !IsMaxed(id) && IsOwned(id) && !offer.Contains(id)).ToList();
                FillDistinctRandom(offer, ownedUpgradableWeapons, 3 - offer.Count);
            }
            // Dernier recours: compléter avec stats restantes
            if (offer.Count < 3){
                var fallback = remainingStats.Where(id => !offer.Contains(id)).ToList();
                FillDistinctRandom(offer, fallback, 3 - offer.Count);
            }
            ui.Show(offer);
            return;
        }

        // 2) À partir de la 4e offre : au moins 1 amélioration déjà possédée
        var ownedUpgradables = AllIds().Where(id => IsOwned(id) && !IsMaxed(id)).ToList();

        if (ownedUpgradables.Count == 0){
            var pool = AllIds().Where(id => !IsMaxed(id)).ToList();
            FillDistinctRandom(offer, pool, 3);
            ui.Show(offer);
            return;
        }

        // a) Forcer 1 pick depuis le possédé
        var oneOwned = ownedUpgradables[Random.Range(0, ownedUpgradables.Count)];
        offer.Add(oneOwned);

        // b) Compléter : mélange stats+armes restants non maxés
        var generalPool = AllIds().Where(id => !IsMaxed(id) && !offer.Contains(id)).ToList();
        FillDistinctRandom(offer, generalPool, 3 - offer.Count);

        // c) S’il manque encore, autoriser des doublons (rare)
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

        // Incrémente le niveau
        int prev = levels.GetValueOrDefault(id,0);
        int next = Mathf.Min(prev + 1, Cap(id));
        levels[id] = next;

        // Applique les effets
        Apply(id, prev, next);

        // (Optionnel) : on pourrait ici logger/telemetry, etc.
    }

    // ---------- Application des effets ----------
    void Apply(UpgradeId id, int prevLevel, int newLevel){
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        // Stats
        var stats = PlayerStats.Instance ? PlayerStats.Instance : FindObjectOfType<PlayerStats>();
        if (IsStat(id)){
            stats?.ApplyStat((StatId)System.Enum.Parse(typeof(StatId), id.ToString()));
            return;
        }

        // Armes
        var wm = player.GetComponent<WeaponManager>();
        if (!wm) wm = player.AddComponent<WeaponManager>();

        switch(id){
            case UpgradeId.Aura:
                if (prevLevel == 0) wm.AddAura();
                else if (newLevel <= 5) wm.UpgradeAura();      // niv 2..5
                else wm.EvolveAura();                          // niv 6
                break;

            case UpgradeId.Starfall:
                if (prevLevel == 0) wm.AddStarfall();
                else if (newLevel <= 5) wm.UpgradeStarfall();  // niv 2..5
                else wm.EvolveStarfall();                      // niv 6
                break;

            case UpgradeId.Orbit:
                if (prevLevel == 0) wm.AddOrbit();
                else if (newLevel <= 5) wm.UpgradeOrbit();     // niv 2..5
                else wm.EvolveOrbit();                         // niv 6
                break;

            case UpgradeId.Lightning:
                if (prevLevel == 0) wm.AddLightning();
                else if (newLevel <= 5) wm.UpgradeLightning();  // niv 2..5
                else wm.EvolveLightning();                      // niv 6
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

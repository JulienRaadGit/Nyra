using UnityEngine;
using Nyra.Upgrades;

namespace Nyra.Systems
{
    /// <summary>
    /// Script de debug pour vérifier la configuration de l'UpgradeSystem
    /// </summary>
    public class UpgradeSystemDebugger : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private UpgradeDatabase upgradeDatabase;
        
        [ContextMenu("Debug UpgradeSystem Configuration")]
        public void DebugConfiguration()
        {
            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
                if (upgradeSystem == null)
                {
                    Debug.LogError("[UpgradeSystemDebugger] Aucun UpgradeSystem trouvé dans la scène !");
                    return;
                }
            }
            
            Debug.Log("[UpgradeSystemDebugger] === Configuration UpgradeSystem ===");
            
            // Vérifier l'UpgradeDatabase
            if (upgradeSystem.upgradeDatabase == null)
            {
                Debug.LogError("[UpgradeSystemDebugger] UpgradeDatabase non assignée dans l'UpgradeSystem !");
                
                // Essayer de trouver une UpgradeDatabase dans Resources
                var db = Resources.Load<UpgradeDatabase>("Upgrades/UpgradeDatabase");
                if (db != null)
                {
                    upgradeSystem.upgradeDatabase = db;
                    Debug.Log("[UpgradeSystemDebugger] UpgradeDatabase trouvée dans Resources et assignée automatiquement");
                }
                else
                {
                    Debug.LogError("[UpgradeSystemDebugger] Aucune UpgradeDatabase trouvée dans Resources/Upgrades/");
                    return;
                }
            }
            else
            {
                Debug.Log($"[UpgradeSystemDebugger] UpgradeDatabase assignée: {upgradeSystem.upgradeDatabase.name}");
            }
            
            // Vérifier les définitions dans la DB
            var upgradeDb = upgradeSystem.upgradeDatabase;
            if (upgradeDb.upgradeDefinitions == null || upgradeDb.upgradeDefinitions.Count == 0)
            {
                Debug.LogError("[UpgradeSystemDebugger] Aucune UpgradeDefinition dans la Database !");
                return;
            }
            
            Debug.Log($"[UpgradeSystemDebugger] {upgradeDb.upgradeDefinitions.Count} UpgradeDefinition trouvées:");
            foreach (var def in upgradeDb.upgradeDefinitions)
            {
                if (def != null)
                {
                    Debug.Log($"  - {def.id}: '{def.label}' (bookSprite: {(def.bookSprite != null ? def.bookSprite.name : "NULL")}, icon: {(def.icon != null ? def.icon.name : "NULL")})");
                }
            }
            
            // Tester les mappings
            Debug.Log("[UpgradeSystemDebugger] === Test des mappings ===");
            TestMapping(UpgradeId.Aura);
            TestMapping(UpgradeId.Starfall);
            TestMapping(UpgradeId.Orbit);
            TestMapping(UpgradeId.Lightning);
            TestMapping(UpgradeId.MaxHpPlus);
            TestMapping(UpgradeId.DamagePlus);
            TestMapping(UpgradeId.MoveSpeed);
        }
        
        private void TestMapping(UpgradeId legacyId)
        {
            var newId = MapToNewUpgradeId(legacyId);
            if (newId.HasValue)
            {
                var definition = upgradeSystem.upgradeDatabase.Get(newId.Value);
                if (definition != null)
                {
                    Debug.Log($"  ✓ {legacyId} -> {newId.Value}: '{definition.label}' (bookSprite: {(definition.bookSprite != null ? "✓" : "✗")}, icon: {(definition.icon != null ? "✓" : "✗")})");
                }
                else
                {
                    Debug.LogWarning($"  ✗ {legacyId} -> {newId.Value}: Définition non trouvée dans la DB");
                }
            }
            else
            {
                Debug.Log($"  - {legacyId}: Pas de mapping vers le nouvel enum");
            }
        }
        
        private Nyra.Upgrades.UpgradeId? MapToNewUpgradeId(UpgradeId legacyId)
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
                _ => null
            };
        }
        
        [ContextMenu("Test Label and Icon")]
        public void TestLabelAndIcon()
        {
            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            }
            
            if (upgradeSystem == null)
            {
                Debug.LogError("[UpgradeSystemDebugger] Aucun UpgradeSystem trouvé !");
                return;
            }
            
            Debug.Log("[UpgradeSystemDebugger] === Test Label et Icon ===");
            
            TestUpgrade(UpgradeId.Aura);
            TestUpgrade(UpgradeId.Starfall);
            TestUpgrade(UpgradeId.Orbit);
            TestUpgrade(UpgradeId.Lightning);
            TestUpgrade(UpgradeId.MaxHpPlus);
            TestUpgrade(UpgradeId.DamagePlus);
            TestUpgrade(UpgradeId.MoveSpeed);
        }
        
        private void TestUpgrade(UpgradeId id)
        {
            var label = upgradeSystem.Label(id);
            var icon = upgradeSystem.Icon(id);
            
            Debug.Log($"  {id}: Label='{label}', Icon={(icon != null ? icon.name : "NULL")}");
        }
    }
}

using UnityEngine;
using UnityEditor;

namespace Nyra.Upgrades
{
    /// <summary>
    /// Utilitaire pour créer une UpgradeDatabase avec des définitions par défaut.
    /// Ce script est principalement destiné à l'éditeur pour faciliter la configuration.
    /// </summary>
    public class UpgradeDatabaseCreator : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Nom de la base de données à créer")]
        public string databaseName = "MainUpgradeDatabase";
        
        [Tooltip("Chemin où créer la base de données (relatif à Assets/)")]
        public string savePath = "Scripts/Upgrades/";
        
        [ContextMenu("Create Example Upgrade Database")]
        public void CreateExampleDatabase()
        {
#if UNITY_EDITOR
            // Créer la base de données
            var database = ScriptableObject.CreateInstance<UpgradeDatabase>();
            
            // Créer les définitions d'exemple
            var definitions = new System.Collections.Generic.List<UpgradeDefinition>();
            
            // HP
            var hpDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            hpDef.id = UpgradeId.HP;
            hpDef.label = "Points de Vie";
            hpDef.description = "Augmente les points de vie maximum du joueur";
            hpDef.maxLevel = 5;
            definitions.Add(hpDef);
            
            // Damage
            var damageDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            damageDef.id = UpgradeId.Damage;
            damageDef.label = "Dégâts";
            damageDef.description = "Augmente les dégâts infligés par le joueur";
            damageDef.maxLevel = 5;
            definitions.Add(damageDef);
            
            // MoveSpeed
            var speedDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            speedDef.id = UpgradeId.MoveSpeed;
            speedDef.label = "Vitesse de Déplacement";
            speedDef.description = "Augmente la vitesse de déplacement du joueur";
            speedDef.maxLevel = 5;
            definitions.Add(speedDef);
            
            // Aura
            var auraDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            auraDef.id = UpgradeId.Aura;
            auraDef.label = "Aura Cauchemardesque";
            auraDef.description = "Une aura de dégâts autour du joueur";
            auraDef.maxLevel = 6; // Les armes peuvent évoluer au niveau 6
            definitions.Add(auraDef);
            
            // Starfall
            var starfallDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            starfallDef.id = UpgradeId.Starfall;
            starfallDef.label = "Chute d'Étoiles";
            starfallDef.description = "Fait tomber des étoiles sur les ennemis";
            starfallDef.maxLevel = 6;
            definitions.Add(starfallDef);
            
            // Orbit
            var orbitDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            orbitDef.id = UpgradeId.Orbit;
            orbitDef.label = "Orbes en Orbite";
            orbitDef.description = "Des orbes tournent autour du joueur";
            orbitDef.maxLevel = 6;
            definitions.Add(orbitDef);
            
            // Lightning
            var lightningDef = ScriptableObject.CreateInstance<UpgradeDefinition>();
            lightningDef.id = UpgradeId.Lightning;
            lightningDef.label = "Éclair en Plein Air";
            lightningDef.description = "Frappe les ennemis avec des éclairs";
            lightningDef.maxLevel = 6;
            definitions.Add(lightningDef);
            
            // Assigner les définitions à la base de données
            database.upgradeDefinitions = definitions;
            
            // Sauvegarder
            string fullPath = $"Assets/{savePath}{databaseName}.asset";
            AssetDatabase.CreateAsset(database, fullPath);
            
            // Sauvegarder chaque définition
            for (int i = 0; i < definitions.Count; i++)
            {
                AssetDatabase.AddObjectToAsset(definitions[i], database);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[UpgradeDatabaseCreator] Base de données créée: {fullPath}");
            Debug.Log($"[UpgradeDatabaseCreator] {definitions.Count} définitions d'upgrade ajoutées");
            
            // Sélectionner la base de données créée
            Selection.activeObject = database;
#endif
        }
        
        [ContextMenu("Validate Current Database")]
        public void ValidateCurrentDatabase()
        {
            var upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            if (upgradeSystem == null)
            {
                Debug.LogWarning("[UpgradeDatabaseCreator] Aucun UpgradeSystem trouvé dans la scène");
                return;
            }
            
            if (upgradeSystem.upgradeDatabase == null)
            {
                Debug.LogWarning("[UpgradeDatabaseCreator] Aucune UpgradeDatabase assignée à l'UpgradeSystem");
                return;
            }
            
            var database = upgradeSystem.upgradeDatabase;
            Debug.Log($"[UpgradeDatabaseCreator] Validation de la base de données: {database.name}");
            Debug.Log($"[UpgradeDatabaseCreator] Nombre de définitions: {database.upgradeDefinitions.Count}");
            
            foreach (var definition in database.upgradeDefinitions)
            {
                if (definition == null)
                {
                    Debug.LogError("[UpgradeDatabaseCreator] Définition null trouvée dans la base de données");
                    continue;
                }
                
                Debug.Log($"[UpgradeDatabaseCreator] - {definition.id}: {definition.label} (max: {definition.maxLevel})");
            }
        }
    }
}

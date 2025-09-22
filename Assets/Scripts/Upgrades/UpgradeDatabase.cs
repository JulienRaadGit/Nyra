using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Nyra.Upgrades
{
    /// <summary>
    /// ScriptableObject contenant toutes les définitions d'upgrades.
    /// Fournit un accès rapide aux définitions via un dictionnaire interne.
    /// </summary>
    [CreateAssetMenu(fileName = "Upgrade Database", menuName = "Nyra/Upgrade Database")]
    public class UpgradeDatabase : ScriptableObject
    {
        [Header("Upgrade Definitions")]
        [Tooltip("Liste de toutes les définitions d'upgrades")]
        public List<UpgradeDefinition> upgradeDefinitions = new List<UpgradeDefinition>();
        
        // Dictionnaire interne pour un lookup rapide
        private Dictionary<UpgradeId, UpgradeDefinition> upgradeLookup;
        
        /// <summary>
        /// Initialise le dictionnaire de lookup au démarrage.
        /// </summary>
        private void OnEnable()
        {
            BuildLookupDictionary();
        }
        
        /// <summary>
        /// Construit le dictionnaire de lookup à partir de la liste des définitions.
        /// </summary>
        private void BuildLookupDictionary()
        {
            upgradeLookup = new Dictionary<UpgradeId, UpgradeDefinition>();
            
            foreach (var definition in upgradeDefinitions)
            {
                if (definition != null)
                {
                    if (upgradeLookup.ContainsKey(definition.id))
                    {
                        Debug.LogWarning($"[UpgradeDatabase] Duplicate upgrade ID found: {definition.id}. Using the last one.");
                    }
                    upgradeLookup[definition.id] = definition;
                }
            }
        }
        
        /// <summary>
        /// Récupère la définition d'un upgrade par son ID.
        /// </summary>
        /// <param name="id">L'identifiant de l'upgrade</param>
        /// <returns>La définition de l'upgrade, ou null si non trouvée</returns>
        public UpgradeDefinition Get(UpgradeId id)
        {
            if (upgradeLookup == null)
            {
                BuildLookupDictionary();
            }
            
            upgradeLookup.TryGetValue(id, out UpgradeDefinition definition);
            return definition;
        }
        
        /// <summary>
        /// Vérifie si un upgrade existe dans la base de données.
        /// </summary>
        /// <param name="id">L'identifiant de l'upgrade</param>
        /// <returns>True si l'upgrade existe, false sinon</returns>
        public bool Contains(UpgradeId id)
        {
            if (upgradeLookup == null)
            {
                BuildLookupDictionary();
            }
            
            return upgradeLookup.ContainsKey(id);
        }
        
        /// <summary>
        /// Récupère tous les IDs d'upgrades disponibles.
        /// </summary>
        /// <returns>Une liste de tous les UpgradeId disponibles</returns>
        public List<UpgradeId> GetAllIds()
        {
            if (upgradeLookup == null)
            {
                BuildLookupDictionary();
            }
            
            return upgradeLookup.Keys.ToList();
        }
        
        /// <summary>
        /// Reconstruit le dictionnaire de lookup (utile après modification de la liste dans l'éditeur).
        /// </summary>
        [ContextMenu("Rebuild Lookup Dictionary")]
        public void RebuildLookupDictionary()
        {
            BuildLookupDictionary();
            Debug.Log($"[UpgradeDatabase] Lookup dictionary rebuilt with {upgradeLookup.Count} entries.");
        }
        
        /// <summary>
        /// Valide la base de données et affiche des warnings pour les problèmes détectés.
        /// </summary>
        private void OnValidate()
        {
            if (upgradeDefinitions == null) return;
            
            var duplicateIds = upgradeDefinitions
                .Where(d => d != null)
                .GroupBy(d => d.id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            
            foreach (var duplicateId in duplicateIds)
            {
                Debug.LogWarning($"[UpgradeDatabase] Duplicate upgrade ID found: {duplicateId}");
            }
        }
    }
}

using UnityEngine;

namespace Nyra.Upgrades
{
    /// <summary>
    /// ScriptableObject définissant les propriétés d'un upgrade.
    /// Contient toutes les informations nécessaires pour l'affichage et la gestion d'un upgrade.
    /// </summary>
    [CreateAssetMenu(fileName = "New Upgrade Definition", menuName = "Nyra/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Identifiant unique de l'upgrade")]
        public UpgradeId id;
        
        [Header("Affichage")]
        [Tooltip("Nom affiché dans l'interface utilisateur")]
        public string label;
        
        [Tooltip("Description détaillée de l'upgrade")]
        [TextArea(2, 4)]
        public string description;
        
        [Tooltip("Icône affichée dans l'interface utilisateur")]
        public Sprite icon;
        
        [Header("Configuration")]
        [Tooltip("Niveau maximum que peut atteindre cet upgrade")]
        [Range(1, 10)]
        public int maxLevel = 5;
        
        /// <summary>
        /// Valide que la définition est correctement configurée.
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(label))
            {
                label = id.ToString();
            }
        }
    }
}

using UnityEngine;

namespace Nyra.Upgrades
{
    /// <summary>
    /// Identifiants des upgrades disponibles dans le jeu.
    /// Utilisé pour référencer les upgrades de manière type-safe.
    /// </summary>
    public enum UpgradeId
    {
        // Stats de base
        HP,
        Damage,
        MoveSpeed,
        
        // Armes
        Aura,
        Starfall,
        Orbit,
        Lightning
    }
}

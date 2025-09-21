using UnityEngine;

/// <summary>
/// Script pour les orbes individuelles de l'arme Orbit.
/// Ce script est attaché au prefab OrbitOrb et gère les dégâts via DamageOnTouch.
/// </summary>
public class OrbitOrb : MonoBehaviour
{
    [Header("Dégâts")]
    [Tooltip("Dégâts infligés par cette orbe")]
    public int damage = 4;
    
    [Header("Références")]
    [Tooltip("Transform du joueur propriétaire")]
    public Transform owner;
    
    private DamageOnTouch damageOnTouch;
    
    void Awake()
    {
        // S'assurer que DamageOnTouch est présent
        damageOnTouch = GetComponent<DamageOnTouch>();
        if (damageOnTouch == null)
        {
            damageOnTouch = gameObject.AddComponent<DamageOnTouch>();
        }
        
        // Configurer DamageOnTouch
        damageOnTouch.damage = damage;
        damageOnTouch.owner = owner;
    }
    
    /// <summary>
    /// Met à jour les dégâts de cette orbe
    /// </summary>
    /// <param name="newDamage">Nouveaux dégâts</param>
    /// <param name="playerOwner">Transform du joueur propriétaire</param>
    public void SetDamage(int newDamage, Transform playerOwner)
    {
        damage = newDamage;
        owner = playerOwner;
        
        if (damageOnTouch != null)
        {
            damageOnTouch.damage = damage;
            damageOnTouch.owner = owner;
        }
    }
}

using UnityEngine;

public class EnemyHitboxDamage : MonoBehaviour
{
    [SerializeField] float dps = 10f;   // dégâts par seconde au contact

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other || other.isTrigger) return;

        // On ne veut toucher que le joueur (par layer ou tag)
        // Option A (layer) :
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // Récupère un composant de PV sur le joueur
        // Adapte à ton projet : Damageable, PlayerStats, etc.
        var dmg = other.GetComponent<Damageable>() ?? other.GetComponentInParent<Damageable>();
        if (dmg != null)
        {
            dmg.Take(dps * Time.deltaTime);
        }
        else
        {
            // fallback si tu gères la vie via PlayerStats
            var ps = other.GetComponent<PlayerStats>() ?? other.GetComponentInParent<PlayerStats>();
            if (ps != null)
            {
                ps.TakeDamage(Mathf.CeilToInt(dps * Time.deltaTime));
            }
        }
    }
}

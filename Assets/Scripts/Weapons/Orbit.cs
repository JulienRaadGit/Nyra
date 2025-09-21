using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weapon that spawns orbiting orbs around the player. Each level adds one
/// additional orb, increases the orbit radius, and increases the damage
/// dealt by each orb. The orbit rotates around the player at a configurable
/// speed. Damage dealt is automatically scaled by PlayerStats.damageMult.
/// </summary>
public class Orbit : MonoBehaviour
{
    [Header("References")]
    /// <summary>
    /// Transform of the owning player. Assigned by WeaponManager when the weapon
    /// is created.
    /// </summary>
    public Transform player;

    /// <summary>
    /// Prefab used for each individual orbiting orb. This prefab should include
    /// a SpriteRenderer for visuals, a CircleCollider2D set as a trigger for
    /// collisions, and a Rigidbody2D (set to kinematic) so that trigger events
    /// are detected. A DamageOnTouch component will be added automatically
    /// when the orb is spawned if it is not present.
    /// </summary>
    public GameObject orbPrefab;

    [Header("Level & Damage")]
    [Tooltip("Current level of the weapon (1 to maxLevel). Each level adds an orb and scales radius/damage.")]
    [Range(1, 5)] public int level = 1;
    [Tooltip("Maximum number of levels supported by this weapon.")]
    public int maxLevel = 5;

    [Tooltip("Base damage dealt by each orb at level 1.")]
    public float baseDamage = 4f;
    [Tooltip("Damage added per level beyond the first.")]
    public float damagePerLevel = 2f;

    [Header("Orbit Settings")]
    [Tooltip("Base orbit radius at level 1.")]
    public float baseRadius = 1.6f;
    [Tooltip("Additional radius added per level beyond the first.")]
    public float radiusPerLevel = 0.2f;

    [Tooltip("Degrees per second at which the orbit rotates around the player.")]
    public float rotationSpeedDegPerSec = 120f;
    [Tooltip("If true, rotate clockwise; otherwise rotate counter-clockwise.")]
    public bool clockwise = true;

    [Header("Visual Options")]
    [Tooltip("If true, scales each orb slightly based on level for visual feedback.")]
    public bool scaleOrbsWithLevel = true;

    // Internal list of spawned orbs so they can be cleaned up when the level changes
    private readonly List<GameObject> _orbs = new();
    private float _currentRadius;
    private bool _isInitialized;
    private bool _isEvolved;
    /// <summary>
    /// Indicates whether the weapon has been evolved (level 6). Evolutions are handled
    /// externally via WeaponManager.EvolveOrbit.
    /// </summary>
    public bool IsEvolved => _isEvolved;

    void Awake()
    {
        // Fallback: if player is not assigned yet, try to find the player by tag
        if (!player && transform.root != null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            player = playerGO ? playerGO.transform : null;
        }
    }

    void Update()
    {
        if (!_isInitialized || player == null) return;
        // Follow the player's position
        transform.position = player.position;
        // Rotate the pivot so that orbs rotate around the player
        float dir = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, dir * rotationSpeedDegPerSec * Time.deltaTime);
        
        // Counter-rotate each orb so their sprites maintain their original orientation
        foreach (GameObject orb in _orbs)
        {
            if (orb != null)
            {
                orb.transform.Rotate(0f, 0f, -dir * rotationSpeedDegPerSec * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Initializes the orbit weapon. Called by WeaponManager when creating the weapon.
    /// </summary>
    /// <param name="playerRef">Transform of the owning player.</param>
    /// <param name="orbPrefabRef">Prefab used for each orb.</param>
    /// <param name="startLevel">Initial level when the weapon is acquired.</param>
    public void Init(Transform playerRef, GameObject orbPrefabRef, int startLevel = 1)
    {
        player = playerRef;
        orbPrefab = orbPrefabRef;
        level = Mathf.Clamp(startLevel, 1, maxLevel);
        _isInitialized = true;
        Rebuild();
    }

    /// <summary>
    /// Returns true if the weapon can level up further.
    /// </summary>
    public bool CanLevelUp() => level < maxLevel;

    /// <summary>
    /// Increases the weapon level by one and rebuilds the orbit.
    /// Called when the player picks an upgrade.
    /// </summary>
    public void LevelUp()
    {
        if (!CanLevelUp()) return;
        level++;
        Rebuild();
    }

    /// <summary>
    /// Evolves the weapon. At evolution level (typically 6), damage is doubled,
    /// the number of orbs is increased by two and rotation speed is boosted.
    /// Only applied once per run.
    /// </summary>
    public void Evolve()
    {
        if (_isEvolved) return;
        _isEvolved = true;
        // Apply evolution bonus: more orbs, larger radius, extra damage, faster rotation
        maxLevel = Mathf.Max(maxLevel, level + 2);
        level += 2;
        baseDamage *= 2.0f;
        rotationSpeedDegPerSec *= 1.4f;
        baseRadius += 0.5f;
        Rebuild();
    }

    /// <summary>
    /// Destroys all previously spawned orbs and creates new ones based on the current level.
    /// </summary>
    private void Rebuild()
    {
        // Clean up existing orbs
        for (int i = _orbs.Count - 1; i >= 0; i--)
        {
            if (_orbs[i] != null)
            {
                Destroy(_orbs[i]);
            }
        }
        _orbs.Clear();

        // Compute current radius and damage values
        int orbCount = Mathf.Clamp(level, 1, maxLevel);
        _currentRadius = baseRadius + (level - 1) * radiusPerLevel;
        float damage = baseDamage + (level - 1) * damagePerLevel;

        for (int i = 0; i < orbCount; i++)
        {
            float angle = (360f / orbCount) * i;
            Vector3 localPos = AngleToLocalPosition(angle, _currentRadius);
            // Instantiate the orb as a child of this pivot
            GameObject orb = Instantiate(orbPrefab, transform);
            orb.transform.localPosition = localPos;

            // Optionally scale orbs with level for visuals
            if (scaleOrbsWithLevel)
            {
                float s = 1f + 0.06f * (level - 1);
                orb.transform.localScale = new Vector3(s, s, 1f);
            }

            // Configure the orb with OrbitOrb script for proper damage handling
            var orbitOrb = orb.GetComponent<OrbitOrb>();
            if (orbitOrb == null)
            {
                orbitOrb = orb.AddComponent<OrbitOrb>();
            }
            orbitOrb.SetDamage(Mathf.RoundToInt(damage), player);
            _orbs.Add(orb);
        }
    }

    /// <summary>
    /// Converts a polar angle and radius into a local position vector.
    /// </summary>
    private Vector3 AngleToLocalPosition(float angleDeg, float radius)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius, 0f);
    }
}
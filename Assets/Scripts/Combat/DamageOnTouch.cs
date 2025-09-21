using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[RequireComponent(typeof(Collider2D))]
public class DamageOnTouch : MonoBehaviour
{
    [Header("Cible")]
    public LayerMask targetMask;          // doit inclure la couche Enemy

    [Header("Dégâts")]
    public int damage = 10;               // on reste en int côté script
    public bool allowTriggerTargets = true;
    public float rehitDelay = 0.35f;      // anti double-hit si plusieurs colliders

    [Header("Ignore collisions avec ce root (ex: Transform du Player)")]
    public Transform owner;               // assigne le Transform du joueur

    // mémorise le dernier composant frappé pour anti double-coup
    private readonly Dictionary<Component, float> _lastHitAt = new();

    void Start()
    {
        Debug.Log($"[DamageOnTouch] Initialized on {gameObject.name}: damage={damage}, targetMask={targetMask.value}, allowTriggerTargets={allowTriggerTargets}, owner={owner?.name ?? "null"}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DamageOnTouch] OnTriggerEnter2D: {other.name} (layer: {other.gameObject.layer})");
        
        if (other == null) return;

        // Filtre layer
        if (targetMask.value != 0 && ((1 << other.gameObject.layer) & targetMask) == 0) 
        {
            Debug.Log($"[DamageOnTouch] Layer filter blocked: {other.name} (layer {other.gameObject.layer} not in targetMask {targetMask.value})");
            return;
        }

        // Autoriser/refuser les cibles qui sont elles-mêmes en trigger
        if (!allowTriggerTargets && other.isTrigger) 
        {
            Debug.Log($"[DamageOnTouch] Trigger target blocked: {other.name} (isTrigger={other.isTrigger}, allowTriggerTargets={allowTriggerTargets})");
            return;
        }

        // Ignorer le joueur et ses enfants
        if (owner && (other.transform == owner || other.transform.IsChildOf(owner))) 
        {
            Debug.Log($"[DamageOnTouch] Owner ignored: {other.name} (owner={owner.name})");
            return;
        }

        Debug.Log($"[DamageOnTouch] Processing damage for: {other.name}");

        // Cherche une méthode de dégâts compatible (int ou float)
        var comps = other.GetComponents<Component>();
        Debug.Log($"[DamageOnTouch] Found {comps.Length} components on {other.name}");
        
        foreach (var c in comps)
        {
            if (c == null) continue;

            Debug.Log($"[DamageOnTouch] Checking component: {c.GetType().Name}");

            if (_lastHitAt.TryGetValue(c, out var t) && Time.time - t < rehitDelay)
            {
                Debug.Log($"[DamageOnTouch] Rehit delay blocked: {c.GetType().Name} (last hit: {Time.time - t:F2}s ago, delay: {rehitDelay}s)");
                continue;
            }

            if (TryInvokeDamage(c, damage))
            {
                Debug.Log($"[DamageOnTouch] SUCCESS: Dealt {damage} damage to {c.GetType().Name} on {other.name}");
                _lastHitAt[c] = Time.time;
                return; // un seul hit par OnTriggerEnter
            }
        }
        
        Debug.Log($"[DamageOnTouch] FAILED: No damage method found for {other.name}");
    }

    // Essaie : TakeDamage(int|float), ApplyDamage(int|float), Hit(int|float), Damage(int|float), ReceiveDamage(int|float)
    private static readonly string[] methodNames = { "TakeDamage", "ApplyDamage", "Hit", "Damage", "ReceiveDamage" };

    private bool TryInvokeDamage(Component target, int amount)
    {
        var type = target.GetType();
        Debug.Log($"[DamageOnTouch] TryInvokeDamage: {type.Name} with amount {amount}");
        
        foreach (var name in methodNames)
        {
            Debug.Log($"[DamageOnTouch] Trying method: {name}");
            
            // (int)
            var miInt = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
            if (miInt != null) 
            { 
                Debug.Log($"[DamageOnTouch] Found {name}(int) method, invoking...");
                miInt.Invoke(target, new object[] { amount }); 
                return true; 
            }

            // (float)
            var miFloat = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null);
            if (miFloat != null) 
            { 
                Debug.Log($"[DamageOnTouch] Found {name}(float) method, invoking...");
                miFloat.Invoke(target, new object[] { (float)amount }); 
                return true; 
            }
        }

        // Cas spécial : si tu as un composant Damageable avec un autre nom de méthode (ex: "Take" ou "Apply")
        var damageable = target.GetComponent<Damageable>();
        if (damageable != null)
        {
            Debug.Log($"[DamageOnTouch] Found Damageable component: {damageable.GetType().Name}, trying Take/Apply methods...");
            
            // Essaie directement d'appeler Take(float) sur le composant Damageable
            try
            {
                Debug.Log($"[DamageOnTouch] Calling damageable.Take({amount}f) directly...");
                damageable.Take((float)amount);
                Debug.Log($"[DamageOnTouch] SUCCESS: Direct call to damageable.Take() worked!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Log($"[DamageOnTouch] Direct call failed: {e.Message}");
            }
            
            // Fallback: essaie avec la réflexion
            var dt = damageable.GetType();
            Debug.Log($"[DamageOnTouch] Damageable type: {dt.Name}");
            
            var mi = dt.GetMethod("Take", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null)
                  ?? dt.GetMethod("Take", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null)
                  ?? dt.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(float) }, null)
                  ?? dt.GetMethod("Apply", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);
                  
            Debug.Log($"[DamageOnTouch] Method found: {(mi != null ? mi.Name + "(" + mi.GetParameters()[0].ParameterType.Name + ")" : "null")}");
            
            if (mi != null) 
            { 
                Debug.Log($"[DamageOnTouch] Found Damageable method: {mi.Name}({mi.GetParameters()[0].ParameterType.Name})");
                
                // Convertir amount en float si la méthode attend un float
                object[] args = mi.GetParameters()[0].ParameterType == typeof(float) ? 
                    new object[] { (float)amount } : 
                    new object[] { amount };
                
                Debug.Log($"[DamageOnTouch] Invoking Damageable method with args: [{string.Join(", ", args)}]");
                mi.Invoke(damageable, args); 
                return true; 
            }
            else
            {
                Debug.Log($"[DamageOnTouch] No Take/Apply method found on Damageable");
            }
        }
        else
        {
            Debug.Log($"[DamageOnTouch] No Damageable component found");
        }

        Debug.Log($"[DamageOnTouch] No damage method found for {type.Name}");
        return false;
    }
}

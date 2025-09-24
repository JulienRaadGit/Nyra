using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script de test pour vérifier que les icônes d'upgrade sont bien chargées.
/// À attacher à un GameObject avec des Image components pour tester l'affichage.
/// </summary>
public class UpgradeIconTester : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("Images pour afficher les icônes de test")]
    public Image[] testImages;
    
    [Tooltip("UpgradeSystem à tester")]
    public UpgradeSystem upgradeSystem;
    
    [Header("Test Controls")]
    [Tooltip("Teste automatiquement au démarrage")]
    public bool testOnStart = true;
    
    void Start()
    {
        if (testOnStart)
        {
            TestAllIcons();
        }
    }
    
    [ContextMenu("Test All Icons")]
    public void TestAllIcons()
    {
        if (upgradeSystem == null)
        {
            upgradeSystem = UpgradeSystem.Instance;
            if (upgradeSystem == null)
            {
                Debug.LogError("[UpgradeIconTester] UpgradeSystem non trouvé !");
                return;
            }
        }
        
        Debug.Log("[UpgradeIconTester] === Test des icônes d'upgrade ===");
        
        // Test des upgrades d'armes (ceux qui ont des icônes dans la DB)
        var weaponUpgrades = new UpgradeId[] 
        { 
            UpgradeId.Aura, 
            UpgradeId.Starfall, 
            UpgradeId.Orbit, 
            UpgradeId.Lightning 
        };
        
        // Test des upgrades de stats
        var statUpgrades = new UpgradeId[] 
        { 
            UpgradeId.MaxHpPlus, 
            UpgradeId.DamagePlus, 
            UpgradeId.MoveSpeed 
        };
        
        int imageIndex = 0;
        
        // Test des armes
        foreach (var upgradeId in weaponUpgrades)
        {
            TestIcon(upgradeId, imageIndex);
            imageIndex++;
        }
        
        // Test des stats
        foreach (var upgradeId in statUpgrades)
        {
            TestIcon(upgradeId, imageIndex);
            imageIndex++;
        }
        
        Debug.Log("[UpgradeIconTester] === Fin du test ===");
    }
    
    void TestIcon(UpgradeId upgradeId, int imageIndex)
    {
        if (imageIndex >= testImages.Length || testImages[imageIndex] == null)
        {
            Debug.LogWarning($"[UpgradeIconTester] Pas d'image disponible pour l'index {imageIndex}");
            return;
        }
        
        var icon = upgradeSystem.Icon(upgradeId);
        var label = upgradeSystem.Label(upgradeId);
        
        Debug.Log($"[UpgradeIconTester] {upgradeId}: {label} -> Icône: {(icon != null ? icon.name : "NULL")}");
        
        // Affiche l'icône dans l'image si disponible
        if (testImages[imageIndex] != null)
        {
            testImages[imageIndex].sprite = icon;
            testImages[imageIndex].enabled = icon != null;
        }
    }
    
    [ContextMenu("Clear All Images")]
    public void ClearAllImages()
    {
        foreach (var image in testImages)
        {
            if (image != null)
            {
                image.sprite = null;
                image.enabled = false;
            }
        }
    }
}

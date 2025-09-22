# Système d'Upgrade avec ScriptableObject

Ce dossier contient le nouveau système d'upgrade basé sur des ScriptableObject pour une meilleure organisation et flexibilité.

## Structure

### UpgradeId.cs
Enum contenant les identifiants des upgrades disponibles :
- `HP` - Points de vie
- `Damage` - Dégâts
- `MoveSpeed` - Vitesse de déplacement
- `Aura` - Arme Aura
- `Starfall` - Arme Starfall

### UpgradeDefinition.cs
ScriptableObject définissant les propriétés d'un upgrade :
- `id` - Identifiant unique
- `label` - Nom affiché dans l'UI
- `description` - Description détaillée
- `icon` - Icône affichée
- `maxLevel` - Niveau maximum (défaut: 5)

### UpgradeDatabase.cs
ScriptableObject contenant toutes les définitions d'upgrades :
- `upgradeDefinitions` - Liste de toutes les définitions
- `Get(UpgradeId id)` - Récupère une définition par ID
- `Contains(UpgradeId id)` - Vérifie si un upgrade existe
- `GetAllIds()` - Récupère tous les IDs disponibles

## Utilisation

### 1. Créer une UpgradeDatabase
1. Clic droit dans le Project → Create → Nyra → Upgrade Database
2. Nommer le fichier (ex: "MainUpgradeDatabase")

### 2. Créer des UpgradeDefinition
1. Clic droit dans le Project → Create → Nyra → Upgrade Definition
2. Configurer chaque upgrade :
   - Sélectionner l'ID approprié
   - Définir le label (nom affiché)
   - Ajouter une description
   - Assigner une icône
   - Ajuster le niveau maximum si nécessaire

### 3. Configurer l'UpgradeSystem
1. Sélectionner le GameObject avec UpgradeSystem
2. Dans l'inspecteur, assigner la UpgradeDatabase créée
3. Le système utilisera automatiquement les définitions de la DB

## Migration depuis l'ancien système

Le nouveau système est rétrocompatible :
- L'ancien enum `UpgradeId` reste fonctionnel
- Les méthodes `Label()` et `Icon()` utilisent la DB en priorité
- Fallback vers l'ancien système si la DB n'est pas configurée

## Avantages

- **Organisation** : Toutes les données d'upgrade centralisées
- **Flexibilité** : Modification facile sans recompiler
- **Réutilisabilité** : Définitions partagées entre scènes
- **Performance** : Lookup rapide via dictionnaire interne
- **Maintenance** : Code plus propre et modulaire

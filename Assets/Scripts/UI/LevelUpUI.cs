using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using Nyra.Upgrades;

namespace Nyra.UI
{
    /// <summary>
    /// Interface utilisateur pour l'affichage des choix d'upgrade.
    /// Affiche les icônes d'armes (aura, starfall, etc.) dans l'interface avec des bougies pour les niveaux.
    /// </summary>
    public class LevelUpUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] public GameObject panel;

        [Header("Legacy Support")]
        [SerializeField] private Button[] legacyButtons;   // Boutons legacy pour compatibilité
        [SerializeField] private TextMeshProUGUI[] legacyLabels;
        [SerializeField] private Image[] legacyIcons;
        [SerializeField] private PipBar[] legacyLevelPipBars;
        [SerializeField] private TextMeshProUGUI[] legacyNiveauLabels; // Libellé "niveau" sous l'icône
        [SerializeField] private TextMeshProUGUI[] legacyNewLabels; // Badge "New" au-dessus de l'icône

        private List<UpgradeId> currentOffers;
        private bool isOpen;
        public bool IsOpen => isOpen;

        void Awake()
        {
            if (!panel) Debug.LogError("[LevelUpUI] 'panel' non assigné (racine LevelUpPanel).");
            // Forcer panel caché au lancement, peu importe l'état dans l'Inspector
            if (panel) panel.SetActive(false);
            isOpen = false;
        }
        
        void Update()
        {
            // Surveiller l'état du panel pour détecter les problèmes
            if (isOpen && panel && !panel.activeSelf)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// Affiche les offres d'upgrade avec les icônes d'armes
        /// </summary>
        /// <param name="offer">Liste des UpgradeId à afficher</param>
        public void Show(List<UpgradeId> offer)
        {
            // Utiliser le mode legacy avec les icônes d'armes
            ShowLegacy(offer);
        }
        
        /// <summary>
        /// Force la réouverture du panel si nécessaire
        /// </summary>
        public void ForceReopen()
        {
            if (panel && !panel.activeSelf)
            {
                panel.SetActive(true);
                isOpen = true;
            }
        }

        /// <summary>
        /// Affiche les offres d'upgrade avec les icônes d'armes et les bougies de niveau
        /// </summary>
        /// <param name="offer">Liste des UpgradeId à afficher</param>
        public void ShowLegacy(List<UpgradeId> offer)
        {
            if (isOpen) return;
            isOpen = true;

            currentOffers = offer ?? new List<UpgradeId>();
            if (!panel) { Debug.LogError("[LevelUpUI] Panel manquant."); return; }

            // 1) Activer le panel
            panel.SetActive(true);

            // 2) Le mettre devant
            panel.transform.SetAsLastSibling();

            // 3) S'assurer que le CanvasGroup permet les clics
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }

            // 4) Forcer l'ordre d'affichage (au-dessus)
            var canvas = panel.GetComponentInParent<Canvas>();
            if (canvas)
            {
                canvas.overrideSorting = true;
                canvas.sortingOrder = 700;
            }

            // 5) Couper les raycasts du joystick via UIRaycastRouter
            UIRaycastRouter.Instance?.ShowModal(panel, sortingOrder: 700);

            // 6) Mettre Time.timeScale = 0f seulement après avoir routé l'UI
            Time.timeScale = 0f;

            // 7) Forcer un Canvas.ForceUpdateCanvases()
            Canvas.ForceUpdateCanvases();

            // 8) Configuration des boutons
            for (int i = 0; i < legacyButtons.Length; i++)
            {
                bool on = i < currentOffers.Count;
                if (legacyButtons[i] != null)
                {
                    legacyButtons[i].gameObject.SetActive(on);
                    if (!on) continue;

                    var upId = currentOffers[i];

                    // Nom
                    if (legacyLabels != null && i < legacyLabels.Length && legacyLabels[i] != null)
                        legacyLabels[i].text = UpgradeSystem.Instance.Label(upId);

                    // Icône
                    if (legacyIcons != null && i < legacyIcons.Length && legacyIcons[i] != null)
                    {
                        var sprite = UpgradeSystem.Instance.Icon(upId);
                        legacyIcons[i].sprite = sprite;
                        legacyIcons[i].enabled = sprite != null;
                        if (legacyIcons[i].gameObject.activeSelf != (sprite != null))
                            legacyIcons[i].gameObject.SetActive(sprite != null);
                    }

                    // Niveau → PIPS (mode 2 phases si max=6)
                    int lvl = SafeLevel(upId);
                    int max = SafeMaxLevel(upId);
                    if (legacyLevelPipBars != null && i < legacyLevelPipBars.Length && legacyLevelPipBars[i] != null)
                    {
                        if (max >= 6)
                            legacyLevelPipBars[i].SetTwoPhase(lvl);
                        else
                            legacyLevelPipBars[i].SetStep(lvl);
                    }

                    // Texte du label "niveau"
                    if (legacyNiveauLabels != null && i < legacyNiveauLabels.Length && legacyNiveauLabels[i] != null)
                    {
                        legacyNiveauLabels[i].text = BuildNiveauText(lvl, max);
                    }

                    // Badge "New" (visible uniquement si niveau == 0)
                    if (legacyNewLabels != null && i < legacyNewLabels.Length && legacyNewLabels[i] != null)
                    {
                        bool showNew = lvl == 0;
                        if (legacyNewLabels[i].gameObject.activeSelf != showNew)
                            legacyNewLabels[i].gameObject.SetActive(showNew);
                    }

                    int k = i;
                    legacyButtons[i].onClick.RemoveAllListeners();
                    legacyButtons[i].onClick.AddListener(() => PickLegacy(k));
                    
                    // S'assurer que le bouton est interactable
                    legacyButtons[i].interactable = true;
                }
            }

            // 9) Sélectionner le premier bouton avec EventSystem
            if (legacyButtons.Length > 0 && legacyButtons[0] != null && legacyButtons[0].gameObject.activeSelf)
            {
                EventSystem.current?.SetSelectedGameObject(legacyButtons[0].gameObject);
            }
        }

        private int SafeLevel(UpgradeId id)
        {
            try { return Mathf.Max(0, UpgradeSystem.Instance.Level(id)); }
            catch { return 0; }
        }

        private int SafeMaxLevel(UpgradeId id)
        {
            try { return Mathf.Max(1, UpgradeSystem.Instance.MaxLevel(id)); }
            catch { return 5; }
        }

        private void PickLegacy(int i)
        {
            // Si le panel n'est pas ouvert mais qu'il devrait l'être, le rouvrir
            if (!isOpen && panel.activeSelf)
            {
                isOpen = true;
            }
            
            if (!isOpen) return;
            isOpen = false;

            // Restaurer l'état : Time.timeScale = 1f
            Time.timeScale = 1f;
            
            // Réactiver les raycasts du joystick
            UIRaycastRouter.Instance?.HideModal(panel);
            UIRaycastRouter.Instance?.ForceEnableJoystick();

            // Désactiver le panel
            panel.SetActive(false);

            if (i >= 0 && i < currentOffers.Count) UpgradeSystem.Instance.Pick(currentOffers[i]);
        }

        private string BuildNiveauText(int lvl, int max)
        {
            // Stats: 5 max, Weapons: 6 max (évo après 5)
            lvl = Mathf.Max(0, lvl);
            max = Mathf.Max(1, max);

            if (max >= 6)
            {
                // Affiche l'indication d'évolution à 5
                if (lvl >= 5 && lvl < 6)
                    return $"niv {Mathf.Min(lvl, 5)}/5 → ÉVO";
                return $"niv {Mathf.Min(lvl, 6)}/6";
            }

            return $"niv {Mathf.Min(lvl, 5)}/5";
        }
    }
}
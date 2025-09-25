using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Nyra.Upgrades;

namespace Nyra.UI
{
    /// <summary>
    /// Interface utilisateur pour l'affichage des choix d'upgrade.
    /// Affiche les sprites de livres dans l'interface legacy avec des bougies pour les niveaux.
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

        private List<UpgradeId> currentOffers;
        private bool isOpen;
        public bool IsOpen => isOpen;

        void Awake()
        {
            if (!panel) Debug.LogError("[LevelUpUI] 'panel' non assigné (racine LevelUpPanel).");
        }

        /// <summary>
        /// Affiche les offres d'upgrade avec les sprites de livres
        /// </summary>
        /// <param name="offer">Liste des UpgradeId à afficher</param>
        public void Show(List<UpgradeId> offer)
        {
            // Utiliser le mode legacy avec les sprites de livres
            ShowLegacy(offer);
        }


        /// <summary>
        /// Affiche les offres d'upgrade avec les sprites de livres et les bougies de niveau
        /// </summary>
        /// <param name="offer">Liste des UpgradeId à afficher</param>
        public void ShowLegacy(List<UpgradeId> offer)
        {
            if (isOpen) return;
            isOpen = true;

            currentOffers = offer ?? new List<UpgradeId>();
            if (!panel)
            {
                Debug.LogError("[LevelUpUI] Panel manquant.");
                return;
            }

            UIRaycastRouter.Instance?.ShowModal(panel, sortingOrder: 700);

            var rt = panel.GetComponent<RectTransform>();
            if (rt)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            Time.timeScale = 0f;

            // Utiliser les boutons legacy
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

                    // Niveau → PIPS
                    int lvl = SafeLevel(upId);
                    int max = SafeMaxLevel(upId);
                    if (legacyLevelPipBars != null && i < legacyLevelPipBars.Length && legacyLevelPipBars[i] != null)
                    {
                        legacyLevelPipBars[i].SetStep(lvl);
                    }

                    int k = i;
                    legacyButtons[i].onClick.RemoveAllListeners();
                    legacyButtons[i].onClick.AddListener(() => PickLegacy(k));
                }
            }

            Debug.Log("[LevelUpUI] Panel ouvert avec sprites de livres et bougies de niveau.");
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
            if (!isOpen) return;
            isOpen = false;

            UIRaycastRouter.Instance?.HideModal(panel);
            Time.timeScale = 1f;
            UIRaycastRouter.Instance?.ForceEnableJoystick();

            if (i >= 0 && i < currentOffers.Count) UpgradeSystem.Instance.Pick(currentOffers[i]);
        }
    }
}
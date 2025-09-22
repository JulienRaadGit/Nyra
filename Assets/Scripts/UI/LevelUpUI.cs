using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour {
    public GameObject panel;
    public Button[] buttons;
    public TextMeshProUGUI[] labels;
    public Image[] icons; // Icônes affichées au-dessus du texte

    List<UpgradeId> current;
    bool isOpen;
    public bool IsOpen => isOpen;

    void Awake(){
        if (!panel) Debug.LogError("[LevelUpUI] 'panel' non assigné (racine LevelUpPanel).");
    }

    public void Show(List<UpgradeId> offer){
        if (isOpen) return;                 // évite 2 ouverts
        isOpen = true;

        current = offer ?? new List<UpgradeId>();
        if (!panel){ Debug.LogError("[LevelUpUI] Panel manquant."); return; }

        // Ouvre en modal via le routeur (coupe joystick + force raycasts + met au-dessus)
        UIRaycastRouter.Instance?.ShowModal(panel, sortingOrder: 700);

        // Fixe pleine-écran/centrage pour éviter hors-champ
        var rt = panel.GetComponent<RectTransform>();
        if (rt){
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        // Pause
        Time.timeScale = 0f;

        // Configure les cartes
        for (int i = 0; i < buttons.Length; i++){
            bool on = i < current.Count;
            buttons[i].gameObject.SetActive(on);
            if (!on) continue;

            labels[i].text = UpgradeSystem.Instance.Label(current[i]);

            // Configure l'icône si disponible
            if (icons != null && i < icons.Length && icons[i] != null){
                var sprite = UpgradeSystem.Instance.Icon(current[i]);
                icons[i].sprite = sprite;
                icons[i].enabled = sprite != null;
                if (icons[i].gameObject.activeSelf != (sprite != null))
                    icons[i].gameObject.SetActive(sprite != null);
            }

            int k = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(()=>Pick(k));
        }

        Debug.Log("[LevelUpUI] Panel ouvert (via router).");
    }

    void Pick(int i){
        if (!isOpen) return;
        isOpen = false;

        // Ferme le modal + reprend le temps
        UIRaycastRouter.Instance?.HideModal(panel);
        Time.timeScale = 1f;

        // Sécurité : s'il y a eu double Show quelque part, on réactive quoi qu'il arrive
        UIRaycastRouter.Instance?.ForceEnableJoystick();

        if (i>=0 && i<current.Count) UpgradeSystem.Instance.Pick(current[i]);
    }
}

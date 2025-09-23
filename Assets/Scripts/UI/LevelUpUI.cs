using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpUI : MonoBehaviour {
    public GameObject panel;
    public Button[] buttons;
    public TextMeshProUGUI[] labels;
    public Image[] icons;              // Icônes au-dessus du texte
    public PipBar[] levelPipBars;      // AJOUT : 1 PipBar par bouton (A,B,C)

    List<UpgradeId> current;
    bool isOpen;
    public bool IsOpen => isOpen;

    void Awake(){
        if (!panel) Debug.LogError("[LevelUpUI] 'panel' non assigné (racine LevelUpPanel).");
    }

    public void Show(List<UpgradeId> offer){
        if (isOpen) return;
        isOpen = true;

        current = offer ?? new List<UpgradeId>();
        if (!panel){ Debug.LogError("[LevelUpUI] Panel manquant."); return; }

        UIRaycastRouter.Instance?.ShowModal(panel, sortingOrder: 700);

        var rt = panel.GetComponent<RectTransform>();
        if (rt){
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        Time.timeScale = 0f;

        for (int i = 0; i < buttons.Length; i++){
            bool on = i < current.Count;
            buttons[i].gameObject.SetActive(on);
            if (!on) continue;

            var upId = current[i];

            // Nom
            if (labels != null && i < labels.Length && labels[i] != null)
                labels[i].text = UpgradeSystem.Instance.Label(upId);

            // Icône
            if (icons != null && i < icons.Length && icons[i] != null){
                var sprite = UpgradeSystem.Instance.Icon(upId);
                icons[i].sprite = sprite;
                icons[i].enabled = sprite != null;
                if (icons[i].gameObject.activeSelf != (sprite != null))
                    icons[i].gameObject.SetActive(sprite != null);
            }

            // NIVEAU → PIPS (step)
            int lvl = SafeLevel(upId);
            int max = SafeMaxLevel(upId); // utile si tu veux SetProportional
            if (levelPipBars != null && i < levelPipBars.Length && levelPipBars[i] != null){
                levelPipBars[i].SetStep(lvl);
                // ou : levelPipBars[i].SetProportional(lvl, max);
            }

            int k = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(()=>Pick(k));
        }

        Debug.Log("[LevelUpUI] Panel ouvert (via router).");
    }

    int SafeLevel(UpgradeId id){
        try { return Mathf.Max(0, UpgradeSystem.Instance.Level(id)); }
        catch { return 0; }
    }

    int SafeMaxLevel(UpgradeId id){
        try { return Mathf.Max(1, UpgradeSystem.Instance.MaxLevel(id)); }
        catch { return 5; }
    }

    void Pick(int i){
        if (!isOpen) return;
        isOpen = false;

        UIRaycastRouter.Instance?.HideModal(panel);
        Time.timeScale = 1f;
        UIRaycastRouter.Instance?.ForceEnableJoystick();

        if (i>=0 && i<current.Count) UpgradeSystem.Instance.Pick(current[i]);
    }
}

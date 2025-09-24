using UnityEngine;
using UnityEngine.UI;

public class UIRaycastRouter : MonoBehaviour
{
    public static UIRaycastRouter Instance;

    [Header("Refs")]
    [Tooltip("Le VirtualJoystick de la scène (sur JoystickArea).")]
    public VirtualJoystick joystick;   // assigne dans l’Inspector

    CanvasGroup joystickCG;            // CanvasGroup sur JoystickArea
    int modalDepth = 0;                // nombre de modaux ouverts

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!joystick) joystick = FindFirstObjectByType<VirtualJoystick>();
        if (joystick)
        {
            var areaGO = joystick.area ? joystick.area.gameObject : joystick.gameObject;
            joystickCG = areaGO.GetComponent<CanvasGroup>();
            if (!joystickCG) joystickCG = areaGO.AddComponent<CanvasGroup>();
            // Par défaut : en jeu normal, le joystick capte
            joystickCG.blocksRaycasts = true;
            joystickCG.interactable   = true;
        }
    }

    /// <summary>
    /// Ouvre un panneau modal : garantit visibilité, ordre, raycasts.
    /// Coupe les raycasts du joystick.
    /// </summary>
    public void ShowModal(GameObject panelRoot, int sortingOrder = 700)
    {
        if (!panelRoot) return;

        // 1) Active & force au-dessus
        panelRoot.SetActive(true);

        var cnv = panelRoot.GetComponent<Canvas>();
        if (!cnv) cnv = panelRoot.AddComponent<Canvas>();
        cnv.overrideSorting = true;
        cnv.sortingOrder = sortingOrder;

        // 2) Raycasts cliquables
        var cg = panelRoot.GetComponent<CanvasGroup>();
        if (!cg) cg = panelRoot.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // 3) GraphicRaycaster obligatoire
        if (!panelRoot.GetComponent<GraphicRaycaster>())
            panelRoot.AddComponent<GraphicRaycaster>();

        // 4) Surface qui capte les clics (si jamais rien n’a d’Image)
        if (!panelRoot.GetComponent<Image>())
        {
            var img = panelRoot.AddComponent<Image>();
            // quasi transparent mais capte les clics
            img.color = new Color(0, 0, 0, 0.01f);
            img.raycastTarget = true;
        }

        // 5) Coupe les raycasts du joystick
        modalDepth++;
        SetJoystickRaycastsEnabled(false);
    }

    /// <summary>
    /// Ferme un modal ouvert via ShowModal.
    /// </summary>
    public void HideModal(GameObject panelRoot)
    {
        if (panelRoot) panelRoot.SetActive(false);

        modalDepth = Mathf.Max(0, modalDepth - 1);
        if (modalDepth == 0)
            SetJoystickRaycastsEnabled(true);
    }

    /// <summary>
    /// Ceinture + bretelles si jamais un modal est resté compté.
    /// </summary>
    public void ForceEnableJoystick()
    {
        modalDepth = 0;
        SetJoystickRaycastsEnabled(true);
    }

    void SetJoystickRaycastsEnabled(bool enabled)
    {
        if (joystickCG)
        {
            joystickCG.blocksRaycasts = enabled;
            joystickCG.interactable   = enabled;
        }
        if (joystick)
        {
            joystick.SetRaycastsEnabled(enabled);
            if (!enabled && joystick.background)
                joystick.background.gameObject.SetActive(false);
        }
    }
}

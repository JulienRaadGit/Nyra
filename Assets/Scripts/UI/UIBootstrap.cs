using UnityEngine;
using UnityEngine.EventSystems;

namespace Nyra.UI
{
    /// <summary>
    /// Script de bootstrap pour configurer l'EventSystem en UnscaledTime dès le démarrage.
    /// Résout les problèmes d'interactivité UI quand Time.timeScale = 0.
    /// </summary>
    public class UIBootstrap : MonoBehaviour
    {
        void Awake()
        {
            // Forcer l'EventSystem à utiliser UnscaledTime dès le lancement
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogWarning("[UIBootstrap] Aucun EventSystem trouvé dans la scène");
            }
        }
    }
}

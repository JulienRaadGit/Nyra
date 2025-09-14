using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public TMP_Text goldText;

    void Start()
    {
        if (!playerStats)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerStats = p.GetComponent<PlayerStats>();
        }
        UpdateText( playerStats ? playerStats.Gold : 0 );
        if (playerStats) playerStats.OnGoldChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (playerStats) playerStats.OnGoldChanged -= UpdateText;
    }

    void UpdateText(int value)
    {
        if (goldText) goldText.text = $"<sprite name=coin> {value}"; // ou simplement $"Gold: {value}"
    }
}

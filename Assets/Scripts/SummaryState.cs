using UnityEngine;

public class SummaryState : MonoBehaviour
{
    public GamificationSystem gamificationSystem;
    public GameObject summaryPanel; // Panel de resumen

    public void ShowSummary()
    {
        // Abre el panel del resumen
        summaryPanel.SetActive(true);

        int totalXP = gamificationSystem.currentXP;
        int totalCurrency = gamificationSystem.currentCurrency;

        Debug.Log($"Summary: XP: {totalXP}, Credits: {totalCurrency}");
    }

    public void FinishSummary()
    {
        summaryPanel.SetActive(false);
        FindObjectOfType<GameStateManager>().ChangeState(GameState.Configuring);
        Debug.Log("Returned to ConfiguringState.");
    }
}

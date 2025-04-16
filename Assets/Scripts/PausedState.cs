using UnityEngine;

public class PausedState : MonoBehaviour
{
    public void ResumeGame()
    {
        FindObjectOfType<GameStateManager>().ChangeState(GameState.Working);
        Debug.Log("Game resumed.");
    }
}

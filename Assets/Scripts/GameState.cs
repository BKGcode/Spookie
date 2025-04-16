using UnityEngine;

public enum GameState
{
    Configuring,
    Working,
    Break,
    Paused
}

public class GameStateManager : MonoBehaviour
{
    public static event System.Action<GameState> OnStateChanged;
    private GameState currentState;

    public void ChangeState(GameState newState)
    {
        if (currentState == newState)
        {
            Debug.LogWarning($"Already in {newState} state.");
            return;
        }

        currentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"Game state changed to: {newState}");
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }
}

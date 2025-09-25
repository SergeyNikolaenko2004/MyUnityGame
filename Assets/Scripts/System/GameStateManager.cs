using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState { Playing, InventoryOpen, Paused, Dialogue }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        ApplyCursorState();
    }

    private void ApplyCursorState()
    {
        switch (CurrentState)
        {
            case GameState.Playing:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;

            case GameState.InventoryOpen:
            case GameState.Paused:
            case GameState.Dialogue:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }

    public void SetState(GameState newState)
    {
        if ((CurrentState == GameState.Paused && newState == GameState.InventoryOpen) ||
            (CurrentState == GameState.InventoryOpen && newState == GameState.Paused))
        {
            Debug.LogWarning($"Cannot switch from {CurrentState} to {newState} - conflicting states!");
            return;
        }

        CurrentState = newState;
        ApplyCursorState();

        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.InventoryOpen:
            case GameState.Paused:
            case GameState.Dialogue:
                Time.timeScale = 0f;
                break;
        }

        Debug.Log($"Game state changed to: {newState}");
    }

    public bool CanReceiveInput()
    {
        return CurrentState == GameState.Playing || CurrentState == GameState.InventoryOpen;
    }

    public bool IsGamePaused()
    {
        return CurrentState == GameState.Paused;
    }

    public bool IsInventoryOpen()
    {
        return CurrentState == GameState.InventoryOpen;
    }
}
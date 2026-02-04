using UnityEngine;

public class PlayerSelectionStore : MonoBehaviour
{
    public static PlayerSelectionStore Instance { get; private set; }

    public int SelectedCharacterIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

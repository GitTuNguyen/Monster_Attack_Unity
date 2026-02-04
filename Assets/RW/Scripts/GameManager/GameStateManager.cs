using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public List<GameObject> lootItemList;
    public bool isGameOver;
    public int enemyKilled;
    public float timer;
    private float timePlayed;
    
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        isGameOver = false;
    }
    private void Start()
    {
        enemyKilled = 0;
        timePlayed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer = Time.timeSinceLevelLoad - timePlayed;
        if (SceneManager.GetActiveScene().name == "Main")
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Time.timeScale == 0)
                {
                    ResumeGame();
                    UIManager.Instance.pauseBoard.gameObject.SetActive(false);
                } else
                {
                    StopGame();
                    UIManager.Instance.pauseBoard.gameObject.SetActive(true);
                }
                
            }
        }        
    }

    public void GameOver()
    {
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        enemySpawner.DestroyAllEnemy();
        ClearLootItem();
        isGameOver = true;
        StopGame();
        UIManager.Instance.gameOverUI.SetActive(true);
    }

    public void StopGame()
    {
        Time.timeScale = 0;
        Debug.Log("Stop Game");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        Debug.Log("Resume Game");
    }

    public void ResetGame()
    {
        ResumeGame();
        ClearLootItem();
        isGameOver = false;
        Player player = Player.LocalPlayer ?? FindFirstObjectByType<Player>();
        enemyKilled = 0;
        UIManager.Instance.UpdateAmountEnemyKilledText();
        timePlayed += timer;
        player.ResetGame();
    }

    public void UpdateEnemyKilled()
    {
        enemyKilled++;
        UIManager.Instance.UpdateAmountEnemyKilledText();
    }

    public void BackToMenu()
    {
        LobbyManager.Instance.ShutdownNetwork();
        SceneManager.LoadScene("Menu");
    }

    public void StartGame()
    {
        if (CharacterSetting.Instance.characterSelected != null)
        {
            ResumeGame();
            SceneManager.LoadScene("Main");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void ClearLootItem()
    {
        if (lootItemList != null)
        {
            foreach(var item in lootItemList)
            {
                if (item != null)
                {
                    Destroy(item);
                }                
            }
            lootItemList.Clear();
        }
    }
    
}


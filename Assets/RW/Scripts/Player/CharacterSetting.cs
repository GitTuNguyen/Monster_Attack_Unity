using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSetting : MonoBehaviour
{
    public static CharacterSetting Instance;
    public List<GameObject> characterPrefabList;
    public List<CharacterOption> characterOptions;

    [Header("Character Seleted Details")]
    public GameObject characterSelectedDetails;
    public GameObject characterSelected;
    public Image characterImage;
    public Image weaponImage;
    public TextMeshProUGUI characterInfo;

    public Button startButton = null;
    public Button readyButton = null;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        SetCharacterOptions();
    }
    private void OnEnable()
    {
        startButton.gameObject.SetActive(NetworkManager.Singleton.IsHost || GameModeManager.Mode == GameMode.Offline);
        readyButton.gameObject.SetActive(!NetworkManager.Singleton.IsHost && GameModeManager.Mode != GameMode.Offline);
    }
    public void SetCharacterOptions()
    {
        int index = 0;
        foreach (var option in characterOptions)
        {
            option.SetCharacter(characterPrefabList[index]);
            option.gameObject.SetActive(true);
            index++;
            if (index == characterPrefabList.Count)
            {
                break;
            }
        }
    }

    public void SelectCharacter(CharacterOption character)
    {
        if (!characterSelectedDetails.activeSelf)
        {
            characterSelectedDetails.SetActive(true);
        }
        characterSelected = character.characterPrefabs;
        characterImage.sprite = character.characterImage.sprite;
        weaponImage.sprite = character.weaponDefaultImage.sprite;
        characterInfo.text = character.characterPrefabs.GetComponent<Player>().characterStats.characterInfo;

        int idx = characterPrefabList.IndexOf(character.characterPrefabs);
        if (PlayerSelectionStore.Instance != null && idx >= 0)
        {
            PlayerSelectionStore.Instance.SelectedCharacterIndex = idx;
            if (GameModeManager.Mode != GameMode.Offline)
            {
                LobbyManager.Instance.ClientReady(idx);
            }
        }
    }

    public void StartButton()
    {
        if (GameModeManager.Mode == GameMode.Offline)
        {
            GameStateManager.Instance.StartGame();
        }
        else
        {
            LobbyManager.Instance.StartGame();
        }
    }

    public void ReadyButton()
    {
        if (PlayerSelectionStore.Instance != null && PlayerSelectionStore.Instance.SelectedCharacterIndex >= 0)
        {
            LobbyManager.Instance.ClientReady(PlayerSelectionStore.Instance.SelectedCharacterIndex);
            readyButton.interactable = false;
        }
    }
}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public Vector2 moveDir;
    public Vector2 frontDir = Vector2.right;

    [Header("Character Prefabs")]
    public List<GameObject> characterPrefabs;

    private GameObject characterInstance;
    private CharacterAnimationController animator;

    private NetworkVariable<Vector2> netMoveDir =
        new(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> netFlip =
        new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        netFlip.OnValueChanged += OnFlipChanged;
    }

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        netFlip.OnValueChanged -= OnFlipChanged;
    }

    //void Start()
    //{
    //    if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
    //    {
    //        SpawnOfflineCharacter();
    //    }
    //}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;
        if (scene.name != "Main") return;

        SpawnCharacterForOwner();
    }
    
    void Update()
    {
        if (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening)
        {
            ReadInput();
            UpdateAnimation(moveDir);
            return;
        }

        if (IsOwner)
        {
            ReadInput();
            SubmitInputServerRpc(moveDir, frontDir);
        }

        if (IsServer)
        {
            UpdateAnimation(netMoveDir.Value);
        }
    }

    private void ReadInput()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        moveDir = new Vector2(h, v);

        if (moveDir != Vector2.zero)
            frontDir = moveDir.normalized;
    }

    [ServerRpc]
    private void SubmitInputServerRpc(Vector2 move, Vector2 front)
    {
        netMoveDir.Value = move;
        frontDir = front;
    }

    private void SpawnCharacterForOwner()
    {
        if (!IsServer) return;

        int index = 0;

        if (LobbyManager.Instance != null)
        {
            index = LobbyManager.Instance.GetCharacterIndex(OwnerClientId);
        }

        index = Mathf.Clamp(index, 0, characterPrefabs.Count - 1);

        characterInstance = Instantiate(
            characterPrefabs[index],
            transform.position,
            Quaternion.identity,
            transform
        );

        NetworkObject netObj = characterInstance.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("Character prefab missing NetworkObject");
            Destroy(characterInstance);
            return;
        }

        netObj.SpawnWithOwnership(OwnerClientId);

        //AttachCharacterClientRpc(
        //    netObj.NetworkObjectId,
        //    new ClientRpcParams
        //    {
        //        Send = new ClientRpcSendParams
        //        {
        //            TargetClientIds = new[] { OwnerClientId }
        //        }
        //    }
        //);
        animator = characterInstance.GetComponent<CharacterAnimationController>();

        var player = characterInstance.GetComponent<Player>();
        if (player != null)
            player.SetPlayerController(this);

        CameraController cam = FindFirstObjectByType<CameraController>();
        if (cam != null)
            cam.target = characterInstance.transform;
    }


    public void SpawnOfflineCharacter()
    {
        if (characterPrefabs == null || characterPrefabs.Count == 0)
        {
            Debug.LogError("No character prefabs set");
            return;
        }

        int index = PlayerSelectionStore.Instance != null
            ? PlayerSelectionStore.Instance.SelectedCharacterIndex
            : 0;

        index = Mathf.Clamp(index, 0, characterPrefabs.Count - 1);

        characterInstance = Instantiate(
            characterPrefabs[index],
            transform.position,
            Quaternion.identity,
            transform
        );

        animator = characterInstance.GetComponent<CharacterAnimationController>();

        var player = characterInstance.GetComponent<Player>();
        if (player != null)
            player.SetPlayerController(this);

        CameraController cam = FindFirstObjectByType<CameraController>();
        if (cam != null)
            cam.target = characterInstance.transform;
    }

    [ClientRpc]
    private void AttachCharacterClientRpc( ulong netId, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(netId, out var obj))
            return;

        characterInstance = obj.gameObject;
        characterInstance.transform.SetParent(transform);

        animator = characterInstance.GetComponent<CharacterAnimationController>();

        var player = characterInstance.GetComponent<Player>();
        if (player != null)
            player.SetPlayerController(this);
    }


    private void UpdateAnimation(Vector2 dir)
    {
        if (!animator) return;

        if (dir == Vector2.zero)
            animator.IdleAnimation();
        else
            animator.RunAnimation();

        if (dir.x != 0)
            netFlip.Value = dir.x < 0;
    }

    private void OnFlipChanged(bool _, bool flip)
    {
        if (animator)
            animator.SetFlip(flip);
    }

    public Vector2 ServerMoveDir => netMoveDir.Value;
}

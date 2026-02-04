using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    [System.Serializable]
    public class LootItems
    {
        public string name;
        public GameObject itemPrefabs;
        public float dropRate;
    }

    public List<LootItems> itemsList;
    public GameObject floatingTextPrefabs;
    protected SpriteRenderer spriteRenderer;

    [Header("Take Hit Flash Effect")]
    [SerializeField]
    protected Material originalMaterial;
    [SerializeField]
    protected Material flashMaterial;
    [SerializeField]
    protected float flashDuration;
    [SerializeField]
    protected Coroutine flashCoroutine;

    [Header("Freeze Effect")]
    [SerializeField]
    protected Material freezeMaterial;
    [SerializeField]
    protected Coroutine freezeCoroutine;
    [SerializeField]
    protected float freezeDuration;
    [SerializeField]
    protected bool isFreezing;
    public Animator enemyAnimator;


    [Header("Enemy Stats")]
    [SerializeField]
    protected float health = 100;
    [SerializeField]
    protected float speed = 3;
    [SerializeField]
    protected float takeHitInterval;
    [SerializeField]
    protected float removeTimeDelay;

    public NetworkVariable<float> NetHealth = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected Vector2 dir;
    protected float timeAfterTakeHit;
    protected bool canTakeHit;
    protected bool isDied;
    protected EnemySpawner enemySpawner;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            NetHealth.Value = health;

        NetHealth.OnValueChanged += OnHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        NetHealth.OnValueChanged -= OnHealthChanged;
    }

    protected virtual void Start()
    {
        timeAfterTakeHit = 0;
        canTakeHit = true;
        isDied = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        flashMaterial = new Material(flashMaterial);
        isFreezing = false;
    }

    protected virtual void Update()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        Move();
        FlipSprite();
        if (!canTakeHit && !isDied)
        {
            timeAfterTakeHit += Time.deltaTime;
            if (timeAfterTakeHit >= takeHitInterval)
            {
                timeAfterTakeHit = 0;
                canTakeHit = true;
            }
        }
    }

    protected virtual void Move()
    {
        if (!isDied && !isFreezing)
        {
            transform.Translate(dir * speed * Time.deltaTime);
        }
    }

    public void SetSpawner(EnemySpawner spawner)
    {
        enemySpawner = spawner;
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (newValue < oldValue)
        {
            ShowFloatingText(oldValue - newValue);
            if (!isFreezing)
                FlashEffect();
        }
        health = newValue;
    }

    public void LoseHP(float dmg)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (canTakeHit && !isDied)
        {
            AudioManager.Instance.PlaySFX("EnemyTakeHit");

            if (!isFreezing)
                FlashEffect();

            if (health > dmg)
            {
                health -= dmg;
                canTakeHit = false;
            }
            else
            {
                health = 0;
                Death();
            }

            NetHealth.Value = health;
        }
    }


    public virtual void Death(bool isKillingAll = false)
    {
        if (!isDied)
        {
            if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
                return;

            DropItem();
            isDied = true;
            if (!isKillingAll)
            {
                enemySpawner.RemoveEnemyFromList(gameObject);
            }

            if (GameModeManager.Mode == GameMode.Multiplayer)
                StartCoroutine(DespawnRoutine());
            else
                Destroy(gameObject, removeTimeDelay);
        }
    }

    private IEnumerator DespawnRoutine()
    {
        yield return new WaitForSeconds(removeTimeDelay);
        NetworkObject.Despawn(true);
    }

    protected void ShowFloatingText(float dame)
    {
        if (floatingTextPrefabs)
        {
            var dameText = Instantiate(floatingTextPrefabs, transform.position, Quaternion.identity, transform);
            dameText.GetComponent<TextMeshPro>().text = $"-{dame}";
            dameText.GetComponent<TextMeshPro>().color = Color.white;
        }
    }

    protected virtual void FlipSprite()
    {

    }

    protected void FlashEffect()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    protected IEnumerator FlashCoroutine()
    {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial;
        flashCoroutine = null;
    }

    protected void DropItem()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        float rand = Random.Range(0f, 100f);
        List<LootItems> possibleDrop = new List<LootItems>();
        foreach (LootItems possibleDropItem in itemsList)
        {
            if (rand < possibleDropItem.dropRate)
            {
                possibleDrop.Add(possibleDropItem);
            }
        }
        if (possibleDrop.Count > 0)
        {
            int index = Random.Range(0, possibleDrop.Count);
            var lootItem = Instantiate(possibleDrop[index].itemPrefabs, this.transform.position, Quaternion.identity);
            GameStateManager.Instance.lootItemList.Add(lootItem);

            if (GameModeManager.Mode == GameMode.Multiplayer)
            {
                var netObj = lootItem.GetComponent<NetworkObject>();
                if (netObj != null)
                    netObj.Spawn();
            }
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (other.CompareTag("Projectiles") && !isDied)
        {
            if (other.transform.TryGetComponent<WeaponBehaviour>(out var weapon))
            {
                LoseHP(weapon.dame);
                weapon.OnAttackEnemy();
            }
        }
    }

    public void Freeze(float timeFreeze)
    {
        freezeDuration = timeFreeze;
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }
        freezeCoroutine = StartCoroutine(FreezeCoroutine());
    }


    protected IEnumerator FreezeCoroutine()
    {
        spriteRenderer.material = freezeMaterial;
        isFreezing = true;
        enemyAnimator.speed = 0;
        yield return new WaitForSeconds(freezeDuration);
        isFreezing = false;
        spriteRenderer.material = originalMaterial;
        freezeCoroutine = null;
        enemyAnimator.speed = 1;
    }
}

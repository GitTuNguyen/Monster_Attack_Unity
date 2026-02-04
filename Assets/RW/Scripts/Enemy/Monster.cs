using Unity.Netcode;
using UnityEngine;

public class Monster : Enemy
{

    [SerializeField]
    private int monsterDame = 20;

    private Player target;

    protected override void Update()
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        target = FindClosestPlayer();
        if (target == null) return;
        dir = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y).normalized;
        base.Update();
    }

    private Player FindClosestPlayer()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0)
            return null;

        Player closest = null;
        float minDistance = Mathf.Infinity;
        foreach (var p in players)
        {
            float d = Vector2.Distance(transform.position, p.transform.position);
            if (d < minDistance)
            {
                minDistance = d;
                closest = p;
            }
        }

        return closest;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (collision.collider.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().LoseHP(monsterDame);
        }
        if (collision.collider.CompareTag("LootItem"))
        {
            Physics2D.IgnoreCollision(collision.gameObject.GetComponent<Collider2D>(), this.GetComponent<Collider2D>());
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (GameModeManager.Mode == GameMode.Multiplayer && !IsServer)
            return;

        if (collision.collider.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Player>().LoseHP(monsterDame);
        }
    }

    public override void Death(bool isKillingAll = false)
    {
        GameStateManager.Instance.UpdateEnemyKilled();
        base.Death(isKillingAll);
    }

    protected override void FlipSprite()
    {
        base.FlipSprite();
        if (target == null)
            return;

        if (transform.position.x > target.transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}

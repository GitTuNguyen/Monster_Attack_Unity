using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private bool isFollowing;
    private Animator animator;
    private Player targetPlayer;
    private Vector3 velocity;

    public float modifier = 5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        TryAssignTarget();
    }

    void Update()
    {
        if (targetPlayer == null)
        {
            TryAssignTarget();
            return;
        }

        if (isFollowing)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPlayer.transform.position,
                ref velocity,
                Time.deltaTime * modifier
            );
        }
    }

    private void TryAssignTarget()
    {
        if (Player.LocalPlayer != null)
        {
            targetPlayer = Player.LocalPlayer;
            return;
        }

        targetPlayer = FindFirstObjectByType<Player>();
    }

    public void PickUp(bool isClaimAll = false)
    {
        isFollowing = true;
        if (isClaimAll)
            modifier *= 0.5f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("PickUpArea"))
            return;

        var player = collision.GetComponentInParent<Player>();
        if (player == null || !player.IsOwner)
            return;

        targetPlayer = player;
        PickUp();
        animator.SetTrigger("PickUp");
    }
}

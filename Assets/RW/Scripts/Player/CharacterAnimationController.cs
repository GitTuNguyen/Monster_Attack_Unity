using Unity.Netcode.Components;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator characterAnimator;
    public SpriteRenderer spriteRenderer;
    public NetworkAnimator networkAnimator;

    public void IdleAnimation()
    {
        characterAnimator.SetBool("IsRun", false);
    }

    public void RunAnimation()
    {
        characterAnimator.SetBool("IsRun", true);
    }

    public void TakeHitAnimation()
    {
        if (networkAnimator != null)
            networkAnimator.SetTrigger("TakeHit");
        else
            characterAnimator.SetTrigger("TakeHit");
    }

    public void DeathAnimation()
    {
        characterAnimator.SetBool("IsDeath", true);
    }

    public void ResetGame()
    {
        characterAnimator.SetBool("IsDeath", false);
        if (networkAnimator != null)
            networkAnimator.SetTrigger("Reset");
        else
            characterAnimator.SetTrigger("Reset");

    }

    public void SetFlip(bool isFlip)
    {
        spriteRenderer.flipX = isFlip;
    }
}

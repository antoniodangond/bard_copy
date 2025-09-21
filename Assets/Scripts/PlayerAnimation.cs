using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;
    public PlayerController playerController;

    public void SetAnimationParams(Vector2 movement, bool IsPlayingLyre, bool isAttacking)
    {
        // Use movement.sqrMagnitutde as a proxy for "is moving"
        animator.SetFloat(AnimatorParams.Speed, movement.sqrMagnitude);
        animator.SetBool(AnimatorParams.IsPlayingLyre, IsPlayingLyre);
        // Set lyre animation state
        if (IsPlayingLyre && isAttacking)
        {
            // don't play an animation, spawn particles instead
            // animator.SetBool(AnimatorParams.IsPlayingLyre, false);
            // animator.SetBool(AnimatorParams.IsAttacking, true);

        }
        else if (IsPlayingLyre)
        {
            animator.SetBool(AnimatorParams.IsPlayingLyre, true);
            animator.SetBool(AnimatorParams.IsAttacking, false);
        }
        else
        {
            animator.SetBool(AnimatorParams.IsPlayingLyre, false);
            animator.SetBool(AnimatorParams.IsAttacking, false);
        }
        // Only set direction when moving, so that idle animation
        // will play in the last moved direction
        if (movement.sqrMagnitude > 0)
        {
            // If moving horizontal or diagonal, use horizontal animations.
            // Set opposite axis to 0 to avoid inconsistent behavior when
            // both axes have a non-zero value.
            if (Math.Abs(movement.x) > 0)
            {
                animator.SetFloat(AnimatorParams.DirectionX, movement.x);
                animator.SetFloat(AnimatorParams.DirectionY, 0f);
            }
            else
            {
                animator.SetFloat(AnimatorParams.DirectionX, 0f);
                animator.SetFloat(AnimatorParams.DirectionY, movement.y);
            }
        }
    }
}

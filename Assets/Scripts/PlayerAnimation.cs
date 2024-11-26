using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    public Animator animator;

    public void SetAnimationParams(Vector2 movement, bool IsPlayingLyre)
    {
        // Use movement.sqrMagnitutde as a proxy for "is moving"
        animator.SetFloat(AnimatorParams.Speed, movement.sqrMagnitude);
        // Set lyre animation state
        animator.SetBool(AnimatorParams.IsPlayingLyre, IsPlayingLyre);
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

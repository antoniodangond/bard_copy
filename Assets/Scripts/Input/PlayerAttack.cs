using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static bool CanAttack = true;
    public static bool CanAOEAttack = true;
    public GameObject Weapon;
    [Range(0f, 1f)]
    public float directionalAttackCoolDownTime;
    public float aOEAttackCooldownTime;

    private WeaponController weaponController;
    private Animator animator;
    // Access Sprite Renderer, default color and time to have character briefly flash red when damaged
    private SpriteRenderer spriteRenderer;
    private Color defaultSpriteColor;
    [Range(0f, 1f)]
    public float colorChangeDuration;

    void Awake()
    {
        weaponController = Weapon.GetComponent<WeaponController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSpriteColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
    }

    public IEnumerator AttackCooldown(float attackCoolDownTime, string attackType)
    {
        if (attackType == "Directional")
        {
            CanAttack = false;
            yield return new WaitForSeconds(attackCoolDownTime);
            CustomEvents.OnAttackFinished?.Invoke();
            CanAttack = true;
        }
        else if (attackType == "AOE")
        {
            // manually shortenting this float value because the animation length was looking weird
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length - 0.35f;
            CanAOEAttack = false;
            // For AOE attack, which has a longer cooldown, we need to invoke on attack finished when the animation is done, because
            // the attack is done long before the cooldown is finished, and it looks bad to have the player frozen in the last frame of the
            // animation for 3 seconds
            StartCoroutine(animationTransitionroutine(animationLength));
            yield return new WaitForSeconds(attackCoolDownTime);
            CanAOEAttack = true;
        }
    }

    private IEnumerator animationTransitionroutine(float animationLength)
    {
        yield return new WaitForSeconds(animationLength);
        CustomEvents.OnAttackFinished?.Invoke();
    }

    public void Attack()
    {
        weaponController.Attack();
        StartCoroutine(AttackCooldown(directionalAttackCoolDownTime, "Directional"));
    }

    public void AOEAttack()
    {
        weaponController.AOEAttack();
        StartCoroutine(AttackCooldown(aOEAttackCooldownTime, "AOE"));
    }

    public IEnumerator DamageColorChangeRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < colorChangeDuration)
        {
            // Calculate t based on elapsed time
            float t = elapsedTime / colorChangeDuration;

            // Lerp between defaultSpriteColor and Color.red
            spriteRenderer.color = Color.Lerp(defaultSpriteColor, Color.red, t);

            // Increment elapsed time
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        
        // Reset to default color
        spriteRenderer.color = defaultSpriteColor;
    }

}

using System;
using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static bool CanAttack = true;
    public GameObject Weapon;
    [Range(0f, 1f)]
    public float directionalAttackCoolDownTime;
    public float aOEAttackCooldownTime;

    private WeaponController weaponController;
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
    }

    public IEnumerator AttackCooldown(float attackCoolDownTime)
    {
        CanAttack = false;
        yield return new WaitForSeconds(attackCoolDownTime);
        CustomEvents.OnAttackFinished?.Invoke();
        CanAttack = true;
    }

    public void Attack()
    {
        weaponController.Attack();
        StartCoroutine(AttackCooldown(directionalAttackCoolDownTime));
    }

    public void AOEAttack()
    {
        weaponController.AOEAttack();
        StartCoroutine(AttackCooldown(aOEAttackCooldownTime));
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

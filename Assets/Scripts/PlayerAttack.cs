using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static bool CanAttack = true;
    public GameObject Weapon;
    [Range (0f, 1f)]
    public float attackCoolDownTime;

    private WeaponController weaponController;
    private PlayerController playerController;
    // Access Sprite Renderer, default color and time to have character briefly flash red when damaged
    private SpriteRenderer spriteRenderer;
    private Color defaultSpriteColor;
    [Range(0f, 1f)]
    public float colorChangeDuration;

    void Awake()
    {
        weaponController = Weapon.GetComponent<WeaponController>();
        playerController = gameObject.GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        defaultSpriteColor = spriteRenderer.color;
    }

    private IEnumerator AttackCooldown()
    {
        CanAttack = false;
        yield return new WaitForSeconds(attackCoolDownTime);
        CustomEvents.OnAttackFinished?.Invoke();
        CanAttack = true;
    }

    public void Attack()
    {
        weaponController.Attack();
        StartCoroutine(AttackCooldown());
    }

    public void DamageColorChange()
    {
        StartCoroutine(DamageColorChangeRoutine());
    }

    private IEnumerator DamageColorChangeRoutine()
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

    public void TakeDamage()
    {
        playerController.Health -= 1;

        if (playerController.Health > 0)
        {
            // TODO: implement "stunned" state
            PlayerController.CurrentState = PlayerState.Stunned;
            DamageColorChange();
            StartCoroutine(AttackCooldown());
            Debug.Log("Ouch!");
            PlayerController.CurrentState = PlayerState.Default;
            Debug.Log($"Player health: {playerController.Health}"); 
        }
        else if (playerController.Health <= 0)
        {
            PlayerController.CurrentState = PlayerState.Dead;
            spriteRenderer.enabled = !spriteRenderer.enabled;
            Debug.Log("Dead!");
        }
    }

}

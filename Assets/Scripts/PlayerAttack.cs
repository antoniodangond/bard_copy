using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public static bool CanAttack = true;
    public GameObject Weapon;
    [Range (0f, 1f)]
    public float attackCoolDownTime;

    private WeaponController weaponController;

    void Awake()
    {
        weaponController = Weapon.GetComponent<WeaponController>();
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

    public void TakeDamage()
    {
        // TODO: implement "stunned" state
        Debug.Log("Ouch!");
    }
}

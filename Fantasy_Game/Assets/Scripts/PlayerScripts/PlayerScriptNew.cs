using UnityEngine;
using System.Collections;

public class PlayerScriptNew : MonoBehaviour
{
    [Header("State")]
    private bool canMove = true;
    private bool canAttack = true;
    private bool canTakeDmg = true;
    private bool dead = false;
    private int maxHealth = 100;
    private int currentHealth = 100;
    private bool tookDamageRecently = false;
    private bool hitWithIce = false;

    [Header("References")]
    public Transform cam;
    public CharacterController _characterController;
    public Animator _animator;
    public GameManagerScript gameManager;
    public int eggsRequired;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
    }


    public void TakeDamage(int amount)
    {
        if (!canTakeDmg || dead) return;

        currentHealth -= amount;
        tookDamageRecently = true;
        _animator.SetTrigger("TookDamage");
        gameManager.UpdateHealth(currentHealth);

        canTakeDmg = false;
        canMove = false;

        if (currentHealth <= 0 && !dead)
        {
            Death();
            return;
        }
        StartCoroutine(IFrames());
    }

    public void Heal(int amount)
    {
        if (dead) return;


        //makes sure you dont go above the current max health
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        gameManager.UpdateHealth(currentHealth);
    }

    //for dodge invulnerability
    public void SetInvulnerable(bool value)
    {
        canTakeDmg = !value;
    }

    private IEnumerator IFrames()
    {
        yield return new WaitForSeconds(0.6f); // stun duration
        tookDamageRecently = false;
        _animator.SetTrigger("mountedHorse");
        canMove = true;
        yield return new WaitForSeconds(1); // remaining i-frame duration
        canTakeDmg = true;
    }

    // Collision events just call TakeDamage()
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(25);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(25);
        }
        if (other.CompareTag("EnemyIce"))
        {
            TakeDamage(25);
            hitWithIce = true;
            StartCoroutine(IcePhysicsOff());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BossFightTriggerTag"))// && gameManager.EggCount() == eggsRequired)
        {
            DisableAttack();
            DisableMovement();
            _animator.SetBool("BossIdleAnimation",true);
            StartCoroutine(BossCutsceneTimer());
        }

    }

    private IEnumerator BossCutsceneTimer()
    {
        yield return new WaitForSeconds(8f);
        EnableAttack();
        EnableMovement();
        _animator.SetBool("BossIdleAnimation", false);
    }


    private IEnumerator IcePhysicsOff()
    {
        yield return new WaitForSeconds(2f);
        hitWithIce = false;
    }

    private void Death()
    {
        _animator.SetTrigger("Death");
        _animator.SetBool("Dead", true);
        canMove = false;
        dead = true;
        gameManager.GameOver();
    }

    public void EnableMovement()
    {
        canAttack = true;
        canMove = true;
    }

    public void DisableMovement()
    {
        canAttack = false;
        canMove = false;
    }

    public void EnableAttack()
    {
        canAttack = true;
    }

    public void DisableAttack()
    {
        canAttack = false;
    }

    //for checks in other scripts if the player is dead
    public bool IsDead()
    {
        return dead;
    }
    public bool CanMove()
    {
        return canMove;
    }
    public bool CanAttack()
    {
        return canAttack;
    }
    public int CurrentHealth()
    {
        return currentHealth;
    }

    public bool TookDamageRecently()
    {
        return tookDamageRecently;
    }

    public bool IsIcey()
    {
        return hitWithIce;
    }
}
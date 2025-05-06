using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStat : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float moveSpeed = 5f;
    private int currentHealth;
    [SerializeField] private float additionalClipSize;
    [SerializeField] private int extraBulletPerShot = 0;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage. Remaining health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
    }


    public float getAdditionalClipSize()
    {
        return additionalClipSize;
    }

    public void setAdditionalClipSize(float additionalClipSize)
    {
        this.additionalClipSize = additionalClipSize;
    }

    public int getExtraBulletPerShot()
    {
        return extraBulletPerShot;
    }

    public void setExtraBulletPerShot(int amount)
    {
        this.extraBulletPerShot = amount;
    }

    public float getMoveSpeed()
    {
        return moveSpeed;
    }

    public void setMoveSpeed(float moveSpeed)
    {
        this.moveSpeed = moveSpeed;
    }

    public int getMaxHealth()
    {
        return this.maxHealth;
    }

    public void setMaxHealth(int amount)
    {
        this.maxHealth = amount;
    }

    public int getCurrentHealth()
    {
        return currentHealth;
    }

    public void setCurrentHealth(int amount)
    {
        this.currentHealth = amount;
    }
}

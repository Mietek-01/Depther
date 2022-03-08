using UnityEngine;
using System;

public abstract class Damageable : MonoBehaviour
{
    [Space(10), Header("Damageable")]
    [SerializeField] int health = 1;

    public int Health => health;

    public event Func<GameObject, bool> OnProtect;
    public event Action<GameObject> OnDie;
    public event Action<GameObject> OnTakeDamage;

    protected virtual void Awake()
    {
        SetSubscribers();
    }

    protected abstract void SetSubscribers();

    public virtual bool TakeDamage(GameObject whoIsAttacking, int damage = 1)
    {
        if (OnProtect != null)
            if (OnProtect.Invoke(whoIsAttacking))
                return false;

        health -= damage;

        if (health <= 0)
            if (OnDie != null)
                OnDie.Invoke(whoIsAttacking);
            else
            if (OnTakeDamage != null)
                OnTakeDamage.Invoke(whoIsAttacking);

        return true;
    }
}
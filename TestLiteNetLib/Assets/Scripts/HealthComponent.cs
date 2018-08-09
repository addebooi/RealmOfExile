using System;
using System.Collections;
using System.Collections.Generic;
using CommunicationContract;
using UnityEngine;
using UnityEngine.UI;

public class HealthComponent : MonoBehaviour
{
    [SerializeField]
    protected float _health;
    public float Health
    {
        get { return _health; }
        set
        {
            this._health = value;
            UpdateIsDead();
            UpdateHealthBar(_health, _maxHealth);
        }
    }
    [SerializeField]
    protected float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            this._maxHealth = value;
            UpdateHealthBar(_health, _maxHealth);
        }
    }

    protected bool _isDead;


    [SerializeField]
    private Image GreenHealthBar;
    [SerializeField]
    private Image RedHealthBar;
   

    public delegate void OnDeath();

    private List<OnDeath> OnDeathCallbacks;

    public void RegisterOnDeath(OnDeath onDeathMethod)
    {
        OnDeathCallbacks.Add(onDeathMethod);
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public void UpdateHealthBar(float cHealth, float mHealth)
    {
        GreenHealthBar.fillAmount = MaxHealth!=0? cHealth / MaxHealth : 1;
        RedHealthBar.fillAmount = 1 - GreenHealthBar.fillAmount;
    }
    // Use this for initialization
    void Start ()
    {
        OnDeathCallbacks = new List<OnDeath>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void UpdateIsDead()
    {
        if (_health <= 0)
        {
            _isDead = true;
            foreach (var c in OnDeathCallbacks)
            {
                c();
            }
        }

        else
            _isDead = false;
    }

    public void VariableUpdate(UpdateVariableData updateVariableData)
    {
        //Got variable Update!
        UpdateVariableHander.SetUpdateVariableData(this, updateVariableData);
    }
}

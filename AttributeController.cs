using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttributeController : MonoBehaviour
{
    [Header("Data file for the attribute controller")]
    [SerializeField]
    public AttributeControllerData m_AttributeControllerData;
    
    [Header("The amount of lives associated with the object")]
    [SerializeField]
    protected int m_Lives = 3;

    [Header("The amount of health associated with the object")]
    [SerializeField]
    protected float m_Health=1;

    [Header("The amount of strength associated with the object")]
    [SerializeField]
    protected float m_Strength=1;

    [Header("The speed associated with the object")]
    [SerializeField]
    protected float m_Speed=1;

    [Header("The mass associated with the object")]
    [SerializeField]
    protected float m_Mass=1;

    [Header("The gravity value associated with the object")]
    [SerializeField]
    protected float m_Gravity = -9.8f; //9.8 meters per second default

    [Header("The gravity value associated with the object when falling")]
    [SerializeField]
    protected float m_FallingGravity = 9.8f; //9.8 meters per second default

    [Header("The gravity scale value associated with the object")]
    [SerializeField]
    protected float m_GravityScale = -2.0f; 

    [Header("The gravity scale value associated with the object when falling")]
    [SerializeField]
    protected float m_FallingGravityScale = 2.0f;

    [Header("The current maladies that are attached to the object")]
    [SerializeField]
    private List<Malady> m_CurrentMaladies = new List<Malady>();

    //If the object is DEAD
    public bool m_IsDead { get; private set; }

    //If the check for maladies has started
    private bool m_MaladyCheckStarted = false;

	private void Awake()
	{
		if(m_AttributeControllerData != null)
		{
            LoadFromData(m_AttributeControllerData);
		}

        m_CurrentMaladies = new List<Malady>();
    }



	public void LoadFromData(AttributeControllerData ac)
	{
        m_Lives = ac.m_Lives;
        m_Health = ac.m_Health;
        m_Strength = ac.m_Strength;
        m_Speed = ac.m_Speed;
        m_Mass = ac.m_Mass;
        m_Gravity = ac.m_Gravity;
        m_FallingGravity = ac.m_FallingGravity;
        m_GravityScale = ac.m_GravityScale;
        m_FallingGravityScale = ac.m_FallingGravityScale;
    }

    public float GetSpeed() {
        return m_Speed;
    }

    public float GetStrength()
    {
        return m_Strength;
    }

    public float GetHealth()
    {
        return m_Health;
    }

    public float GetMass()
    {
        return m_Mass;
    }

    public float GetGravity()
	{
        return m_Gravity;
	}

    public void SetGravity(float newGravity)
	{
        m_Gravity = newGravity;
	}


    public float GetFallingGravity()
    {
        return m_FallingGravity;
    }

    public void SetFallingGravity(float newGravity)
    {
        m_FallingGravity = newGravity;
    }

    public float GetGravityScale()
    {
        return m_GravityScale;
    }

    public void SetGravityScale(float newGravity)
    {
        m_GravityScale = newGravity;
    }


    public float GetFallingGravityScale()
    {
        return m_FallingGravityScale;
    }

    public void SetFallingGravityScale(float newGravity)
    {
        m_GravityScale = newGravity;
    }

    public void Initialize(float health, float strength, float speed)
	{
        m_Health = health;
        m_Strength = strength;
        m_Speed = speed;
        m_CurrentMaladies = new List<Malady>();
        m_IsDead = false;
        m_MaladyCheckStarted = false;
        m_Mass = 1.0f;

    }

    public void AffectStrength(float strengthAffector)
	{
        m_Strength += strengthAffector;
	}

    public void AffectSpeed(float speedAffector)
	{
        m_Speed += speedAffector;
	}

    public void SetSpeed(float newSpeed)
	{
        m_Speed = newSpeed;
	}

    public void SetStrength(float newStrength)
	{
        m_Strength = newStrength;
	}

    public void Damage(float dmgAmount)
	{
        m_Health -= dmgAmount;

        if(m_Health <= 0)
		{
            m_Health = 0;
            m_Lives -= 1;
            
            if (m_Lives <= 0)
            {
                m_Lives = 0;
                m_IsDead = true;
            }

        }

        
    }

    public void Heal(float healAmount)
	{
        m_Health += healAmount;
        if (m_Health > 0) m_IsDead = false;
    }

    public void SetHealth(float totalHealth)
	{
        m_Health = totalHealth;
	}

    public void DamagingMalady(DamagingMaladies affect)
	{
        if (affect != null)
		{
            m_CurrentMaladies.Add(affect);
            affect.StartDamageEvent();
            StartCoroutine(affect.DamageEventCoroutine());

            if (!m_MaladyCheckStarted)
            {
                StartCoroutine(MaladyCheck());
                m_MaladyCheckStarted = true;
            }

        }
        
    }


    IEnumerator MaladyCheck()
	{
        var mCount = 0;

        while(m_CurrentMaladies.Count > 0)
		{
            if(m_CurrentMaladies.ElementAt(mCount).m_IsActive == false) m_CurrentMaladies.RemoveAt(mCount);
            mCount = (mCount >= m_CurrentMaladies.Count - 1) ? 0 : mCount + 1;
            yield return new WaitForSeconds(0.1f);
		}

        m_MaladyCheckStarted = false;
        yield return null;
	}

}

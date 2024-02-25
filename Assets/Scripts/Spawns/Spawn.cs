using System;
using ClashRoyaleClone.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ClashRoyaleClone.Spawns
{
    public class Spawn : SpawnBase
    {
        [HideInInspector] public States state;
        public enum States
        {
	        Idle, 
            Seeking, 
            Attacking, 
            Dead, 
        }

        [HideInInspector] public AttackTypeEnum attackType;
        public enum AttackTypeEnum
        {
            Melee,
            Ranged,
        }

        [HideInInspector] public Spawn target;
	    public HpBar healthBar;

        [HideInInspector] public float hitPoints;
        [HideInInspector] public float attackRange;
        [HideInInspector] public float attackRatio;
        [HideInInspector] public float lastBlowTime = -1000f;
        [HideInInspector] public float damage;
		[HideInInspector] public AudioClip attackAudioClip;
        
        [HideInInspector] public float timeToActNext = 0f;

		[Header("Projectile for Ranged and Spells")]
		public GameObject projectilePrefab;
		public Transform projectileSpawnPoint;

		private Projectile _projectile;
		protected AudioSource AudioSource;

		public UnityAction<Spawn> OnDealDamage, OnProjectileFired;


		public virtual void SetTarget(Spawn t)
        {
            target = t;
            t.OnDie += TargetIsDead;
        }

        public virtual void StartAttack()
        {
            state = States.Attacking;
        }

        public virtual void DealBlow()
        {
            lastBlowTime = Time.time;
        }

		public void DealDamage()
		{

			OnDealDamage?.Invoke(this);
		}
		public void FireProjectile()
		{
			

			OnProjectileFired?.Invoke(this);
		}

        public virtual void Seek()
        {
            state = States.Seeking;
        }

        protected void TargetIsDead(SpawnBase p)
        {
            state = States.Idle;
            
            target.OnDie -= TargetIsDead;

            timeToActNext = lastBlowTime + attackRatio;
        }
        
        public bool IsTargetInRange()
        {
	        return (transform.position - target.transform.position).sqrMagnitude <= attackRange * attackRange;
        }

        public float SufferDamage(float amount)
        {
            hitPoints -= amount;
            healthBar.TakeDamage(amount);
            Debug.Log("Suffering damage, new health: " + hitPoints, gameObject);
            if(state != States.Dead && hitPoints <= 0f)
            {
	            healthBar.gameObject.SetActive(false);
                Die();
            }

            return hitPoints;
        }

		public virtual void Stop()
		{
			state = States.Idle;
		}

        protected virtual void Die()
        {
            state = States.Dead;

			OnDie?.Invoke(this);
        }
    }
    
}
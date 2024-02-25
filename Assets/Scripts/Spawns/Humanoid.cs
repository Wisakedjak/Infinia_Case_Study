using ClashRoyaleClone.Cards;
using UnityEngine;
using UnityEngine.AI;

namespace ClashRoyaleClone.Spawns
{
    public class Humanoid : Spawn
    {
        private float speed;

        private Animator animator;
        private NavMeshAgent navMeshAgent;

        private void Awake()
        {
            SpawnType = SpawnTypeEnum.Humanoid;

            animator = GetComponent<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
			AudioSource = GetComponent<AudioSource>();
        }
        
        public void Activate(SpawnOwnerEnum owner, SpawnData spawnData)
        {
            spawnOwner = owner;
            hitPoints = spawnData.hitPoints;
            SpawnTargetType = spawnData.targetType;
            attackRange = spawnData.attackRange;
            attackRatio = spawnData.attackRatio;
            speed = spawnData.speed;
            damage = spawnData.damagePerAttack;
			attackAudioClip = spawnData.attackClip;
			DieAudioClip = spawnData.dieClip;
            
            navMeshAgent.speed = speed;
            animator.SetFloat("MoveSpeed", speed); 

            state = States.Idle;
            navMeshAgent.enabled = true;
        }

        public override void SetTarget(Spawn s)
        {
            base.SetTarget(s);
        }

        public override void Seek()
        {
            if(target == null)
                return;

            base.Seek();

            navMeshAgent.SetDestination(target.transform.position);
            navMeshAgent.isStopped = false;
            animator.SetBool("isMoving", true);
        }
        
        public override void StartAttack()
        {
            base.StartAttack();

            navMeshAgent.isStopped = true;
            animator.SetBool("isMoving", false);
        }
        
        public override void DealBlow()
        {
            base.DealBlow();

            animator.SetTrigger("Attack");
            transform.forward = (target.transform.position - transform.position).normalized; //turn towards the target
        }

		public override void Stop()
		{
			base.Stop();

			navMeshAgent.isStopped = true;
			animator.SetBool("isMoving", false);
		}

        protected override void Die()
        {
            base.Die();

            navMeshAgent.enabled = false;
            animator.SetTrigger("Dead");
        }
    
    }
}
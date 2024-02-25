using ClashRoyaleClone.Cards;
using UnityEngine;

namespace ClashRoyaleClone.Spawns
{
    public class Castle : Spawn
    {
        private void Awake()
        {
            AudioSource = GetComponent<AudioSource>();
        }

        public void Activate(SpawnOwnerEnum owner, SpawnData spawnData)
        {
            SpawnType = spawnData.spawnType;
            spawnOwner = owner;
            hitPoints = spawnData.hitPoints;
            SpawnTargetType = spawnData.targetType;
            attackAudioClip = spawnData.attackClip;
            DieAudioClip = spawnData.dieClip;

            //constructionTimeline.Play();
        }

        protected override void Die()
        {
            base.Die();
            //audioSource.PlayOneShot(dieAudioClip, 1f);

            //Debug.Log("Building is dead", gameObject);
            //destructionTimeline.Play();
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ClashRoyaleClone.Spawns
{
    public class SpawnBase : MonoBehaviour
    {
        public SpawnTypeEnum SpawnType;
		
        [HideInInspector] public SpawnOwnerEnum spawnOwner;
        [HideInInspector] public SpawnTargetEnum SpawnTargetType; //TODO: move to ThinkingPlaceable?
        [HideInInspector] public AudioClip DieAudioClip;

        public UnityAction<SpawnBase> OnDie;

        public enum SpawnTypeEnum
        {
            Humanoid,
            Spell,
            Buff,
            Castle,
            Obstacle,
        }

        public enum SpawnTargetEnum
        {
            OnlyBuildings,
            Both,
            None, 
        }

        public enum SpawnOwnerEnum
        {
            Player,
            Opponent,
            None,
        }
    }
}
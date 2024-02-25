using ClashRoyaleClone.Spawns;
using UnityEngine;
using UnityEngine.Serialization;

namespace ClashRoyaleClone.Cards
{
    [CreateAssetMenu(fileName = "New Spawn", menuName = "Spawn")]
    public class SpawnData : ScriptableObject
    {
        [Header("Common")]
        public SpawnBase.SpawnTypeEnum spawnType;
        public GameObject spawnPrefab;
        public GameObject enemyPrefab;
        public GameObject previewPrefab;
        public int cost = 5;
        public string cardName;
        
        [Header("Units and Buildings")]
        public Spawn.AttackTypeEnum attackType = Spawn.AttackTypeEnum.Melee;
        public SpawnBase.SpawnTargetEnum targetType = SpawnBase.SpawnTargetEnum.Both;
        public float attackRatio = 1f;
        public float damagePerAttack = 2f;
        public float attackRange = 1f;
        public float hitPoints = 10f;
        public AudioClip attackClip, dieClip;

        [Header("Units")]
        public float speed = 5f; 
        
        [Header("Obstacles and Spells")]
        public float lifeTime = 5f;
        
        [Header("Spells")]
        public float damagePerSecond = 1f;
    }
}
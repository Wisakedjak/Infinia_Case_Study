using ClashRoyaleClone.Cards;
using UnityEngine;

namespace ClashRoyaleClone
{
    [CreateAssetMenu(fileName = "New Card", menuName = "Card")]
    public class CardData : ScriptableObject
    {
        [Header("Card graphics")]
        public Sprite cardImage;
        public Sprite cardFrameImage;

        [Header("List of Spawns to be dropped by this card")]
        public SpawnData spawnsData;
        public Vector3[] relativeOffsets;
    }
}
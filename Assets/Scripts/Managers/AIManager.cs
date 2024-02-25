using System;
using System.Collections;
using ClashRoyaleClone.Cards;
using ClashRoyaleClone.Spawns;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ClashRoyaleClone.Managers
{
    public class AIManager : MonoBehaviour
    {
        public event Action<CardInGame, Vector3, SpawnBase.SpawnOwnerEnum> OnCardUsed;
        
        public float aiMoveDelay = 12;
        
        private bool _isAIActive = false;
        private Coroutine _aiMoveCoroutine;
        public CardList aiDeck;
        
        public void MakeMove()
        {
            _isAIActive = true;
            _aiMoveCoroutine = StartCoroutine(CreateRandomCards());
        }
        

        public void StopPlaying()
        {
            _isAIActive = false;
            StopCoroutine(_aiMoveCoroutine);
        }

        private IEnumerator CreateRandomCards()
        {
            while(_isAIActive)
            {
                yield return new WaitForSeconds(aiMoveDelay);
                var newPos = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(55f, 70f));
                OnCardUsed?.Invoke(aiDeck.GetNextCardFromDeck(), newPos, SpawnBase.SpawnOwnerEnum.Opponent);
            }
        }
    }
}
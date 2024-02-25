using System.Collections.Generic;
using UnityEngine;

namespace ClashRoyaleClone.Cards
{
    [CreateAssetMenu(fileName = "New Card List", menuName = "Card List")]
    public class CardList : ScriptableObject
    {
        public List<CardData> cards;
        private int _cardIndex = 0;
        public CardInGame GetNextCardFromDeck()
        {
            _cardIndex++;
            if(_cardIndex >= cards.Count)
                _cardIndex = 0;

            var cardInGame = new GameObject().AddComponent<CardInGame>();
            cardInGame.SetCardDataForAI(cards[_cardIndex]);
            return cardInGame;
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ClashRoyaleClone.UI
{
    public class Card : MonoBehaviour
    {
        [SerializeField] private Image cardFrame;
        [SerializeField] private Image cardIcon;
        public Image tickIcon;
        [SerializeField] private TextMeshProUGUI manaCost,cardName;
        [SerializeField] private Button addCardButton;
        
        public event Action<CardData,int> OnAddCardButtonClicked;
        public event Action<int> OnRemoveCardButtonClicked;
        
        private CardData _cardData;

        public bool isInDeck = false;
        public int cardIndex;
        
        public void SetCardData(CardData cardData,int cardIndex)
        {
            _cardData = cardData;
            cardFrame.sprite = _cardData.cardFrameImage;
            cardIcon.sprite = _cardData.cardImage;
            manaCost.text = _cardData.spawnsData.cost.ToString();
            cardName.text = _cardData.spawnsData.cardName;
            if (!isInDeck)
            {
                this.cardIndex = cardIndex;
                addCardButton.onClick.AddListener(AddCardButtonClicked);
                SetButtonInteractable(true);
                isInDeck = true;
            }
            else
            {
                this.cardIndex = cardIndex;
                addCardButton.onClick.RemoveAllListeners();
                addCardButton.onClick.AddListener(RemoveCardButtonClicked);
            }
            
        }

        public void RemoveCardFromDeck()
        {
            tickIcon.gameObject.SetActive(false);
            addCardButton.onClick.RemoveAllListeners();
            addCardButton.onClick.AddListener(AddCardButtonClicked);
            SetButtonInteractable(true);
        }

        public CardData GetCardData()
        {
            var tmpData = _cardData;
            return tmpData;
        }

        private void AddCardButtonClicked()
        {
            tickIcon.gameObject.SetActive(true);
            addCardButton.interactable = false;
            addCardButton.onClick.RemoveAllListeners();
            OnAddCardButtonClicked?.Invoke(_cardData,cardIndex);
        }

        private void RemoveCardButtonClicked()
        {
            OnRemoveCardButtonClicked?.Invoke(cardIndex);
            Destroy(gameObject);
            
        }
        
        public void SetButtonInteractable(bool interactable)
        {
            addCardButton.interactable = interactable;
        }
    }
}
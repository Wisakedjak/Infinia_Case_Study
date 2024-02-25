using System;
using System.Threading.Tasks;
using ClashRoyaleClone.Spawns;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ClashRoyaleClone.Cards
{
    public class CardInGame : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private Image cardFrame;
        [SerializeField] private Image cardIcon;
        [SerializeField] private TextMeshProUGUI manaCost,cardName;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SpawnBase.SpawnTypeEnum spawnType;
        

        public CardData cardData;
        public int cardId;
        public bool isCardPlayable = false;
        
        public event Action<CardInGame,Vector2> OnDragAction;
        public event Action<CardInGame> OnPointerDownAction, OnPointerUpAction;
        
        private Vector3 _initialPosition;
        
        public void SetCardData(CardData cardData,int id)
        {
            this.cardData = cardData;
            cardFrame.sprite = this.cardData.cardFrameImage;
            cardIcon.sprite = this.cardData.cardImage;
            manaCost.text = this.cardData.spawnsData.cost.ToString();
            cardName.text = this.cardData.spawnsData.cardName;
            spawnType = this.cardData.spawnsData.spawnType;
            cardId = id;
        }

        public void SetCardDataForAI(CardData cardData)
        {
            this.cardData = cardData;
        }
        
        public async Task<Task> MoveCardToPosition(Vector3 position)
        {
            _initialPosition = position;
            await transform.DOMove(position, .5f).AsyncWaitForCompletion();
            return Task.CompletedTask;
        }
        
        public void MoveCardToInitialPosition()
        {
            transform.position = _initialPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isCardPlayable) 
                OnDragAction?.Invoke(this, eventData.delta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isCardPlayable) 
                OnPointerUpAction?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isCardPlayable) 
                OnPointerDownAction?.Invoke(this);
        }
        
        public void OpenCloseCardAlpha(bool isActive)
        {
            canvasGroup.alpha = (isActive) ? 1f : 0f;
        }
        
        public void ChangePlayableState(bool isActive)
        {
            isCardPlayable = isActive;
            canvasGroup.alpha = (isActive) ? 1f : 0.5f;
        }
    }
}
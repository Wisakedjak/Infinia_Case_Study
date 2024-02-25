using System;
using System.Collections.Generic;
using ClashRoyaleClone.Cards;
using ClashRoyaleClone.Spawns;
using ClashRoyaleClone.Utility;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace ClashRoyaleClone.Managers
{
    public class CardManager : MonoBehaviour
    {
        public LayerMask summonAreaFieldMask;
        public CardList CardList { get; private set; }
        public CardList AIDeckList { get; private set; }
        private List<CardData> Deck { get; set; } = new List<CardData>();
        private List<CardData> AIDeck { get; set; } = new List<CardData>();
        public event Action OnDeckFull;
        public event Action<CardInGame, Vector3, SpawnBase.SpawnOwnerEnum> OnCardUsed;
        public event Action<CardInGame> OnCardUsedOutsidePlayArea; 
        [SerializeField] private GameObject[] cards=new GameObject[4];

        private Camera _mainCamera;
        private bool _isCardActive = false;
        private GameObject _previewParent;
        private readonly Vector3 _upFingerOffset = new Vector3(0f, 1f, 0f);
        private void Awake()
        {
            CardList = Resources.Load<CardList>("CardList");
            AIDeckList = Resources.Load<CardList>("AIDeck");
            _previewParent = new GameObject("PreviewParent");
            _mainCamera = Camera.main;
        }
        
        public List<CardData> GetDeck()
        {
            return Deck;
        }

        public List<CardData> GetAIDeck()
        {
            AIDeck = AIDeckList.cards;
            return AIDeck;
        }

        public List<CardData> GetShuffledDeck()
        {
            Deck.Shuffle();
            return Deck;
        }

        public bool AddCardToDeck(CardData cardData)
        {
            
            if (Deck.Count<8)
            {
                Deck.Add(cardData);
                if (Deck.Count==8)
                {
                    OnDeckFull?.Invoke();
                }
                return true;
            }
            return false;
        }

        public void RemoveCardFromDeck(CardData cardData)
        {
            if (Deck.Contains(cardData))
            {
                Deck.Remove(cardData);
            }
        }
        
        public void CardTapped(CardInGame card)
        {
            Debug.Log("Card tapped: " + card.cardData.name + " by " + SpawnBase.SpawnOwnerEnum.Player);
            card.GetComponent<RectTransform>().SetAsLastSibling();
            //forbiddenAreaRenderer.enabled = false;
        }
        
        public void CardDragged(CardInGame card, Vector2 dragAmount)
        {
            card.transform.Translate(dragAmount);

            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            
            bool planeHit = Physics.Raycast(ray, out hit, Mathf.Infinity, summonAreaFieldMask);

            if(planeHit)
            {
                Debug.Log("Card dragged: " + card.cardData.name + " at " + hit.point + " by " + SpawnBase.SpawnOwnerEnum.Player);
                if(!_isCardActive)
                {
                    Debug.Log("Card dragged: " + card.cardData.name + " at " + hit.point + " by " + SpawnBase.SpawnOwnerEnum.Player + " - Card is not active");
                    _isCardActive = true;
                    _previewParent.transform.position = hit.point;
                    card.OpenCloseCardAlpha(false); 

                    SpawnData dataToSpawn = card.cardData.spawnsData;
                    Vector3[] offsets = card.cardData.relativeOffsets;
                    GameObject newPlaceable = GameObject.Instantiate<GameObject>(dataToSpawn.previewPrefab,hit.point + _upFingerOffset, Quaternion.identity, _previewParent.transform);
                    
                }
                else
                {
                    Debug.Log("Card dragged: " + card.cardData.name + " at " + hit.point + " by " + SpawnBase.SpawnOwnerEnum.Player + " - Card is active");
                    _previewParent.transform.position = hit.point;
                }
            }
            else
            {
                if (card.cardData.spawnsData.spawnType==SpawnBase.SpawnTypeEnum.Spell)
                {
                    RaycastHit hitt;
                    Ray rayy = _mainCamera.ScreenPointToRay(Input.mousePosition);
            
                    bool planeHitt = Physics.Raycast(rayy, out hitt, Mathf.Infinity);
                    _previewParent.transform.position = hitt.point;
                    return;
                }
                Debug.Log("Card dragged: " + card.cardData.name + " by " + SpawnBase.SpawnOwnerEnum.Player + " - Card is not active");
                if (!_isCardActive) return;
                _isCardActive = false;
                card.OpenCloseCardAlpha(true);

                ClearPreviewObjects();
            }
        }

        public void CardReleased(CardInGame card)
        {
            RaycastHit hit;
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (card.cardData.spawnsData.spawnType==SpawnBase.SpawnTypeEnum.Humanoid)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, summonAreaFieldMask))
                {
                    Debug.Log("Card used: " + card.cardData.name + " at " + hit.point + " by " + SpawnBase.SpawnOwnerEnum.Player);
                    OnCardUsed?.Invoke(card, hit.point + _upFingerOffset, SpawnBase.SpawnOwnerEnum.Player);

                    ClearPreviewObjects();
                }
                else
                {
                    Debug.Log("Card released outside the play area");
                    OnCardUsedOutsidePlayArea?.Invoke(card);
                    card.MoveCardToInitialPosition();
                    card.OpenCloseCardAlpha(true);
                }

                _isCardActive = false;
            }
            else
            {
                ClearPreviewObjects();
                Physics.Raycast(ray, out hit, Mathf.Infinity);
                OnCardUsed?.Invoke(card, hit.point + _upFingerOffset, SpawnBase.SpawnOwnerEnum.Player);
                _isCardActive = false;
            }
            
        }
        
        private void ClearPreviewObjects()
        {
            
            for(int i=0; i<_previewParent.transform.childCount; i++)
            {
                Destroy(_previewParent.transform.GetChild(i).gameObject);
            }
        }
    }
}
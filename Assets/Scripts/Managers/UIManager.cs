using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClashRoyaleClone.Cards;
using ClashRoyaleClone.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClashRoyaleClone.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Serialized Fields")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button deckButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button returnToMainMenuButton;
        [SerializeField] private GameObject deckMenu;
        [SerializeField] private GameObject endGameMenu;
        [SerializeField] private Transform notInDeckCardsParent;
        [SerializeField] private Transform inDeckCardsParent;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private GameObject cardInGamePrefab;
        [SerializeField] private List<Transform> cardPoints=new List<Transform>(4);
        [SerializeField] private Transform cardSpawnPoint;
        [SerializeField] private List<Image> manaSlots=new List<Image>(10);
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI manaTextShadow;
        [SerializeField] private TextMeshProUGUI winOrLoseTxt;
        
        [Header("Views")] 
        [SerializeField] private GameObject gamePlayView;
        [SerializeField] private GameObject mainMenuView;

        private List<CardData> Deck { get; set; } = new List<CardData>();
        private List<CardData> UnusedDeck { get; set; } = new List<CardData>();
        private int _cardIndexToSpawn=0;
        private int _mana=4;
        private Tween _manaSequence;
        private List<Card> _cards = new List<Card>();

        private List<CardInGame> CardsInGame { get; set; } = new List<CardInGame>();

        public event Action OnStartButtonClicked;
        public event Action OnDeckButtonClicked;
        public event Action<CardData,int> OnAddCardButtonClickedEvent;
        public event Action<CardData> OnRemoveCardButtonClickedEvent;
        public event Action<CardInGame> OnPointerDownAction; 
        public event Action<CardInGame> OnPointerUpAction; 
        public event Action<CardInGame,Vector2> OnDragAction; 
        public event Action OnRestartButtonClicked; 
        public event Action OnReturnToMainMenuButtonClicked; 

        private void Awake()
        {
            startButton.onClick.AddListener(StartButtonClicked);
            deckButton.onClick.AddListener(DeckButtonClicked);
            saveButton.onClick.AddListener(SaveButtonClicked);
            restartButton.onClick.AddListener(RestartButtonClicked);
            returnToMainMenuButton.onClick.AddListener(ReturnToMainMenuButtonClicked);
            WriteManaText();
        }
        
        public void EndGame(bool win)
        {
            winOrLoseTxt.text = win ? "You Win!" : "You Lose!";
            winOrLoseTxt.gameObject.SetActive(true);
            endGameMenu.SetActive(true);
        }

        private void ReturnToMainMenuButtonClicked()
        {
            OnReturnToMainMenuButtonClicked?.Invoke();
        }

        private void RestartButtonClicked()
        {
            OnRestartButtonClicked?.Invoke();
        }

        #region Subscriptions

        private void DeckButtonClicked()
        {
            OnDeckButtonClicked?.Invoke();
        }
        
        private void StartButtonClicked()
        {
            OnStartButtonClicked?.Invoke();
        }
        private void OnAddCardButtonClicked(CardData cardData,int cardIndex)
        {
            Debug.Log("Card added to deck: " + cardData.name);
            OnAddCardButtonClickedEvent?.Invoke(cardData,cardIndex);
        }

        private void OnRemoveCardButtonClicked(int cardIndex)
        {
            _cards[cardIndex].RemoveCardFromDeck();
            OpenNotInDeckCardsInteractable();
            saveButton.interactable=false;
            OnRemoveCardButtonClickedEvent?.Invoke(_cards[cardIndex].GetCardData());
        }
        #endregion
       
        
        public void LoadDeck(List<CardData> cards,bool deckLoaded)
        {
            if (!deckLoaded)
            {
                for (var index = 0; index < cards.Count; index++)
                {
                    var cardData = cards[index];
                    var card = Instantiate(cardPrefab, notInDeckCardsParent).GetComponent<Card>();
                    card.SetCardData(cardData,index);
                    card.isInDeck = false;
                    card.OnAddCardButtonClicked += OnAddCardButtonClicked;
                    _cards.Add(card);
                }
            }
            ShowHideDeckMenu(true);
        }
        
        
        
        private void ShowHideDeckMenu(bool show)
        {
            deckMenu.SetActive(show);
        }
        
        public void ShowHideMainMenu(bool show)
        {
            mainMenuView.SetActive(show);
        }
        
        public void ShowHideGamePlayView(bool show)
        {
            gamePlayView.SetActive(show);
        }
        
        
        
        public void AddedCardToDeck(CardData cardData,int cardIndex)
        {
            var card = Instantiate(cardPrefab, inDeckCardsParent).GetComponent<Card>();
            card.isInDeck = true;
            card.SetCardData(cardData,cardIndex);
            card.OnRemoveCardButtonClicked += OnRemoveCardButtonClicked;
        }
        
        private void SaveButtonClicked()
        {
            ShowHideDeckMenu(false);
        }

        public void DeckFull()
        {
            foreach (Transform card in notInDeckCardsParent)
            {
                card.GetComponent<Card>().SetButtonInteractable(false);
            }
            saveButton.interactable=true;
        }

        private void OpenNotInDeckCardsInteractable()
        {
            var tmpCards = _cards.FindAll(x => !x.tickIcon.IsActive());
            tmpCards.ForEach(x=>x.SetButtonInteractable(true));
        }
        
        public void SetDeck(List<CardData> deck)
        {
            Deck = deck;
        }

        public async Task<Task> CreateFirstCards()
        {
            for (int i = 0; i < 4; i++)
            {
                var card = Instantiate(cardInGamePrefab, cardSpawnPoint).GetComponent<CardInGame>();
                card.SetCardData(Deck[i],i);
                var task=card.MoveCardToPosition(cardPoints[i].position);
                _cardIndexToSpawn++;
                card.ChangePlayableState(true);
                CardsInGame.Add(card);
                card.OnPointerDownAction += OnPointerDown;
                card.OnDragAction += OnDrag;
                card.OnPointerUpAction += OnPointerUp;
                await Task.WhenAll(task);
            }

            for (int i = 4; i < Deck.Count; i++)
            {
                UnusedDeck.Add(Deck[i]);
            }

            return Task.CompletedTask;
        }

        private void OnPointerUp(CardInGame obj)
        {
            OnPointerUpAction?.Invoke(obj);
        }

        private void OnDrag(CardInGame arg1, Vector2 arg2)
        {
            CardsInGame.Remove(arg1);
            OnDragAction?.Invoke(arg1, arg2);
        }

        private void OnPointerDown(CardInGame obj)
        {
            OnPointerDownAction?.Invoke(obj);
        }

        public CardInGame CreateCard(CardData cardDataToAdd)
        {
            var card = Instantiate(cardInGamePrefab, cardSpawnPoint).GetComponent<CardInGame>();
            card.SetCardData(UnusedDeck[0],0);
            card.OnPointerDownAction += OnPointerDown;
            card.OnDragAction += OnDrag;
            card.OnPointerUpAction += OnPointerUp;
            UnusedDeck.Remove(card.cardData);
            Debug.Log("UnusedDeck Count: "+UnusedDeck.Count);
            if(cardDataToAdd != null)
                UnusedDeck.Add(cardDataToAdd);
            Debug.Log("UnusedDeck Count: "+UnusedDeck.Count);
            return card;
        }

        public void DestroyCards()
        {
            foreach (Transform card in cardSpawnPoint)
            {
                Destroy(card.gameObject);
            }
            CardsInGame.Clear();
            UnusedDeck.Clear();
            StopGainMana();
            for (int i = 0; i < manaSlots.Count; i++)
            {
                if (i<_mana)
                {
                    manaSlots[i].fillAmount = 1;
                }
                else
                {
                    manaSlots[i].fillAmount = 0;
                }
            }
            endGameMenu.SetActive(false);
        }

        public void StartGainMana()
        {
            var time = 2f;
            if (_manaSequence!= null)
            {
                var remainingTime = _manaSequence.Duration(false) - _manaSequence.Elapsed(false);
                manaSlots[_mana].fillAmount=_manaSequence.ElapsedPercentage(false);
                time =  remainingTime;
                _manaSequence.Kill();
                
            }
            _manaSequence= manaSlots[_mana].DOFillAmount(1, time).OnComplete(() =>
            {
                _mana++;
                WriteManaText();
                ChangeCardsUsableStateWithMana();
                _manaSequence = null;
                if (_mana > 9) return;
                
                StartGainMana();
            });
        }
        
        public void UseMana(int value)
        {
            for (int i = 0; i < value; i++)
            {
                manaSlots[_mana-i-1].fillAmount = 0;
            }

            if (_mana<10)
            {
                manaSlots[_mana].fillAmount = 0;
            }
            else
            {
                manaSlots[^1].fillAmount = 0;
            }
           
            _mana -= value;
            WriteManaText();
            StartGainMana();
            
        }

        private void StopGainMana()
        {
            _manaSequence.Kill();
            _mana = 4;
        }

        private void WriteManaText()
        {
            manaText.text = _mana.ToString();
            manaTextShadow.text = _mana.ToString();
        }

        public List<Transform> GetCardPoints()
        {
            return cardPoints;
        }
        
        public void AddCardToInGameCardsList(CardInGame card)
        {
            CardsInGame.Add(card);
        }
        
        
        public void ChangeCardsUsableStateWithMana(){
            foreach (var card in CardsInGame)
            {
                card.ChangePlayableState(card.cardData.spawnsData.cost <= _mana);
            }
        }

        public void CardUsedOutsidePlayArea(CardInGame obj)
        {
            CardsInGame.Add(obj);
        }
    }
}
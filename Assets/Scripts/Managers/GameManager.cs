using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClashRoyaleClone.Cards;
using ClashRoyaleClone.Pool;
using ClashRoyaleClone.Spawns;
using ClashRoyaleClone.UI;
using Unity.AI.Navigation;
using Unity.Mathematics;
using UnityEngine;

namespace ClashRoyaleClone.Managers
{
    public class GameManager : MonoBehaviour
    {
        public NavMeshSurface navMeshSurface;
        public GameObject playersCastle, opponentCastle;
        public SpawnData castleSpawnData;
        public ProjectileSpawner ProjectileSpawner;

        private CardManager _cardManager;
        private UIManager _uiManager;
        private AudioManager _audioManager;
        private AIManager _aiManager;

        private CardInGame _nextCard;
        private bool _deckLoaded = false;
        
        private List<Spawn> _playerHumanoids=new List<Spawn>(), _opponentHumanoids=new List<Spawn>();
        private List<Spawn> _playerBuildings=new List<Spawn>(), _opponentBuildings=new List<Spawn>();
        private List<Spawn> _allPlayersSpawns=new List<Spawn>(), _allOpponentsSpawns=new List<Spawn>(); 
        private List<Spawn> _allSpawns=new List<Spawn>();
        private List<Projectile> _allProjectiles=new List<Projectile>();
        private bool _gameOver = false;
        private bool _startAllSpawnProcess = false;
        private void Awake()
        {
            _cardManager = GetComponent<CardManager>();
            _uiManager = GetComponent<UIManager>();
            _aiManager = GetComponent<AIManager>();
        }
        
        private void Start()
        {
            Subscriptions();
        }
        
        private void Subscriptions()
        {
            _uiManager.OnStartButtonClicked += OnStartButtonClicked;
            _uiManager.OnDeckButtonClicked += OnDeckButtonClicked;
            _uiManager.OnAddCardButtonClickedEvent += OnAddCardButtonClicked;
            _uiManager.OnRemoveCardButtonClickedEvent += _cardManager.RemoveCardFromDeck;
            _uiManager.OnPointerDownAction += _cardManager.CardTapped;
            _uiManager.OnPointerUpAction += _cardManager.CardReleased;
            _uiManager.OnDragAction += _cardManager.CardDragged;
            _cardManager.OnDeckFull += _uiManager.DeckFull;
            _cardManager.OnCardUsed += OnCardUse;
            _aiManager.OnCardUsed += OnCardUse;
            _cardManager.OnCardUsedOutsidePlayArea += _uiManager.CardUsedOutsidePlayArea;
            _uiManager.OnRestartButtonClicked += RestartGame;
            _uiManager.OnReturnToMainMenuButtonClicked += ReturnToMainMenu;
        }
        

        #region Subscriptions

        private void OnDeckButtonClicked()
        {
            _uiManager.LoadDeck(_cardManager.CardList.cards,_deckLoaded);
            _deckLoaded = true;
        }
        
        private void OnAddCardButtonClicked(CardData cardData,int cardIndex)
        {
            if (_cardManager.AddCardToDeck(cardData))
            {
             _uiManager.AddedCardToDeck(cardData,cardIndex);   
            }
        }
        
        private async void OnStartButtonClicked()
        {
            if (_deckLoaded)
            {
                _aiManager.aiDeck = _cardManager.AIDeckList;
                SetupSpawn(playersCastle, castleSpawnData, SpawnBase.SpawnOwnerEnum.Player);
                SetupSpawn(opponentCastle, castleSpawnData, SpawnBase.SpawnOwnerEnum.Opponent);
                _uiManager.ShowHideMainMenu(false);
                _uiManager.ShowHideGamePlayView(true);
                _uiManager.SetDeck(_cardManager.GetShuffledDeck());
                Task task = _uiManager.CreateFirstCards();
                await task;
                _uiManager.ChangeCardsUsableStateWithMana();
                _nextCard=_uiManager.CreateCard(null);
                StartGainMana();
                _gameOver = false;
                _aiManager.MakeMove();
            }
        }

        private async void OnCardUse(CardInGame card, Vector3 vector3, SpawnBase.SpawnOwnerEnum arg3)
        {
            SpawnData spawnData = card.cardData.spawnsData;
            if (spawnData.spawnType==SpawnBase.SpawnTypeEnum.Spell)
            {
                GameObject prefabToSpawn = (arg3 == SpawnBase.SpawnOwnerEnum.Player) ? spawnData.spawnPrefab : spawnData.enemyPrefab;
                GameObject newPlaceableGO = Instantiate<GameObject>(prefabToSpawn, vector3 /*+ card.cardData.relativeOffsets[0]*/, Quaternion.identity);
            }
            else
            {
                Quaternion rot = (arg3 == SpawnBase.SpawnOwnerEnum.Player) ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
                GameObject prefabToSpawn = (arg3 == SpawnBase.SpawnOwnerEnum.Player) ? spawnData.spawnPrefab : spawnData.enemyPrefab;
                GameObject newPlaceableGO = Instantiate<GameObject>(prefabToSpawn, vector3 /*+ card.cardData.relativeOffsets[0]*/, rot);
                SetupSpawn(newPlaceableGO, spawnData, arg3);
                _startAllSpawnProcess = true;
            }
            
            if (arg3==SpawnBase.SpawnOwnerEnum.Opponent)return;
            _uiManager.UseMana(card.cardData.spawnsData.cost);
            _uiManager.ChangeCardsUsableStateWithMana();
            _nextCard.cardId = card.cardId;
            Task task = _nextCard.MoveCardToPosition(_uiManager.GetCardPoints()[_nextCard.cardId].position);
            await task;
            _nextCard.ChangePlayableState(true);
            _uiManager.AddCardToInGameCardsList(_nextCard);
            _uiManager.ChangeCardsUsableStateWithMana();
            _nextCard = _uiManager.CreateCard(card.cardData);
        }

        #endregion

        #region Mana

        private void StartGainMana()
        {
            _uiManager.StartGainMana();
        }

        #endregion

        #region Spawns

        private void SetupSpawn(GameObject go, SpawnData spawnData, SpawnBase.SpawnOwnerEnum owner)
        {
                switch(spawnData.spawnType)
                {
                    case SpawnBase.SpawnTypeEnum.Humanoid:
                        Humanoid humanoid = go.GetComponent<Humanoid>();
                        humanoid.Activate(owner, spawnData); 
						humanoid.OnDealDamage += OnSpawnDealtDamage;
						humanoid.OnProjectileFired += OnProjectileFired;
                        humanoid.healthBar.SetHpBar(spawnData.hitPoints);
                        AddSpawnToList(humanoid); 
                        break;

                    case SpawnBase.SpawnTypeEnum.Castle:
                        Castle castle = go.GetComponent<Castle>();
                        castle.Activate(owner, spawnData);
						castle.OnDealDamage += OnSpawnDealtDamage;
						castle.OnProjectileFired += OnProjectileFired;
                        castle.healthBar.SetHpBar(spawnData.hitPoints);
                        castle.healthBar.gameObject.SetActive(true);
                        AddSpawnToList(castle);
                        castle.OnDie += OnCastleDead;
                        break;
                    case SpawnBase.SpawnTypeEnum.Spell:
                        //Spell sScript = newPlaceable.AddComponent<Spell>();
                        //sScript.Activate(pFaction, cardData.hitPoints);
                        //TODO: activate the spell and… ?
                        break;
                }

                go.GetComponent<SpawnBase>().OnDie += OnPlaceableDead;
        }
        private void OnProjectileFired(Spawn spawn)
        {
            Vector3 adjTargetPos = spawn.target.transform.position;
            adjTargetPos.y = 1.5f;
            Quaternion rot = Quaternion.LookRotation(adjTargetPos-spawn.projectileSpawnPoint.position);

            Projectile prj = Instantiate<GameObject>(spawn.projectilePrefab, spawn.projectileSpawnPoint.position, rot).GetComponent<Projectile>();
            prj.target = spawn.target;
            prj.damage = spawn.damage;
            _allProjectiles.Add(prj);
        }
        private void OnPlaceableDead(SpawnBase spawnBase)
        {
            spawnBase.OnDie -= OnPlaceableDead;
            
            switch(spawnBase.SpawnType)
            {
                case SpawnBase.SpawnTypeEnum.Humanoid:
                    Humanoid humanoid = (Humanoid)spawnBase;
                    RemoveSpawnFromList(humanoid);
                    humanoid.OnDealDamage -= OnSpawnDealtDamage;
                    humanoid.OnProjectileFired -= OnProjectileFired;
                    Dispose(humanoid);
                    break;

                case SpawnBase.SpawnTypeEnum.Castle:
                    Castle castle = (Castle)spawnBase;
                    RemoveSpawnFromList(castle);
                    castle.OnDealDamage -= OnSpawnDealtDamage;
                    castle.OnProjectileFired -= OnProjectileFired;
                    break;

                case SpawnBase.SpawnTypeEnum.Spell:
                    //TODO: can spells die?
                    break;
            }
        }
        private void OnSpawnDealtDamage(Spawn spawn)
        {
            if(spawn.target.state != Spawn.States.Dead)
            {
                float newHealth = spawn.target.SufferDamage(spawn.damage);
                Debug.Log("New Health: " + newHealth);
            }
        }
        private void OnCastleDead(SpawnBase spawnBase)
        {
            spawnBase.OnDie -= OnCastleDead;
            _gameOver = true;
            
            Spawn spawn;
            for(int i=0; i<_allSpawns.Count; i++)
            {
                spawn = _allSpawns[i];
                if(spawn.state != Spawn.States.Dead)
                {
                    spawn.Stop();
                    spawn.transform.LookAt(spawnBase.transform.position);
                }
            }
            _uiManager.EndGame(WinOrLose(spawnBase));
            Debug.Log("Game Over!");
            _aiManager.StopPlaying();
        }
        
        private bool WinOrLose(SpawnBase spawnBase)
        {
            if (spawnBase.spawnOwner == SpawnBase.SpawnOwnerEnum.Opponent)
            {
                return true;
            }
            return false;
        }
        private async void Dispose(Spawn spawn)
        {
            await Task.Delay(TimeSpan.FromSeconds(3f));

            Destroy(spawn.gameObject);
        }
        private IEnumerator RebuildNavmesh()
        {
            yield return new WaitForEndOfFrame();

            navMeshSurface.BuildNavMesh();
        }
        private void AddSpawnToList(Spawn spawn)
        {
            _allSpawns.Add(spawn);

            switch (spawn.spawnOwner)
            {
                case SpawnBase.SpawnOwnerEnum.Player:
                {
                    _allPlayersSpawns.Add(spawn);
            	
                    if(spawn.SpawnType == SpawnBase.SpawnTypeEnum.Humanoid)
                        _playerHumanoids.Add(spawn);
                    else
                        _playerBuildings.Add(spawn);
                    break;
                }
                case SpawnBase.SpawnOwnerEnum.Opponent:
                {
                    _allOpponentsSpawns.Add(spawn);
            	
                    if(spawn.SpawnType == SpawnBase.SpawnTypeEnum.Humanoid)
                        _opponentHumanoids.Add(spawn);
                    else
                        _opponentBuildings.Add(spawn);
                    break;
                }
                case SpawnBase.SpawnOwnerEnum.None:
                default:
                    break;
            }
        }
        private void RemoveSpawnFromList(Spawn spawn)
        {
            _allSpawns.Remove(spawn);

            switch (spawn.spawnOwner)
            {
                case SpawnBase.SpawnOwnerEnum.Player:
                {
                    _allPlayersSpawns.Remove(spawn);
            	
                    if(spawn.SpawnType == SpawnBase.SpawnTypeEnum.Humanoid)
                        _playerHumanoids.Remove(spawn);
                    else
                        _playerBuildings.Remove(spawn);
                    break;
                }
                case SpawnBase.SpawnOwnerEnum.Opponent:
                {
                    _allOpponentsSpawns.Remove(spawn);
            	
                    if(spawn.SpawnType == SpawnBase.SpawnTypeEnum.Humanoid)
                        _opponentHumanoids.Remove(spawn);
                    else
                        _opponentBuildings.Remove(spawn);
                    break;
                }
                case SpawnBase.SpawnOwnerEnum.None:
                default:
                    break;
            }
        }
        private List<Spawn> GetAttackList(SpawnBase.SpawnOwnerEnum owner, SpawnBase.SpawnTargetEnum targetType)
        {
            switch(targetType)
            {
                case SpawnBase.SpawnTargetEnum.Both:
                    return (owner == SpawnBase.SpawnOwnerEnum.Player) ? _allOpponentsSpawns : _allPlayersSpawns;
                case SpawnBase.SpawnTargetEnum.OnlyBuildings:
                    return (owner == SpawnBase.SpawnOwnerEnum.Player) ? _opponentBuildings : _playerBuildings;
            }
            return null;
        }

        private bool FindClosestSpawn(Vector3 position, List<Spawn> spawns, out Spawn spawn)
        {
            var spawnsOrdered=spawns.OrderBy(x=>Vector3.Distance(x.transform.position,position)).ToList();
            if (spawnsOrdered.Count>0)
            {
                spawn = spawnsOrdered[0];
                return true;
            }
            spawn = null;
            return false;
        }

        #endregion

        #region Update

        private void Update()
        {
            if(_gameOver)
                return;

            Spawn targetSpawn; 
			Spawn spawn; 

			for(int i=0; i<_allSpawns.Count; i++)
            {
                spawn = _allSpawns[i];

                if(_startAllSpawnProcess)
                    spawn.state = Spawn.States.Idle;

                switch(spawn.state)
                {
                    case Spawn.States.Idle:

                        bool isTargetFound = FindClosestSpawn(spawn.transform.position, GetAttackList(spawn.spawnOwner, spawn.SpawnTargetType), out targetSpawn);
                        spawn.SetTarget(targetSpawn);
						spawn.Seek();
                        break;


                    case Spawn.States.Seeking:
						if(spawn.IsTargetInRange())
                    	{
							spawn.StartAttack();
						}
                        break;
                        

					case Spawn.States.Attacking:
						if(spawn.IsTargetInRange())
						{
							if(Time.time >= spawn.lastBlowTime + spawn.attackRatio)
							{
								spawn.DealBlow();
								
							}
						}
						break;

					case Spawn.States.Dead:
						break;
                }
            }

			Projectile currentProjectile;
			float distanceToTarget;
			for(int i=0; i<_allProjectiles.Count; i++)
            {
				currentProjectile = _allProjectiles[i];
                if(currentProjectile==null)continue;
				distanceToTarget = currentProjectile.Move();
				if(distanceToTarget >= 1f)
				{
					if(currentProjectile.target.state != Spawn.States.Dead)
					{
						float newHP = currentProjectile.target.SufferDamage(currentProjectile.damage);
						currentProjectile.target.healthBar.TakeDamage(currentProjectile.damage);
					}
					Destroy(currentProjectile.gameObject);
					_allProjectiles.RemoveAt(i);
				}
			}

            _startAllSpawnProcess = false;
        }

        #endregion

        private async void RestartGame()
        {
            ClearAllSpawns();
            _uiManager.DestroyCards();
            SetupSpawn(playersCastle, castleSpawnData, SpawnBase.SpawnOwnerEnum.Player);
            SetupSpawn(opponentCastle, castleSpawnData, SpawnBase.SpawnOwnerEnum.Opponent);
            _uiManager.SetDeck(_cardManager.GetShuffledDeck());
            Task task = _uiManager.CreateFirstCards();
            await task;
            _uiManager.ChangeCardsUsableStateWithMana();
            _nextCard=_uiManager.CreateCard(null);
            _gameOver= false;
            StartGainMana();
            _aiManager.MakeMove();
        }
        
        private void ClearAllSpawns()
        {
            var tmpSpawns = _allSpawns.Where(x=>x.SpawnType==SpawnBase.SpawnTypeEnum.Humanoid).ToList();
            for(int i=0; i<tmpSpawns.Count; i++)
            {
                Destroy(tmpSpawns[i].gameObject);
            }

            for (int i = 0; i < _allProjectiles.Count; i++)
            {
                if (_allProjectiles[i] == null) continue;
                Destroy(_allProjectiles[i].gameObject);
            }
            _allProjectiles.Clear();
            _allSpawns.Clear();
            _allPlayersSpawns.Clear();
            _allOpponentsSpawns.Clear();
            _playerHumanoids.Clear();
            _opponentHumanoids.Clear();
            _playerBuildings.Clear();
            _opponentBuildings.Clear();
        }

        private void ReturnToMainMenu()
        {
            ClearAllSpawns();
            _uiManager.DestroyCards();
            _uiManager.ShowHideMainMenu(true);
            _uiManager.ShowHideGamePlayView(false);
        }
        
        

    }
}
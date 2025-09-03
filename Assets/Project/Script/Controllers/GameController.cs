using System;
using System.Collections.Generic;
using DG.Tweening;
using Gazeus.DesafioMatch3.Core;
using Gazeus.DesafioMatch3.Models;
using Gazeus.DesafioMatch3.Views;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private BoardView _boardView;
        [SerializeField] private BoardView _patternView;
        [SerializeField] private ButtonsController _buttonsController;
        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;
        [SerializeField] private GameMode _gameMode;
        [SerializeField] private List<TileType> _tileTypes;

        [SerializeField] private PlayerResourcesView _playerResourcesView;
        
        private GameService _gameEngine;
        private bool _isAnimating;
        private bool _isShowingHint;
        private int _selectedX = -1;
        private int _selectedY = -1;

        private readonly int scoreMultiplier = 10;

        private Tween suggestionCallTween;
        private Item selectedItem;
        
        #region Unity
        private void Awake()
        {
            _gameEngine = new GameService();
            _boardView.TileClicked += OnTileClick;
            
            suggestionCallTween = DOVirtual.DelayedCall(15f, GetHint).SetLoops(-1);
        }

        private void OnDestroy()
        {
            _boardView.TileClicked -= OnTileClick;
        }

        private void Start()
        {
            Table<Tile> board = _gameEngine.StartGame(_boardWidth, _boardHeight, _gameMode, _tileTypes);
            _boardView.CreateBoard(board);
            
            if(_gameMode == GameMode.FindThePattern)
                _patternView.CreateBoard(new Table<Tile>(_gameEngine.GetPattern()));
            //UpdatePattern();
        }
        #endregion

        private void AnimateBoard(List<BoardSequence> boardSequences, int index, Action onComplete)
        {
            BoardSequence boardSequence = boardSequences[index];

            Sequence sequence = DOTween.Sequence();
            sequence.Append(_boardView.DestroyTiles(boardSequence.MatchedPosition).
                OnComplete(()=>{AddPoints(boardSequence.MatchedPosition.Count*scoreMultiplier);}));
            sequence.Append(_boardView.MoveTiles(boardSequence.MovedTiles));
            sequence.Append(_boardView.CreateTile(boardSequence.AddedTiles));

            index += 1;
            if (index < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, index, onComplete);
            }
            else
            {
                sequence.onComplete += () => onComplete();
            }
        }

        public void TilesSlideLeft()
        {
            if (_isAnimating) return;

            _isAnimating = true;

            Sequence fullSequence = DOTween.Sequence();

            Dictionary<int, Sequence> sequences = new Dictionary<int, Sequence>();
            
            for (int i = 0; i < _boardHeight; i++)
            {
                for (int j = 0; j < _boardWidth; j++)
                {
                    sequences.TryAdd(j, DOTween.Sequence());
                    sequences.TryGetValue(j, out Sequence partialSequence);
                    
                    if(j == _boardWidth-1)
                        continue;
                    
                    partialSequence.Join(_boardView.SwapTiles(j, i, j + 1, i));
                }
            }
            
            foreach (Sequence s in sequences.Values)
                fullSequence.Append(s);
            
            fullSequence.onComplete += () =>
            {
                List<BoardSequence> result = _gameEngine.TableSlideLeft();
                AnimateBoard(result, 0, () => _isAnimating = false);
            };
        }
        
        public void TilesSlideRight()
        {
            if (_isAnimating) return;

            _isAnimating = true;

            Sequence fullSequence = DOTween.Sequence();

            Dictionary<int, Sequence> sequences = new Dictionary<int, Sequence>();
            
            for (int i = _boardHeight - 1; i >= 0; i--)
            {
                for (int j = _boardWidth - 1; j >= 0; j--)
                {
                    sequences.TryAdd(j, DOTween.Sequence());
                    sequences.TryGetValue(j, out Sequence partialSequence);
                    
                    if(j == 0)
                        continue;
                    
                    partialSequence.Join(_boardView.SwapTiles(j, i, j - 1, i));
                }
            }
            
            foreach (Sequence s in sequences.Values)
                fullSequence.Append(s);

            suggestionCallTween.Pause();
            
            fullSequence.onComplete += () =>
            {
                List<BoardSequence> result = _gameEngine.TableSlideRight();
                AnimateBoard(result, 0, OnFinishAnimating);
            };
        }

        private void SquareRotateClockwise(int x, int y)
        {
            if(_isAnimating) return;

            _isAnimating = true;
            
            Sequence sequence = DOTween.Sequence();

            if (x < 1)
                x = 1;
            if (y < 1)
                y = 1;

            sequence.Append(_boardView.SwapTiles(x, y, x, y - 1));
            sequence.Join(_boardView.SwapTiles(x, y - 1, x - 1, y - 1));
            sequence.Join(_boardView.SwapTiles(x-1, y - 1, x - 1, y));

            suggestionCallTween.Pause();

            sequence.onComplete += () =>
            {
                List<BoardSequence> result = _gameEngine.SquareRotateClockwise(x, y);
                AnimateBoard(result, 0, OnFinishAnimating);
            };
        }
        
        private void SquareRotateCounterClockwise(int x, int y)
        {
            if(_isAnimating) return;

            _isAnimating = true;
            
            Sequence sequence = DOTween.Sequence();

            if (x < 1)
                x = 1;
            if (y < 1)
                y = 1;

            sequence.Append(_boardView.SwapTiles(x-1, y, x-1, y-1));
            sequence.Join(_boardView.SwapTiles(x-1, y-1, x, y-1));
            sequence.Join(_boardView.SwapTiles(x, y-1, x, y));

            suggestionCallTween.Pause();

            sequence.onComplete += () =>
            {
                List<BoardSequence> result = _gameEngine.SquareRotateCounterClockwise(x, y);
                AnimateBoard(result, 0, OnFinishAnimating);
            };
        }

        public void SetItem(Item item)
        {
            DeselectTile();
            selectedItem = item;
            _buttonsController.ActivateButtons();
        }
        
        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            switch (selectedItem)
            {
                case Item.None:
                    if (_selectedX > -1 && _selectedY > -1)
                    {
                        //Deselect if far click
                        if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                            DeselectTile();
                        //Run swap tiles
                        else 
                            TryTileSwap(x, y);
                    }
                    else
                    {
                        //Set selected
                        SetSelectedTile(x, y);
                    }
                    break;
                case Item.FreeSwap:
                    if (_selectedX > -1 && _selectedY > -1)
                            TryTileSwap(x, y);
                    else
                        SetSelectedTile(x, y);
                    break;
                case Item.Pick:
                    DestroyTile(x,y);
                    break;
                
                case Item.Bomb:
                    DestroyExplosion(x,y);
                    break;
                
                case Item.SquareRotate:
                    SquareRotateCounterClockwise(x,y);
                    break;
                

            }
            
            
            
            if(_isAnimating)
                _boardView.ClearSelectedSpotEffect();
            else
                _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
            
        }

        private void TryTileSwap(int x, int y)
        {
            _isAnimating = true;
            _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
            {
                bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y);
                if (isValid)
                {
                    suggestionCallTween.Pause();
                    List<BoardSequence> swapResult = _gameEngine.SwapTile(_selectedX, _selectedY, x, y);
                    AnimateBoard(swapResult, 0, ()=>
                    {
                        if(_gameMode == GameMode.FindThePattern)
                            UpdatePatternView();
                        OnFinishAnimating();
                    });
                            
                }
                else
                {
                    _boardView.SwapTiles(x, y, _selectedX, _selectedY).onComplete += OnFinishAnimating;
                }
            };
        }

        private void DestroyTile(int x, int y)
        {
            if (_isAnimating) return;

            _isAnimating = true;
            
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.DestroySingleTile(x, y);
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        private void DestroyExplosion(int x, int y)
        {
            if (_isAnimating) return;
            
            suggestionCallTween.Restart();
            _isAnimating = true;
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.Explosion(x, y, 5);
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        public void EarthquakeEffect()
        {
            if (_isAnimating) return;
            
            suggestionCallTween.Restart();

            _isAnimating = true;
            
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.EarthQuake();
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        public void GetHint()
        {
            if (_isAnimating)
                return;

            if (_isShowingHint)
                return;

            suggestionCallTween.Restart();
            
            _isShowingHint = true;
            var suggestions = _gameEngine.GetSuggestions();

            if (suggestions.Count == 0)
            {
                _isShowingHint = false;
                return;
            }
                
            
            Vector2Int tile = suggestions[Random.Range(0, suggestions.Count)];
            
            _boardView.PlayTileSuggestionAnimate(tile.x, tile.y).onComplete += () => { _isShowingHint = false;};
        }
        
        private void UpdatePatternView()
        {
            BoardSequence boardSequence = new BoardSequence();
            
            var patternTable = _gameEngine.GetPattern();

            boardSequence.MatchedPosition = new List<Vector2Int>();
            boardSequence.AddedTiles = new List<AddedTileInfo>();
            
            for (int x = 0; x < patternTable.Width; x++)
            for (int y = 0; y < patternTable.Height; y++)
            {
                boardSequence.MatchedPosition.Add(new Vector2Int(x,y));
                boardSequence.AddedTiles.Add(new AddedTileInfo()
                {
                    Position = new Vector2Int(x,y),
                    Type = patternTable[x,y]
                });
            }
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_patternView.DestroyTiles(boardSequence.MatchedPosition));
            sequence.Append(_patternView.CreateTile(boardSequence.AddedTiles));

        }

        private void OnFinishAnimating()
        {
            _isAnimating = false;
            suggestionCallTween.Restart();
            DeselectTile();
            SetItem((int)Item.None);
        }
        
        private void SetSelectedTile(int x, int y)
        {
            _selectedX = x;
            _selectedY = y;
            _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
        }
        private void DeselectTile()
        {
            _selectedX = -1;
            _selectedY = -1;
            _boardView.ClearSelectedSpotEffect();
            _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
        }
        
        //TODO Receber conjuntos de linhas e calcular aqui a pontuação
        private void AddPoints(int value)
        {
            _playerResourcesView.AddPoints(value);
        }
        
    }
}

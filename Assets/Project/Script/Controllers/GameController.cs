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
        [SerializeField] private int _boardHeight = 10;
        [SerializeField] private int _boardWidth = 10;

        [SerializeField] private PlayerResourcesView _playerResourcesView;
        
        private GameService _gameEngine;
        private bool _isAnimating;
        private bool _isShowingHint;
        private int _selectedX = -1;
        private int _selectedY = -1;

        private readonly int scoreMultiplier = 10;

        private Tween suggestionCallTween;
        
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
            List<List<Tile>> board = _gameEngine.StartGame(_boardWidth, _boardHeight);
            _boardView.CreateBoard(board);
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

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        bool isValid = _gameEngine.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (isValid)
                        {
                            suggestionCallTween.Pause();
                            List<BoardSequence> swapResult = _gameEngine.SwapTile(_selectedX, _selectedY, x, y);
                            AnimateBoard(swapResult, 0, OnFinishAnimating);
                            
                        }
                        else
                        {
                            _boardView.SwapTiles(x, y, _selectedX, _selectedY).onComplete += () => _isAnimating = false;
                        }
                        _selectedX = -1;
                        _selectedY = -1;
                        _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
            
            if(_isAnimating)
                _boardView.ClearSelectedSpotEffect();
            else
                _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
            
        }

        public void DestroySelectedTile()
        {
            if (_isAnimating) return;
            
            if(_selectedX<0 || _selectedY<0)
                return;
            
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.DestroySingleTile(_selectedX, _selectedY);
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        public void DestroyExplosion()
        {
            if (_isAnimating) return;
            
            if(_selectedX<0 || _selectedY<0)
                return;
            
            suggestionCallTween.Restart();
            
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.Explosion(_selectedX, _selectedY, 5);
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        public void EarthquakeEffect()
        {
            if (_isAnimating) return;
            
            suggestionCallTween.Restart();
            
            _boardView.ClearSelectedSpotEffect();
            suggestionCallTween.Pause();
            List<BoardSequence> result = _gameEngine.EarthQuake();
            AnimateBoard(result, 0, OnFinishAnimating);
        }

        public void GetHint()
        {
            Debug.Log("GetHint");
            
            if(_isAnimating)
                return;
            
            if(_isShowingHint)
                return;

            suggestionCallTween.Restart();
            
            _isShowingHint = true;
            var suggestions = _gameEngine.GetSuggestions();
            var tile = suggestions[Random.Range(0, suggestions.Count - 1)];
            
            _boardView.PlayTileSuggestionAnimate(tile.x, tile.y).onComplete += () => { _isShowingHint = false;};
        }

        private void OnFinishAnimating()
        {
            _isAnimating = false;
            suggestionCallTween.Restart();
            _selectedX = -1;
            _selectedY = -1;
            _boardView.SetTileSpotSelectedEffect(_selectedX,_selectedY);
        }
        
        //TODO Receber conjuntos de linhas e calcular aqui a pontuação
        private void AddPoints(int value)
        {
            _playerResourcesView.AddPoints(value);
        }
        
    }
}

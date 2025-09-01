using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public class TileSpotView : MonoBehaviour
    {
        public event Action<int, int> Clicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _highlightImage;
        [SerializeField] private Image _suggestionImage;

        private Tween _suggestionTween;
        
        private int _x;
        private int _y;

        #region Unity
        private void Awake()
        {
            _button.onClick.AddListener(OnTileClick);
        }
        #endregion

        public Tween AnimatedSetTile(GameObject tile)
        {
            StopSuggestionTween();
            tile.transform.SetParent(transform);
            tile.transform.DOKill();

            return tile.transform.DOMove(transform.position, 0.3f);
        }

        public void SetPosition(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public void SetTile(GameObject tile)
        {
            StopSuggestionTween();
            tile.transform.SetParent(transform, false);
            tile.transform.position = transform.position;
        }

        public void SetHighlight(bool value)
        {
            StopSuggestionTween();
            _highlightImage.gameObject.SetActive(value);
        }

        public Tween AnimateSuggestionHighlight()
        {
            StopSuggestionTween();
            
            float animationTime = 1f;

            _suggestionTween = DOVirtual.Color(Color.clear, Color.yellow, animationTime, c => _suggestionImage.color = c).SetLoops(4);
            _suggestionTween.onComplete+=(() => { _suggestionImage.color = Color.clear; });

            return _suggestionTween;
        }

        private void StopSuggestionTween()
        {
            if(_suggestionTween!=null)
                _suggestionTween.Complete();
        }

        private void OnTileClick()
        {
            Clicked?.Invoke(_x, _y);
        }
    }
}

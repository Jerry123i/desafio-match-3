using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public class SceneTransitionView : MonoBehaviour
    {
        [SerializeField] private RectTransform _slider;
        public void RunTransitionAnimation(Action onComplete)
        {
            AnimateTransitionSlider().onComplete += () => { onComplete?.Invoke(); };
        }

        private Tween AnimateTransitionSlider()
        {
            return _slider.DOMoveX(-100f, .85f).SetEase(Ease.OutSine);
        }
        
    }
}

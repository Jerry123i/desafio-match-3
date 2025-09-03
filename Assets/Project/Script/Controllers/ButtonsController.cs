using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ButtonsController : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private Button _pickButton;
        [SerializeField] private Button _bombButton;
        [SerializeField] private Button _squareRotateButton;
        [SerializeField] private Button _freeSwapButton;

        private void Awake()
        {
            _pickButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.Pick);
                _pickButton.interactable = false;
            });
            
            _bombButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.Bomb);
                _bombButton.interactable = false;
            });
            
            _squareRotateButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.SquareRotate);
                _squareRotateButton.interactable = false;
            });
            
            _freeSwapButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.FreeSwap);
                _freeSwapButton.interactable = false;
            });
            
        }

        public void ActivateButtons()
        {
            _pickButton.interactable = true;
            _bombButton.interactable = true;
            _squareRotateButton.interactable = true;
            _freeSwapButton.interactable = true;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class ButtonsController : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;
        [SerializeField] private Button pickButton;
        [SerializeField] private Button bombButton;
        [SerializeField] private Button squareRotateButton;

        private void Awake()
        {
            pickButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.Pick);
                pickButton.interactable = false;
            });
            
            bombButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.Bomb);
                bombButton.interactable = false;
            });
            
            squareRotateButton.onClick.AddListener(() =>
            {
                _gameController.SetItem(Item.SquareRotate);
                squareRotateButton.interactable = false;
            });
            
        }

        public void ActivateButtons()
        {
            pickButton.interactable = true;
            bombButton.interactable = true;
            squareRotateButton.interactable = true;
        }
    }
}

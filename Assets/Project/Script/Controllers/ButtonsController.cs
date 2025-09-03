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
            
        }

        public void ActivateButtons()
        {
            pickButton.interactable = true;
            bombButton.interactable = true;
        }
    }
}

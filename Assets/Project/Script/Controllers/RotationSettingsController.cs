using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class RotationSettingsController : MonoBehaviour
    {
        [SerializeField] private GameController _gameController;

        [SerializeField] private Button _cwButton;
        [SerializeField] private Button _ccButton;

        private RotationDirection _currentDirection;
        
        private void Awake()
        {
            _cwButton.onClick.AddListener(()=>SetDirection(RotationDirection.ClockWise));
            _ccButton.onClick.AddListener(()=>SetDirection(RotationDirection.CounterClockWise));
            
            SetDirection(RotationDirection.ClockWise);
            
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Q))
                SetDirection(RotationDirection.ClockWise);
            
            if(Input.GetKeyDown(KeyCode.E))
                SetDirection(RotationDirection.CounterClockWise);
        }

        private void SetDirection(RotationDirection direction)
        {
            
            _currentDirection = direction;

            _cwButton.interactable = _currentDirection != RotationDirection.ClockWise;
            _ccButton.interactable = _currentDirection != RotationDirection.CounterClockWise;
            
            if(_gameController == null)
                return;

            _gameController.SetDirection(_currentDirection);

        }
    }
}

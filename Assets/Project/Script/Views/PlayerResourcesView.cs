using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Models;
using UnityEngine;
using TMPro;

namespace Gazeus.DesafioMatch3.Views
{
    public class PlayerResourcesView : MonoBehaviour
    {
        private PlayerResources _playerResources;
        [SerializeField] private TextMeshProUGUI scoreDisplay;

        private void Awake()
        {
            _playerResources = new PlayerResources();
        }

        public void AddPoints(int value)
        {
            _playerResources.Score += value;
            scoreDisplay.text = _playerResources.Score.ToString("0000");
        }
        
    }
}

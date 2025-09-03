using System;
using System.Collections;
using System.Collections.Generic;
using Gazeus.DesafioMatch3.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Controllers
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _standard;
        [SerializeField] private Button _rotateSquare;
        [SerializeField] private Button _match4;
        [SerializeField] private Button _pattern;

        [SerializeField] private Button _quit;

        [SerializeField] private GameSceneManager _sceneManager;
        
        private void Awake()
        {
            _standard.onClick.AddListener(()=>CallScene(1));
            _rotateSquare.onClick.AddListener(() =>CallScene(2));
            _match4.onClick.AddListener(() =>CallScene(3));
            _pattern.onClick.AddListener(() =>CallScene(4));

            _quit.onClick.AddListener(Application.Quit); 
        }

        private void CallScene(int i)
        {
            _sceneManager.SetScene(i);
            DisableAllButtons();
        }
        
        private void DisableAllButtons()
        {
            _standard.interactable = false;
            _rotateSquare.interactable = false;
            _match4.interactable = false;
            _pattern.interactable = false;

            _quit.interactable = false;
        }
        
    }
}

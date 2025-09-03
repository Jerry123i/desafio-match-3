using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Core
{
    public class GameSceneManager : MonoBehaviour
    {
        [SerializeField] private SceneTransitionView _visualizer;
        public void SetScene(int index)
        {
            if (_visualizer != null)
                _visualizer.RunTransitionAnimation(() => UnityEngine.SceneManagement.SceneManager.LoadScene(index));
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(index);
        }
    }
}

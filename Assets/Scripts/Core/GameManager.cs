using Core;
using UnityEngine;
using UnityUtils.BaseClasses;

namespace Game.Core
{
    public class GameManager : SingletonBehavior<GameManager>
    {
        public bool isPlay;
        public bool levelIsComplete;
        public bool vibrationIsOn = true;

        private void Start()
        {
            levelIsComplete = false;
            EventDispatcher.OnGamePlay.AddListener(StartGame);
            EventDispatcher.OnGameEnd.AddListener(EndGame); 
            EventDispatcher.OnGameOver.AddListener(EndGame);
            EventDispatcher.OnLevelComplete.AddListener(EndGame);
        }

        private void StartGame()
        {
            isPlay = true;
        }
        
        private void EndGame()
        {
            isPlay = false; 
            levelIsComplete = false;
        }
    }
}
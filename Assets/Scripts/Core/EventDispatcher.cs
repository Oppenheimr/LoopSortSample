using UnityEngine.Events;

namespace Core
{
    public static class EventDispatcher
    {
        #region Events
        // Game Events
        public static readonly UnityEvent OnGamePlay = new();
        public static readonly UnityEvent OnGameFinish = new();
        public static readonly UnityEvent OnGameEnd = new();
        public static readonly UnityEvent OnGameOver = new();
        public static readonly UnityEvent OnGameRestart = new();
        // Level Events
        public static readonly UnityEvent OnLevelComplete = new();
        public static readonly UnityEvent OnClickNextLevel = new();
        public static readonly UnityEvent<int> OnLevelChanged = new();
        
        public static readonly UnityEvent OnPlayerReady = new();
        public static readonly UnityEvent<int> OnUnitPurchased = new();
        #endregion

        #region Event Methods
        // Game Events
        public static void OnGamePlayEvent() => OnGamePlay?.Invoke();
        public static void OnGameFinishEvent() => OnGameFinish?.Invoke();
        public static void OnGameEndEvent() => OnGameEnd?.Invoke();
        public static void OnGameOverEvent() => OnGameOver?.Invoke();
        public static void OnGameRestartEvent() => OnGameRestart?.Invoke();
        
        // Level Events
        public static void OnLevelChangedEvent(int level) => OnLevelChanged?.Invoke(level);
        public static void OnClickNextLevelEvent() => OnClickNextLevel?.Invoke();
        public static void OnLevelCompleteEvent() => OnLevelComplete?.Invoke();
        
        public static void OnPlayerReadyEvent() => OnPlayerReady?.Invoke();
        public static void OnUnitPurchasedEvent(int unitId) => OnUnitPurchased?.Invoke(unitId);
        #endregion
        
        public static void Reset()
        {
            // Reset all events
            OnGamePlay.RemoveAllListeners();
            OnGameFinish.RemoveAllListeners();
            OnGameEnd.RemoveAllListeners();
            OnGameOver.RemoveAllListeners();
            OnGameRestart.RemoveAllListeners();
            
            OnLevelComplete.RemoveAllListeners();
            OnClickNextLevel.RemoveAllListeners();
            OnLevelChanged.RemoveAllListeners();
            
            OnPlayerReady.RemoveAllListeners();
            OnUnitPurchased.RemoveAllListeners();
        }
    }
}
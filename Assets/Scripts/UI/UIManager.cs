using Core;
using UnityEngine;
using UnityUtils.BaseClasses.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private BasePanel _endPanel;

        private void OnEnable() => EventDispatcher.OnLevelComplete.AddListener(_endPanel.Show);
        private void OnDisable() => EventDispatcher.OnLevelComplete.RemoveListener(_endPanel.Show);

        public void OnClickNextLevel()
        {
            EventDispatcher.OnClickNextLevelEvent();
            _endPanel.Hide();
        }

        public void OnClickPlay() => EventDispatcher.OnGamePlayEvent();
    }
}
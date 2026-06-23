using Core;
using UnityEngine;
using UnityUtils.BaseClasses.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private BasePanel _endPanel;

        private void Awake()
        {
            EventDispatcher.OnLevelComplete.AddListener(_endPanel.Show);
        }

        public void OnClickNextLevel()
        {
            EventDispatcher.OnClickNextLevelEvent();
            _endPanel.Hide();
        }
    }
}
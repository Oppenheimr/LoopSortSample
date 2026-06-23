using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;
using UnityUtils.Extensions;

namespace UnityUtils.BaseClasses.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePanel : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] protected UnityEvent _onShow;
        [SerializeField] protected UnityEvent _onHide;
        
        [Header("Animation")]
        [SerializeField] protected float _animTime = .2f;
        [SerializeField] protected bool _startTest;
        [SerializeField] protected bool _scaleAnim;
        
        protected const float _fullTransparency = 0;
        protected const float _fullOpacity = 1;
        protected Vector3 startPos;
        protected Vector3 startScale;

        private CanvasGroup _canvasGroup;
        public CanvasGroup Group => _canvasGroup ? _canvasGroup : (_canvasGroup = GetComponentInChildren<CanvasGroup>());
        
        private AudioSource _audioSource;
        public AudioSource Audio
        {
            get
            {
                if (_audioSource)
                    return _audioSource;

                gameObject.TryAddComponent(out _audioSource);
                return _audioSource;
            }
        }

        protected virtual void Start()
        {
            startPos = transform.localPosition;
            startScale = transform.localScale;
            Group.alpha = _fullTransparency;
            Group.interactable = _startTest;
            if (_startTest)
                ShowPanel();
        }

        public void ShowOrHide()
        {
            if (Mathf.Approximately(Group.alpha, _fullTransparency))
                ShowPanel();
            else
                HidePanel();
        }
        
        public virtual void Show() => ShowPanel();
        public void Hide() => HidePanel();

        public virtual TweenerCore<float, float, FloatOptions> ShowPanel()
        {
            //Set the panel to interactable and block raycasts
            Group.interactable = true;
            Group.blocksRaycasts = true;
            
            //Play the audio source if it is not null
            Audio?.Play();
            
            //Invoke the onShow event
            _onShow?.Invoke();
            
            //If scale animation is enabled, scale the panel to its original size
            if (_scaleAnim)
                transform.DOScale(Vector3.one, _animTime);
            
            //Move the panel to its original position
            transform.DOLocalMove(Vector3.zero, _animTime);
            return Group.DOFade(_fullOpacity, _animTime);
        }

        public virtual TweenerCore<float, float, FloatOptions> HidePanel()
        {
            //Set the panel to not interactable and block raycasts
            Group.interactable = false;
            Group.blocksRaycasts = false;
            
            //Stop the audio source if it is not null
            Audio?.Stop();
            
            //Invoke the onHide event
            _onHide?.Invoke();
            
            //If scale animation is enabled, scale the panel to its original size
            if (_scaleAnim)
                transform.DOScale(startScale, _animTime);
            
            //Move the panel to its original position
            transform.DOLocalMove(startPos, _animTime);
            return Group.DOFade(_fullTransparency, _animTime);
        }
        
        public virtual TweenerCore<float, float, FloatOptions> HidePanel(float time)
        {
            //Set the panel to not interactable and block raycasts
            Group.interactable = false;
            Group.blocksRaycasts = false;
            
            //Stop the audio source if it is not null
            Audio?.Stop();
            
            //Invoke the onHide event
            _onHide?.Invoke();
            
            //Stop the audio source if it is not null
            if (_scaleAnim)
                transform.DOScale(startScale, time);
            
            //Move the panel to its original position
            transform.DOLocalMove(startPos, time);
            return Group.DOFade(_fullTransparency, time);
        }
    }

    /// <summary>
    /// Türetilecek tim sınıflara "Ins" tanımı ekler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BasePanel<T> : BasePanel where T : BasePanel, new()
    {
        private void OnEnable() => instance = instance = FindAnyObjectByType<T>();
        
        private static T instance;
        public static T Instance => instance ? instance : (instance = FindAnyObjectByType<T>());
        public static T SlowInstance => instance ? instance : (instance = FindAnyObjectByType<T>(FindObjectsInactive.Include));
    }
}
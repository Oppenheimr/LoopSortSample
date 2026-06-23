using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Prefs;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityUtils.Extensions;
using UnityUtils.Helpers;

namespace Core
{
    public static class Audio
    {
        private static AudioSource _musicSource;
        private static List<AudioSource> _sfxSources;
        private static AudioSource _sceneTranslationChannel;
        private static int _sfxSourcesPoolId;
        private static AudioSource[] _sceneAudios;
        private static List<Button> _sceneButtons;
        
        //SFX clip lists
        private static PlayList _buttons;
        private static bool _initialized;
        
        private static readonly FloatPref SFX_Volume = new FloatPref(0.5f);
        private static readonly FloatPref MusicVolume = new FloatPref(0.75f);
        
        public static float MusicVolumeLevel
        {
            get => MusicVolume.Value;
            set
            {
                MusicVolume.Value = value;
                _musicSource.volume = value;
            }
        }
        ///Sfx Volume for Audio Sources : _sfxSources
        public static float SfxVolume
        {
            get => SFX_Volume.Value;
            set
            {
                SFX_Volume.Value = value;
                _sceneTranslationChannel.volume = value;
                
                foreach (var source in _sfxSources)
                {
                    if (source == null)
                    {
                        _sfxSources.Remove(source);
                        continue;
                    }
                    
                    source.volume = value;
                }
            }
        }
        
        public static void Initialize()
        {
            if (_initialized)
                return;
            
            _musicSource = CoreManager.Instance.gameObject.AddComponent<AudioSource>();
            _musicSource.volume = MusicVolume.Value;
            _musicSource.loop = true;
            
            _sceneTranslationChannel = CoreManager.Instance.gameObject.AddComponent<AudioSource>();
            _sceneTranslationChannel.volume = SFX_Volume.Value;
            _sceneTranslationChannel.loop = false;

            _sceneTranslationChannel.clip = AudioData.Instance.sceneTranslation;
            _sceneTranslationChannel.Stop();
            
            _sfxSources = new List<AudioSource>();
            var sfxObject = new GameObject("Play Once");
            var sfxSource = sfxObject.AddComponent<AudioSource>();
            _sfxSourcesPoolId = ObjectPooler.CreatePool(sfxSource);
            Debug.Log(" Sfx Pool Id: " + _sfxSourcesPoolId);
            if (!AudioData.Instance)
                return;

            _buttons = new PlayList(AudioData.Instance.buttons);
            
            SetupNewSceneButtons();
            _initialized = true;
        }

        public static void Reset()
        {
            _sceneTranslationChannel = null;
            _musicSource = null;
            _sfxSources = null;
            _sfxSourcesPoolId = 0;
            _initialized = false;
        }
        
#region Music
        public static void PlayMusic(AudioClip clip)
        {
            if (_musicSource.clip == clip)
                return;
            
            _musicSource.DOFade(0, 2).OnComplete(() => {
                _musicSource.clip = clip;
                _musicSource.Play();
                
                _musicSource.DOFade(MusicVolumeLevel, 1);
            });
        }
#endregion
        
        public static void PlayButton() => PlaySfx(_buttons.GetClip());
        public static void PlaySceneTranslation() => _sceneTranslationChannel.SmoothPlay(SfxVolume, loop: false);
        public static void StopSceneTranslation() => _sceneTranslationChannel.SmoothStop(1).SetDelay(0.1f);
        public static float TranslationTime => _sceneTranslationChannel.clip.length;
        
        public static void PlaySfx(AudioClip clip)
        {
            if (SfxVolume <= 0)
                return;
            
            if(_sfxSources != null)
            {
                _sfxSources.ClearAllNulls();
            }
            else
            {
                _sfxSources = new List<AudioSource>();
            }
            
            var sfxComponent = ObjectPooler.GetPoolObject(_sfxSourcesPoolId);
            if (sfxComponent.SafeComponentCast(out AudioSource sfxSource))
                AudioSourceHelper.PlayOnceAndDeActive(sfxSource, clip, SfxVolume);
            _sfxSources.Add(sfxSource);
        }
        
        public static void SetupNewSceneButtons()
        {
            _sceneButtons = Object.FindObjectsByType<Button>
                (FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            
            foreach (var button in _sceneButtons.Where(button => !button.CompareTag("IgnoreButtonVoice")))
                button.onClick.AddListener(PlayButton);
        }

        public static void TryAddButton(Button button)
        {
            //button eğer _buttons içerisinde yoksa ekle
            if (_sceneButtons.Any(sceneButton => button == sceneButton))
                return;
            
            _sceneButtons.Add(button);
            button.onClick.AddListener(PlayButton);
        }
    }
}
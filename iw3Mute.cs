
using HoshinoLabs.Udon.IwaSync3;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components.Video;
using VRC.SDK3.Video.Components;
using VRC.SDK3.Video.Components.AVPro;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;

/*
Mutes with fade other AudioSources when IwaSync3 Player starts Play and resumes when stopped (https://booth.pm/ja/items/2666275)
*/
namespace VRC_Nayukie.WorldAssets.USharp
{
    public class iw3Mute : UdonSharpBehaviour
    {

        [SerializeField]
        private GameObject[] setVolumeObj;

        [SerializeField]
        private GameObject[] showWhenPlaying;

        private const float _audioLevel = 1f;
        private const float _timeToFade = 5f;
        private float[] _defaultLevels;
        private bool _fade = false;
        private float _fadeTarget = 0f;
        private float _fadeCurrentTime = 0f;

        private float _fadeUpDelay = 20f;
        private float _fadeUpWait = 0;

         void Start()
        {
            //Debug.Log($"[Audio Muter] Started `{nameof(iw3Mute)}`.");

            VideoCore iw3core = GameObject.Find("Udon (VideoCore)").GetComponent<VideoCore>();
            VideoController iw3controller = GameObject.Find("Udon (VideoController)").GetComponent<VideoController>();

            iw3core.AddListener(this);
            iw3controller.AddListener(this);

            _defaultLevels = new float[setVolumeObj.Length];

            UpdateDefaultLevels();
        }

         void Update()
        {

            if (!_fade)
            {
                if(_fadeUpWait>0)
                {
                    _fadeUpWait += Time.deltaTime;
                    if(_fadeUpWait > _fadeUpDelay)
                    {                       
                        _fade = true;
                    }
                }
                return;
            }

            // Skip Fade up
            _fadeUpWait = 0;

            float cur = (_fadeCurrentTime / _timeToFade);

            int i = 0;
            foreach (GameObject toggleObject in setVolumeObj)
            {
                AudioSource AudioEmitter = toggleObject.GetComponent<AudioSource>();

                if (AudioEmitter != null)
                {

                    if (_fadeTarget != _audioLevel) // fade to 0
                    {
                        //Debug.Log("Lowering audio level to " + cur + ", Time>" + _fadeCurrentTime);
                        if (AudioEmitter.volume > cur) AudioEmitter.volume = cur;
                    }
                    else
                    {
                        float abb = Mathf.Abs(cur - 1);
                        if (_defaultLevels[i] < abb)
                        {
                           // Debug.Log("Setting audio level to default (" + _defaultLevels[i] + ")");
                            AudioEmitter.volume = _defaultLevels[i];
                        }
                        else
                        {
                            //Debug.Log("Rising audio level to " + abb);
                            if (AudioEmitter.volume < abb)
                            {
                                AudioEmitter.volume = abb;
                            }
                        }
                    }
                }
                i++;
            }

            _fadeCurrentTime -= Time.deltaTime;

            if (_fadeCurrentTime < 0f) _fade = false;

        }

        private void UpdateDefaultLevels()
        {
            
            int i = 0;
            foreach (GameObject toggleObject in setVolumeObj)
            {
                if (toggleObject == null) continue;

                AudioSource AudioEmitter = toggleObject.GetComponent<AudioSource>();

                if (AudioEmitter != null)
                {
                    _defaultLevels[i] = AudioEmitter.volume;
                }
                i++;
            }

        }

        #region VideoEvent
        public override void OnVideoEnd()
        {
            OnPlayerStop();

            foreach (GameObject toggleObject in showWhenPlaying)
            {
                if (toggleObject == null) continue;
                toggleObject.SetActive(false);
            }
        }

        public override void OnVideoError(VideoError videoError)
        {
            
        }

        public override void OnVideoLoop()
        {

        }

        public override void OnVideoReady()
        {

        }

        public override void OnVideoStart()
        {
            OnPlayerPlay();

            foreach (GameObject toggleObject in showWhenPlaying)
            {
                if (toggleObject == null) continue;
                toggleObject.SetActive(true);
            }
        }
        #endregion

        #region VideoCoreEvent
        public void OnPlayerPlay()
        {
            //Debug.Log("Audio muter Play, fading to 0");
            if (!_fade)
            {
                UpdateDefaultLevels();
            }
            _fadeCurrentTime = _timeToFade;
            _fade = true;
            _fadeTarget = 0f;
        }

        public void OnPlayerPause()
        {
            _fadeCurrentTime = _timeToFade;
            _fade = true;
            _fadeTarget = 1f;
        }

        public void OnPlayerStop()
        {
          //  Debug.Log("Audio muter Stop, fading to 1f");

            _fadeUpWait = 0.01f;

            _fadeCurrentTime = _timeToFade;
            _fade = false;
            _fadeTarget = 1f;
        }

        public void OnChangeURL()
        {

        }
        #endregion
    }

}

using System.Collections;
using System.Collections.Generic;
using BS.Manager;
using BS.Player;
using BS.UI;
using UnityEngine;
using UnityEngine.Events;

namespace BS.Intro.Manager
{
    public class IntroController : BaseManager<IntroController>
    {
        public PlayerController _player;
        public StartMenuManager _startMenu;
        public IntroStartArea _startArea;

        private void Awake()
        {
            if (_startMenu != null)
            {
                _startMenu.Init();
                _startMenu.OnPlay.AddListener(OnPlay);
            }
            else
            {
                Debug.LogWarning("Intro에 Start Menu가 연결되어있지 않습니다.");
            }

            if (_startArea != null)
            {
                _startArea.Init();
                _startArea.OnEnter.AddListener(OnEnter);
            }
        }

        private void Start()
        {
            _player.PhysicManager.Freeze();
            _player.AnimController.Off();
        }

        void OnEnter()
        {
            _player.IsControllable = true;
        }

        IEnumerator StartIntroLoop()
        {
            yield return StartCoroutine(_startMenu.FadeOutStartMenuLoop(1f));
            _player.AnimController.On();
            _player.PhysicManager.UnFreeze();
            _player.PhysicManager.AddForce(Vector2.one * 300);
            yield return null;
        }

        void OnPlay()
        {
            StartCoroutine(StartIntroLoop());
        }
    }
}

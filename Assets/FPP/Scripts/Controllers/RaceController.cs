using UnityEngine;
using FPP.Scripts.Core;
using FPP.Scripts.Enums;
using FPP.Scripts.Systems;
using FPP.Scripts.Patterns;

namespace FPP.Scripts.Controllers
{
    public class RaceController : MonoBehaviour
    {
        public bool isPlayerProgressionDisabled;
        
        private Player _player; // 플레이어 정보
        private SaveSystem _saveSystem; // 플레이어 정보 저장, 로드
        private TrackController _trackController; // 트렉 세그먼트별로 트랙을 구성, 트랙을 이동시켜 플레이어가 주행중임을 표현
        private GameObject _trackControllerGameObject;
        
        private void OnEnable()
        {
            // 이벤트 버스 패턴
            RaceEventBus.Subscribe(RaceEventType.END, EndRace);
            RaceEventBus.Subscribe(RaceEventType.REPLAY, ReplayRace);
            RaceEventBus.Subscribe(RaceEventType.RESTART, RestartRace);
        }

        private void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventType.END, EndRace);
            RaceEventBus.Unsubscribe(RaceEventType.REPLAY, ReplayRace);
            RaceEventBus.Unsubscribe(RaceEventType.RESTART, RestartRace);
        }
        
        private void Awake()
        {
            _saveSystem = new SaveSystem();
            _player = _saveSystem.LoadPlayer();

            if (_player == null)
                _player = new Player();
            
            _trackControllerGameObject = Instantiate(Resources.Load("TrackController", typeof(GameObject))) as GameObject;

            if (_trackControllerGameObject != null)
                _trackController = _trackControllerGameObject.GetComponent<TrackController>();
        }

        private void Start()
        {
            // 플레이어 정보 중 이전 트랙 인덱스 유효 검사 후 현재 인덱스 트랙 세그먼트 생성
            if (_trackController)
                if (IsTrackInList(_player.currentTrack))
                    _trackController.SpawnTrack(_player.currentTrack, false);
        }

        private void RestartRace()
        {
            // 현재 인덱스 트랙 세그먼트 생성
            if (_trackController)
                _trackController.SpawnTrack(_player.currentTrack, false);
        }

        private void ReplayRace()
        {
            // 리플레이 정보와 함께 현재 인덱스 트랙 세그먼트 생성
            if (_trackController)
                _trackController.SpawnTrack(_player.currentTrack, true);
        }

        private void EndRace()
        {
            // 현재 트랙 인덱스를 증가시키는 것은 트랙의 순서가 변경되지 않고 일관성을 유지할 것이라고 가정합니다.
            int nextTrackIndex = _player.currentTrack + 1; // Incrementing the current track index assumes that the order of the tracks will not change and stay consistent
                
            if (IsTrackInList(nextTrackIndex)) // 인덱스 유효 검사
            {
                _player.currentTrack = nextTrackIndex;
                _saveSystem.SavePlayer(_player); // 현재 인덱스 저장

                // 현재 인덱스 트랙 세그먼트 생성
                if (_trackController)
                    _trackController.SpawnTrack(_player.currentTrack, false);
            }
            else
            {
                Debug.LogError("No more tracks available!");
                // TODO: When no more tracks available in the track list, load end of circuit menu.  
            }
        }

        // 인덱스 유효 검사
        private bool IsTrackInList(int trackIndex)
        {
            if (trackIndex >= 0 && trackIndex < _trackController.tracks.Count)
                return true;

            return false;
        }
    }
}
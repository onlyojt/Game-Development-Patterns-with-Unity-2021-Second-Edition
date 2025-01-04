using UnityEngine;
using System.Linq;
using FPP.Scripts.Enums;
using FPP.Scripts.Patterns;
using System.Collections.Generic;
using FPP.Scripts.Ingredients.Bike;
using FPP.Scripts.Ingredients.Track;

namespace FPP.Scripts.Controllers
{
    public class TrackController : Observer // 트랙 세그먼트별로 트랙을 구성
    {
        private float _trackSpeed;
        private bool _isTrackLoaded;
        private bool _isReplayEnabled;
        private int _currentActiveRail;
        private int _currentTrackIndex;
        private GameObject _trackParent;
        private Transform _segmentParent;
        private List<GameObject> _segments;
        private Transform _previousSegment;
        private Stack<GameObject> _segmentStack;
        private Vector3 _currentPosition = new (0, 0, 0);
        
        [Tooltip("List of race tracks")]
        public List<RaceTrack> tracks = new();
        
        [Tooltip("Number of rails per track")] 
        [SerializeField]
        private int railAmount; // TODO: This should be customizable thru a property in RaceTrack scriptable objects.
        
        [Tooltip("Starting line rail number")] // 게임 시작때 레일 설정
        [SerializeField]
        private int startingRail;
        
        [Tooltip("Dampen the speed of the track")] 
        [Range(0.0f, 100.0f)] 
        [SerializeField]
        private float speedDampener;
        
        [Tooltip("Initial amount of segment to load at start")] 
        [SerializeField]
        private int initialSegmentAmount; // 한번에 로드할 세그먼트 수

        [Tooltip("Amount of incremental segments to load at run")] 
        [SerializeField]
        private int incrementalSegmentAmount;
        
        private void Update()
        {
            // 트랙을 뒤로 이동시켜 플레이어가 주행중임을 표현
            if (_segmentParent)
                _segmentParent.transform.Translate(Vector3.back * (_trackSpeed * Time.deltaTime));
        }

        public bool IsNextRailAvailable(BikeDirection direction) // TODO: Remove conditions and use bike direction enums values to evaluate rail availability. 
        {
            if (direction == BikeDirection.Left)
            {
                if (_currentActiveRail != 1)
                {
                    _currentActiveRail -= 1;
                    return true;
                }
            }
            
            if (direction == BikeDirection.Right)
            {
                if (_currentActiveRail != railAmount) 
                {
                    _currentActiveRail += 1;
                    return true;
                }
            }

            return false;
        }

        public void SpawnTrack(int trackIndex, bool isReplayEnabled)
        {
            _currentActiveRail = startingRail; // 4개의 레일중 현재 레일 설정
            
            _isReplayEnabled = isReplayEnabled;
            
            if (_currentTrackIndex <= trackIndex)
            {
                _currentTrackIndex = trackIndex;
                ReserveTrackSegments(_currentTrackIndex); // 트랙의 세그먼트들
            }

            SpawnTrack();
        }
        
        private void ReserveTrackSegments(int trackIndex)
        {
            // 트랙의 세그먼트들
            _segments = Enumerable.Reverse(tracks[trackIndex].segments).ToList();
        }

        // BaseTrack의 Segments를 부모로 
        private void SpawnTrack()
        {
            if (!_isReplayEnabled)
            {
                Destroy(_trackParent);
                
                _trackParent = Instantiate(Resources.Load("BaseTrack", typeof(GameObject))) as GameObject;

                if (_trackParent != null)
                    _segmentParent = _trackParent.transform.Find("Segments"); // 세그먼트의 부모 오브젝트
            }
            else // 리플레이 모드
            {
                _trackParent.GetComponent<BaseTrack>().ResetBikeToSpawnPoint();
            }

            _previousSegment = null;

            // 트랙의 세그먼트들을 Stack으로 변환
            _segmentStack = new Stack<GameObject>(_segments);
            
            SpawnSegment(initialSegmentAmount);
            
            if (!_isReplayEnabled)
                RaceEventBus.Publish(RaceEventType.COUNTDOWN); // 이벤트 버스
        }

        // 정해진 갯수만큼 세그먼트 생성
        private void SpawnSegment(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                if (_segmentStack.Count > 0)
                {
                    GameObject segment = Instantiate(_segmentStack.Pop(), _segmentParent.transform);

                    if (!_previousSegment) 
                        _currentPosition.z = 0;
                    
                    if (_previousSegment)
                        _currentPosition.z = _previousSegment.position.z + tracks[_currentTrackIndex].segmentLength;

                    segment.transform.position = _currentPosition;
                    
                    segment.AddComponent<TrackSegment>(); 
                    
                    segment.GetComponent<TrackSegment>().trackController = this;
                    
                    _previousSegment = segment.transform;
                }
            }
        }

        // 다음 세그먼트 생성
        public void LoadNextSegment()
        {
            SpawnSegment(incrementalSegmentAmount);
        }

        // BikeController로 부터 이벤트 발생 (옵저버 패턴)
        public override void Notify(Subject subject)
        {
            BikeController bikeController = subject.GetComponent<BikeController>();
            
            if (bikeController)
                _trackSpeed = bikeController.CurrentSpeed - (bikeController.CurrentSpeed * speedDampener / 100);
        }
    }
}
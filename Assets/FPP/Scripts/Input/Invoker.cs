using UnityEngine;
using System.Linq;
using FPP.Scripts.Enums;
using FPP.Scripts.Patterns;
using System.Collections.Generic;

namespace FPP.Scripts.Input
{
    class Invoker : MonoBehaviour
    {
        private bool _isRecording;
        private bool _isReplaying;
        private float _replayTime;
        private float _recordingTime;
        private SortedList<float, Command> _recordBackup;
        private SortedList<float, Command> _recordedCommands = new ();
        
        void OnEnable()
        {
            RaceEventBus.Subscribe(RaceEventType.START, Record);
            RaceEventBus.Subscribe(RaceEventType.REPLAY, Replay);
        }

        void OnDisable()
        {
            RaceEventBus.Unsubscribe(RaceEventType.START, Record);
            RaceEventBus.Unsubscribe(RaceEventType.REPLAY, Replay);
        }

        public void ExecuteCommand(Command command)
        {
            command.Execute();
           
            if (_isRecording) 
                _recordedCommands.Add(_recordingTime, command); // 플레이 시간과 함께 입력값 저장
        }

        public void Record()
        {
            _recordingTime = 0.0f;
            _isRecording = !_isRecording;
        }

        public void Replay() 
        {
            _replayTime = 0.0f;
            _isReplaying = true;
            
            // For the FPP, we will not integrate serialization, so this is just a placeholder implementation
            if (_recordBackup != null)
            {
                // 이전 리플레이때 _recordedCommands.RemoveAt(0) 제거 되므로 백업(_recordBackup)으로 다시 리플레이
                _recordedCommands = new(_recordBackup);
            }
            else
            {
                _recordedCommands.Reverse();
                _recordBackup = new(_recordedCommands); // 백업(_recordBackup)
            }
        }
        
        void FixedUpdate()
        {
            if (_isRecording)
                _recordingTime += Time.deltaTime; // 플레이중일때 리플레이 시간 저장

            if (_isReplaying)
            {
                _replayTime += Time.deltaTime; // 리플레이때 현재 시간 저장
                
                // 아무 요소라도 포함되어있는지 확인
                if (_recordedCommands.Any())
                {
                    // 두 float값이 거의 같은지 비교 (==으로는 float값 비교불가 -> 타이머, 위치, 회전값등에 사용)
                    if (Mathf.Approximately(_replayTime, _recordedCommands.Keys[0]))
                    {
                        _recordedCommands.Values[0].Execute();
                        _recordedCommands.RemoveAt(0);
                    }
                }
            }
        }
    }
}
using UnityEngine;
using FPP.Scripts.Enums;
using System.Collections;
using FPP.Scripts.Patterns;
using FPP.Scripts.Ingredients.Bike;

namespace FPP.Scripts.Cameras
{
    public class FollowCamera : Observer
    {
        private bool _isRotation;
        private int _currentDirection;
        
        [SerializeField] private CameraLaserHit cameraLaserHit;
        [SerializeField] private CameraTurboBlur cameraTurboBlur;
        
        private void ToggleTurboMode(bool isOn)
        {
            cameraTurboBlur.enabled = isOn;
        }

        public void Distort()
        {
            StartCoroutine(DistortCamera());
        }
        
        IEnumerator DistortCamera()
        {
            cameraLaserHit.enabled = true;
            yield return new WaitForSeconds(0.1f);
            cameraLaserHit.enabled = false;
        }

        public override void Notify(Subject subject)
        {
            BikeController controller = subject.GetComponent<BikeController>();
            if (controller)
                ToggleTurboMode(controller.BikeEngine.IsTurboOn);
        }
        
        public IEnumerator Turn(float duration, BikeDirection direction)
        {
            float time = 0;
            
            // Rotation
            Quaternion startRotation = transform.rotation; // 현재 카메라의 회전 값
            Vector3 targetRotation = new Vector3(0, 20.0f * (int)direction, 0); // 바이크 방향에 따라 20도 회전
            Quaternion endRotation = Quaternion.Euler(targetRotation); // 목표 방향으로 회전해야 할 최종 회전 값

            // 방향이 변경 되었는 지 확인
            if (_currentDirection != (int) direction)
                _isRotation = true;
            else
                _isRotation = false;
            
            _currentDirection = (int)direction;
            
            // Position
            Vector3 startPosition = transform.position;

            Vector3 endPosition;

            // 방향 전환 시에는 x축으로 1.5f 이동
            if (_isRotation)
                endPosition = new Vector3(startPosition.x + 1.5f * -_currentDirection, startPosition.y, startPosition.z);
            else
                endPosition = new Vector3(startPosition.x + 1.5f * _currentDirection, startPosition.y, startPosition.z);
            
            while (time < duration)
            {
                // 지정된 시간(float duration)동안 두 위치 사이를 이동
                transform.position = Vector3.Lerp(startPosition, endPosition, time / duration);
                
                if (_isRotation)
                {
                    // 이전 방향과 다른경우 지정된 시간(float duration)동안 회전
                    transform.rotation = Quaternion.Lerp(startRotation, endRotation, time / duration);
                }

                time += Time.deltaTime;
                yield return null; // 한 프레임 리턴
            }
            
            transform.position = endPosition;
        }
    }
}
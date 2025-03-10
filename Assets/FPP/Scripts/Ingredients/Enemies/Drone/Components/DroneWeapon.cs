using UnityEngine;
using FPP.Scripts.Enums;
using System.Collections.Generic;
using FPP.Scripts.Ingredients.Bike;

namespace FPP.Scripts.Ingredients.Enemies.Drone.Components
{
    public class DroneWeapon : MonoBehaviour
    {
        public bool isBeamOn;
        public DroneController DroneController { get; set; }

        private float _strength;
        private GameObject _target;
        private LineRenderer _line;
        private RaycastHit _beamHit;
        private Vector3 _beamDirection;

        public void ActivateWeapon(float strength)
        {
            _strength = strength;
            
            _line = gameObject.AddComponent<LineRenderer>();    // 빔(선)을 화면에 그리는 데 사용
            _line.startWidth = 0.1f;
            _line.endWidth = 0.1f; // 빔의 시작과 끝의 너비를 각각 0.1
            _line.useWorldSpace = true; // 선이 월드 좌표계에서 렌더링
            _line.material = new Material(Shader.Find("Sprites/Default"));
            _line.startColor = Color.red;

            _beamDirection = transform.TransformDirection(Vector3.back) * DroneController.beamDistance; // 빔 방향 계산
            _beamDirection = Quaternion.Euler(DroneController.beamAngle, 0.0f, 0f) * _beamDirection; // 빔 방향에 beamAngle만큼 회전을 적용

            // 타겟 오브젝트 생성
            _target = new GameObject("Target", typeof(BoxCollider));
            _target.transform.SetParent(transform);

            // 타겟 위치 설정
            Vector3 pos = Quaternion.AngleAxis(DroneController.beamAngle, Vector3.right) * Vector3.back *
                          DroneController.beamDistance;
            _target.transform.localPosition = pos;

            isBeamOn = true;
        }

        void Update()
        {
            if (isBeamOn)
            {
                List<Vector3> pos = new List<Vector3>();
                pos.Add(transform.position);
                pos.Add(_target.transform.position);
                _line.SetPositions(pos.ToArray());

                Debug.DrawRay(transform.position, _beamDirection, Color.blue);
                
                if (Physics.Raycast(transform.position, _beamDirection, out _beamHit, DroneController.beamDistance))
                {
                    if (_beamHit.collider.GetComponent<BikeController>())
                    {
                        Debug.DrawRay(transform.position, _beamDirection, Color.green);
                        _beamHit.collider.GetComponent<BikeController>().TakeDamage((int) DamageType.Laser, DamageType.Laser);
                    }
                }
            }
        }
    }
}
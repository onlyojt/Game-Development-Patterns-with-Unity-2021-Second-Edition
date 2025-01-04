using UnityEngine;
using FPP.Scripts.Weaponry;

namespace FPP.Scripts.Ingredients.Bike.Elements
{
    public class BikeWeapon : MonoBehaviour
    {
        public bool isDebugOn;
        public WeaponConfiguration weaponConfig; // 기본무기 설정값 (스크립터블 오브젝트)
        public WeaponAttachment mainAttachment; // 첫번쩨 무기 증가값 (스크립터블 오브젝트)
        public WeaponAttachment secondaryAttachment; // 두번째 무기 증가값 (스크립터블 오브젝트)

        private IWeapon _weapon; // 데커레이터 패턴 인터페이스
        private float _beamTimer;
        private LineRenderer _beam;
        private Vector3 _startPosition;
        private Vector3 _currentPosition;

        void Start()
        {
            InitWeapon();
        }

        void Update()
        {
            _startPosition = transform.position;
            
            if (isDebugOn)
                Debug.DrawRay(_startPosition, transform.TransformDirection(Vector3.forward) * _weapon.range, Color.green);

            if (_beam.enabled)
            {
                _beam.SetPosition(0, _startPosition);
                _beam.SetPosition(1, _startPosition + transform.forward * _weapon.range);

                if (_beamTimer > 0)
                    _beamTimer -= Time.deltaTime;
                else
                    _beam.enabled = false;
            }
        }

        public void Fire()
        {
            _beamTimer = _weapon.fireTime;
            _beam.enabled = true;
        }

        private void InitWeapon()
        {
            _weapon = new Weapon(weaponConfig); // 기본무기 세팅
            DecorateWeapon();
            InitBeam();
        }

        private void InitBeam()
        {
            _beam = gameObject.AddComponent<LineRenderer>();
            _beam.startWidth = 0.05f;
            _beam.endWidth = 0.05f;
            _beam.useWorldSpace = true;
            _beam.material = new Material(Shader.Find("Sprites/Default"));
            _beam.startColor = _weapon.BeamColor;
            _beam.enabled = false;
        }

        private void DecorateWeapon()
        {
            // 기존 무기에 증가값을 가지고 있는 스크립터블 오브젝트
            if (mainAttachment && !secondaryAttachment)
                _weapon = new WeaponDecorator(_weapon, mainAttachment);

            // 기존무기를 첫번째 증가값을 적용, 두번째는 스크립터블 오브젝트 설정값
            if (mainAttachment && secondaryAttachment)
                _weapon = new WeaponDecorator(new WeaponDecorator(_weapon, mainAttachment), secondaryAttachment); 
        }
    }
}
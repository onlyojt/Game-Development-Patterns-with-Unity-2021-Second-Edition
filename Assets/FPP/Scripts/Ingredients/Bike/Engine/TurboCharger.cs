using UnityEngine;
using System.Collections;

namespace FPP.Scripts.Ingredients.Bike.Engine
{
    public class TurboCharger : MonoBehaviour
    {
        public BikeEngine BikeEngine { get; set; }
        public bool IsTurboOn { get; private set; }
        
        private CoolingSystem _coolingSystem;

        public void ToggleTurbo(CoolingSystem coolingSystem)
        {
            _coolingSystem = coolingSystem;
           
            if (BikeEngine.IsEngineOn) 
                StartCoroutine(TurboCharge());
        }

        IEnumerator TurboCharge()
        {
            IsTurboOn = true;
            
            // 쿨링 시스템 멈춤
            _coolingSystem.PauseCooling();
            
            // 속도 증가
            BikeEngine.CurrentSpeed = 
                BikeEngine.CurrentSpeed + (BikeEngine.CurrentSpeed * BikeEngine.TurboBoostAmount / 100);

            // 연료소모량 증가 (오정택 추가)
            float defaultRate = BikeEngine.burnRate;
            BikeEngine.burnRate *= 2.0f;

            // 터보 유지기간
            yield return new WaitForSeconds(BikeEngine.TurboDuration);

            IsTurboOn = false;
            
            // 쿨링 시스템 시작
            _coolingSystem.PauseCooling();

            // 연료 소모율 원복 (오정택 추가)
            BikeEngine.burnRate = defaultRate;

            // 속도 원복
            if (BikeEngine.IsEngineOn) 
                BikeEngine.CurrentSpeed = BikeEngine.DefaultSpeed;
        }
    }
}
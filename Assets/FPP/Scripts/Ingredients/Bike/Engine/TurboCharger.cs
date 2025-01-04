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
            
            // �� �ý��� ����
            _coolingSystem.PauseCooling();
            
            // �ӵ� ����
            BikeEngine.CurrentSpeed = 
                BikeEngine.CurrentSpeed + (BikeEngine.CurrentSpeed * BikeEngine.TurboBoostAmount / 100);

            // ����Ҹ� ���� (������ �߰�)
            float defaultRate = BikeEngine.burnRate;
            BikeEngine.burnRate *= 2.0f;

            // �ͺ� �����Ⱓ
            yield return new WaitForSeconds(BikeEngine.TurboDuration);

            IsTurboOn = false;
            
            // �� �ý��� ����
            _coolingSystem.PauseCooling();

            // ���� �Ҹ��� ���� (������ �߰�)
            BikeEngine.burnRate = defaultRate;

            // �ӵ� ����
            if (BikeEngine.IsEngineOn) 
                BikeEngine.CurrentSpeed = BikeEngine.DefaultSpeed;
        }
    }
}
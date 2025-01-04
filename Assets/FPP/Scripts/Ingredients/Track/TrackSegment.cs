using UnityEngine;
using FPP.Scripts.Controllers;

namespace FPP.Scripts.Ingredients.Track
{
    public class TrackSegment : MonoBehaviour
    {
        public TrackController trackController;

        private void OnDestroy()
        {
            // 다음 세그먼트 로드
            if (trackController)
                trackController.LoadNextSegment();
        }
    }
}
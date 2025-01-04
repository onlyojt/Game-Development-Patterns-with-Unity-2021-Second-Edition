using UnityEngine;
using FPP.Scripts.Controllers;

namespace FPP.Scripts.Ingredients.Track
{
    public class TrackSegment : MonoBehaviour
    {
        public TrackController trackController;

        private void OnDestroy()
        {
            // ���� ���׸�Ʈ �ε�
            if (trackController)
                trackController.LoadNextSegment();
        }
    }
}
using TMPro;
using UnityEngine;
using FPP.Scripts.Core;
using FPP.Scripts.Systems;
using UnityEngine.SceneManagement;

namespace FPP.Scripts.UI.Menu
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Player Stats")]
        public TMP_Text playerName;
        public TMP_Text sessionDuration;
        
        [Header("Build Version")]
        public string versionPrefix;
        public TextAsset versionFile;
        public TMP_Text versionNumber;

        private Player _player;
        private SaveSystem _saveSystem;

        void Awake()
        {
            _saveSystem = new SaveSystem();
            _player = _saveSystem.LoadPlayer(); // 저장된 플레이어 정보 로드
        }

        void Start()
        {
            // 각종 정보 표시
            DisplayPlayerInfo();
            DisplayVersionNumber();
        }

        public void Play()
        {
            SceneManager.LoadScene("RaceTrack"); // Play 버튼-> RaceTrack씬 로드
        }

        public void Exit()
        {
            Application.Quit();
        }

        public void Reset()
        {
            _saveSystem.DeleteSave(); // Reset 버튼 -> 플레이어 정보 삭제, Registration씬 로드
            SceneManager.LoadScene("Registration");
        }

        private void DisplayVersionNumber()
        {
            versionNumber.text = versionPrefix + versionFile.text;
        }

        private void DisplayPlayerInfo()
        {
            if (_player != null)
            {
                playerName.text = _player.playerName;
                sessionDuration.text = _player.lastSessionDuration.ToString(); // TODO: format the string to present HH:MM:SS
            }
        }
    }
}
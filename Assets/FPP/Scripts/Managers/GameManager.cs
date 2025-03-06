using System;
using System.IO;
using UnityEngine;
using FPP.Scripts.Core;
using FPP.Scripts.Systems;
using FPP.Scripts.Patterns;
using FPP.Scripts.Services;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace FPP.Scripts.Managers
{
    public class GameManager : Singleton<GameManager> // 싱글턴 패턴
    {
        private Player _player;
        private SaveSystem _saveSystem;

        private DateTime _sessionStartTime;
        private DateTime _sessionEndTime;

        void Start()
        {
            RegisterGlobalServices(); // 서비스 로케이터에 등록
            LoadPlayer(); // 저장되어 있는 플레이어 정보 로드
            StartSessionTimer(); // _sessionStartTime 시작시간 세팅
        }

        private void StartSessionTimer()
        {
            // _sessionStartTime 시작시간 세팅
            _sessionStartTime = DateTime.Now;
        }

        private void LoadPlayer()
        {
            _saveSystem = new SaveSystem();
            _player = _saveSystem.LoadPlayer();

            if (_player == null)
                SceneManager.LoadScene("Registration"); // 저장정보가 없으면 "Registration" 씬 로드
            else
                SceneManager.LoadScene("MainMenu"); // 저장정보가 있으면 "MainMenu" 씬 로드
        }

        private void RegisterGlobalServices()
        {
            IDailyChallengeService dailyChallengeService;

            // 디버깅 모드에 따라 오늘의 도전이라는 가상 서비스 등록
            if (Debug.isDebugBuild)
                dailyChallengeService = new Services.Mocks.DailyChallengeService();
            else
                dailyChallengeService = new DailyChallengeService();

            // 서비스 로케이터 패턴
            ServiceLocator.RegisterService(dailyChallengeService);
        }

        void OnApplicationQuit()
        {
            _sessionEndTime = DateTime.Now;

            TimeSpan timeDifference =
                _sessionEndTime.Subtract(_sessionStartTime);

            if (_player != null)
            {
                _player.lastSessionDuration = timeDifference;
                _saveSystem.SavePlayer(_player);
            }
        }

        // 다음은 유니티 아이콘을 추출하기 위한 추가 코드(본 프로젝트와는 관계없음)
        [MenuItem("Tools/Extract Built-in Icons")]
        public static void ExtractIcons()
        {
            string outputPath = "Assets/ExtractedIcons/";
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // 자주 사용하는 아이콘 이름 리스트
            string[] commonIconNames = new string[]
            {
"d_MeshFilter Icon"
            };


            foreach (string iconName in commonIconNames)
            {
                // 아이콘 텍스처 가져오기
                Texture2D sourceTexture = EditorGUIUtility.IconContent(iconName).image as Texture2D;
                if (sourceTexture != null)
                {
                    // 텍스처 복제
                    Texture2D readableTexture = CreateReadableTexture(sourceTexture);
                    if (readableTexture != null)
                    {
                        byte[] bytes = readableTexture.EncodeToPNG();
                        string sanitizedFileName = SanitizeFileName(iconName);
                        File.WriteAllBytes($"{outputPath}/{sanitizedFileName}.png", bytes);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to create readable texture for icon: {iconName}");
                    }
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"Icons extracted to: {outputPath}");
        }

        private static Texture2D CreateReadableTexture(Texture2D sourceTexture)
        {
            RenderTexture tempRT = RenderTexture.GetTemporary(
                sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(sourceTexture, tempRT);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tempRT;

            Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
            readableTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tempRT);

            return readableTexture;
        }

        // 파일 이름에서 허용되지 않는 문자를 제거
        private static string SanitizeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }
            return fileName;
        }
    }
}
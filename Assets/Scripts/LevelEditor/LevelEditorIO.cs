using System.IO;
using ArrowPuzzle.Common;
using ArrowPuzzle.Data;
using TMPro;
using UnityEngine;

namespace ArrowPuzzle.LevelEditor
{
    public class LevelEditorIO : MonoBehaviour
    {
        [Header("Save / Load")]
        [SerializeField] private string resourcesFolderRelativeToAssets = "Resources/Levels";
        [SerializeField] private string fileNameWithoutExtension = "editor_level_01";
        [SerializeField] private TMP_Text ioStatusText;
        [SerializeField] private TextAsset loadFromTextAsset;

        public string CurrentFileName => fileNameWithoutExtension;

        public void SetFileName(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileNameWithoutExtension = fileName.Trim();
            }
        }

        public string GetDefaultSavePath()
        {
            string assetsRoot = Application.dataPath;
            string folder = Path.Combine(assetsRoot, resourcesFolderRelativeToAssets);
            return Path.Combine(folder, $"{fileNameWithoutExtension}.json");
        }

        public bool SaveLevel(LevelData levelData)
        {
            if (levelData == null)
            {
                SetStatus("Save failed: level is null.");
                return false;
            }

            string json = JsonLevelSerializer.Serialize(levelData, true);
            string path = GetDefaultSavePath();
            string folder = Path.GetDirectoryName(path);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(path, json);
            SetStatus($"Saved JSON: {path}");
            return true;
        }

        public LevelData LoadLevel()
        {
            if (loadFromTextAsset != null)
            {
                LevelData fromTextAsset = JsonLevelSerializer.Deserialize(loadFromTextAsset);
                SetStatus($"Loaded from TextAsset: {loadFromTextAsset.name}");
                return fromTextAsset;
            }

            string path = GetDefaultSavePath();
            if (!File.Exists(path))
            {
                SetStatus($"Load failed: file not found at {path}");
                return null;
            }

            string json = File.ReadAllText(path);
            LevelData levelData = JsonLevelSerializer.Deserialize(json);
            SetStatus($"Loaded JSON: {path}");
            return levelData;
        }

        public void CopyLevelJsonToClipboard(LevelData levelData)
        {
            if (levelData == null)
            {
                SetStatus("Copy failed: level is null.");
                return;
            }

            GUIUtility.systemCopyBuffer = JsonLevelSerializer.Serialize(levelData, true);
            SetStatus("Copied level JSON to clipboard.");
        }

        private void SetStatus(string message)
        {
            Debug.Log(message);
            if (ioStatusText != null)
            {
                ioStatusText.text = message;
            }
        }
    }
}

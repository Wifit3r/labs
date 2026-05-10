using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Глобальне сховище (Singleton). Зберігає життя, зіткнення, час, монети, таблицю рекордів.
/// Забезпечує збереження/завантаження у JSON.
/// </summary>
public class GameData : MonoBehaviour
{
    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameData");
                _instance = go.AddComponent<GameData>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // Подія програшу
    public event Action OnGameOver;

    [Header("Параметри")]
    public int maxLives = 3;
    public float levelTimeLimit = 120f;

    // Поточний стан
    public int Lives { get; private set; }
    public int Collisions { get; private set; }
    public float ElapsedTime { get; private set; }
    public int CoinsCollected { get; private set; }
    public bool IsGameOver { get; private set; }

    // Таблиця рекордів
    public List<RecordEntry> Records { get; private set; } = new List<RecordEntry>();

    private bool isRunning;
    private string savePath;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "gamedata.json");
        LoadData();
        ResetForNewRun();
    }

    void Update()
    {
        if (isRunning && !IsGameOver)
        {
            ElapsedTime += Time.deltaTime;

            // Обмеження часу рівня
            if (ElapsedTime >= levelTimeLimit)
            {
                TriggerGameOver();
            }
        }
    }

    public void StartRun()
    {
        isRunning = true;
    }

    public void ResetForNewRun()
    {
        Lives = maxLives;
        Collisions = 0;
        ElapsedTime = 0f;
        CoinsCollected = 0;
        IsGameOver = false;
        isRunning = false;
    }

    public void CollectCoin()
    {
        if (IsGameOver) return;
        CoinsCollected++;
    }

    public void LoseLife()
    {
        if (IsGameOver) return;

        Lives--;
        Collisions++;

        if (Lives <= 0)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        isRunning = false;

        // Додати запис до таблиці рекордів
        Records.Add(new RecordEntry
        {
            coins = CoinsCollected,
            time = ElapsedTime,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        // Сортувати за монетами (спадання)
        Records.Sort((a, b) => b.coins.CompareTo(a.coins));

        // Залишити топ-10
        if (Records.Count > 10)
            Records.RemoveRange(10, Records.Count - 10);

        SaveData();

        // Генерувати подію програшу
        OnGameOver?.Invoke();
    }

    public void FinishLevel()
    {
        isRunning = false;

        Records.Add(new RecordEntry
        {
            coins = CoinsCollected,
            time = ElapsedTime,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        Records.Sort((a, b) => b.coins.CompareTo(a.coins));
        if (Records.Count > 10)
            Records.RemoveRange(10, Records.Count - 10);

        SaveData();
    }

    // --- Збереження / Завантаження JSON ---

    public void SaveData()
    {
        SaveFile data = new SaveFile
        {
            records = Records
        };
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Дані збережено: {savePath}");
    }

    public void LoadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveFile data = JsonUtility.FromJson<SaveFile>(json);
            Records = data.records ?? new List<RecordEntry>();
            Debug.Log($"Дані завантажено: {Records.Count} рекордів");
        }
        else
        {
            Records = new List<RecordEntry>();
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveData();
    }
}

[Serializable]
public class RecordEntry
{
    public int coins;
    public float time;
    public string date;
}

[Serializable]
public class SaveFile
{
    public List<RecordEntry> records = new List<RecordEntry>();
}

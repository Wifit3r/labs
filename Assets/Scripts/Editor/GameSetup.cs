using UnityEngine;
using UnityEditor;

/// <summary>
/// Автоматичне налаштування тегів та створення сцени гри.
/// Запуск: меню Tools > Setup Runner Game
/// </summary>
public class GameSetup : EditorWindow
{
    [MenuItem("Tools/Setup Runner Game")]
    public static void SetupGame()
    {
        AddTag("Obstacle");
        AddTag("Pit");
        AddTag("Finish");
        AddTag("BoostPickup");

        CreateGameScene();

        Debug.Log("Runner Game налаштовано! Натисніть Play.");
    }

    static void AddTag(string tagName)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Перевірка чи тег вже існує
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"Тег '{tagName}' додано.");
    }

    static void CreateGameScene()
    {
        // Гравець
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.GetComponent<Renderer>().material.color = Color.blue;

        // Додаємо CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0f, 0f, 0f);
        cc.height = 2f;
        cc.radius = 0.5f;

        player.AddComponent<PlayerController>();
        player.AddComponent<PitDetector>();

        // Камера
        Camera.main.gameObject.AddComponent<CameraFollow>();
        Camera.main.GetComponent<CameraFollow>().target = player.transform;

        // Генератор полоси
        GameObject track = new GameObject("TrackGenerator");
        track.AddComponent<TrackGenerator>();

        // GameManager
        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();

        // Освітлення (якщо немає)
        if (FindObjectOfType<Light>() == null)
        {
            GameObject light = new GameObject("DirectionalLight");
            Light l = light.AddComponent<Light>();
            l.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
    }
}

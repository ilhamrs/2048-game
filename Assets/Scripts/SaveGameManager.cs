using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FlatBuffers;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;
    // string path = "/Data"+".sav";
    public string filename;
    public int score;
    void Awake()
    {
        Instance = this;
    }
    public void Save(int score)
    {
        FlatBufferBuilder builder = new(1);

        FlatBuffers.Offset<SaveGame.Data> data = SaveGame.Data.CreateData(builder, score);

        builder.Finish(data.Value);

        byte[] buffer = builder.SizedByteArray();

        string fullPath = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(fullPath, buffer);
        Debug.Log("Wrote " + buffer.Length + " bytes to " + fullPath);
    }

    void Start()
    {
        Load();
    }
    void Load()
    {
        try
        {
            string fullPath = Path.Combine(Application.persistentDataPath, filename);

            if (File.Exists(fullPath))
            {
                ByteBuffer loader = new(File.ReadAllBytes(fullPath));
                SaveGame.Data loadData = SaveGame.Data.GetRootAsData(loader);

                int value = loadData.Highscore;
                score = value;

                GameManager.Instance.SetHighScore(value);
                Debug.Log("Loaded highscore: " + value);
            }
            else
            {
                Debug.LogWarning("Save file not found. Starting with score 0.");
                score = 0;
                GameManager.Instance.SetHighScore(0);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to load data: " + e.Message);
            score = 0;
            GameManager.Instance.SetHighScore(0);
        }
    }

    private void OnApplicationQuit() {
        // Save();
    }
}

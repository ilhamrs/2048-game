using System.Collections;
using System.Collections.Generic;
using System.IO;
using FlatBuffers;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveGameManager Instance;
    string path = "/Data"+".sav";
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

        File.WriteAllBytes(Application.dataPath + path, buffer);
        Debug.Log("wrote "+builder.Offset + " byte to "+path);
    }
    void Start()
    {
        Load();
    }
    void Load(){

        try{
            ByteBuffer loader = new(File.ReadAllBytes(Application.dataPath + path));
            SaveGame.Data loadData = SaveGame.Data.GetRootAsData(loader);

            int value = loadData.Highscore;

            score = value;

            // Debug.LogError(value);

            GameManager.Instance.SetHighScore(value);
        }catch{
            score = 0;
            GameManager.Instance.SetHighScore(0);
        }
        
    }
    private void OnApplicationQuit() {
        // Save();
    }
}

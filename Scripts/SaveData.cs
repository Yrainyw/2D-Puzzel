using System.Collections.Generic;

namespace Game;

public class SaveData() // 存储游戏存档数据
{
    private const string SFX_BUS_NAME = "SFX";
    private const string MUSIC_BUS_NAME = "Music";

    public Dictionary<string, LevelCompletionData> LevelCompletionStatus { get; private set; } = new(); // 记录每个关卡的完成状态（关卡ID → 通关数据）
    public float Music { get; set; }  = 0.8f;
    public float SFX { get; set; } = 0.8f;
    public bool IsFullScreen { get; set; } = false;

    // 保存指定关卡的完成状态
    public void SaveLevelCompletion(string id, bool completed)
    {
        if (!LevelCompletionStatus.ContainsKey(id)) // 如果关卡不存在，就创建一条新记录
        {
            LevelCompletionStatus[id] = new LevelCompletionData();
        }

        LevelCompletionStatus[id].IsCompleted= completed; // 更新关卡是否通关
    }

    // 保存音乐或音效音量
    public void SaveVolume(string busName, float volume)
    {
        if (busName == MUSIC_BUS_NAME)
        {
            Music = volume;
        }
        else if (busName == SFX_BUS_NAME)
        {
            SFX = volume;
        }
    }

    // 获取存档中的音量
    public float GetVolume(string busName)
    {
        if (busName == MUSIC_BUS_NAME)
        {
            return Music;
        }
        else if (busName == SFX_BUS_NAME)
        {
            return SFX;
        }

        return 0.8f;
    }

    public void SaveIsFullScreen(bool isFullScreen)
    {
        IsFullScreen = isFullScreen;
    }
}
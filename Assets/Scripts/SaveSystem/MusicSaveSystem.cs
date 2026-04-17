using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 저장된 음악을 로컬 디스크에 JSON 으로 저장/불러오기
/// 각 스테이지별로 여러 곡 저장 가능 (폴리브릿지 솔루션 브라우저 느낌)
/// </summary>
public static class MusicSaveSystem
{
    private static string Root => Path.Combine(Application.persistentDataPath, "music");

    private static string StageFolder(int stageIndex)
        => Path.Combine(Root, $"stage_{stageIndex}");

    [Serializable]
    private class RecordingWrapper { public MusicRecording data; }

    public static string Save(MusicRecording recording)
    {
        if (recording == null) return null;
        Directory.CreateDirectory(StageFolder(recording.stageIndex));

        if (string.IsNullOrEmpty(recording.id))
            recording.id = Guid.NewGuid().ToString("N");
        if (recording.timestampUnix == 0)
            recording.timestampUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string path = Path.Combine(StageFolder(recording.stageIndex), recording.id + ".json");
        string json = JsonUtility.ToJson(new RecordingWrapper { data = recording }, true);
        File.WriteAllText(path, json);

        Debug.Log($"[MusicSave] 저장됨: {path}");
        return path;
    }

    public static List<MusicRecording> LoadAll(int stageIndex)
    {
        var list = new List<MusicRecording>();
        var folder = StageFolder(stageIndex);
        if (!Directory.Exists(folder)) return list;

        foreach (var file in Directory.GetFiles(folder, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var wrapper = JsonUtility.FromJson<RecordingWrapper>(json);
                if (wrapper?.data != null) list.Add(wrapper.data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MusicSave] 읽기 실패 {file}: {e.Message}");
            }
        }

        list.Sort((a, b) => b.timestampUnix.CompareTo(a.timestampUnix));
        return list;
    }

    public static bool Delete(MusicRecording recording)
    {
        if (recording == null) return false;
        string path = Path.Combine(StageFolder(recording.stageIndex), recording.id + ".json");
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    public static string ExportToString(MusicRecording recording)
        => JsonUtility.ToJson(new RecordingWrapper { data = recording }, false);

    public static MusicRecording ImportFromString(string json)
    {
        var wrapper = JsonUtility.FromJson<RecordingWrapper>(json);
        return wrapper?.data;
    }
}

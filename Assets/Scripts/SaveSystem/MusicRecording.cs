using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 저장 가능한 한 곡의 음악 (스테이지 클리어 시 녹음된 음표 목록)
/// </summary>
[Serializable]
public class MusicRecording
{
    public string id;
    public int stageIndex;
    public string stageName;
    public string authorName = "나";
    public long timestampUnix;
    public int efficiencyScore;
    public int usageCount;
    public List<RecordedNoteData> notes = new();

    public string GetTimestampString()
    {
        var dt = DateTimeOffset.FromUnixTimeSeconds(timestampUnix).LocalDateTime;
        return dt.ToString("yyyy-MM-dd HH:mm");
    }
}

[Serializable]
public class RecordedNoteData
{
    public string instrumentName;
    public int noteIndex;
    public float time;
    public float volume = 1f;
}

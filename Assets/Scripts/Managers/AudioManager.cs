using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 사운드 재생 + 음악 녹음/재생 담당
/// 빌드 페이즈에서 어떤 악기의 몇 번째 음이 언제 재생되었는지 기록
/// 녹음된 시퀀스는 MusicRecording 으로 추출해 디스크에 저장 가능
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    private readonly List<RecordedNoteData> recordedNotes = new();
    private float recordStartTime;

    public IReadOnlyList<RecordedNoteData> CurrentRecording => recordedNotes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── 녹음 제어 ─────────────────────────────────────────

    public void StartRecording()
    {
        recordedNotes.Clear();
        recordStartTime = Time.time;
    }

    /// <summary>
    /// 판정 시점에서 악기가 연주한 음을 재생 + 녹음
    /// </summary>
    public void PlayAndRecord(InstrumentData instrument, int noteIndex, float volume = 1f)
    {
        if (instrument == null) return;
        if (instrument.noteClips == null || instrument.noteClips.Length == 0) return;

        int safeIndex = Mathf.Clamp(noteIndex, 0, instrument.noteClips.Length - 1);
        var clip = instrument.noteClips[safeIndex];
        if (clip != null) sfxSource.PlayOneShot(clip, volume);

        recordedNotes.Add(new RecordedNoteData
        {
            instrumentName = instrument.instrumentName,
            noteIndex = safeIndex,
            time = Time.time - recordStartTime,
            volume = volume,
        });
    }

    public MusicRecording BuildRecording(int stageIndex, string stageName, int efficiencyScore, int usageCount)
    {
        return new MusicRecording
        {
            stageIndex = stageIndex,
            stageName = stageName,
            efficiencyScore = efficiencyScore,
            usageCount = usageCount,
            notes = new List<RecordedNoteData>(recordedNotes),
        };
    }

    // ── 현재 녹음 재생 ────────────────────────────────────

    public void PlayRecordedMusic() => PlayRecording(BuildCurrentAsRecording());

    private MusicRecording BuildCurrentAsRecording()
    {
        return new MusicRecording { notes = new List<RecordedNoteData>(recordedNotes) };
    }

    /// <summary>
    /// 저장된 MusicRecording 을 재생 (음악 갤러리에서 사용)
    /// </summary>
    public void PlayRecording(MusicRecording recording)
    {
        if (recording == null || recording.notes == null || recording.notes.Count == 0)
        {
            Debug.Log("[AudioManager] 재생할 음표 없음");
            return;
        }
        StopAllCoroutines();
        StartCoroutine(PlayRecordingCoroutine(recording));
    }

    public void StopPlayback() => StopAllCoroutines();

    private IEnumerator PlayRecordingCoroutine(MusicRecording recording)
    {
        var registry = InstrumentRegistry.Load();
        float startTime = Time.time;
        foreach (var note in recording.notes)
        {
            float waitTime = note.time - (Time.time - startTime);
            if (waitTime > 0) yield return new WaitForSeconds(waitTime);

            var inst = registry?.Find(note.instrumentName);
            if (inst == null) continue;
            if (inst.noteClips == null || inst.noteClips.Length == 0) continue;

            int idx = Mathf.Clamp(note.noteIndex, 0, inst.noteClips.Length - 1);
            var clip = inst.noteClips[idx];
            if (clip != null) sfxSource.PlayOneShot(clip, note.volume);
        }
    }
}

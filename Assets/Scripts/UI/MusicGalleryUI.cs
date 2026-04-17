using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 저장된 음악 목록을 스테이지별로 보여주는 갤러리 UI
/// (폴리브릿지의 "다른 플레이어 솔루션" 브라우저 느낌)
/// 메인메뉴 혹은 GameScene 내 패널로 노출
/// </summary>
public class MusicGalleryUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject root;

    [Header("스테이지 탭")]
    [SerializeField] private Transform stageTabParent;
    [SerializeField] private GameObject stageTabPrefab;   // Button + TextMeshProUGUI(child)

    [Header("기록 목록")]
    [SerializeField] private Transform recordListParent;
    [SerializeField] private GameObject recordEntryPrefab; // Button + TextMeshProUGUI(child)

    [Header("재생 정보")]
    [SerializeField] private TextMeshProUGUI nowPlayingText;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button closeButton;

    private int selectedStageIndex = 0;
    private readonly List<GameObject> stageTabs = new();
    private readonly List<GameObject> recordEntries = new();

    private void Awake()
    {
        if (stopButton != null) stopButton.onClick.AddListener(OnStopClicked);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        if (root != null) root.SetActive(true);
        RefreshStageTabs();
        SelectStage(selectedStageIndex);
    }

    public void Close()
    {
        AudioManager.Instance?.StopPlayback();
        if (root != null) root.SetActive(false);
    }

    private void RefreshStageTabs()
    {
        foreach (var t in stageTabs) Destroy(t);
        stageTabs.Clear();

        var stages = StageManager.Instance?.AllStages;
        if (stages == null || stageTabParent == null || stageTabPrefab == null) return;

        for (int i = 0; i < stages.Length; i++)
        {
            int stageIdx = i;
            var go = Instantiate(stageTabPrefab, stageTabParent);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = $"Stage {stages[i].stageIndex}";
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => SelectStage(stageIdx));
            stageTabs.Add(go);
        }
    }

    private void SelectStage(int index)
    {
        selectedStageIndex = index;
        var stages = StageManager.Instance?.AllStages;
        if (stages == null || index < 0 || index >= stages.Length) return;

        RefreshRecordList(stages[index].stageIndex);
    }

    private void RefreshRecordList(int stageIndex)
    {
        foreach (var e in recordEntries) Destroy(e);
        recordEntries.Clear();

        var list = MusicSaveSystem.LoadAll(stageIndex);
        if (recordListParent == null || recordEntryPrefab == null) return;

        if (list.Count == 0)
        {
            var empty = Instantiate(recordEntryPrefab, recordListParent);
            var label = empty.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = "아직 저장된 음악이 없습니다.\n스테이지를 클리어하면 여기에 저장됩니다.";
            var btn = empty.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
            recordEntries.Add(empty);
            return;
        }

        foreach (var rec in list)
        {
            var go = Instantiate(recordEntryPrefab, recordListParent);
            var label = go.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"{rec.authorName}  |  점수 {rec.efficiencyScore}  |  악기 {rec.usageCount}회\n{rec.GetTimestampString()}";

            var btn = go.GetComponent<Button>();
            if (btn != null)
            {
                var captured = rec;
                btn.onClick.AddListener(() => PlayRecord(captured));
            }
            recordEntries.Add(go);
        }
    }

    private void PlayRecord(MusicRecording rec)
    {
        if (nowPlayingText != null)
            nowPlayingText.text = $"♪ 재생 중: {rec.authorName} (Stage {rec.stageIndex})";
        AudioManager.Instance?.PlayRecording(rec);
    }

    private void OnStopClicked()
    {
        AudioManager.Instance?.StopPlayback();
        if (nowPlayingText != null) nowPlayingText.text = "";
    }
}

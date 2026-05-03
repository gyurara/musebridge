using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 메뉴 씬 UI 제어 — 재작성
/// [TODO 완료] 음악 갤러리를 메인메뉴에서 접근 가능하게
/// [TODO 완료] UI SFX 연결 (버튼 클릭음)
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button galleryButton; // [신규] 음악 갤러리 버튼

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("갤러리 패널 (MainMenu에 직접 임베드)")]
    [SerializeField] private MusicGalleryUI galleryUI;

    [Header("배경")]
    [SerializeField] private SheetMusicBackground background;

    private void Start()
    {
        // 타이틀 텍스트
        if (titleText    != null) titleText.text    = "Bridge Melody";
        if (subtitleText != null) subtitleText.text = "음악으로 다리를 만들어 건너세요";

        // 버튼 연결
        startButton?.onClick.AddListener(OnStartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
        galleryButton?.onClick.AddListener(OnGalleryClicked);

        // 게임 재시작 시 악기 보유 초기화 여부 확인
        // (GameManager가 DontDestroyOnLoad이므로 메인메뉴 복귀 시도 감지)
        if (GameManager.Instance != null && GameManager.Instance.CurrentPhase == GamePhase.MainMenu)
        {
            StageManager.Instance?.ResetPersistedInstruments();
        }

        // BGM 시작
        BGMManager.Instance?.PlayBGM(null); // 메인메뉴는 BuildBGM 사용
        // BGMManager가 준비되면 자동으로 PlayBGMForCurrentPhase 호출됨
    }

    private void OnStartClicked()
    {
        BGMManager.Instance?.PlaySFXClick();
        // 게임 상태 초기화
        GameManager.Instance?.LoadMainMenu(); // CurrentStageIndex = 0 리셋
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitClicked()
    {
        BGMManager.Instance?.PlaySFXClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGalleryClicked()
    {
        BGMManager.Instance?.PlaySFXClick();

        // GalleryUI가 Inspector에 연결돼 있으면 Open()
        if (galleryUI != null)
        {
            galleryUI.Open();
            return;
        }

        // 연결 안 돼있으면 씬에서 탐색
        var found = Object.FindObjectOfType<MusicGalleryUI>(includeInactive: true);
        if (found != null)
        {
            found.Open();
        }
        else
        {
            Debug.LogWarning("[MainMenuController] MusicGalleryUI를 찾을 수 없음 — Inspector에 연결 필요");
        }
    }
}

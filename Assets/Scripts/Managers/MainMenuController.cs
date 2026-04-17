using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 메인 메뉴 씬 UI 제어
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("메인 메뉴 배경")]
    [SerializeField] private SheetMusicBackground background; // 옵션

    private void Start()
    {
        startButton?.onClick.AddListener(OnStartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        if (titleText != null) titleText.text = "Bridge Melody";
        if (subtitleText != null) subtitleText.text = "음악으로 다리를 만들어 건너세요";
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

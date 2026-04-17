using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 사이드바에 생성되는 악기 버튼 하나
/// 선택 시 하이라이트, 키 표시
/// </summary>
public class InstrumentSidebarButton : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI keyLabel;

    [Header("색상")]
    [SerializeField] private Color defaultColor = new Color(0.9f, 0.9f, 0.9f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.3f, 0.75f, 1f, 0.95f);

    private InstrumentData boundInstrument;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnButtonClicked);
    }

    public void Setup(InstrumentData instrument)
    {
        boundInstrument = instrument;

        if (nameLabel != null) nameLabel.text = instrument.instrumentName;
        if (keyLabel != null) keyLabel.text = $"[{instrument.activationKey}]";
        if (iconImage != null && instrument.instrumentIcon != null)
            iconImage.sprite = instrument.instrumentIcon;

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : defaultColor;
    }

    private void OnButtonClicked()
    {
        // BuildPhaseManager에 선택 악기 전달
        BuildPhaseManager.Instance?.SelectInstrument(boundInstrument);
    }
}

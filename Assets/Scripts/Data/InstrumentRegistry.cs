using UnityEngine;

/// <summary>
/// 전체 악기 목록 레지스트리
/// 런타임에서 이름으로 InstrumentData 를 찾기 위해 사용 (음악 재생 시)
/// Resources/InstrumentRegistry.asset 에 배치
/// </summary>
[CreateAssetMenu(fileName = "InstrumentRegistry", menuName = "BridgeRhythm/Instrument Registry")]
public class InstrumentRegistry : ScriptableObject
{
    public InstrumentData[] instruments;

    private static InstrumentRegistry _cached;
    public static InstrumentRegistry Load()
    {
        if (_cached != null) return _cached;
        _cached = Resources.Load<InstrumentRegistry>("InstrumentRegistry");
        return _cached;
    }

    public InstrumentData Find(string instrumentName)
    {
        if (instruments == null) return null;
        foreach (var inst in instruments)
            if (inst != null && inst.instrumentName == instrumentName) return inst;
        return null;
    }
}

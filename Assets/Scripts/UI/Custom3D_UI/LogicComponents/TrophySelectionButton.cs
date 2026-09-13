using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TrophySelectionButton : Button
{
    public int trophyIndex = 0;

    private RaceSettings _raceSettings;

    public override void Init()
    {
        base.Init();
        _raceSettings = RaceSettings.Instance;
        if (_raceSettings == null)
        {
            Debug.LogWarning("[TrophySelectionButton] RaceSettings instance not found");
        }
    }

    public override void OnConfirmSelection()
    {
        if (_raceSettings != null)
        {
            _raceSettings.OnTrophySelect(trophyIndex);
        }
        base.OnConfirmSelection();
    }
}

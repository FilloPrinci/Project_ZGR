using UnityEngine;

public class TrackSelectionButton : Button
{
    public int trackIndex = 0;

    private RaceSettings _raceSettings;

    public override void Init()
    {
        base.Init();
        _raceSettings = RaceSettings.Instance;
        if (_raceSettings == null)
        {
            Debug.LogWarning("[TrackSelectionButton] RaceSettings instance not found");
        }
    }

    public override void OnConfirmSelection()
    {
        if (_raceSettings != null)
        {
            _raceSettings.OnRaceTrackSelect(trackIndex);
        }
        base.OnConfirmSelection();
    }
}

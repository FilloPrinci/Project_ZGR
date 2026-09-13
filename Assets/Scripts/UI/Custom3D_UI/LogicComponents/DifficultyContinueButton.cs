using UnityEngine;

// Shared LogicComponent for every item of the Difficulty selection group. A plain Button can
// only navigate to one fixed nextGroupComponent, but after difficulty the destination depends
// on the session mode chosen earlier on the "Select Mode" screen (single race vs trophy), so
// this branches between the two instead.
public class DifficultyContinueButton : Button
{
    public UI_GroupComponent singleRaceNextGroup;
    public UI_GroupComponent trophyNextGroup;

    private RaceSettings _raceSettings;

    public override void Init()
    {
        base.Init();
        _raceSettings = RaceSettings.Instance;
        if (_raceSettings == null)
        {
            Debug.LogWarning("[DifficultyContinueButton] RaceSettings instance not found");
        }
    }

    public override void OnConfirmSelection()
    {
        bool trophyMode = _raceSettings != null && _raceSettings.GetSelectedSessionMode() == SessionMode.Trophy;
        UI_GroupComponent target = trophyMode ? trophyNextGroup : singleRaceNextGroup;

        Debug.Log("Navigate to " + (target != null ? target.GroupName : "NULL"));
        if (_manager != null && target != null)
        {
            _manager.NavigateTo(target);
        }
    }
}

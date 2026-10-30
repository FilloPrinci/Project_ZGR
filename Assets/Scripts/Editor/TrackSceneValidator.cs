using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Checks the track wiring that fails silently at runtime: a missing soundtrack reference just skips
// the music, and missing boundary colliders just leave the CPUs without side sensors.
public static class TrackSceneValidator
{
    private const string SettingsPrefabPath = "Assets/Prefabs/System/Settings.prefab";
    private const string TemplateFolder = "/_Template/";

    [MenuItem("Tools/ZGR/Validate Active Track Scene")]
    private static void ValidateActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        List<string> errors = new List<string>();
        List<string> warnings = new List<string>();
        Validate(scene, errors, warnings);

        foreach (string warning in warnings)
            Debug.LogWarning($"[TrackSceneValidator] {scene.name}: {warning}");
        foreach (string error in errors)
            Debug.LogError($"[TrackSceneValidator] {scene.name}: {error}");
        Debug.Log($"[TrackSceneValidator] {scene.name}: {errors.Count} errors, {warnings.Count} warnings");
    }

    public static void Validate(Scene scene, List<string> errors, List<string> warnings)
    {
        RaceManager raceManager = FindInScene<RaceManager>(scene);
        if (raceManager == null)
        {
            errors.Add("no RaceManager in the scene");
            return;
        }

        ValidateRaceManager(raceManager, errors);
        ValidateCheckpoints(raceManager, errors);
        ValidateCPUColliders(scene, errors);
        ValidatePresentation(scene, errors);
        ValidateSoundtrack(raceManager, errors, warnings);
        ValidateHoverLayers(scene, raceManager, warnings);
        ValidateLighting(scene, warnings);
        ValidateRegistration(scene, errors, warnings);
    }

    private static void ValidateRaceManager(RaceManager raceManager, List<string> errors)
    {
        Require(raceManager.mainCamera, "RaceManager.mainCamera", errors);
        Require(raceManager.playerPrefab, "RaceManager.playerPrefab", errors);
        Require(raceManager.trackMainCollider, "RaceManager.trackMainCollider", errors);
        Require(raceManager.presentationManager, "RaceManager.presentationManager", errors);
        Require(raceManager.playersCollisionDetection, "RaceManager.playersCollisionDetection", errors);
        Require(raceManager.namesLoader, "RaceManager.namesLoader", errors);
        Require(raceManager.finishLineManager, "RaceManager.finishLineManager", errors);

        if (raceManager.raceGrid == null)
            errors.Add("RaceManager.raceGrid is not set");
        else if (raceManager.raceGrid.transform.childCount == 0)
            errors.Add("RaceManager.raceGrid has no spawn points");
    }

    private static void ValidateCheckpoints(RaceManager raceManager, List<string> errors)
    {
        if (raceManager.checkPointListParent == null)
        {
            errors.Add("RaceManager.checkPointListParent is not set");
            return;
        }

        if (raceManager.checkPointListParent.transform.childCount < 2)
            errors.Add("no checkpoints: assign checkpointSourceCSV and run 'Generate Checkpoints from Curve' on RaceManager");
    }

    private static void ValidateCPUColliders(Scene scene, List<string> errors)
    {
        CPUManager cpuManager = FindInScene<CPUManager>(scene);
        if (cpuManager == null)
        {
            errors.Add("no CPUManager in the scene");
            return;
        }

        Require(cpuManager.leftCollider, "CPUManager.leftCollider", errors);
        Require(cpuManager.rightCollider, "CPUManager.rightCollider", errors);
    }

    private static void ValidatePresentation(Scene scene, List<string> errors)
    {
        PresentationManager presentationManager = FindInScene<PresentationManager>(scene);
        if (presentationManager == null)
        {
            errors.Add("no PresentationManager in the scene");
            return;
        }

        List<CameraMovement> movements = presentationManager.presentationMovements;
        for (int i = 0; i < movements.Count; i++)
        {
            if (movements[i].type == CameraMovementType.Orbit && movements[i].pivot == null)
                errors.Add($"presentation movement {i} is an Orbit without a pivot");
            if (movements[i].type == CameraMovementType.SetTransform && movements[i].targetTransform == null)
                errors.Add($"presentation movement {i} is a SetTransform without a target");
        }
    }

    private static void ValidateSoundtrack(RaceManager raceManager, List<string> errors, List<string> warnings)
    {
        AudioSource soundtrack = raceManager.sountrackAudioSource;
        if (soundtrack == null)
        {
            errors.Add("RaceManager.sountrackAudioSource is not set: the race would have no music");
            return;
        }

        if (soundtrack.resource == null)
            errors.Add("the soundtrack AudioSource has no clip");
        if (soundtrack.playOnAwake)
            warnings.Add("the soundtrack has Play On Awake on: it should only start with the race");
    }

    // Vehicles hover on whatever their raycast hits, so solid scenery on a hover layer can lift them.
    private static void ValidateHoverLayers(Scene scene, RaceManager raceManager, List<string> warnings)
    {
        if (raceManager.playerPrefab == null) return;
        PlayerController playerController = raceManager.playerPrefab.GetComponentInChildren<PlayerController>(true);
        if (playerController == null) return;

        int hoverMask = playerController.hoverRaycastMask.value;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name != "Map") continue;

            foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.isTrigger && (hoverMask & (1 << collider.gameObject.layer)) != 0)
                    warnings.Add($"scenery collider '{collider.name}' is on hover layer '{LayerMask.LayerToName(collider.gameObject.layer)}'");
            }
        }
    }

    private static void ValidateLighting(Scene scene, List<string> warnings)
    {
        // Lightmapping.lightingDataAsset only reports on the active scene.
        if (SceneManager.GetActiveScene() != scene) return;

        string lightingDataPath = AssetDatabase.GetAssetPath(Lightmapping.lightingDataAsset);
        if (!lightingDataPath.StartsWith("Assets/"))
            warnings.Add("lighting has not been baked");
        else if (!lightingDataPath.StartsWith($"{Path.ChangeExtension(scene.path, null)}/"))
            warnings.Add($"lighting data belongs to another scene: {lightingDataPath}");
    }

    private static void ValidateRegistration(Scene scene, List<string> errors, List<string> warnings)
    {
        bool inBuild = false;
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
            inBuild |= buildScene.enabled && buildScene.path == scene.path;

        bool inTrackList = false;
        GameObject settings = AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath);
        SceneReferences sceneReferences = settings != null ? settings.GetComponentInChildren<SceneReferences>(true) : null;
        if (sceneReferences != null)
        {
            foreach (TrackSceneData track in sceneReferences.trackSceneDataList)
                inTrackList |= track.sceneName == scene.name;
        }

        // trackSceneDataList feeds the menu, the trophy and Demo Mode, so the template must stay out of it.
        if (scene.path.Contains(TemplateFolder))
        {
            if (inBuild || inTrackList)
                errors.Add("the template must not be registered in Build Settings or trackSceneDataList");
            return;
        }

        if (!inBuild)
            warnings.Add("not enabled in Build Settings");
        if (!inTrackList)
            warnings.Add("not listed in Settings.prefab > trackSceneDataList");
    }

    private static void Require(Object reference, string label, List<string> errors)
    {
        if (reference == null)
            errors.Add($"{label} is not set");
    }

    private static T FindInScene<T>(Scene scene) where T : Component
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            T component = root.GetComponentInChildren<T>(true);
            if (component != null) return component;
        }
        return null;
    }
}

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CameraMovement))]
public class CameraMovementDrawer : PropertyDrawer
{
    private const float LineH  = 18f;
    private const float Pad    = 2f;
    private const float Step   = LineH + Pad;
    private const float SepH   = 8f;   // separator before fade section

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return TotalLines(property) * Step + SepH + Pad;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProp     = property.FindPropertyRelative("type");
        var durationProp = property.FindPropertyRelative("duration");
        var curveProp    = property.FindPropertyRelative("curve");
        var type         = (CameraMovementType)typeProp.enumValueIndex;

        Rect row = new Rect(position.x, position.y + Pad, position.width, LineH);

        // --- Movement fields ---
        EditorGUI.PropertyField(row, typeProp, new GUIContent("Type"));       row.y += Step;
        EditorGUI.PropertyField(row, durationProp, new GUIContent("Duration (s)")); row.y += Step;
        EditorGUI.PropertyField(row, curveProp, new GUIContent("Curve"));     row.y += Step;

        switch (type)
        {
            case CameraMovementType.SetTransform:
                Field(ref row, property, "targetTransform", "Target Transform");
                break;

            case CameraMovementType.Orbit:
                Field(ref row, property, "pivot",                  "Pivot");
                Field(ref row, property, "radius",                 "Radius");
                Field(ref row, property, "heightOffset",           "Height Offset");
                Field(ref row, property, "orbitDegrees",           "Degrees");
                Field(ref row, property, "startFromPivotRotation", "Start From Pivot Rotation");
                break;

            case CameraMovementType.ZoomIn:
                Field(ref row, property, "lookAtTarget", "Look At Target");
                Field(ref row, property, "fovDelta",     "FOV Reduction");
                break;

            case CameraMovementType.ZoomOut:
                Field(ref row, property, "lookAtTarget", "Look At Target");
                Field(ref row, property, "fovDelta",     "FOV Increase");
                break;

            case CameraMovementType.MoveForward:
            case CameraMovementType.MoveBackward:
            case CameraMovementType.MoveUp:
            case CameraMovementType.MoveDown:
            case CameraMovementType.MoveLeft:
            case CameraMovementType.MoveRight:
                Field(ref row, property, "distance", "Distance (units)");
                break;
        }

        // --- Fade section ---
        row.y += SepH;
        EditorGUI.DrawRect(new Rect(row.x, row.y, row.width, 1f), new Color(0.4f, 0.4f, 0.4f));
        row.y += Pad;

        DrawFadeGroup(ref row, property.FindPropertyRelative("fadeIn"),  "Fade In  (start)");
        DrawFadeGroup(ref row, property.FindPropertyRelative("fadeOut"), "Fade Out (end)");

        EditorGUI.EndProperty();
    }

    private static void Field(ref Rect row, SerializedProperty parent, string name, string label)
    {
        EditorGUI.PropertyField(row, parent.FindPropertyRelative(name), new GUIContent(label));
        row.y += Step;
    }

    private static void DrawFadeGroup(ref Rect row, SerializedProperty fadeProp, string label)
    {
        var enabledProp = fadeProp.FindPropertyRelative("enabled");
        EditorGUI.PropertyField(row, enabledProp, new GUIContent(label));
        row.y += Step;

        if (enabledProp.boolValue)
        {
            EditorGUI.indentLevel++;
            Field(ref row, fadeProp, "duration", "Duration (s)");
            Field(ref row, fadeProp, "curve",    "Curve");
            EditorGUI.indentLevel--;
        }
    }

    private static int TotalLines(SerializedProperty property)
    {
        // base: type + duration + curve = 3
        var type = (CameraMovementType)property.FindPropertyRelative("type").enumValueIndex;
        int lines = type switch
        {
            CameraMovementType.SetTransform                                        => 4,  // + targetTransform
            CameraMovementType.Orbit                                               => 8,  // + pivot, radius, heightOffset, degrees, startFromPivot
            CameraMovementType.ZoomIn or CameraMovementType.ZoomOut               => 5,  // + lookAtTarget, fovDelta
            _                                                                      => 4   // + distance
        };

        // fadeIn and fadeOut: each is 1 line (toggle) + 2 more if enabled
        lines += FadeLines(property.FindPropertyRelative("fadeIn"));
        lines += FadeLines(property.FindPropertyRelative("fadeOut"));

        return lines;
    }

    private static int FadeLines(SerializedProperty fadeProp)
    {
        bool enabled = fadeProp.FindPropertyRelative("enabled").boolValue;
        return enabled ? 3 : 1; // toggle + (duration + curve) if enabled
    }
}

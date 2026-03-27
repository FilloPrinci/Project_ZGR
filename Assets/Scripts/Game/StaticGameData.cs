using UnityEngine;

public enum ReleasePlatform
{
    Windows,
    WebGL,
    Linux
}

public static class StaticGameData
{
    public static ReleasePlatform CurrentReleasePlatform = ReleasePlatform.Windows;



}

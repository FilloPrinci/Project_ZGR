using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private float fixedDeltaTime = 0.0f;

    private int fixedUpdateCount = 0;
    private float fixedUpdateTimer = 0f;
    private int fixedUpdatesPerSecond = 0;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f; // Smoothed deltaTime
    }

    void FixedUpdate()
    {
        fixedUpdateCount++;
    }

    void LateUpdate()
    {
        // Conta quanti FixedUpdate al secondo
        fixedUpdateTimer += Time.deltaTime;
        if (fixedUpdateTimer >= 1f)
        {
            fixedUpdatesPerSecond = fixedUpdateCount;
            fixedUpdateCount = 0;
            fixedUpdateTimer = 0f;
        }

        fixedDeltaTime = Time.fixedDeltaTime; // costante, ma utile da mostrare
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 6 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.white;

        float fps = 1.0f / deltaTime;
        string text =
            string.Format("{0:0.} FPS\n", fps) +
            string.Format("Delta Time: {0:0.000} sec\n", Time.deltaTime) +
            string.Format("Fixed Delta Time: {0:0.000} sec\n", fixedDeltaTime) +
            string.Format("FixedUpdate Rate: {0} Hz", fixedUpdatesPerSecond);

        GUI.Label(rect, text, style);
    }
}

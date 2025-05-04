using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera introCamera;
    public Camera[] playerCameras; // Assegna da 1 a 4 camere
    public int playerCount = 1;

    public void Start()
    {
        ShowIntroCamera();
    }

    void ShowIntroCamera()
    {
        introCamera.enabled = true;
        foreach (Camera cam in playerCameras)
        {
            cam.enabled = false;
        }
    }

    public void StartPlayerCameras()
    {
        introCamera.enabled = false;

        for (int i = 0; i < playerCameras.Length; i++)
        {
            if (i < playerCount)
            {
                playerCameras[i].enabled = true;
                playerCameras[i].rect = GetViewportRect(playerCount, i);
            }
            else
            {
                playerCameras[i].enabled = false;
            }
        }
    }

    Rect GetViewportRect(int count, int index)
    {
        switch (count)
        {
            case 1:
                return new Rect(0f, 0f, 1f, 1f); // Full screen
            case 2:
                return index == 0 ? new Rect(0f, 0.5f, 1f, 0.5f) : new Rect(0f, 0f, 1f, 0.5f);
            case 3:
                if (index == 0) return new Rect(0f, 0.5f, 0.5f, 0.5f);
                if (index == 1) return new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                return new Rect(0f, 0f, 1f, 0.5f);
            case 4:
                return new Rect((index % 2) * 0.5f, (index < 2 ? 0.5f : 0f), 0.5f, 0.5f);
            default:
                return new Rect(0f, 0f, 1f, 1f);
        }
    }
}

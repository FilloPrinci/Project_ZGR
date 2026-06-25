using UnityEngine;

public class FinishLineManager : MonoBehaviour
{
    public GameObject CheckerboardFlag;
    public GameObject GreenFlag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCheckerboardFlag();
    }

    public void SetCheckerboardFlag()
    {
        GreenFlag.SetActive(false);

        CheckerboardFlag.SetActive(true);
    }


    public void SetGreenFlag()
    {
        CheckerboardFlag.SetActive(false);

        GreenFlag.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

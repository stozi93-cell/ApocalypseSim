using UnityEngine;

public static class StampedeManager
{
    private static bool stampedeActive = false;

    public static void ReportStampedeStart(Vector2 position)
{
    stampedeActive = true;
    highlightTriggered = false;
    Debug.Log("STAMPede detected at " + position);
}

    public static void ReportStampedeDeath(Vector2 position)
{
    if (highlightTriggered)
        return;

    highlightTriggered = true;
    Debug.Log("HIGHLIGHT: STAMPEDE FIRST DEATH");
}

    static void TriggerStampedeHighlight(Vector2 position)
    {
        Debug.Log("HIGHLIGHT: STAMPEDE FIRST DEATH");
        // Future: pause sim, side-view, choices
        
        Time.timeScale = 0.2f;

    }
    private static bool highlightTriggered = false;

}

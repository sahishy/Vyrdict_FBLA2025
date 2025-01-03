using UnityEngine;

public class TileDisplayHandler : MonoBehaviour
{
    public static TileDisplayHandler instance;
    public bool displayActive = false;

    void Awake() {
        instance = this;
    }

    public void ShowDisplay(Buildable buildable) {
        displayActive = true;
        Debug.Log($"Display Shown: {buildable.name}");
    }
    public void HideDisplay() {
        if(!displayActive) {
            return;
        }
        
        displayActive = false;
        Debug.Log("Display Hidden");
    }
}

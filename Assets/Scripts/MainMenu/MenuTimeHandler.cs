using UnityEngine;

public class MenuTimeHandler : MonoBehaviour
{
    [SerializeField] private float dayTimer;
    [SerializeField] private float dayDuration;
    [SerializeField] private Gradient timeColor;
    [SerializeField] private Transform mainLight;

    void Update()
    {
        dayTimer += Time.deltaTime;

        if(dayTimer >= dayDuration) {
            dayTimer = 0;
        }

        mainLight.rotation = Quaternion.Euler((dayTimer / dayDuration) * 360, -30, 0);
        RenderSettings.ambientLight = timeColor.Evaluate(dayTimer / dayDuration) * 1.7f;
    }
}

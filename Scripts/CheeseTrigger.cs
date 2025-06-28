using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(BoxCollider2D))]
public class GoalTrigger : MonoBehaviour
{
    private bool finished = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (finished) return;
        if (!col.CompareTag("Player")) return;

        finished = true;

        // 1) стопаем таймер
        var timer = FindObjectOfType<TimerUI>();
        if (timer != null)
            timer.StopAndShowCongratulations();

        // 2) возвращаем нормальную освещённость
        //    (мы же сделали Ambient чёрным в URP Lighting → Ambient)
        RenderSettings.ambientLight = Color.white;

        // 3) можно, при желании, заодно отключить сам локальный свет:
        var light2D = col.GetComponentInChildren<Light2D>();
        if (light2D != null)
            light2D.enabled = false;
    }
}

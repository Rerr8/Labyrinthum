using UnityEngine;
using TMPro; // или TMPro, если ты используешь TextMeshPro
using UnityEngine.SceneManagement;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText; // если TextMeshPro → public TextMeshProUGUI timerText;
    public TextMeshProUGUI congratsText;
    [Tooltip("Имя сцены главного меню из Build Settings")]
    public string mainMenuSceneName = "MainMenu";

    private float timeElapsed;
    private bool running = false;

    void Start()
    {
        // Запускаем таймер в тот же кадр, когда стартует лабиринт
        timeElapsed = 0f;
        running = true;

    }

    void Update()
    {
        if (running)
        {
            // Обновляем таймер
            timeElapsed += Time.deltaTime;
            int min = Mathf.FloorToInt(timeElapsed / 60f);
            int sec = Mathf.FloorToInt(timeElapsed % 60f);
            timerText.text = $"{min:00}:{sec:00}";
        }
        else
        {
            // Ждём Enter для возврата в меню
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }

    public void StopAndShowCongratulations()
    {
        running = false;
        if (congratsText == null) return;

        // формируем сообщение
        int min = Mathf.FloorToInt(timeElapsed / 60f);
        int sec = Mathf.FloorToInt(timeElapsed % 60f);
        congratsText.text = $"Congratulations!\n{min:00}:{sec:00}\n\nPress ENTER to return";
        congratsText.gameObject.SetActive(true);
    }
}

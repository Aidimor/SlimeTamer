using UnityEngine;
using SimpleJSON;
using LoLSDK;

public class QuestionHandler : MonoBehaviour
{
  
    private bool isSubscribed = false; // Evita doble suscripción
    private int scorePerQuestion = 1;  // Ajusta según tu juego
    private int maxProgress = 8;       // Total de progresos definidos

    // Inicia el cuestionario
    public void StartStageQuestionary()
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ LOLSDK no inicializado");
            return;
        }

        if (!isSubscribed)
        {
            LOLSDK.Instance.AnswerResultReceived += HandleAnswerResult;
            isSubscribed = true;
        }

        LOLSDK.Instance.ShowQuestion();
    }

    private void HandleAnswerResult(string json)
    {
        var answerResult = JSON.Parse(json);
        string isCorrect = answerResult["isCorrect"];
        ProcessAnswer(isCorrect == "true");
    }

    private void ProcessAnswer(bool isCorrect)
    {
        GameEventsScript.Instance._winRound = true;
        int currentProgress = MainController.Instance._saveLoadValues._progress;
        int currentScore = 0;

        if (isCorrect)
        {
            Debug.Log("✅ Correcto");
            currentProgress++;
            currentScore = scorePerQuestion;
        }
        else
        {
            Debug.Log("❌ Incorrecto");
            currentProgress++;
        }

        // Guardar progreso interno
        MainController.Instance._saveLoadValues._progress = currentProgress;

        // Reportar progreso
        SubmitProgressToLoL(currentProgress, currentScore);


   

        // Desuscribirse
        if (isSubscribed)
        {
            LOLSDK.Instance.AnswerResultReceived -= HandleAnswerResult;
            isSubscribed = false;
        }

        StartCoroutine(MainGameplayScript.Instance.ExitNumerator());
    }

    private void SubmitProgressToLoL(int currentProgress, int score)
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ LOLSDK no inicializado — progreso no enviado.");
            return;
        }

        LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress, score);
        Debug.Log($"📊 Progreso enviado a LoL: {currentProgress}/{maxProgress}, Score: {score}");
    }

    // 🔹 Permite simular una respuesta correcta con la tecla L
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("🧩 Simulación manual: presionaste L (Correcto)");
            ProcessAnswer(true);

        }
    }
}

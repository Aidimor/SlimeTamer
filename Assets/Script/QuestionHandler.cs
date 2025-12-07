using System.Collections;
using UnityEngine;
using SimpleJSON;
using LoL;
using LoLSDK;

public class QuestionHandler : MonoBehaviour
{
    private int scorePerQuestion = 1;
    private int maxProgress = 8;

    public bool IsAnswerProcessed { get; private set; } = false;

    private bool isSubscribed = false;
    private bool isShowingQuestion = false;

    [System.Serializable]
    private class FinalState { public int progress; public int score; }

    private void Start()
    {
        StartCoroutine(WaitForSDKAndSubscribe());
    }

    private IEnumerator WaitForSDKAndSubscribe()
    {
        // Esperar hasta que el SDK exista
        int frameWait = 0;
        while (LOLSDK.Instance == null && frameWait < 300) // timeout ~300 frames (~5s)
        {
            frameWait++;
            yield return null;
        }

        if (LOLSDK.Instance == null)
        {
            Debug.LogError("❌ [Q] Timeout esperando LOLSDK.Instance en Start.");
            yield break;
        }

        if (!isSubscribed)
        {
            LOLSDK.Instance.AnswerResultReceived += HandleAnswerResult;
            isSubscribed = true;
            Debug.Log("✅ [Q] Suscripción a AnswerResultReceived realizada en Start/WaitForSDK.");
        }
    }

    private void OnDestroy()
    {
        if (isSubscribed && LOLSDK.Instance != null)
        {
            LOLSDK.Instance.AnswerResultReceived -= HandleAnswerResult;
            Debug.Log("🗑️ [Q] Desuscripción de AnswerResultReceived realizada en OnDestroy.");
        }
    }

    // Public: iniciar el flujo de pregunta
    public void StartStageQuestionary()
    {
        // No llamamos ShowQuestion hasta estar suscritos (o hasta timeout)
        if (!isSubscribed)
        {
            Debug.Log("⚠️ [Q] StartStageQuestionary llamado pero aún no estamos suscritos. Esperando suscripción...");
            StartCoroutine(ShowQuestionWhenSubscribed(120)); // timeout 120 frames (~2s)
            return;
        }

        ShowQuestionImmediate();
    }

    private IEnumerator ShowQuestionWhenSubscribed(int timeoutFrames)
    {
        int waited = 0;
        while (!isSubscribed && waited < timeoutFrames)
        {
            waited++;
            yield return null;
        }

        if (!isSubscribed)
        {
            Debug.LogError("❌ [Q] No se logró suscribir antes de ShowQuestion (timeout). No se mostrará la pregunta.");
            yield break;
        }

        ShowQuestionImmediate();
    }

    private void ShowQuestionImmediate()
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ [Q] LOLSDK.Instance es null al intentar ShowQuestion.");
            return;
        }

        if (GameInitScript.Instance == null)
        {
            Debug.LogError("❌ [Q] GameInitScript no está inicializado. No se puede mostrar la pregunta.");
            return;
        }

        IsAnswerProcessed = false;
        isShowingQuestion = true;
        GameInitScript.Instance.ShowQuestion();
        Debug.Log("⬆️ [Q] Question mostrada. Esperando respuesta del SDK...");
    }

    private void HandleAnswerResult(string json)
    {
        Debug.Log($"🔥 [Q] HandleAnswerResult INVOCADO con JSON: {json}");

        try
        {
            var answerResult = JSON.Parse(json);
            bool isCorrect = answerResult["correct"]?.AsBool ?? false;
            ProcessAnswer(isCorrect);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [Q] Error parseando JSON en HandleAnswerResult: {e.Message}");
            // Para evitar que el flujo quede congelado
            IsAnswerProcessed = true;
            isShowingQuestion = false;
        }
    }

    private void ProcessAnswer(bool isCorrect)
    {
        Debug.Log($"🔁 [Q] ProcessAnswer comenzando. isCorrect={isCorrect}");

        if (MainController.Instance == null)
        {
            Debug.LogError("❌ [Q] MainController no está inicializado. Desbloqueando flujo.");
            IsAnswerProcessed = true;
            isShowingQuestion = false;
            return;
        }

        if (GameEventsScript.Instance != null)
            GameEventsScript.Instance._winRound = true;

        int currentProgress = MainController.Instance._saveLoadValues._progress;
        int currentScore = 0;

        currentProgress++;
        if (isCorrect) currentScore = scorePerQuestion;

        MainController.Instance._saveLoadValues._progress = currentProgress;
        MainController.Instance.SaveProgress();
        Debug.Log($"💾 [Q] Progreso actualizado internamente: {currentProgress}, score pregunta: {currentScore}");

        // Enviar progreso al SDK
        SubmitProgressToLoL(currentProgress, currentScore);

        // Marcar final state para el harness
        if (GameInitScript.Instance != null)
        {
            var finalState = new FinalState { progress = currentProgress, score = currentScore };
            string finalJson = JsonUtility.ToJson(finalState);

            // Llamar a GameIsComplete ANTES de intentar reanudar el juego
            GameInitScript.Instance.GameIsComplete(finalJson);
            Debug.Log("🎉 [Q] GameIsComplete llamado con: " + finalJson);
        }
        else
        {
            Debug.LogError("❌ [Q] GameInitScript es null al intentar GameIsComplete.");
        }

        // Intentamos reanudar el juego con retries (por si el harness tarda en liberar)
        StartCoroutine(TryExitNumeratorWithRetries(6, 0.15f)); // 6 intentos, 150ms entre ellos

        // Marcar flags
        IsAnswerProcessed = true;
        isShowingQuestion = false;
        Debug.Log("🟢 [Q] ProcessAnswer finalizado. IsAnswerProcessed = true");
    }

    private IEnumerator TryExitNumeratorWithRetries(int attempts, float delaySeconds)
    {
        if (MainGameplayScript.Instance == null)
        {
            Debug.LogWarning("⚠️ [Q] MainGameplayScript.Instance es null — no se podrá llamar ExitNumerator.");
            yield break;
        }

        int attempt = 0;
        while (attempt < attempts)
        {
            attempt++;
            Debug.Log($"🔁 [Q] Intentando ExitNumerator, intento {attempt}/{attempts}...");

            // Llamamos al IEnumerator del gameplay
            // Intentamos envolverlo con try/catch por si lanza excepciones
            bool started = false;
            try
            {
                StartCoroutine(MainGameplayScript.Instance.ExitNumerator());
                started = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ [Q] Exception al StartCoroutine(ExitNumerator): {ex.Message}");
            }

            if (started)
            {
                Debug.Log($"✅ [Q] ExitNumerator llamado (intento {attempt}). Esperando breve confirmación...");
                // Si el ExitNumerator hace algo visible o cambia estado, aquí puedes comprobar condiciones.
                // Esperamos un poco y luego salimos porque lo más probable es que ya se reanudó.
                yield return new WaitForSeconds(delaySeconds);
                yield break;
            }

            yield return new WaitForSeconds(delaySeconds);
        }

        Debug.LogWarning("⚠️ [Q] No se pudo ejecutar ExitNumerator después de varios intentos.");
    }

    private void SubmitProgressToLoL(int currentProgress, int score)
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ [Q] LOLSDK.Instance es null — progreso no enviado.");
            return;
        }

        LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress, score);
        Debug.Log($"📊 [Q] Progreso enviado a LoL: {currentProgress}/{maxProgress}, Score: {score}");
    }
}

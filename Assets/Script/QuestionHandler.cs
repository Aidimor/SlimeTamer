using UnityEngine;
using SimpleJSON;
using LoL;
using LoLSDK;

public class QuestionHandler : MonoBehaviour
{
    private int scorePerQuestion = 1;
    private int maxProgress = 8;

    // Flag para la coroutine de espera.
    public bool IsAnswerProcessed { get; private set; } = false;

    // Usamos Awake para garantizar que la suscripción ocurra tan pronto como el script esté activo.
    private void Awake()
    {
        // 🚨 CRÍTICO: Suscribirse inmediatamente si el SDK existe.
        if (LOLSDK.Instance != null)
        {
            LOLSDK.Instance.AnswerResultReceived += HandleAnswerResult;
            Debug.Log("✅ [Q] Suscripción a AnswerResultReceived realizada en Awake.");
        }
        else
        {
            Debug.LogError("❌ [Q] ERROR: LOLSDK no está disponible en Awake. La suscripción falló.");
        }
    }

    // Usamos OnDestroy para limpiar la suscripción cuando el objeto es destruido, previniendo fugas de memoria.
    private void OnDestroy()
    {
        if (LOLSDK.Instance != null)
        {
            LOLSDK.Instance.AnswerResultReceived -= HandleAnswerResult;
            Debug.Log("🗑️ [Q] Desuscripción de AnswerResultReceived realizada en OnDestroy.");
        }
    }

    // Inicia el cuestionario
    public void StartStageQuestionary()
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ [Q] LOLSDK no inicializado. No se puede mostrar la pregunta.");
            return;
        }

        if (GameInitScript.Instance == null)
        {
            Debug.LogError("❌ [Q] GameInitScript no está inicializado. No se puede mostrar la pregunta.");
            return;
        }

        // 1. Resetear el flag antes de mostrar la pregunta
        IsAnswerProcessed = false;

        // Llamamos a la función que usa LOLSDK.Instance.ShowQuestion()
        GameInitScript.Instance.ShowQuestion();
        Debug.Log("⬆️ [Q] QuestionHandler: Pregunta mostrada. Esperando HandleAnswerResult...");
    }

    private void HandleAnswerResult(string json)
    {
        // 🚨 CRÍTICO: Este Log debería aparecer SÍ O SÍ si el SDK dispara el evento.
        Debug.Log($"🔥 [Q] HandleAnswerResult FIRED con JSON: {json}");

        try
        {
            var answerResult = JSON.Parse(json);
            bool isCorrect = answerResult["correct"]?.AsBool ?? false;
            ProcessAnswer(isCorrect);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [Q] Error al parsear JSON en HandleAnswerResult: {e.Message}");
            // Asegúrate de desbloquear el flujo incluso si falla el JSON para evitar el congelamiento.
            IsAnswerProcessed = true;
        }
    }

    /// <summary>
    /// Procesa la respuesta, actualiza el estado del juego, notifica al SDK y desbloquea el flujo del juego.
    /// </summary>
    private void ProcessAnswer(bool isCorrect)
    {
        if (MainController.Instance == null)
        {
            Debug.LogError("❌ [Q] MainController no está inicializado. No se puede procesar la respuesta.");
            IsAnswerProcessed = true; // Desbloquear el flujo principal.
            return;
        }

        // LÓGICA DEL USUARIO: Marcar la ronda como ganada
        if (GameEventsScript.Instance != null)
        {
            GameEventsScript.Instance._winRound = true;
        }

        int currentProgress = MainController.Instance._saveLoadValues._progress;
        int currentScore = 0;

        if (isCorrect)
        {
            Debug.Log("✅ [Q] Respuesta Correcta");
            currentProgress++;
            currentScore = scorePerQuestion;
        }
        else
        {
            Debug.Log("❌ [Q] Respuesta Incorrecta");
            currentProgress++;
        }

        // Guardar progreso interno
        MainController.Instance._saveLoadValues._progress = currentProgress;

        // 1. Guardar estado completo en el SDK
        MainController.Instance.SaveProgress();
        Debug.Log("💾 [Q] Progreso guardado internamente.");

        // 2. Reportar progreso y score al SDK
        SubmitProgressToLoL(currentProgress, currentScore);

        // 3. Notificar al harness de LoL que la actividad ha finalizado (CRUCIAL para desbloquear).
        if (GameInitScript.Instance != null)
        {
            // Nota: El score debe ser el acumulado si estás usando un sistema de puntos, 
            // o solo el de la pregunta si es por progreso. Usaremos el de la pregunta para este ejemplo.
            string finalStateJson = JsonUtility.ToJson(new { progress = currentProgress, score = currentScore });

            // 💡 LLAMADA AL SDK PARA LIBERAR EL HARNESS Y EVITAR CONGELAMIENTO 💡
            GameInitScript.Instance.GameIsComplete(finalStateJson);
            Debug.Log("🎉 [Q] GameIsComplete llamado. El harness debería estar desbloqueado.");
        }
        else
        {
            Debug.LogError("❌ [Q] GameInitScript no disponible para llamar a GameIsComplete.");
        }

        // 4. LÓGICA DEL USUARIO: Iniciar el Coroutine de salida del UI (Ahora que el harness está desbloqueado).
        if (MainGameplayScript.Instance != null)
        {
            StartCoroutine(MainGameplayScript.Instance.ExitNumerator());
            Debug.Log("🏃 [Q] ProcessAnswer: Llamando a ExitNumerator para reanudar el juego.");
        }

        // 5. Marcar el flag para que el GameFlowManager (yield return new WaitUntil...) continúe.
        IsAnswerProcessed = true;
        Debug.Log("🟢 [Q] QuestionHandler: Respuesta procesada. IsAnswerProcessed = true. FIN DEL FLUJO.");
    }

    private void SubmitProgressToLoL(int currentProgress, int score)
    {
        if (LOLSDK.Instance == null)
        {
            Debug.LogWarning("⚠️ [Q] LOLSDK no inicializado — progreso no enviado.");
            return;
        }

        LOLSDK.Instance.SubmitProgress(currentProgress, maxProgress, score);
        Debug.Log($"📊 [Q] Progreso enviado a LoL: {currentProgress}/{maxProgress}, Score: {score}");
    }
}
using UnityEngine;
using SimpleJSON;
using LoLSDK;

public class QuestionHandler : MonoBehaviour
{
    [SerializeField] private MainGameplayScript _scriptMain;

    // Esta función será llamada desde JS cuando la respuesta llegue
    private void HandleAnswerResult(string json)
    {
        var answerResult = SimpleJSON.JSON.Parse(json);
        string isCorrect = answerResult["isCorrect"];

        // Procesa la respuesta de inmediato
        if (isCorrect == "true") Debug.Log("✅ Correcto");
        else if (isCorrect == "false") Debug.Log("❌ Incorrecto");
        else Debug.Log("⚪ No respondió / cerró overlay");

        // Continúa el juego directamente desde aquí
        if (_scriptMain._scriptEvents._currentEventPrefab != null)
            Destroy(_scriptMain._scriptEvents._currentEventPrefab);

        _scriptMain._scriptMain._bordersAnimator.SetBool("BorderOut", false);

        // En vez de corutinas que dependen del canvas, llama un método que avance el flujo:
        _scriptMain._scriptEvents.AdvanceStage();

        // Remueve el delegado
        LOLSDK.Instance.AnswerResultReceived -= HandleAnswerResult;
    }



    public void StartStageQuestionary()
    {
        // Llama a LOLSDK para mostrar la pregunta
        LOLSDK.Instance.ShowQuestion();
    }
}

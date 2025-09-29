using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntroEventScript : MonoBehaviour
{
    [Header("Referencias principales")]
    public MainGameplayScript _scriptMain;
    public GameObject[] _worlds;



    private Queue<string> sentences; // Cola de frases
    private bool isTyping = false;   // Para controlar si está escribiendo
    private string currentSentence;  // Frase actual
    private Coroutine typingCoroutine;

    [System.Serializable]
    public class WorldIntroAssets
    {
        [System.Serializable]
        public class DialogeAssets
        {
            // Sigue siendo string (un solo bloque con saltos de línea)
            [TextArea(3, 10)]
            public string _dialoges;

            public enum PrincessPose
            {
                Nothing,
                Pose1,
                Pose2,
                Pose3
            }
            public PrincessPose _princessPose;
        }

        public DialogeAssets[] _dialogeAssets;
        public int _onDialoge;
    }

    [Header("Assets de mundos")]
    public WorldIntroAssets[] _worldIntroAssets;

    private void Start()
    {
        sentences = new Queue<string>();
       //_scriptMain._scriptEvents._centerDialogeAssets._parent.SetActive(false);
    }

    private void Update()
    {
        // Avanza con la barra espaciadora
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    DisplayNextSentence();
        //}
    }

    public void StartIntroVoid()
    {
        // Coloca el slime en su posición inicial
        _scriptMain._scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition =
            _scriptMain._scriptFusion._slimeStartPos[0];

        //// Activa el mundo actual
        //_worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);

        // Obtiene el bloque de diálogo correcto
        //var dialogeBlock = _worldIntroAssets[_scriptMain._scriptMain._onWorldGlobal]
        //    ._dialogeAssets[_worldIntroAssets[_scriptMain._scriptMain._onWorldGlobal]._onDialoge];

        // Convierte el string en array de frases (separadas por saltos de línea)
        //StartDialogue(dialogeBlock._dialoges.Split('\n'));
    }

    public void ExitIntroVoid()
    {
        // Coloca el slime en su posición inicial
        _scriptMain._scriptFusion._slimeRenderer.GetComponent<RectTransform>().anchoredPosition =
            _scriptMain._scriptFusion._slimeStartPos[1];

        //// Activa el mundo actual
        //_worlds[_scriptMain._scriptMain._onWorldGlobal].SetActive(true);

        // Obtiene el bloque de diálogo correcto
        //var dialogeBlock = _worldIntroAssets[_scriptMain._scriptMain._onWorldGlobal]
        //    ._dialogeAssets[_worldIntroAssets[_scriptMain._scriptMain._onWorldGlobal]._onDialoge];

        // Convierte el string en array de frases (separadas por saltos de línea)
        //StartDialogue(dialogeBlock._dialoges.Split('\n'));
    }

    public void StartDialogue(string[] dialogueLines)
    {
        _scriptMain._scriptEvents._centerDialogeAssets._parent.SetActive(true);
        sentences.Clear();

        foreach (string line in dialogueLines)
        {
            // Quita espacios innecesarios
            if (!string.IsNullOrWhiteSpace(line))
                sentences.Enqueue(line.Trim());
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Si ya está escribiendo, salta al final de la frase
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            _scriptMain._scriptEvents._centerDialogeAssets._dialogeText.text = currentSentence;
            isTyping = false;
            return;
        }

        // Si ya no hay más frases, termina el diálogo
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Obtener la siguiente frase
        currentSentence = sentences.Dequeue();

        // Iniciar la escritura
        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        _scriptMain._scriptEvents._centerDialogeAssets._dialogeText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            _scriptMain._scriptEvents._centerDialogeAssets._dialogeText.text += letter;
            yield return new WaitForSeconds(_scriptMain._scriptEvents._centerDialogeAssets.typingSpeed);
        }
        isTyping = false;
    }

    void EndDialogue()
    {
        //_scriptMain.dialoguePanel.SetActive(false);
        Debug.Log("Diálogo terminado");
    }


}

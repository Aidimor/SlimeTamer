using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WaterFillEvent : MonoBehaviour
{
    public MainGameplayScript _scriptMain;
    public bool _fillBool;
    public Image _water;
    public GameObject[] _worlds;
    // Start is called before the first frame update
    void Start()
    {
        //_scriptMain = GameObject.Find(("CanvasIndestructible/MAIN/MainController")).gameObject.GetComponent<MainController>();
        //_worlds[_scriptMain._scriptMain._onWorldGlobal].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_fillBool)
        {
            case false:
                _water.transform.localPosition = Vector2.Lerp(_water.transform.localPosition, new Vector2(_water.transform.localPosition.x, -100), 2 * Time.deltaTime);
                break;
            case true:
                _water.transform.localPosition = Vector2.Lerp(_water.transform.localPosition, new Vector2(_water.transform.localPosition.x, -50), 0.5f * Time.deltaTime);
                break;
        }
    }
}

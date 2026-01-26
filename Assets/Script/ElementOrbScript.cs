using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementOrbScript : MonoBehaviour
{
    public int ID;
    public int _onPose;
    public Image _circle;
    public Image _sprite;
    public Sprite[] _allIcons;
    public TextMeshProUGUI _letter;
    public TextMeshProUGUI _quantityText;
    public int _quantity;
    public Color[] _allColors;
    // Start is called before the first frame update
 


    public void ElementSetVoid()
    {
        //_sprite.sprite = _allIcons[ID];
        _circle.color = _allColors[ID];
        _letter.color = _allColors[ID];
        if(_quantity > 0)
        {
            _quantityText.text = _quantity.ToString();
        }
        else
        {
            _quantityText.text = "";
        }

        switch (ID)
        {
            case 0:
                _letter.text = "C";           
                break;
            case 1:
                _letter.text = "H";
                break;
            case 2:
                _letter.text = "O";
                break;
            case 3:
                _letter.text = "Fe";
                break;
        }
    }
}

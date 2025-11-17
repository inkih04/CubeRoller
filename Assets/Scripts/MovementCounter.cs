
using TMPro;
using UnityEngine;

public class MovementCounter : MonoBehaviour {
    public TextMeshProUGUI movementsText;
    private int movements = 0;

    void Start()
    {
        UpdateText();
    }

    public void registerMove() {
        movements++;
        UpdateText();
    
    }

    public void UpdateText()
    {
        movementsText.text = "Saltos: " + movements.ToString();
    }






}
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestResultUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField scoreField;
    [SerializeField] private TMP_InputField comboField;
    [SerializeField] private Button playButton;
    [SerializeField] private ResultUIView resultUIView;

    void Start()
    {
        playButton.onClick.AddListener(async () => {
            playButton.interactable = false;
            await resultUIView.PlayResultUI(int.Parse(scoreField.text), int.Parse(comboField.text));
            playButton.interactable = true;
        }
        );
    }

    void Update()
    {
        
    }
}

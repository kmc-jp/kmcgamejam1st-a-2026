using UnityEngine;
using UnityEngine.UI;

public class TestGameUI : MonoBehaviour
{
    [SerializeField] private Button addScoreButton;
    [SerializeField] private Button resetComboButton;
    [SerializeField] private GameUIView gameUIView;
    private int combo = 0;
    private int score = 0;
    void Start()
    {
        addScoreButton.onClick.AddListener(
            () =>
            {
                int scoreAddition = 100 + 10 * combo;
                score += scoreAddition;
                gameUIView.Test_AddScore(score);
                gameUIView.ShowScoreAddition(scoreAddition);
                gameUIView.OnComboUpdated(++combo);
            }
            );
        resetComboButton.onClick.AddListener(
            () =>
            {
                combo = 0;
                gameUIView.OnComboUpdated(0);
            }
            );
    }

    void Update()
    {
        
    }
}

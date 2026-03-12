using TMPro;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;

public class ResultUIView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreValueText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI comboValueText;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public async UniTask PlayResultUI(int score, int combo)
    {
        // テキスト一式を見えなくする
        scoreText.DOFade(0f, 0f);
        scoreText.rectTransform.localPosition = new Vector3(
            -1020f,
            scoreText.rectTransform.localPosition.y,
            scoreText.rectTransform.localPosition.z);
        scoreValueText.DOFade(0f, 0f);
        comboText.DOFade(0f, 0f);
        comboText.rectTransform.localPosition = new Vector3(
            -1020f,
            comboText.rectTransform.localPosition.y,
            comboText.rectTransform.localPosition.z);
        comboValueText.DOFade(0f, 0f);

        // 1. "Score"テキストをツイーン
        var scoreTextSequence = DOTween.Sequence()
            .Append(scoreText.rectTransform.DOAnchorPosX(-240f, 1f))
            .Join(scoreText.DOFade(1f, 1f));
        await scoreTextSequence.AsyncWaitForCompletion();

        // 2. スコア表示
        var scoreValueTextSequence = DOTween.Sequence().
            Append(DOVirtual.Int(0, score, 2f, v => { scoreValueText.text = $"{v:N0}"; }))
            .Join(scoreValueText.DOFade(1f, 1f));
        await scoreValueTextSequence.AsyncWaitForCompletion();

        // 3. "Combo"テキストをツイーン
        var comboTextSequence = DOTween.Sequence()
            .Append(comboText.rectTransform.DOAnchorPosX(-240f, 1f))
            .Join(comboText.DOFade(1f, 1f));
        await comboTextSequence.AsyncWaitForCompletion();

        // 4. コンボ数表示
        var comboValueTextSequence = DOTween.Sequence().
            Append(DOVirtual.Int(0, combo, 2f, v => { comboValueText.text = $"{v:N0}"; }))
            .Join(comboValueText.DOFade(1f, 1f));
        await comboValueTextSequence.AsyncWaitForCompletion();
    }
}

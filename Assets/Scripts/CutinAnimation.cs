using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;
public class CutinAnimation : MonoBehaviour
{
    [SerializeField] private Image backRibbon;
    [SerializeField] private Image parallelogram; // カットインの平行四辺形
    [SerializeField] private Image cutinImage;
    [SerializeField] private Sprite[] cutinSprites;
    void Start()
    {
        transform.localScale = new Vector3(1f, 0f, 1f); // 見えないようにする
    }

    public async UniTask PlayCutin()
    {
        parallelogram.rectTransform.localPosition = new Vector3(1920f, 0f, 0f);
        cutinImage.sprite = cutinSprites[Random.Range(0, cutinSprites.Length)]; // カットイン画像をランダム決定
        var cutinSequence = DOTween.Sequence();
        cutinSequence
            .Append(transform.DOScaleY(1f, 0.3f)).SetEase(Ease.OutCubic) // 1. 全体を縦に開く
            .Join(parallelogram.rectTransform.DOLocalMoveX(240f, 0.3f))
            .Append(parallelogram.rectTransform.DOLocalMoveX(-240f, 2f).SetEase(Ease.OutQuad))
            .Append(transform.DOScaleY(0f, 0.3f)).SetEase(Ease.OutCubic)
            .Join(parallelogram.rectTransform.DOLocalMoveX(-1920f, 0.3f));
        // 下準備ここまで
        await cutinSequence.AsyncWaitForCompletion();
    }

    public void PlayCutinSync()
    {
        PlayCutin().Forget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

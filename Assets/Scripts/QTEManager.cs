using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using System.Collections.Generic;
enum QTEInputType
{
    Up,
    Down,
    Left,
    Right,
}
public struct QTEAction
{
    public List<bool[]> inputPatterns; // 入力パターンのリスト
    public float timeLimit; // 制限時間
    private int progress; // 現在の入力の進行状況を追跡

    public QTEAction(int difficluty, float defaultTimeLimit)
    {
        // 難易度に応じて入力パターンと制限時間を設定
        inputPatterns = new List<bool[]>();
        // 難易度i: 合計i回の入力を要求する
        // 難易度が高いほど同時に2つ押ししないといけないパターンが増える
        for (int i = 0; i <= difficluty; i++)
        {
            // 同時に2つ押さなければならない確率: 0.75 * (1 - 0.9^difficulty)
            if (Random.value < 0.75f * (1 - Mathf.Pow(0.9f, difficluty)))
            {
                // 同時に2つ押すパターン
                bool[] pattern = new bool[4];
                int first = Random.Range(0, 4);
                int second;
                do
                {
                    second = Random.Range(0, 4);
                } while (second == first);
                pattern[first] = true;
                pattern[second] = true;
                inputPatterns.Add(pattern);
            }
            else
            {
                // 単純に1つ押すパターン
                bool[] pattern = new bool[4];
                int index = Random.Range(0, 4);
                pattern[index] = true;
                inputPatterns.Add(pattern);
            }
        }
        // 入力1つあたりの制限時間は徐々に短くなるが、全体の制限時間は少しずつ長くなっていく
        timeLimit = defaultTimeLimit * (1f + Mathf.Pow(difficluty / 10f, 0.8f));
        progress = 0;
    }
    /// <summary>
    /// 現在の入力が求められている入力に一致しているかをチェックし、進行状況を更新する
    /// </summary> <param name="currentInput">現在の入力状態を表すbool配列（Up, Down, Left, Rightの順）</param>
    public int CheckInput(bool[] currentInput)
    {
        // 現在の入力が求められている入力に一致すればprogressを進める
        // 間違えた入力の場合には進捗をリセットするが、同時に複数押しをする場合に一部しか押してなかったとしても進捗をリセットしないようにする
        bool[] requiredInput = inputPatterns[progress];
        for (int i = 0; i < 4; i++)
        {
            if (requiredInput[i] && !currentInput[i])
            {
                // 必要な入力が押されていない場合は不正解
                return progress; // 不正解
            }
            if (!requiredInput[i] && currentInput[i])
            {
                // 不要な入力が押されている場合は不正解
                progress = 0;
                return progress; // 不正解
            }
        }
        progress++;
        return progress; // 正解
    }
    public bool IsCompleted()
    {
        return progress >= inputPatterns.Count;
    }
}

// ゲームの進行の流れ
// 1. ゲーム開始前: 目覚まし時計が鳴るまでの時間をランダムに設定
// 2. ゲーム開始後: 目覚ましが鳴り始めるまでの時間をカウントダウン
// 3. 目覚まし時計が鳴り始めてからfirstInputActionを受け取れるようにし、QTEによるゲームが開始
// (ここからQTEManagerがロジックの責任を持つ部分)
// 4. 受け付けるべきQTE入力をランダムに、限度時間を回を重ねるごとに短く設定する
// 5. プレイヤーが正しい入力をした場合、コンボカウントを増やし、次のQTEに進む
// 6. 時間切れになった場合、ゲームオーバー

class QTEManager: MonoBehaviour
{
    [SerializeField] private float defaultQTETimeLimit = 2.0f; // デフォルトのQTE時間制限（秒）
    private int countOfQTEs = 0; // これまでに出現したQTEの数
    private int comboCount = 0;
    private float qteTimeLimit = 0.5f; // QTEの時間制限（秒）
    private QTEAction currentQTEAction; // 現在のQTEアクション

    // ゲームの最初の入力を検出するためのInputAction
    [SerializeField] private InputAction upInputAction;
    [SerializeField] private InputAction downInputAction;
    [SerializeField] private InputAction leftInputAction;
    [SerializeField] private InputAction rightInputAction;

    // UIなどに伝達するためのイベント　R3を使用
    [SerializeField] private QTEPrompt qTEPrompt;
    public Subject<float> onTimeLimitReset = new(); // 制限時間がリフレッシュされたとき
    public Subject<float> onTimeLimitTick = new(); // 制限時間の更新時


	[SerializeField] GameManager GameManager;

	public void Reset()
	{
        comboCount = 0;
	}

	void OnEnable()
    {
        upInputAction?.Enable();
        downInputAction?.Enable();
        leftInputAction?.Enable();
        rightInputAction?.Enable();
        SetNextQTEAction(); // 最初のQTEアクションを設定
	}

	void OnDisable()
    {
        upInputAction?.Disable();
        downInputAction?.Disable();
        leftInputAction?.Disable();
        rightInputAction?.Disable();
    }

    void Update()
    {
        if (qteTimeLimit > 0)
        {
            qteTimeLimit -= Time.deltaTime;
            onTimeLimitTick.OnNext(qteTimeLimit);
            if (qteTimeLimit <= 0)
            {
                // 時間切れの処理
                Debug.Log("時間切れ！ゲームオーバー");
                // ゲームオーバーのロジックをここに追加
                Debug.Log($"最終コンボ数: {comboCount}");
                GameManager.QTEEnded(comboCount);
            }
        }
        bool[] currentInput = new bool[4] {
            upInputAction != null && upInputAction.IsPressed(),
            downInputAction != null && downInputAction.IsPressed(),
            leftInputAction != null && leftInputAction.IsPressed(),
            rightInputAction != null && rightInputAction.IsPressed()
        };
        int progress = currentQTEAction.CheckInput(currentInput);
        qTEPrompt.UpdateSlotBackColor(progress);
        if (currentQTEAction.IsCompleted())
        {
            countOfQTEs++;
            Debug.Log($"QTE成功！コンボ数: {comboCount + 1}");
            GameManager.AddScore(100 + comboCount * 10); // スコア加算
            SetNextQTEAction();
        }
    }

    private void SetNextQTEAction()
    {
        countOfQTEs++;
        // ランダムに次のQTEアクションを設定
        currentQTEAction = new QTEAction(countOfQTEs / 5, defaultQTETimeLimit);
        // コンボ数に応じて時間制限を短くする
        qteTimeLimit = currentQTEAction.timeLimit;
        qTEPrompt.Setup(currentQTEAction);
        Debug.Log($"次のQTEアクション: {currentQTEAction}, 時間制限: {qteTimeLimit}");
        onTimeLimitReset.OnNext(qteTimeLimit);
        onTimeLimitTick.OnNext(qteTimeLimit);
    }
}
using UnityEngine;
// ↓シーン遷移を行うために必ず追加します
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // ゲーム画面へ遷移するメソッド
    public void GoToGame()
    {
        SceneManager.LoadScene("gameSceneProto"); // 括弧内は作成したシーン名と完全に一致させます
    }

    // リザルト画面へ遷移するメソッド
    public void GoToResult()
    {
        SceneManager.LoadScene("ResultScene");
    }

    // スタート画面へ遷移するメソッド
    public void GoToStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}
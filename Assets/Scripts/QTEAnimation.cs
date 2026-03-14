using UnityEngine;

class QTEAnimation: MonoBehaviour
{
    [SerializeField] private QTEManager qteManager;
    // アニメーションイベントから呼び出される関数
    public void OnAnimationComplete()
    {
        Debug.Log("アニメーション完了");
        qteManager.OnAnimationComplete();
    }
}
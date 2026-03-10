using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BtnKariUI : MonoBehaviour
{
    [SerializeField] GameManager GameManager;
    [SerializeField] Button ButtonStart;

    public void OnBtnStart()
    {
        CallGameStart();
	}


	public async UniTask CallGameStart()
    {
        if(ButtonStart != null) ButtonStart.interactable = false;
        await GameManager.GameStart();
		if(ButtonStart != null) ButtonStart.interactable = true;
    }
}

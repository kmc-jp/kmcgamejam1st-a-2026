using UnityEngine;
using R3;

public class BGMManager : MonoBehaviour
{
    [SerializeField] AudioClip BGM;
    [SerializeField] AudioClip ResultBGM;
    [SerializeField] GameManager gameManager;
    AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = BGM;
        audioSource.Play();

        gameManager.State.Subscribe(state => {
            if (state == GameState.Final)
            {
                audioSource.clip = ResultBGM;
                audioSource.loop = false;
                audioSource.Play();
            }
        }).AddTo(this);
    }
}

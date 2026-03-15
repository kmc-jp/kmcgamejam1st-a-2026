using UnityEngine;

public class BGMManager : MonoBehaviour
{
    AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
    }
}

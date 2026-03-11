using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicCon : MonoBehaviour
{
    AudioClip Audio;
    bool IsBGM;
    float Volume;

    [HideInInspector] public AudioSource AudioSource;
    [HideInInspector] public Action<MusicCon> OnInspectorAction;

	private void Awake()
	{
        AudioSource.resource = Audio;
        AudioSource.volume = Volume;
	}

	//音量調整
	public void Play()
    {
        if(IsBGM)
        {
            AudioSource.Play();
        }
        else
        {
            AudioSource.PlayOneShot(Audio);
        }
    }
    public void Stop()
        => AudioSource.Stop();

#if UNITY_EDITOR
    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(MusicCon))]
    public class MusicConEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // target は処理コードのインスタンスだよ！ 処理コードの型でキャストして使ってね！
            MusicCon Con = target as MusicCon;

            EditorGUILayout.BeginHorizontal();
            AudioClip audio = EditorGUILayout.ObjectField("Audio", null, typeof(AudioClip), true) as AudioClip;
            bool BGM = EditorGUILayout.Toggle("IsBGM", false);
            float volume = (float)EditorGUILayout.IntSlider("volume", 100, 0, 100) / 100;
            if(GUILayout.Button("削除")) Con.OnInspectorAction(Con);
            EditorGUILayout.EndHorizontal();
            Con.Audio = audio;
            Con.IsBGM = BGM;
            Con.Volume = volume;
        }
    }
#endif
}

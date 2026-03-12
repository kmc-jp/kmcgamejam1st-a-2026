using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using System;
using TMPro;
using R3;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class MusicCon : IDisposable
{
    [SerializeField] public string Name = "新しい曲";
    [SerializeField] AudioClip Audio;
    [SerializeField] bool IsBGM;
    [SerializeField, Range(0, 1)] float Volume = 1;

    [HideInInspector] public GameObject AudioSourceProvider;
    [HideInInspector] public AudioSource AudioSource;
    [SerializeField, HideInInspector] public bool Delete = false;

    public MusicCon(AudioSource AS)
    {
        AudioSource = AS;
        AudioSource.resource = Audio;
        AudioSource.volume = Volume;
        Volume = 1;
    }

    //音量調整
    public void Play(float Volume = 1)
    {
        AudioSource.loop = IsBGM;
        if(IsBGM)
        {
            AudioSource.Play();
        }
        else
        {
            AudioSource.PlayOneShot(Audio, Volume * this.Volume);
        }
    }
	public void Stop()
        => AudioSource.Stop();

	static void NullAndFakeNullCheck(object obj)
        => Debug.Log($"Null : {obj is null}, Fake Null : {obj == null}");

    public void Dispose()
    {
        Stop();
        if(!(AudioSourceProvider?.IsDestroyed() ?? true)) GameObject.Destroy(AudioSourceProvider);
    }

#if UNITY_EDITOR
    /// <summary>
    /// カスタムプロパティドローワー
    /// </summary>
    [CustomPropertyDrawer(typeof(MusicCon))]
    public class MusicConDrawer : PropertyDrawer
    {
        // 1行の高さ（標準は 18px 程度）
        private const float LineHeight = 18;
        private const float Spacing = 2;

        // 描画に必要な高さをUnityに伝える
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => (LineHeight * 2) + Spacing;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // インスペクタ
            // Name [srting] Audio [AudioClip] IsBGM
            // Volume ===・---- ☑ [[削除]]
            label = EditorGUI.BeginProperty(position, label, property);

            float fullwidth = position.width;
      
            Rect namelabelRect   = new Rect(position.x,                    position.y,                        fullwidth * 0.1f - Spacing, LineHeight);
            Rect nameRect        = new Rect(position.x + fullwidth * 0.1f, position.y,                        fullwidth * 0.2f - Spacing, LineHeight);
            Rect audiolabelRect  = new Rect(position.x + fullwidth * 0.3f, position.y,                        fullwidth * 0.1f - Spacing, LineHeight);
            Rect audioRect       = new Rect(position.x + fullwidth * 0.4f, position.y,                        fullwidth * 0.3f - Spacing, LineHeight);
            Rect isBGMlabelRect  = new Rect(position.x + fullwidth * 0.7f, position.y,                        fullwidth * 0.2f - Spacing, LineHeight);
            Rect isBGMRect       = new Rect(position.x + fullwidth * 0.9f, position.y,				    	  fullwidth * 0.1f,           LineHeight);
            Rect volumelabelRect = new Rect(position.x,                    position.y + LineHeight + Spacing, fullwidth * 0.1f - Spacing, LineHeight);
            Rect volumeRect      = new Rect(position.x + fullwidth * 0.1f, position.y + LineHeight + Spacing, fullwidth * 0.6f - Spacing, LineHeight);
            Rect deleteRect      = new Rect(position.x + fullwidth * 0.7f, position.y + LineHeight + Spacing, fullwidth * 0.3f - Spacing, LineHeight);

            SerializedProperty nameProp = property.FindPropertyRelative(nameof(MusicCon.Name));
            SerializedProperty audioProp = property.FindPropertyRelative(nameof(MusicCon.Audio));
            SerializedProperty isBGMProp = property.FindPropertyRelative(nameof(MusicCon.IsBGM));
            SerializedProperty volumeProp = property.FindPropertyRelative(nameof(MusicCon.Volume));
            SerializedProperty deleteProp = property.FindPropertyRelative(nameof(MusicCon.Delete));

			EditorGUI.LabelField(namelabelRect, new GUIContent(nameof(MusicCon.Name)));
			EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
			EditorGUI.LabelField(audiolabelRect, new GUIContent(nameof(MusicCon.Audio)));
			EditorGUI.PropertyField(audioRect, audioProp, GUIContent.none);
			EditorGUI.LabelField(isBGMlabelRect, new GUIContent(nameof(MusicCon.IsBGM)));
            EditorGUI.PropertyField(isBGMRect, isBGMProp, GUIContent.none);
            EditorGUI.LabelField(volumelabelRect, new GUIContent(nameof(MusicCon.Volume)));
            EditorGUI.PropertyField(volumeRect, volumeProp, GUIContent.none);
            if(GUI.Button(deleteRect, "削除")) deleteProp.boolValue = true;

            EditorGUI.EndProperty();
        }
    }

    /// <summary>
    /// カスタムインスペクター
    /// </summary>
    [CustomEditor(typeof(MusicCon))]
    public class MusicConEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // target は処理コードのインスタンスだよ！ 処理コードの型でキャストして使ってね！
            //MusicCon Con = target as MusicCon;

            //EditorGUILayout.BeginHorizontal();
            //AudioClip audio = EditorGUILayout.ObjectField("Audio", null, typeof(AudioClip), true) as AudioClip;
            //bool BGM = EditorGUILayout.Toggle("IsBGM", false);
            //float volume = (float)EditorGUILayout.IntSlider("volume", 100, 0, 100) / 100;
            //if(GUILayout.Button("削除")) Con.OnInspectorDelete(Con);
            //EditorGUILayout.EndHorizontal();
            //Con.Audio = audio;
            //Con.IsBGM = BGM;
            //Con.Volume = volume;
        }
    }
#endif
}

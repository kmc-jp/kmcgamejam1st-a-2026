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

    [NonSerialized] GameObject AudioSourceProvider;
    [NonSerialized] AudioSource Source;
    [SerializeField, HideInInspector] public bool Delete = false;

    public MusicCon(AudioSource AS, GameObject AudioSourceProvider)
    {
        Source = AS;
        this.AudioSourceProvider = AudioSourceProvider;
        Source.resource = Audio;
        Source.volume = Volume;
        Volume = 1;
    }

    //音量調整
    public void Play(float Volume = 1)
    {
        try
        {
			Source.loop = IsBGM;
            if(IsBGM)
            {
                Source.Play();
            }
            else
            {
                Source.PlayOneShot(Audio, Volume * this.Volume);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            throw;
        }
    }
	public void Stop()
        => Source?.Stop();

	static void NullAndFakeNullCheck(object obj)
        => Debug.Log($"Null : {obj is null}, Fake Null : {obj == null}");

    public void Dispose()
    {
        Stop();
        //Debug.Log("Dispose");
        if(AudioSourceProvider != null)
		{
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
			{
				//Debug.Log("delay call!");
				if (AudioSourceProvider != null)
				{
                    var ASP = AudioSourceProvider;
                    AudioSourceProvider = null;
					//Selection.activeObject = null;
                    //Debug.Log("Undo.DestroyObjectImmediate!!!!");
					Undo.DestroyObjectImmediate(ASP);
					EditorApplication.RepaintHierarchyWindow();
				}
                else
                {
                    //Debug.Log("AudioSourceProvider is null or fake!");
                }
			};
#else
            if (AudioSourceProvider != null && !AudioSourceProvider.IsDestroyed()) GameObject.Destroy(AudioSourceProvider);
            AudioSourceProvider = null;
#endif
        }
        else
        {
            //Debug.Log("AudioSourceProvider is null in outer");
        }
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
#endif
}

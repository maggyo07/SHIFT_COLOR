using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioBGMSystemScript : MonoBehaviour
{
    //有無のチェックフラグ
    private static bool created = false;
    private string un_loade_scene_name;     //破棄されたときのシーンの名前(破棄されるごとに上書き)
    private bool audio_down_flag = false;  //ボリュームを徐々に下げるかどうか
    public float audio_volume_down_speed = 0.1f;   //ボリュームを徐々に下げる時のスピード
    private AudioSource audio_source;     //AudioSystemScriptを所有しているオブジェクトのAudioSource
    public AudioClip mode_audio;         //メインとタイトルシーン以外の時のサウンドデータ
    public AudioClip battle_audio;       //メインシーン時のサウンド

    // Use this for initialization
    void Start()
    {
        if (!created)
        {
            DontDestroyOnLoad(gameObject);
            created = true;
            //AudioSource情報を取得
            audio_source = GetComponent<AudioSource>();
            //シーンが変更されたときに呼ぶために関数をセットする
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            //シーンが破棄されたときに呼ぶために関数をセットする
            SceneManager.sceneUnloaded += OnSceneUnLoade;
        }
        else
            Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //ボリュームを下げるフラグがONの時、ボリュームを下げる
        if (audio_down_flag)
        {
            if (audio_source.volume > 0)
                audio_source.volume -= audio_volume_down_speed;
            else
                audio_down_flag = false;//これ以上下げても意味がないのでフラグをOFFにする　
        }
    }

    //シーンが変更されたときに呼ばれる関数
    //prev_sceneは使えません
    void OnActiveSceneChanged(Scene prev_scene, Scene next_scene)
    {
        //タイトルシーンに変更されたら音楽を停止
        if (next_scene.name == "TitleScene")
            audio_source.Stop();

        //メニュシーンに変更されたら専用音楽を再生
        if (next_scene.name == "MenuScene")
        {
            //タイトルかメインシーンからメニューシーンに変更されていたら
            if (un_loade_scene_name == "TitleScene" ||
               un_loade_scene_name == "MainScene")
            {

                audio_source.Stop();
                audio_source.clip = mode_audio;
                audio_source.Play();
            }
        }

        //メインシーンに変更されたら専用音楽を再生
        if (next_scene.name == "MainScene")
        {
            //ボリュームを戻す
            audio_source.volume = 1.0f;
            audio_source.Stop();
            audio_source.clip = battle_audio;
            audio_source.Play();
        }
    }

    //シーンが破棄されたときによがれる関数
    void OnSceneUnLoade(Scene scene)
    {
        un_loade_scene_name = scene.name;
    }

    //ボリュームを徐々に下げるフラグをONにする
    public void MenuAudioVolumeDown()
    {
        audio_down_flag = true;
    }
}

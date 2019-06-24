using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioSESystemScript : MonoBehaviour
{
    private static bool created = false;

    public AudioClip normal_SE;  //通常のボタンが押されたときに流すSE
    public AudioClip cancel_SE;  //キャンセルボタンが押されたときに流すSE
    public AudioClip character_decision_SE; //キャラクターを決定したときに流すSE
    public AudioClip gamestart_SE;   //ゲームスタートボタンが押されたときに流すSE
    public AudioClip putpiece_SE;            //ピースを配置したときに流れるSE
    public AudioClip gameset_SE;             //ゲーム終了の演出が出たら流れるSE
    public AudioClip gauge_collect_max_SE;  //ゲージが最大まで溜まった時に流れるSE
    public AudioClip start_SC_SE;           //SC発動時に流れるSE
    public AudioClip start_CC_SE;           //CC発動時に流れるSE
    public AudioClip SCorCC_button_on_SE;      //SCかCCのボタンがONにされたときに流れるSE
    public AudioClip SCorCC_button_off_SE;     //SCかCCのボタンがOFFにされたときに流れるSE
    public AudioClip help_triangle_SE;          //ヘルプの三角ボタン(ページ進めたり戻ったり)を押したときに流れるSE

    private static AudioSource audio_source;    //音楽を流す本体
    // Use this for initialization
    void Start ()
    {
        if (!created)
        {
            DontDestroyOnLoad(gameObject);
            //自身のAudioSource情報を取得
            audio_source = GetComponent<AudioSource>();
            //各ボタンの情報を更新
            ButtonInformationUpData();

            created = true;
        }
        else
            Destroy(gameObject);

        

        //シーンが変更されたときに呼ぶために関数をセットする
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    //シーンが切り替わった時に呼ばれる
    void OnActiveSceneChanged(Scene prev_scene, Scene next_scene)
    {
        //各ボタンの情報を更新
        ButtonInformationUpData();
    }

    //各ボタンの情報を更新
    public void ButtonInformationUpData()
    {
        //全てのボタンオブジェクト情報を取得
        GameObject[] normal_button_obj = GameObject.FindGameObjectsWithTag("NormalButton");
        GameObject[] cancel_button_obj = GameObject.FindGameObjectsWithTag("CancelButton");
        GameObject[] character_decision_button_obj = GameObject.FindGameObjectsWithTag("CharacterDecisionButton");
        GameObject[] gamestart_button_obj = GameObject.FindGameObjectsWithTag("GameStartButton");

        //各ボタン情報のメモリを確保
        Button[] normal_button = new Button[normal_button_obj.Length];
        Button[] cancel_button = new Button[cancel_button_obj.Length];
        Button[] character_decision_button = new Button[character_decision_button_obj.Length];
        Button[] gamestart_button = new Button[gamestart_button_obj.Length];

        //ボタンオブジェクトのボタンコンポネントを取得
        for (int i = 0; i < normal_button_obj.Length; i++)
            normal_button[i] = normal_button_obj[i].GetComponent<Button>();
        for (int i = 0; i < cancel_button_obj.Length; i++)
            cancel_button[i] = cancel_button_obj[i].GetComponent<Button>();
        for (int i = 0; i < character_decision_button_obj.Length; i++)
            character_decision_button[i] = character_decision_button_obj[i].GetComponent<Button>();
        for (int i = 0; i < gamestart_button_obj.Length; i++)
            gamestart_button[i] = gamestart_button_obj[i].GetComponent<Button>();
        
        //各ボタンが押されたときに専用SEを流すようにする
        for (int i = 0; i < normal_button.Length; i++)
            normal_button[i].onClick.AddListener(() => audio_source.PlayOneShot(normal_SE));
        for (int i = 0; i < cancel_button.Length; i++)
            cancel_button[i].onClick.AddListener(() => audio_source.PlayOneShot(cancel_SE));
        for (int i = 0; i < character_decision_button.Length; i++)
            character_decision_button[i].onClick.AddListener(() => audio_source.PlayOneShot(character_decision_SE));
        for (int i = 0; i < gamestart_button.Length; i++)
        {
            gamestart_button[i].onClick.AddListener(() => audio_source.PlayOneShot(gamestart_SE));
            //ゲームスタートボタンだけはフィードを行うので追加
            gamestart_button[i].onClick.AddListener(() => GetComponent<AudioBGMSystemScript>().MenuAudioVolumeDown());
        }

        //ヘルプボタンオブジェクト情報を取得
        GameObject help_button_obj = GameObject.FindGameObjectWithTag("HelpButton");
        if (help_button_obj != null)
        {
            //ヘルプボタンにSEをつける
            help_button_obj.GetComponent<Button>().onClick.AddListener(() => audio_source.PlayOneShot(normal_SE));
            //ヘルプの各ボタンにSEをつける
            SetHelpButtonSE(help_button_obj.transform);
        }

    }

    //ピースを設置したときに呼ばれる
    public void PutPieceAudio()
    {
        audio_source.PlayOneShot(putpiece_SE);
    }

    //ゲーム終了の演出がスタートするときに呼ばれる
    public void GameSetAudio()
    {
        audio_source.PlayOneShot(gameset_SE);
    }

    //ゲージが最大まで溜まった時に呼ばれる
    public void GaugeCollectMaxAudio()
    {
        audio_source.PlayOneShot(gauge_collect_max_SE);
    }

    //SC発動時に呼ばれる
    public void StartSCAudio()
    {
        audio_source.PlayOneShot(start_SC_SE);
    }

    //CC発動時に呼ばれる
    public void StartCCAudio()
    {
        audio_source.PlayOneShot(start_CC_SE);
    }

    //SC・CCボタンをONにしたときに呼ばれる
    public void OnSCorCCButton()
    {
        audio_source.PlayOneShot(SCorCC_button_on_SE);
    }

    //SC・CCボタンがOFFににたときに呼ばれる
    public void OffSCorCCButton()
    {
        audio_source.PlayOneShot(SCorCC_button_off_SE);
    }

    //ヘルプにある各ボタン(ヘルプボタン以外)にSEをつける(tagがなければつかない)
    public void SetHelpButtonSE(Transform parent)
    {
        for(int i = 0; i < parent.transform.childCount; i++)
        {
            //子のゲームオブジェクト情報
            GameObject child_obj = parent.transform.GetChild(i).gameObject;
            //子のボタンコンポネント情報
            Button child_button = child_obj.GetComponent<Button>();

            //子にボタンコンポネントがあるかどうか
            if (child_button != null)
            {
                if (child_obj.tag == "NormalButton")
                    child_button.onClick.AddListener(() => audio_source.PlayOneShot(normal_SE));
                else if (child_obj.tag == "HelpTrangleButton")
                    child_button.onClick.AddListener(() => audio_source.PlayOneShot(help_triangle_SE));
            }
            else
                SetHelpButtonSE(child_obj.transform);
        }
    }
}

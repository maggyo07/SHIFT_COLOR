using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_CC_GaugeScript : MonoBehaviour
{
    private float rise_skill;                //スキルゲージの1ターンで上昇する値
    [System.NonSerialized]
    public int SC_num=0;                   //使用可能SCの数(現在SCがいくつ溜まっているか)
    [System.NonSerialized]
    public int SC_accumulated_tane = 0;    //１つのSCがどれくらい溜まっているか
    public int max_SC_num = 3;              //SCゲージの数(SCの溜まる数)
    public int max_SC_accumulated_tane = 6; //何ターンでSCが1つ溜まるのか
    public GameObject player_obj;           //自身の親のプレイヤー情報
    public Text skill_name_text;            //SC CCの文字を表示するテキスト情報
    public Button SC_button;                //SCを使用する際に使うボタン
    public Button CC_button;                //CCを使用する際に使うボタン
    public bool SC_button_flag;             //SCボタンが選択されてるかどうか
    public bool CC_button_flag;             //CCボタンが選択されているかどうか
    public RectTransform[] skill_gauges;       //スキルゲージの段階別のオブジェクト情報
    public Sprite skill_gauge_sprite;      //ゲージ(段階)が最大でないときの専用画像
    public Sprite skill_gauge_max_sprite;  //ゲージ(段階)が最大の時の専用画像

    private bool CC_past_use_flag;                   //CCを過去に使用したかどうか
    [System.NonSerialized]
    public bool skill_use_flag;                    //スキルを使用できる状態かどうかフラグ

    // Use this for initialization
    void Start () {
        //1段階のスキルゲージの最大値を取得
        float max_CS = skill_gauges[0].sizeDelta.x;
        //ゲージが１たまった時の上昇率
        rise_skill = max_CS / max_SC_accumulated_tane;
        //SC/CCボタンフラグ初期化
        SC_button_flag = false;
        CC_button_flag = false;
        //CC使用状態を更新
        CC_past_use_flag = false;
        //初期はスキルを使用できる状態にする
        skill_use_flag = true;

    }
	
	// Update is called once per frame
	void Update ()
    {

        //ゲージの状態を更新する
        GaugeUpDate();

        //文字の情報を更新する
        //SCが１つも溜まっていないなら文字の色を黒色にする
        if (SC_num == 0)
        {
            skill_name_text.color = Color.black;
            skill_name_text.text = "SC";
        }
        //SCが1つ以上溜まっている状態
        else
        {
            //テキストの色をプレイヤーんも好きな色にする
            skill_name_text.color = player_obj.GetComponent<PlayerScript>().favorite_color;
            //SCがMAX溜まっている(CCが使える状態)になったら火のパーティクルを表示させ文字をCCに、色はプレイヤーの好きな色にする
            if (SC_num == max_SC_num)
            {
                skill_name_text.text = "CC";
            }
            //SCがMAX溜まっていないなら文字だけを変える
            else
            {
                skill_name_text.text = "SC";
            }
        }

        //SC/CCのボタンの更新を行う
        ButtenUpdate();

    }

    //ターンが経過したときに実行される
    public void TurnElapsed()
    {
        //ゲージの上昇数
        //通常は１
        int rising_number = 1;
        //プレイヤーオブジェクト(親)のplayerScript情報を取得
        PlayerScript player_script = player_obj.GetComponent<PlayerScript>();
        //スキルゲージ上昇アップの状態異常にかかっていたら
        if((player_script.state.state & PlayerScript.StateAbnormality.State.SkillGaugeRise) == PlayerScript.StateAbnormality.State.SkillGaugeRise)
        {
            //ゲージの上昇数をセットする
            rising_number = player_script.state.skill_gauge_rise_up_num;
        }

        for (int i = 0; i < rising_number; i++)
        {
            //SCがMAXまで溜まっていなければ溜まる
            if (SC_num != max_SC_num)
            {
                //今回で1つのSCがたまりそうでないなら+1する
                if (SC_accumulated_tane < max_SC_accumulated_tane - 1)
                    SC_accumulated_tane++;
                else
                {
                    SC_num++;
                    SC_accumulated_tane = 0;
                    //今回のSC上昇により、CCが使えるようになったら
                    //SEを流す
                    if (SC_num == max_SC_num)
                        GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().GaugeCollectMaxAudio();
                }
            }
        }
    }

    //SC/CCボタンの更新
    void ButtenUpdate()
    {
        //SCがある状態の場合SCボタンを使えるようにする
        if (SC_num > 0)
            SC_button.interactable = true;
        //SCがない状態の場合SCボタンを使えないようにする
        else
            SC_button.interactable = false;

        //CCが使える状態の場合かつ過去にCCを使用していないときCCボタンを使えるようにする
        if (SC_num == max_SC_num && CC_past_use_flag == false)
            CC_button.interactable = true;
        //CCが使えない状態の場合CCボタンを使えないようにする
        else
            CC_button.interactable = false;

        //スキルが使えない状態ならSC・CCを使えないようにする
        if(skill_use_flag == false)
        {
            SC_button.interactable = false;
            CC_button.interactable = false;
        }

        //SCボタンのONOFF状況によって枠を表示するかどうかを決める
        SC_button.transform.GetChild(0).gameObject.SetActive(SC_button_flag);

        //CCボタンのONOFF状況によって枠を表示するかどうかを決める
        CC_button.transform.GetChild(0).gameObject.SetActive(CC_button_flag);

    }

    //SCボタンがクリックされたときの処理
    public void OnSCButtenClick()
    {
        //親のオブジェクトネームがPlayer2かつAIフラグがONなら操作できないようにする
        if (player_obj.name == "Player2" && PlayerManagemaentScript.AI_flag == true)
            return;

        //SCボタンが選択されたのでCCボタンフラグをOFF
        CC_button_flag = false;

        //押したときのON　OFF状態によってSEを流す
        if (SC_button_flag == false)
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().OnSCorCCButton();
        else
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().OffSCorCCButton();

        SC_button_flag = !SC_button_flag;  //SCボタンフラグを反転させる
    }

    //CCボタンがクリックされたときの処理
    public void OnCCButtenClick()
    {
        //親のオブジェクトネームがPlayer2かつAIフラグがONなら操作できないようにする
        if (player_obj.name == "Player2" && PlayerManagemaentScript.AI_flag == true)
            return;

        //CCボタンが選択されたのでCCボタンフラグをOFF
        SC_button_flag = false;

        //押したときのON　OFF状態によってSEを流す
        if (CC_button_flag == false)
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().OnSCorCCButton();
        else
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().OffSCorCCButton();


        CC_button_flag = !CC_button_flag;  //CCボタンフラグを反転させる
    }

    //SCを使ったときの処理
    public void UsedSC()
    {
        SC_num--;
        SC_button_flag = false;
    }

    //CCを使ったときの処理
    //引数1 SC    :CCを使用した時にSC何個分ゲージを減らすか
    //引数2 CC    :CCを使用した時にCC何個分ゲージを減らすか
    public void UsedCC(uint SC,uint CC)
    {
        SC_num -= (int)SC;

        //SCの数が０以下になったら０にする
        if (SC_num < 0)
            SC_num = 0;

        //CC数個分減らすなら、SCの数を０にする
        if (CC > 0)
            SC_num = 0;


        CC_button_flag = false;
        CC_past_use_flag = true;
    }

    //ゲージの状態を更新する
    public void GaugeUpDate()
    {
        if(SC_num != max_SC_num)
        {
            //ゲージ(段階)の長さを調整する
            skill_gauges[SC_num].sizeDelta = new Vector2(rise_skill * SC_accumulated_tane, skill_gauges[SC_num].sizeDelta.y);
            //ゲージ(段階)の画像を差し替える
            skill_gauges[SC_num].GetComponent<Image>().sprite = skill_gauge_sprite;
        }

        //SCの溜まり具合によってゲージ(段階)の状態を変更する
        //SCがたまっていたら長さを最大にし、画像を差し替える
        //SCがたまっていないかつ現在上昇中のゲージ(段階)であれば、
        //長さを０にし、画像を差し替える
        for(int i = 0; i < max_SC_num; i++)
        {
            //現在上昇中のゲージ(段階)以外
            if (SC_num != i)
            {
                //ゲージ(段階)が最大まで溜まっていたら
                if(i <= SC_num-1)
                {
                    //ゲージ(段階)の長さをMAXにする
                    skill_gauges[i].sizeDelta = new Vector2(rise_skill * max_SC_accumulated_tane, skill_gauges[i].sizeDelta.y);
                    //ゲージ(段階)が最大の時専用の画像に差し替える
                    skill_gauges[i].GetComponent<Image>().sprite = skill_gauge_max_sprite;
                }
                //ゲージ(段階)がたまっていなかったら
                else
                {
                    //ゲージ(段階)の長さを０にする
                    skill_gauges[i].sizeDelta = new Vector2(0, skill_gauges[i].sizeDelta.y);
                    //ゲージ(段階)が最大以外の時専用の画像に差し替える
                    skill_gauges[i].GetComponent<Image>().sprite = skill_gauge_sprite;
                }
            }

        }
    }

}

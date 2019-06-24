using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusDisplayScript : MonoBehaviour
{
    //状態異常を表示するための構造体
    public struct StatusDisplay
    {
        //ターン数（文字列）
        public string turn_text;
        //状態異常の画像
        public Sprite status_sprite;
    }

    //状態異常を表示するためのスクリプト
    public PlayerScript player_script;
    //ターン数を表示するためのText
    public Text turn_display;

    //スキル封印の状態異常の時の画像
    public Sprite skill_seal_sprite;
    //移動数倍化の状態異常の時の画像
    public Sprite quick_move_sprite;
    //スキルゲージ上昇率アップの状態異常の時の画像
    public Sprite skill_gauge_rise_sprite;


    //画像を変えるタイミング
    //画像を変えるタイミングにアニメーション側でtrueを代入
    public bool change_images_flag;

    //現在蓄積している状態異常の情報
    private List<StatusDisplay> state_accumulation;

    //state_accumulationの要素数
    private int state_count;

    // Use this for initialization
    void Start ()
    {
        //リストを初期化
        state_accumulation = new List<StatusDisplay>();

        //カウント数を初期化
        state_count = 0;

        //画像を変えるタイミングをfalseにする
        change_images_flag = false;
        
        //表示するターン数をなしにする
        turn_display.text = "";

    }
	
	// Update is called once per frame
	void Update () {
        //状態異常を更新する
        StateAbnormalityNum();

        //現在状態異常が蓄積していたら
        if(state_accumulation.Count != 0)
        {
            //現在状態異常が１つなら
            if(state_accumulation.Count == 1)
            {
                //要素数0の状態異常を表示する
                GetComponent<Image>().sprite = state_accumulation[0].status_sprite;
                turn_display.text = state_accumulation[0].turn_text;
            }
            //現在状態異常が２つ以上なら
            else
            {
                //画像を変えるタイミングなら変える
                if (change_images_flag)
                {
                    //状態異常の表示を変更する
                    GetComponent<Image>().sprite = state_accumulation[state_count].status_sprite;
                    turn_display.text = state_accumulation[state_count].turn_text;
                    //カウントを進める
                    state_count++;

                    //カウントがMAX以上なら０に戻す
                    if (state_count >= state_accumulation.Count)
                        state_count = 0;
                }
            }
        }
        //現在状態異常が蓄積していなかったら
        else
        {
            //表示している状態異常を初期化する
            GetComponent<Image>().sprite = null;
            turn_display.text = "";
        }

        //ターン数を表示するTextの透明度を画像のと同じにする
        turn_display.color = new Color(turn_display.color.r, turn_display.color.g, turn_display.color.b, GetComponent<Image>().color.a);
    }

    //状態異常を更新する
    void StateAbnormalityNum()
    {
        //リストを初期化
        state_accumulation.Clear();

        //状態異常：移動数倍化
        if ((player_script.state.state & PlayerScript.StateAbnormality.State.QuickMove) == PlayerScript.StateAbnormality.State.QuickMove)
        {
            //リストに入れるための用
            StatusDisplay set;
            set.status_sprite   = quick_move_sprite;
            set.turn_text       = (player_script.state.quivk_move_turn).ToString();
            state_accumulation.Add(set);
        }
        //状態異常：スキルゲージ上昇率アップ
        if ((player_script.state.state & PlayerScript.StateAbnormality.State.SkillGaugeRise) == PlayerScript.StateAbnormality.State.SkillGaugeRise)
        {
            //リストに入れるための用
            StatusDisplay set;
            set.status_sprite = skill_gauge_rise_sprite;
            set.turn_text = "∞";
            state_accumulation.Add(set);
        }
        //状態異常：スキル封印
        if ((player_script.state.state & PlayerScript.StateAbnormality.State.SkillSeal) == PlayerScript.StateAbnormality.State.SkillSeal)
        {
            //リストに入れるための用
            StatusDisplay set;
            set.status_sprite = skill_seal_sprite;
            set.turn_text = (player_script.state.skill_seal_turn).ToString();
            state_accumulation.Add(set);
        }
    }
}

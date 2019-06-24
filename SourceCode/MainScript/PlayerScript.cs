using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public Color favorite_color;        //自身の好きな色
    public GameObject SC_CC_Gauge;      //SC/CCのゲージオブジェクト(自身の子)
    public Text[] name_text;              //プレイヤーの名前を表示するText情報

    //状態異常に関する構造体
    public struct StateAbnormality
    {
        public State state;               //状態異常
        public int skill_seal_turn;     //スキル封印のスキルが何ターン続くか
        public int skill_gauge_rise_up_num; //スキルゲージ上昇アップでターン経過時にゲージをどのくらい上昇させるか

        public int quivk_move_multiple; //移動数倍化のスキルで何倍にするのか
        public int quivk_move_turn;     //移動数倍化のスキルが何ターン続くか
        public int quivk_move_attack_count; //移動数倍化のスキルで現在のターンで何回攻撃したか

                private SC_CC_GaugeScript SC_CC_gauge_script;//SC・CCゲージのスクリプト情報
        //状態異常管理用列挙
        public enum State
        {
            SkillSeal       = 1,        //スキルが使えなくなる状態異常
            QuickMove       = 1 << 1,   //行動数倍化(１ターンに２回色を移せる)
            SkillGaugeRise  = 1 << 2,   //スキルゲージの上昇率を上げる状態異常
        }

        //初期化関数
        public void Initialization(GameObject SC_CC_gauge_obj)
        {
            state = 0;          //状態を通常の状態にする
            skill_seal_turn = 0;//ターン数を０にする
            skill_gauge_rise_up_num = 0;//スキルゲージ上昇アップの単位を０にする
            quivk_move_multiple = 0;//倍数を０にする
            quivk_move_turn = 0;//ターン数を０にする
            quivk_move_attack_count = 0;//攻撃回数を初期化

            SC_CC_gauge_script = SC_CC_gauge_obj.GetComponent<SC_CC_GaugeScript>();
        }

        //スキル封印の状態異常をつける
        //引数1 turn_num  :ターン数(スキル封印を何ターン継続させるか)
        public void SkliiSeal(int turn_num)
        {
            //スキル封印の状態異常をつける
            state |= State.SkillSeal;
            //スキル封印の続くターン数をセットする
            skill_seal_turn += turn_num;
            //スキルボタンを押せないようにする
            SC_CC_gauge_script.skill_use_flag = false;
            //スキルボタンの選択状態をfalseにする
            SC_CC_gauge_script.SC_button_flag = false;
            SC_CC_gauge_script.CC_button_flag = false;
        }

        //移動数倍化の状態異常をつける
        //引数1 multiple  :倍数(何倍速にするか　例)２なら1ターンに２回行動できる)
        //引数2 turn_num  :ターン数(移動数倍化を何ターン継続させるか)
        public void QuivkMove(int multiple, int turn_num)
        {
            //移動数倍化の状態異常をつける
            state |= State.QuickMove;
            //移動数倍化の続くターン数をセット
            quivk_move_turn = turn_num;
            
            //移動数倍化の倍数をセット
            quivk_move_multiple = multiple;
            //攻撃回数をMAXにする。状態異常を受けた次のターンから効果を発動させる
            quivk_move_attack_count = quivk_move_multiple;
        }

        //スキルゲージ上昇アップの状態異常をつける
        //引数1 rising_number :上昇数
        public void SkillGaugeRiseUp(int rising_up_number)
        {
            //スキルゲージ上昇アップの状態異常をつける
            state |= State.SkillGaugeRise;
            //スキルゲージ上昇率を設定する
            skill_gauge_rise_up_num = rising_up_number;
        }

        //移動数倍化スキルの状態を確認する
        //戻り値 bool  :true = 状態異常にかかっていないまたわ移動数倍化の効果が切れている(現在のターンのみ)
        //              false= 状態異常にかかっていてかつ移動数倍化の効果が継続している(現在のターンのみ)
        public bool QuivkMoveStatusCheck()
        {
            //移動数倍化の状態異常にかかっていたら
            if((state & State.QuickMove) == State.QuickMove)
            {
                //効果が継続していたら
                if(quivk_move_attack_count != quivk_move_multiple)
                {
                    //攻撃するので攻撃回数を加算する
                    quivk_move_attack_count++;
                    return false;
                }
            }
            return true;
        }
        //ターンが進んだ時に呼ばれる
        public void TurnElapsed()
        {
            //スキル封印の状態異常にかかっているなら
            if ((state & State.SkillSeal) == State.SkillSeal)
            {
                //スキル封印の継続ターン数を1ターン分減らす
                skill_seal_turn--;

                //スキル封印の継続ターン数が０以下なら状態異常を消す
                if(skill_seal_turn <= 0)
                {
                    //一応初期化
                    skill_seal_turn = 0;
                    //スキルを使用できるようにする
                    SC_CC_gauge_script.skill_use_flag = true;
                    //スキル封印の状態異常を回復させる
                    state &= ~State.SkillSeal;
                }
            }
            //移動数倍化の状態異常にかかっているなら
            if((state & State.QuickMove) == State.QuickMove)
            {
                //移動数倍化の継続ターン数を１ターン分減らす
                quivk_move_turn--;
                //攻撃回数を初期化
                //１を代入しているのはカウントのタイミングが攻撃後なので
                quivk_move_attack_count = 1;
                //移動数倍化の継続ターン数が０以下なら状態異常を消す
                if (quivk_move_turn <= 0)
                {
                    //一応初期化
                    quivk_move_turn = 0;
                    quivk_move_attack_count = 0;
                    //移動数倍化の状態異常を回復させる
                    state &= ~State.QuickMove;
                }
            }
        }
    }
    public StateAbnormality state;//状態管理

    
    // Use this for initialization
    void Start () {
        if(gameObject.name == "Player1")
        {
            for (int i = 0; i < name_text.Length; i++)
                name_text[i].text = PlayerManagemaentScript.player1_name;
            GetComponent<PlayerIconScript>().IconDataUpData();
        }
        else
        {
            for (int i = 0; i < name_text.Length; i++)
                name_text[i].text = PlayerManagemaentScript.player2_name;
            GetComponent<PlayerIconScript>().IconDataUpData();
        }

        //状態異常を初期化
        state.Initialization(SC_CC_Gauge);
    }
	
	// Update is called once per frame
	void Update () {
	}

  
}

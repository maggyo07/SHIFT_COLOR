using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//ターン制御用
public class TurnSystemScript : MonoBehaviour {
    public string attack_prayer_name;  //現在攻撃中のプレイヤーの名前
    private bool order_flag = false;     //現在順番を決めている途中かどうか
    private float roulette_rotate = 0.0f;       //ルーレットの回転度
    public float roulette_speed = 10.0f;        //ルーレットの回転速度
    public int roulette_rotate_max_num = 0;     //ルーレットのMax回転数(指定数回転をしたら次の回転の途中で止まる)
    private float roulette_stope_rotate = 0.0f; //ルーレットが指定数回転をした後、どの位置で止まるかの回転度(MAX360)
    private List<int> order;                    //順番を格納している。値はプレイヤーID（１＝player1 ; 2=player2）
    private string entire_map_obj_name = "EntireMap";              //EntireMapオブジェクトの名前(SelectSystemScriptを持っているオブジェクト)
    private int turn = 0;                       //現在のターン数(ルーレットを回す準備をする関数のところでカウントを進める)
    private int first_continuous_count=1;   //同じプレイヤーが連続で先攻になったかどうか(先攻が２回連続でなった場合、次のターンは先攻以外になる)
    private int first_player_id=0;          //先攻になったプレイヤーのID(1ターン前の時の先攻プレイヤーのID。first_continuous_countと一緒に使う)
    public float stop_time_before_roulette_start = 0.0f;   //ルーレットがスタートする前のステイ時間
    public float stop_time_after_roulette_end = 0.0f;    //ルーレットが終わった後のステイ時間
    public GameObject game_progress_obj;                //ゲームの進行状況を表示するオブジェクト
    public GameObject roulette_object;  //順番を決めるときに使うルーレットオブジェクト
    public Image roulette_left_image; //ルーレットの左の画像データ(先手後手表示用)
    public Image roulette_right_image;  //ルーレットの右の画像データ(先手後手表示用)
    public Sprite preemption_sprite;                    //先手の文字の画像データ
    public Sprite late_sprite;                          //後手の文字の画像データ
    public int roulette_start_interval = 10;                 //ルーレットをスタートする間隔 例)5なら5ターン毎にルーレットで先行後攻を決める
    public GameProgressScript game_progress_script;     //ゲームの進行状態を表示するオブジェクトのスクリプト
    // Use this for initialization
    void Start () {
        order = new List<int>();
        //ルーレットを作成
        //roulette_object = Instantiate(roulette_object) as GameObject;
        roulette_object.name = "roulette";
        //roulette_object.transform.parent = GameObject.Find("Canvas").transform;
        //ルーレットをスタートする準備をする
        StartCoroutine(RouletteState());
    }
	
	// Update is called once per frame
	void Update () {
        //順番を決めてる途中なら順番を決める
        if (order_flag)
        {
            StartCoroutine(OrderDecide());
        }
        else
        {
            
        }
    }

    //順番(先攻後攻)を決める
    IEnumerator OrderDecide()
    {
        //ルーレットが終了するまで出番の名前をなしにする
        game_progress_script.SetTheNumberName("");
        //ルーレットを回転させる
        roulette_rotate += roulette_speed;
        roulette_object.transform.rotation = Quaternion.AngleAxis(roulette_rotate, -Vector3.forward);
        //回転数が最大値以上の時
        if(roulette_rotate / 360f >= roulette_rotate_max_num)
        {
            //ストップする角度(誤差 +-speed/2)になったら回転をストップしてルーレットの回転角度をストップする角度にする
            if (roulette_stope_rotate - roulette_speed / 2 <= roulette_rotate % 360f &&
                roulette_stope_rotate + roulette_speed / 2 >= roulette_rotate % 360f)
            {
                //ルーレットの角度をストップする角度にする
                roulette_rotate = roulette_stope_rotate;
                //順番が決め終わったのでflagをOFFにする
                order_flag = false;

                //ルーレットの左右の文字の表示をする
                roulette_left_image.gameObject.SetActive(true);
                roulette_right_image.gameObject.SetActive(true);
                //数秒間待った後、ルーレットのアクティブをOFFにする
                yield return new WaitForSeconds(stop_time_after_roulette_end);
                //ルーレットとその左右の文字を非表示にする
                roulette_object.SetActive(false);
                roulette_left_image.gameObject.SetActive(false);
                roulette_right_image.gameObject.SetActive(false);
                //パス判定を行い、パス判定ありならもう一度出番を交代する
                if (GameObject.Find(entire_map_obj_name).GetComponent<EntireMapScript>().PathJudgment(attack_prayer_name))
                {
                    TuneChange();
                }
                //ルーレットが終了したので出番の名前を表示する
                game_progress_script.SetTheNumberName(attack_prayer_name);
                //ルーレットが終了したのでSelectSystemScriptの動作を起動させる
                GameObject.Find(entire_map_obj_name).GetComponent<SelectSystemScript>().system_flag = true;
            }
        }
    }

    //ルーレットをスタートする準備を行う
    IEnumerator RouletteState()
    {
        //ターンを進める
        AdvanceTurn();

        //ルーレットをスタートするのでSelectSystemScriptの動作を停止させる
        GameObject.Find(entire_map_obj_name).GetComponent<SelectSystemScript>().system_flag = false;

        //ルーレットのアクティブをONにする
        roulette_object.SetActive(true);
        //ルーレットの角度を0.0fに戻す
        roulette_rotate = 0.0f;
        roulette_object.transform.rotation = Quaternion.AngleAxis(roulette_rotate, -Vector3.forward);

        //1ターン前に先攻だったプレイヤーIDを入れる
        if(order.Count > 0)
            first_player_id = order[0];
        //順番を格納しているListをクリアする
        order.Clear();

        //ストップする角度(先攻)を決める
        int rondom;

        //ランダムで先攻を決める
        rondom = Random.Range(1, 3);

        //先攻が2回連続して同じプレイヤーだったら
        //先攻だったプレイヤー以外を先攻にする
        if(first_continuous_count == 2)
        {
            if (first_player_id == 1)
                rondom = 2;
            else
                rondom = 1;
        }

        //今回の先攻と1ターン前の先攻のプレイヤーが一緒だった場合
        //連続で先攻になっているのでカウントを進める
        if (first_player_id == rondom)
            first_continuous_count++;
        //今回の先攻と1ターン前の先攻のプレイヤーが違う場合
        //連続ではないので１を代入する
        else
            first_continuous_count = 1;

        if (rondom == 1)
        {
            roulette_stope_rotate = 270f;
            attack_prayer_name = "Player1";
            order.Add(1);
            order.Add(2);
            //ルーレットの左右の文字を変換
            //先行がプレイヤー１なので左に先行、右に後攻の文字にする
            roulette_left_image.sprite = preemption_sprite;  //左の文字を変換
            roulette_left_image.gameObject.SetActive(false);         //左の文字を非表示にする
            roulette_right_image.sprite = late_sprite;  //右の文字を変換
            roulette_right_image.gameObject.SetActive(false);         //右の文字を非表示にする
        }
        else
        {
            roulette_stope_rotate = 90f;
            attack_prayer_name = "Player2";
            order.Add(2);
            order.Add(1);
            //ルーレットの左右の文字を変換
            //先行がプレイヤー2なので左に後手、右に先手の文字にする
            roulette_left_image.sprite = late_sprite;  //左の文字を変換
            roulette_left_image.gameObject.SetActive(false);         //左の文字を非表示にする
            roulette_right_image.sprite = preemption_sprite;  //右の文字を変換
            roulette_right_image.gameObject.SetActive(false);         //右の文字を非表示にする
        }

        //数秒後にルーレットを回す
        yield return new WaitForSeconds(stop_time_before_roulette_start);
        //順番を決める
        order_flag = true;
        
    }

    //出番を交代する
    public void TuneChange()
    {
        int player_id;//player～の数値の部分

        //現在攻撃が終わったプレイヤーのIDを取得
        player_id = int.Parse("" + attack_prayer_name[attack_prayer_name.Length - 1]);

        //順番を確認
        for(int id = 0; id < order.Count; id++)
        {
            //プレイヤーIDが一致したら
            if (player_id == order[id])
            {
                //現在攻撃が終わったプレイヤーが順番の一番最後だったら
                if(id == order.Count-1)
                {
                    //ターン数がroulette_start_intervalの倍数なら
                    //ルーレットを回し先行後攻を決める
                    if ((turn+1) % roulette_start_interval == 0)
                        StartCoroutine(RouletteState());
                    //そうでなければ、攻撃するプレイヤーを変え、
                    //ターンを進める
                    else
                    {
                        //攻撃するプレイヤーを変える
                        attack_prayer_name = "Player" + order[id - 1];
                        //ターンを進める
                        AdvanceTurn();
                        //パス判定を行い、パス判定ありならもう一度出番を交代する
                        if (GameObject.Find(entire_map_obj_name).GetComponent<EntireMapScript>().PathJudgment(attack_prayer_name))
                            TuneChange();
                    }
                }
                //現在攻撃が終わったプレイヤーが順番の一番最後でなければ
                //次のプレイヤーが攻撃する
                else
                {
                    attack_prayer_name = "Player" + order[id+1];

                    //パス判定を行い、パス判定ありならもう一度出番を交代する
                    if (GameObject.Find(entire_map_obj_name).GetComponent<EntireMapScript>().PathJudgment(attack_prayer_name))
                        TuneChange();
                }
                break;
            }
        }
        //出番を交代したので名前を更新する
        game_progress_script.SetTheNumberName(attack_prayer_name);
    }

    //ターンを進める時に実行する
    void AdvanceTurn()
    {
        //ターンを進める
        turn++;
        //ターンが進んだので表示情報を更新する
        game_progress_obj.GetComponent<GameProgressScript>().TurnNumUpdata(turn);

        //ターンが進んだので全てのPlayer(Tag)の子のCC/CSゲージを溜める
        GameObject[] player = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < player.Length; i++)
        {
            //プレイヤーのプレイヤースクリプト情報を取得
            PlayerScript player_script = player[i].GetComponent<PlayerScript>();

            //ゲージの状態を更新する
            player_script.SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>().TurnElapsed();
            //状態異常の状態を更新する
            player_script.state.TurnElapsed();

        }
    }

    //引数player_idが先攻か後攻かを返す
    //先攻ならtrueを後攻ならfalseを返す
    public bool GetAfterwardsAttack(int player_id)
    {
        if (order[0] == player_id)
            return true;
        else
            return false;
    }
}

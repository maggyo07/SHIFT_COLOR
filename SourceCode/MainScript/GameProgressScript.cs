using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameProgressScript : MonoBehaviour {

    public Text player1_piece_text;    //プレイヤー１のピースの数を表示するText情報
    public Text player2_piece_text;    //プレイヤー２のピースの数を表示するText情報
    public Text turn_text;             //ターン数を表示するText情報

    public GameObject numbering_disolay;//出番をわかりやすくする用の枠情報
    public GameObject entire_map_obj;  //EntireMapのオブジェクト情報
    public Transform player1;          //playey1オブジェクト(台紙)  numbering_disolayの位置を調整する用
    public Transform player2;          //Player2オブジェクト(台紙)　numbering_disolayの位置を調整する用

    // Use this for initialization
    void Start () {
        //プレイヤーのピース数を初期化する
        player1_piece_text.text = "2";
        player2_piece_text.text = "2";
        //ターン数を初期化する
        turn_text.text = "1";
        //まだ出番は決まっていないので非表示にする
        numbering_disolay.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //プレイヤーのピース数を更新する
    public void PlayerPieceNumUpdate()
    {
        int player1_piece_num = 0;
        int player2_piece_num = 0;
        //二人のプレイヤーのピース数を取得
        entire_map_obj.GetComponent<EntireMapScript>().PlayerPieceNum(ref player1_piece_num, ref player2_piece_num);

        //プレイヤーピース数が更新されたので表示を変更する
        player1_piece_text.text = (player1_piece_num).ToString();
        player2_piece_text.text = (player2_piece_num).ToString();
    }

    //ターン数を変更するときに呼ぶ
    public void TurnNumUpdata(int tune_num)
    {
        turn_text.text = (tune_num).ToString();
    }

    //現在出番のプレイヤーの名前を変更する
    public void SetTheNumberName(string attack_prayer_name)
    {
        if(attack_prayer_name == "Player1")
        {
            //出番が決まったので表示する
            numbering_disolay.SetActive(true);
            //位置を調整
            numbering_disolay.GetComponent<RectTransform>().position = player1.GetComponent<RectTransform>().position;
        }
        else if(attack_prayer_name == "Player2")
        {
            //出番が決まったので表示する
            numbering_disolay.SetActive(true);
            //位置を調整
            numbering_disolay.GetComponent<RectTransform>().position = player2.GetComponent<RectTransform>().position;
        }
        else
        {
            //出番を決めてている途中なので非表示
            numbering_disolay.SetActive(false);
        }
    }
}

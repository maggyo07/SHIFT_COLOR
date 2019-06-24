using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameEndScript : MonoBehaviour
{
    //GameSetの文字を表示するためのText情報
    public Text game_set_text;  
    //どのプレイヤーが勝利したのかどうかを表示するText情報
    public Text game_winner_text;
    // Use this for initialization
    void Start ()
    {
        //最初は非表示にする
        gameObject.SetActive(false);
        game_set_text.gameObject.SetActive(false);
        game_winner_text.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        //自身のアクティブがtrueなら
		if(gameObject.activeSelf == true)
        {
            //GameSetのtextを表示
            EndEffect();
        }
	}

    //終了演出
    public void EndEffect()
    {
        //GameWinnerTextが非表示ならGameSetTextを表示する
        if (!game_winner_text.gameObject.activeSelf)
            game_set_text.gameObject.SetActive(true);
        //GameSetTextのアニメーションが終わるまで待つ
        if(!game_set_text.GetComponent<Animation>().isPlaying)
        {
            //一番最初に入って生きたときにgame_winner_text情報を更新する
            if(game_set_text.gameObject.activeSelf)
                //game_winner_text情報を更新
                UpDataGameWinnerText();

            //GameSetのTextを非表示にする
            game_set_text.gameObject.SetActive(false);
            //GameWinnerTextを表示する
            game_winner_text.gameObject.SetActive(true);
           
            if(!game_winner_text.GetComponent<Animation>().isPlaying)
            {
                SceneManager.LoadScene("MenuScene");
                game_winner_text.gameObject.SetActive(false);
            }
        }
    }

    //game_winner_textのtext情報を更新
    void UpDataGameWinnerText()
    {
        //各プレイヤーのピース数を取得
        int player1_piece_num = 0;
        int player2_piece_num = 0;
        GameObject.Find("EntireMap").GetComponent<EntireMapScript>().PlayerPieceNum(ref player1_piece_num, ref player2_piece_num);

        //player１のピースが多かったらplayer１の勝利
        if (player1_piece_num > player2_piece_num)
        {
            game_winner_text.text = "WIN";
            game_winner_text.transform.GetChild(0).GetComponent<Text>().text = PlayerManagemaentScript.player1_name;
        }
        //player２のピースが多かったらplayer２の勝利
        else if (player1_piece_num < player2_piece_num)
        {
            game_winner_text.text = "WIN";
            game_winner_text.transform.GetChild(0).GetComponent<Text>().text = PlayerManagemaentScript.player2_name;
        }
        //同点
        else
        {
            game_winner_text.text = "DROW";
        }
    }
}

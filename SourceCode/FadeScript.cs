using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeScript : MonoBehaviour {
    public float speed = 0.01f; //フィード(暗転)の速さ
    public Text flashing_text;          //点滅するテキスト情報(点滅アニメーションのあるボタンの子)
    public float alfa;                 //A値を操作するための変数
    public bool feed_flag;      //フィードを開始するか否か
   	// Use this for initialization
	void Start () {
        feed_flag = false;
        alfa = 0.0f;
	}
	
	// Update is called once per frame
	void Update () {

        //フィードフラグがONならフィード(暗転)を行う
        if (feed_flag == true)
        {
            //フィードを行うために透明化を進める
            Image image = GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, alfa);
            if (alfa < 1.0f)
                alfa += speed;
            else
                alfa = 1.0f;

            //フィード(暗転)が最後まで行ったらシーンを移動する
            if(alfa >= 1.0f)
            {
                //現在のシーンの名前を取得する
                string scene_name = SceneManager.GetActiveScene().name;
                //現在のシーンがタイトルなら、メニューに移動
                if (scene_name == "TitleScene")
                    SceneManager.LoadScene("MenuScene");
                //現在のシーンがモードCPUなら、メインに移動する
                if (scene_name == "ModeCPUScene")
                    SceneManager.LoadScene("MainScene");

                
            }
        }
	}

    //点滅するテキスト情報の親(Botten)がクリックされたときに呼ばれる
    public void OnClick()
    {
        //点滅するテキストの点滅を終了させる
        flashing_text.GetComponent<Animation>().Stop();
        //点滅アニメーションの途中で止めるため、透明度が統一しないので、透明度を1.0fにする
        flashing_text.color = new Color(flashing_text.color.r, flashing_text.color.g, flashing_text.color.b, 1.0f);
        //フィード(暗転)を開始する
        feed_flag = true;
    }
}

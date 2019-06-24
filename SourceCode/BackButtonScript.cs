using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonScript : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //ボタンが押されたときに実行する
    public void OnClick()
    {
        //現在のシーンの名前
        string scene_name = SceneManager.GetActiveScene().name;
        //現在のシーンがメニューの場合、タイトルに移動する
        if (scene_name == "MenuScene")
            SceneManager.LoadScene("TitleScene");
        //現在のシーンがキャラクターセレクトの場合、メニューに戻る
        if (scene_name == "CharacterSelect(CPU)Scene")
            SceneManager.LoadScene("MenuScene");
        //現在のシーンがモードCPUの場合、キャラクターセレクトに戻る
        if (scene_name == "ModeCPUScene")
            SceneManager.LoadScene("CharacterSelect(CPU)Scene");
        //現在のシーンがネットワーク接続方法の場合、メニューに戻る
        if (scene_name == "NetworkConnectionScene")
            SceneManager.LoadScene("MenuScene");
        //現在のシーンがネットワークサーバーの場合、メニューに戻る
        if (scene_name == "NetworkServerScene")
            SceneManager.LoadScene("MenuScene");

    }
}

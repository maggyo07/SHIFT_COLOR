using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MeunButtonsScript : MonoBehaviour
{
    public bool pointer_flag;    //マウスポインタが接触フラグ
    public Image frame;          //枠の画像データ(子)
	// Use this for initialization
	void Start () {
        //マウスポインタフラグを初期化
        pointer_flag = false;
        //枠の画像を非表示にする
        frame.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {

	}

    //マウスポインタがオブジェクト(自身)に入るときに呼ばれる関数
    public void OnPointerEnter()
    {
        //マウスポインタフラグをONにする
        pointer_flag = true;
        //枠の画像を表示する
        frame.gameObject.SetActive(pointer_flag);
    }

    //マウスポインタがオブジェクト(自身)を出るときに呼ばれる関数
    public void OnPointerExit()
    {
        //マウスポインタフラグをOFFにする
        pointer_flag = false;
        //枠の画像を非表示にする
        frame.gameObject.SetActive(pointer_flag);
    }

    //ボタンが押されたら呼ばれる関数
    public void OnClick()
    {
        //自身の名前がCPUで始まるならCPU対戦ボタンと判断する
        if (gameObject.name.StartsWith("CPU"))
        {
            SceneManager.LoadScene("CharacterSelect(CPU)Scene");//CPU対戦ボタンが押されたらきゃれくたーセレクトシーンへ移動する
            PlayerManagemaentScript.AI_flag = true;//AIフラグをONにする
        }
        //自身の名前がLocalで始まるならローカル対戦ボタンと判断する
        else if (gameObject.name.StartsWith("Local"))
        {
            SceneManager.LoadScene("NetworkConnectionScene");//Local対戦ボタンが押されたらネットワーク接続方法シーンへ移動する
        }
        else if(gameObject.name.StartsWith("PVP"))
        {
            SceneManager.LoadScene("CharacterSelect(CPU)Scene");//CPU対戦ボタンが押されたらきゃれくたーセレクトシーンへ移動する
            PlayerManagemaentScript.AI_flag = false;//AIフラグをOFFにする
        }
        else
            Debug.Log("まとめ");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeunDescriptionTextScript : MonoBehaviour
{
    //メニューボタン構造体
    [System.Serializable]
    public struct MenuButton
    {
        public MeunButtonsScript script;   //ボタン情報のスクリプト
        public string desctiption_text;    //説明文
    }

    //全てのボタンの情報
    public MenuButton[] buttons;

    public Text desctption_text;            //各説明文を表示させるText(子)
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
        //表示する説明文を初期化すう
        desctption_text.text = "";

        //ボタンの数分回す
        for (int i = 0; i < buttons.Length; i++)
        {
            //ボタンが選択されていたら
            if (buttons[i].script.pointer_flag)
            {
                //説明文を選択されていたボタンの説明文にする
                desctption_text.text = buttons[i].desctiption_text;
                break;
            }
        }
    }
}

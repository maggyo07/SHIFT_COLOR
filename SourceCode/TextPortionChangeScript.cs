using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//必要なコンポーネントを定義
[RequireComponent(typeof(Text))]

public class TextPortionChangeScript : MonoBehaviour {
    //自身のText情報
    Text text;

    //Textの一部分を変更するために必要な情報
    [System.Serializable]
    public struct TextChange
    {
        //変更する部分の文字列
        public string text;
        //変更する色情報
        public Color color;
    }

    //Textの一部分を変更する情報配列
    public TextChange[] text_changes;

    // Use this for initialization
    void Start()
    {
        //自身のText情報を取得
        text = GetComponent<Text>();
        //Textの状態text_changesを元に更新する
        TextUpdata();
    }
	
	// Update is called once per frame
	void Update ()
    {
        TextUpdata();

    }

    //Textの状態text_changesを元に更新する
    void TextUpdata()
    {
        //text.textの文字列情報を分解するためのList
        List<string> text_disassembly = new List<string>();

        //文字列分解Listにtext.text情報を入れる
        text_disassembly.Add(text.text);

        //Textの一部分を変更する情報がある分回す
        foreach (TextChange text_change in text_changes)
        {
            //分解した分回す
            for(int i = 0; i < text_disassembly.Count; i++)
            {
                //文字列検索をする
                int iDat = text_disassembly[i].IndexOf(text_change.text);
                //文字列検索が成功していたら　かつ
                //すでに変更されていなかったら
                if (iDat != -1 && text_disassembly[i].IndexOf("<") == -1)
                {
                    //文字列検索にひかかった文字列の情報を取得
                    string chack_text = text_disassembly[i];
                    //文字列を分解するためListの一部を消去
                    text_disassembly.RemoveAt(i);

                    //文字列を３つに分解する---------------------------------
                   
                    //文字列を分解した時の前の情報
                    string before_text = "";
                    //文字列を分解した時の中心の情報
                    string while_text = "";
                    //文字列を分解した時の後の情報
                    string rear_text = "";

                    //チェックする文字列を一文字ごとに処理をする
                    for (int j = 0;j < chack_text.Length; j++)
                    {
                        //検索した文字列の最初なら変更するcolor情報を追加する
                        if (j == iDat)
                        {
                            //RGBAを16進数にし、文字列にしたものを格納用
                            string r_hexadecimal = "", g_hexadecimal = "", b_hexadecimal = "", a_hexadecimal = "";

                            //RGBAを16進数にし、文字列にしたものを取得する
                            r_hexadecimal = Convert.ToString(Mathf.RoundToInt(text_change.color.r * 255), 16);
                            g_hexadecimal = Convert.ToString(Mathf.RoundToInt(text_change.color.g * 255), 16);
                            b_hexadecimal = Convert.ToString(Mathf.RoundToInt(text_change.color.b * 255), 16);
                            a_hexadecimal = Convert.ToString(Mathf.RoundToInt(text_change.color.a * 255), 16);

                            //もし、16進数を文字列にした時、長さが1なら前に0を追加する
                            if (r_hexadecimal.Length == 1)
                                r_hexadecimal = "0" + r_hexadecimal;
                            if (g_hexadecimal.Length == 1)
                                g_hexadecimal = "0" + g_hexadecimal;
                            if (b_hexadecimal.Length == 1)
                                b_hexadecimal = "0" + b_hexadecimal;
                            if (a_hexadecimal.Length == 1)
                                a_hexadecimal = "0" + a_hexadecimal;

                            while_text += "<color=#" + r_hexadecimal + g_hexadecimal + b_hexadecimal + a_hexadecimal + ">";
                        }

                        //現在探索している文字が検索した文字列の前にあるなら前の情報に追加する
                        if (j < iDat)
                            before_text += chack_text[j];
                        //現在探索している文字が検索した文字列の後にあるなら後の情報に追加する
                        else if (j >= iDat + text_change.text.Length)
                            rear_text += chack_text[j];
                        //現在探索している文字が検索した文字列のどれかなら中心の情報に追加する
                        else 
                            while_text += chack_text[j];

                        //検索した文字列の最後なら</color>を追加する
                        if (j == iDat + text_change.text.Length - 1)
                            while_text += "</color>";
                    }
                    //Listに追加する位置情報
                    int add_pos = i;

                    //前の情報が存在していたら追加する
                    if(before_text != "")
                        text_disassembly.Insert(add_pos++, before_text);
                    //中心の情報が存在していたら追加する
                    if(while_text != "")
                        text_disassembly.Insert(add_pos++, while_text);
                    //後の情報が存在していたら追加する
                    if(rear_text != "")
                        text_disassembly.Insert(add_pos, rear_text);
                    //-------------------------------------------------------
                }
            }
        }

        //text.text情報を初期化する
        text.text = "";

        //text_disassemblyの情報をすべてtext.text情報に追加する
        foreach (string t in text_disassembly)
            text.text += t;
       
    }
}

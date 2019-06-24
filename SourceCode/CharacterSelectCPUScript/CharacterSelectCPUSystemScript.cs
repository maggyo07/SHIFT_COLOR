using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//Icon情報を管理する
public class CharacterSelectCPUSystemScript : MonoBehaviour
{
    //表示するため専用のIconデータ
    [System.Serializable]
    public struct DisplayIconData
    {
        Text name;        //Iconの名前
        Image mount;       //台紙
        Image icon;        //Iconの画像データ
    }
    //自身の子のIconオブジェクト情報リスト
    private List<GameObject> icon_objs;
    //選択したアイコンなどを表示するオブジェクト情報 (静的)
    public GameObject select_icon_display1;
    public GameObject select_icon_display2;
    //選択したアイコンを表示するオブジェクトの名前(子Textで表示している文) (静的)
    private string select_icon_name1;
    private string select_icon_name2;

    //アイコン情報をまとめているパネル
    public GameObject icons_panel;

    //現在どちらに選択したアイコンを表示するか
    private bool icon_display_flag = true; //true = select_icon_display1に  false = select_icon_display2

    //シーンを移動したかどうかフラグ
    private bool scene_move_flag;

    // Use this for initialization
    void Start()
    {
        //Iconオブジェクトリストを初期化
        icon_objs = new List<GameObject>();

        //Iconオブジェクト情報を取得する-------
        //全ての子の情報を取得
        var children = icons_panel.GetComponentInChildren<Transform>();
        foreach (Transform child in children)
        {
            //Iconの子(Iconは背景なのでその子を取得)
            icon_objs.Add(child.gameObject);
        }
        //-----------------------------------------------

        //プレイヤー情報を初期化する
        PlayerManagemaentScript.PlayerDataInitialization();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //子アイコンがクリックされたときに呼び出される
    public void OnClick(GameObject choice_obj)
    {
        //左クリックの時
        if (Input.GetMouseButtonUp(0))
        {
            //決定されたので使えないようにする
            choice_obj.SetActive(false);
            //選択された子アイコンが判明したので
            //選択する順番を変える
            //最後ならシーンを切り替える
            if (icon_display_flag == true)
            {
                icon_display_flag = false;
            }
            else
            {
                //シーンを移動する
                SceneManager.LoadScene("ModeCPUScene");
                //シーン移動したのでフラグをONにする
                scene_move_flag = true;
            }
        }

        //右クリックの時
        if (Input.GetMouseButtonUp(1))
        {
            //現在の画像データを取得する
            if (icon_display_flag == true)
            {
                //Verを更新する
                PlayerManagemaentScript.IconVerUpPlayer1();
                //選択したアイコンなどを表示するオブジェクト情報を更新する
                select_icon_display1.GetComponent<PlayerIconScript>().IconDataUpData();
            }
            else
            {
                //Verを更新する
                PlayerManagemaentScript.IconVerUpPlayer2();
                //選択したアイコンなどを表示するオブジェクト情報を更新する
                select_icon_display2.GetComponent<PlayerIconScript>().IconDataUpData();
            }
        }
    }

    //子アイコンとカーソルが合わさった時(最初)に呼び出される
    public void OnPointerEnter(GameObject choice_obj)
    {
        //子アイコンオブジェクト分回す
        for (int i = 0; i < icon_objs.Count; i++)
        {
            //子アイコンの誰なのかを検索する
            if (icon_objs[i] == choice_obj)
            {
                //見つかったのでその子のアイコンの画像を
                //現在選択中のselect_icon_displayの画像にする
                if (icon_display_flag == true)
                {
                    //Iconと名前情報を更新する
                    PlayerManagemaentScript.StorageNameName_and_IconInformationPlayer1(i);
                    //選択したアイコンなどを表示するオブジェクト情報を更新する
                    select_icon_display1.GetComponent<PlayerIconScript>().IconDataUpData();
                }
                else
                {
                    //Iconと名前情報を更新する
                    PlayerManagemaentScript.StorageNameName_and_IconInformationPlayer2(i);
                    //選択したアイコンなどを表示するオブジェクト情報を更新する
                    select_icon_display2.GetComponent<PlayerIconScript>().IconDataUpData();
                }
            }
        }
    }

    //子アイコンとカーソルが離れた時(最後)に呼び出される
    public void OnPointerExit()
    {
        //シーンを移動していないなら
        if (scene_move_flag == false)
        {
            //子アイコンとカーソルがあっていないときは
            //現在選択流のselect_icon_displayの画像を消す
            if (icon_display_flag == true)
            {
                //Iconと名前情報をなしにする
                PlayerManagemaentScript.StorageNameName_and_IconInformationPlayer1(-1);
                //選択したアイコンなどを表示するオブジェクト情報を更新する
                select_icon_display1.GetComponent<PlayerIconScript>().IconDataUpData();
            }
            else
            {
                //Iconと名前情報をなしにする
                PlayerManagemaentScript.StorageNameName_and_IconInformationPlayer2(-1);
                //選択したアイコンなどを表示するオブジェクト情報を更新する
                select_icon_display2.GetComponent<PlayerIconScript>().IconDataUpData();
            }
        }

    }
}

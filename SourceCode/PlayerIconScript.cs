using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerIconScript : MonoBehaviour
{
    //台紙のImage情報
    public Image mount_image;
    //IconのImage情報
    public Image icon_image;
    //名前のText情報
    public Text name_text;

    // Use this for initialization
    void Start()
    {
        //アイコン、台紙、名前データを更新する
        IconDataUpData();
        //PlayerManagemaentに自身の情報を登録する
        PlayerManagemaentScript.player_icon_scripts.Add(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //アイコン、台紙、名前データを更新する
    public void IconDataUpData()
    {
        if (name.StartsWith("Player1"))
        {
            icon_image.sprite = PlayerManagemaentScript.GetIconImageDataPlayer1();
            mount_image.sprite = PlayerManagemaentScript.GetMountImageDataPlayer1();
            name_text.text = PlayerManagemaentScript.player1_name;
        }

        if (name.StartsWith("Player2"))
        {
            icon_image.sprite = PlayerManagemaentScript.GetIconImageDataPlayer2();
            mount_image.sprite = PlayerManagemaentScript.GetMountImageDataPlayer2();
            name_text.text = PlayerManagemaentScript.player2_name;
        }
    }

    //このオブジェクトが削除されたときの呼ばれるメソッド
    void OnDestroy()
    {
        //PlayerManagemaentに自身の情報を削除する
        PlayerManagemaentScript.player_icon_scripts.Remove(this);
    }
}

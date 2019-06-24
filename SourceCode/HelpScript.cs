using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HelpScript : MonoBehaviour
{
    //説明するためのパネルを表示するためのパネル情報
    [SerializeField]
    private GameObject explanation_panel_obj;

    //項目欄オブジェクト情報
    [SerializeField]
    private GameObject item_field_obj;

    //説明するためのパネルを表示するためのパネル情報の
    //左のボタン
    [SerializeField]
    private GameObject left_button_obj;

    //説明するためのパネルを表示するためのパネル情報の
    //右のボタン
    [SerializeField]
    private GameObject right_button_obj;

    //現在のページ数
    private int page_num = 0;

    //最大ページ数
    private int max_page_num = 0;

    //説明をするためのパネルたち
    private List<GameObject> description_panels; 

    // Use this for initialization
    void Start ()
    {
        //初期化する
        description_panels = new List<GameObject>();

        //説明するためのパネルを表示するためのパネルを非表示にする
        explanation_panel_obj.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    //説明するためのパネルを表示する
    //引数1 explanation_panels_parent ：説明するためのパネルたちの親のオブジェクト情報
    private void ExplanationPanelsDisplay(GameObject explanation_panels_parent)
    {
        //すべての子を非表示にし、Listに追加する
        foreach(Transform child in explanation_panels_parent.transform)
        {
            //非表示にする
            child.gameObject.SetActive(false);
            //パネル情報を追加する
            description_panels.Add(child.gameObject);
        }

        //最大ページ数と現在のページ数を設定
        page_num = 0;
        max_page_num = description_panels.Count;

        //最初パネルだけ表示する
        description_panels[0].SetActive(true);
    }

    //左のボタンをクリックしたときに呼ばれる
    public void OnLeftButtonClick()
    {
        //現在表示ている説明パネルを非表示にする
        description_panels[page_num].SetActive(false);
        //ページを戻る
        page_num--;
        //新しく変更されたページを表示する
        description_panels[page_num].SetActive(true);

        //右のボタンを表示する
        right_button_obj.SetActive(true);

        //一番左まで行ったら左のボタンを非表示にする
        if (page_num == 0)
            left_button_obj.SetActive(false);

    }

    //右のボタンをクリックしたときに呼ばれる
    public void OnRightButtonClick()
    {
        //現在表示ている説明パネルを非表示にする
        description_panels[page_num].SetActive(false);
        //ページを進める
        page_num++;
        //新しく変更されたページを表示する
        description_panels[page_num].SetActive(true);

        //左のボタンを表示する
        left_button_obj.SetActive(true);

        //一番右まで行ったら右のボタンを非表示にする
        if (page_num == max_page_num-1)
            right_button_obj.SetActive(false);
    }

    //ヘルプボタンが押されたときに呼ばれる
    public void OnHelpButtonClick()
    {
        //既に、項目欄が表示されていたら非表示にする
        if (item_field_obj.activeSelf)
            item_field_obj.SetActive(false);
        //表示されていなければ、項目欄を表示する
        else
            item_field_obj.SetActive(true);

    }

    //戻る(X)ボタンを押されたときに呼ばれる
    public void OnBackButtonClick()
    {
        //説明するためのパネルをすべて非表示に
        foreach (GameObject obj in description_panels)
            obj.SetActive(false);

        description_panels.Clear();
        //説明するためのパネルを表示するためのパネルを非表示にする
        explanation_panel_obj.SetActive(false);
        //項目欄オブジェクトを表示する
        item_field_obj.SetActive(true);
        //ヘルプボタンを使用できるようにする
        GetComponent<Button>().interactable = true;
    }

    //項目欄のどれかを押されたときに呼ばれる
    //引数1 explanation_panels_parent ：説明するためのパネルたちの親のオブジェクト情報
    public void OnItemButtonClick(GameObject explanation_panels_parent)
    {
        //ヘルプボタンを使用できないようにする
        GetComponent<Button>().interactable = false;

        //説明するためのパネルを表示するためのパネルを表示する
        explanation_panel_obj.SetActive(true);
        //項目欄パネルを非表示にする
        item_field_obj.SetActive(false);
        //説明するためのパネルを表示する
        ExplanationPanelsDisplay(explanation_panels_parent);

        //左、右のボタンを表示するかどうかを決める---------
        left_button_obj.SetActive(false);

        if (max_page_num > 1)
            right_button_obj.SetActive(true);
        else
            right_button_obj.SetActive(false);
        //--------------------------------------------------
    }
}

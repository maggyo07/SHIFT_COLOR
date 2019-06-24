using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

//初期のマップ作製
public class EntireMapScript : MonoBehaviour {
    public int map_size = 8;           //マップのサイズ(2乗)
    //何もない色がない時のピース(マップの1つ1つ)のテクスチャ
    public Sprite normal_piece; 
    public GameObject block_prefab;     //ピース(マップの1つ1つ)
    private GameObject[,] map;          //全体のマップ

    //固有のスキル管理用構造体
    public struct UniqueSkill
    {
        string attack_player_name;  //現在攻撃中のプレイヤーの名前(HARUやCHISAKIなど)
        string put_piece_name;   //置かれたピースの名前
        int put_piece_pos_x;   //置かれたピースの位置情報X(ここを中心にスキルを発動)
        int put_piece_pos_y;   //置かれたピースの位置情報Y(ここを中心にスキルを発動)
        Sprite piece_sprite;    //スキル発動時にピースを置き換える際に上書きする画像データ
        GameObject[,] map;      //オブジェクト型のマップ情報
        int map_size;           //マップのサイズ

        //初期化関数
        //引数１ player_name   :スキルを使用したプレイヤーの名前(HARUやCHISAKIなど)
        //引数２ piece_name    :置かれたピースの名前
        //引数２ piece_pos_x   :置かれたピースの位置情報
        //引数３ piece_pos_y   :置かれたピースの位置情報
        //引数４ sprite        :スキル発動時にピースを置き換える際に上書きする画像データ
        //引数５ Map[,]        :オブジェクト型のマップ情報
        //引数６ Map_size      :マップサイズ
        public void Initialization(string player_name,string piece_name,int piece_pos_x,int piece_pos_y,Sprite sprite,GameObject[,] Map,int Map_size)
        {
            //各情報を格納する
            attack_player_name = player_name;
            put_piece_name = piece_name;
            put_piece_pos_x = piece_pos_x;
            put_piece_pos_y = piece_pos_y;
            piece_sprite = sprite;
            map = Map;
            map_size = Map_size;
        }

        //候補だけを出すときに使用する初期化関数
        //引数１ player_name   :スキルを使用したプレイヤーの名前(HARUやCHISAKIなど)
        //引数２ piece_pos_x   :置かれたピースの位置情報
        //引数３ piece_pos_y   :置かれたピースの位置情報
        //引数４ Map[,]        :オブジェクト型のマップ情報
        //引数５ Map_size      :マップサイズ
        public void Initialization(string player_name,int piece_pos_x, int piece_pos_y, GameObject[,] Map, int Map_size)
        {
            attack_player_name = player_name;
            put_piece_pos_x = piece_pos_x;
            put_piece_pos_y = piece_pos_y;
            map = Map;
            map_size = Map_size;
        }

        //スキルを発動する(引数が設定されていたら候補を引数に渡す)
        //引数がセットされていたらピースに色を移さない
        //引数1 candidate_pieces  :候補を取得したい場合は設定する
        //引数3 attack_player_obj :現在攻撃中のプレイヤーオブジェクト情報(クリックしたプレイヤー)
        //引数4 partner_player_obj:相手のプレイヤーオブジェクト情報(現在攻撃中でないプレイヤー)
        public void SkillInvocation(List<GameObject> candidate_pieces, GameObject attack_player_obj, GameObject partner_player_obj)
        {
            //色を移すかどうかのフラグ
            //true = 色を移す　false = 候補を引数に渡す
            bool shift_color_flag;

            //引数の設定状態を見てフラグのONOFFを設定する
            if (candidate_pieces == null)
                shift_color_flag = true;
            else
            {
                candidate_pieces.Clear();
                shift_color_flag = false;
            }

            //現在攻撃中、相手のプレイヤーオブジェクト情報からPlayerスクリプト情報を取得
            PlayerScript attack_player_script = null;
            if(attack_player_obj != null)
                attack_player_script = attack_player_obj.GetComponent<PlayerScript>();
            PlayerScript partner_player_script = null;
            if(partner_player_obj != null)
                partner_player_script = partner_player_obj.GetComponent<PlayerScript>();

            //現在攻撃中のプレイヤーのSC_CCゲージスクリプト情報を取得   
            SC_CC_GaugeScript SC_CC_gauge_script = null;
            if(attack_player_script != null)
                SC_CC_gauge_script = attack_player_script.SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>();
            //-----------------------プレイヤー事にスキル内容を変更--------------------
            //プレイヤー名がHARUなら「クロスブレイク」発動
            if (attack_player_name == "HARU")
            {
                //方角(上下左右)
                int[,] direction = { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
                //移すピースの位置
                int direction_x = 0;
                int direction_y = 0;
                for (int i = 0; i < 4; i++)
                {
                    direction_x = put_piece_pos_x + direction[i, 0];
                    direction_y = put_piece_pos_y + direction[i, 1];
                    while (true)
                    {
                        //マップ内かどうかを調べる
                        if (RangeDetermination(direction_x, direction_y))
                        {
                            //色移しアクションを行う
                            if (shift_color_flag)
                            {
                                //マップ内なら十字の1ピースに色を移す
                                //名前と画像を差し替える
                                map[direction_x, direction_y].name = put_piece_name;
                                map[direction_x, direction_y].GetComponent<Image>().sprite = piece_sprite;
                            }
                            //候補のデータを引数に渡す
                            else
                            {
                                //範囲内のオブジェクト情報を格納する
                                candidate_pieces.Add(map[direction_x, direction_y]);
                            }
                            direction_x += direction[i, 0];
                            direction_y += direction[i, 1];
                        }
                        else
                            break;
                    }
                }
                //色移しアクションを行ったら
                if (shift_color_flag)
                {
                    //CC分ゲージを減らす
                    SC_CC_gauge_script.UsedCC(0,1);
                }
            }
            //プレイヤー名がKAZUYAなら「ツインダイアグナル」発動
            else if (attack_player_name == "KAZUYA")
            {
                //方向情報           左斜め上    左斜め下     右斜め上  右斜め下
                int[,] direction = { { -1, 1 }, { -1, -1 }, { 1, 1 }, { 1, -1 } };
                //移すピースの位置
                int direction_x = 0;
                int direction_y = 0;
                for (int i = 0; i < 4; i++)
                {
                    direction_x = put_piece_pos_x + direction[i, 0];
                    direction_y = put_piece_pos_y + direction[i, 1];
                    while (true)
                    {
                        //マップ内かどうかを調べる
                        if (RangeDetermination(direction_x, direction_y))
                        { 
                            //色移しアクションを行う
                            if (shift_color_flag)
                            {
                                //マップ内なら十字の1ピースに色を移す
                                //名前と画像を差し替える
                                map[direction_x, direction_y].name = put_piece_name;
                                map[direction_x, direction_y].GetComponent<Image>().sprite = piece_sprite;
                            }
                            //候補のデータを引数に渡す
                            else
                            {
                                //範囲内のオブジェクト情報を格納する
                                candidate_pieces.Add(map[direction_x, direction_y]);
                            }
                            direction_x += direction[i, 0];
                            direction_y += direction[i, 1];
                        }
                        else
                            break;
                    }
                }
                //色移しアクションを行ったら
                if (shift_color_flag)
                {
                    //CC分ゲージを減らす
                    SC_CC_gauge_script.UsedCC(0, 1);
                }
            }
            //プレイヤー名がASATOなら「囲繞無双」発動
            else if (attack_player_name == "ASATO")
            {
                /*
                //方角                   右        下        左          上
                int[,] direction = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };
                //移すピースの位置
                int direction_x = put_piece_pos_x - 2;
                int direction_y = put_piece_pos_y + 2;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        //マップ内かどうかを調べる
                        if(RangeDetermination(direction_x, direction_y))
                        {
                            //色移しアクションを行う
                            if (shift_color_flag)
                            {
                                //マップ内なら十字の1ピースに色を移す
                                //名前と画像を差し替える
                                map[direction_x, direction_y].name = put_piece_name;
                                map[direction_x, direction_y].GetComponent<Image>().sprite = piece_sprite;
                            }
                            //候補のデータを引数に渡す
                            else
                            {
                                //範囲内のオブジェクト情報を格納する
                                candidate_pieces.Add(map[direction_x, direction_y]);
                            }
                        }
                        direction_x += direction[i, 0];
                        direction_y += direction[i, 1];
                    }
                }
                */
                //方角              
                int[,] direction = { { 1, 0 }};
                //移すピースの位置
                int direction_x = put_piece_pos_x - 2;
                int direction_y = put_piece_pos_y + 2;
                for (int i = 0; i < 2; i++)
                {
                    if(i == 0)
                    {
                        direction_x = 0;
                        direction_y = map_size-1;
                    }
                    else
                    {
                        direction_x = 0;
                        direction_y = 0;
                    }
                    //マップ内かどうかを調べる
                    while(RangeDetermination(direction_x, direction_y))
                    {
                        //色移しアクションを行う
                        if (shift_color_flag)
                        {
                            //マップ内なら十字の1ピースに色を移す
                            //名前と画像を差し替える
                            map[direction_x, direction_y].name = put_piece_name;
                            map[direction_x, direction_y].GetComponent<Image>().sprite = piece_sprite;
                        }
                        //候補のデータを引数に渡す
                        else
                        {
                            //範囲内のオブジェクト情報を格納する
                            candidate_pieces.Add(map[direction_x, direction_y]);
                        }
                        direction_x += direction[0, 0];
                        direction_y += direction[0, 1];
                    }
                    
                }
                //色移しアクションを行ったら
                if (shift_color_flag)
                {
                    //CC分ゲージを減らす
                    SC_CC_gauge_script.UsedCC(0, 1);
                    //自身にスキル封印の状態異常を12ターン掛ける
                    attack_player_script.state.SkliiSeal(12);
                }
            }
            //上の条件に当てはまらなかったら色移しアクションをすべて「CC」と同様にする
            else
            {
                //方角(上下左右)
                int[,] direction = new int[4, 2] { { -1, 0 }, { 1, 0 }, { 0, -1 }, { 0, 1 } };
                //移すピースの位置
                int direction_x = 0;
                int direction_y = 0;
                for (int i = 0; i < 4; i++)
                {
                    direction_x = put_piece_pos_x + direction[i, 0];
                    direction_y = put_piece_pos_y + direction[i, 1];
                    //マップ内かどうかを調べる
                    if( RangeDetermination(direction_x,direction_y))
                    { 
                        //色移しアクションを行う
                        if (shift_color_flag)
                        {
                            //マップ内なら周囲の1ピースに色を移す
                            //名前と画像を差し替える
                            map[direction_x, direction_y].name = put_piece_name;
                            map[direction_x, direction_y].GetComponent<Image>().sprite = piece_sprite;
                        }
                        else
                        {
                            //範囲内のオブジェクト情報を格納する
                            candidate_pieces.Add(map[direction_x, direction_y]);
                        }
                        
                    }
                }

                //色移しアクションを行ったら
                if(shift_color_flag)
                {
                    //プレイヤー名がCHISAKIなら「本気でいくよ！」追加効果発動
                    if (attack_player_name == "CHISAKI")
                    {
                        //SC2回分ゲージを減らす
                        SC_CC_gauge_script.UsedCC(1,0);
                        //スキルゲージ上昇率アップの状態異常をつける
                        attack_player_script.state.SkillGaugeRiseUp(2);
                    }
                    //プレイヤー名がYUKIなら「スキルシール」追加効果発動
                    else if (attack_player_name == "YUKI")
                    {
                        //SC1回分ゲージを減らす
                        SC_CC_gauge_script.UsedCC(1, 0);
                        //相手にスキル封印の状態異常を６ターン掛ける
                        partner_player_script.state.SkliiSeal(6);
                    }
                    //プレイヤー名がREANなら「クイックムーブ」追加効果発動
                    else if (attack_player_name == "REAN")
                    {
                        //ゲージは減らさない
                        SC_CC_gauge_script.UsedCC(1, 0);
                        //自身に移動数倍化の状態異常を4ターンつける
                        attack_player_script.state.QuivkMove(2,4);
                    }
                }
            }
            //---------------------------------------------------------------------
        }
        //オーバーロード：候補だけ出すとき専用
        public void SkillInvocation(List<GameObject> candidate_pieces) { SkillInvocation(candidate_pieces,null,null); }
        //オーバーロード：色を移すとき専用
        public void SkillInvocation(GameObject attack_player_obj, GameObject partner_player_obj) { SkillInvocation(null, attack_player_obj, partner_player_obj); }

        //位置情報が範囲内かどうかを判定　成功=true 失敗=false;
        public bool RangeDetermination(int pos_x, int pos_y)
        {
            if (pos_x < map_size && pos_x >= 0 &&
                pos_y < map_size && pos_y >= 0)
            {
                return true;
            }
            return false;
        }
    }
    // Use this for initialization
    void Start () {
        //マップのメモリをセット
        map = new GameObject[map_size, map_size];
        
        //マップを初期化
        for (int map_y = 0; map_y < map_size; map_y++)
            for (int map_x = 0; map_x < map_size; map_x++)
            {
                //ピース(マップの1つ1つ)を作成
                GameObject Piece;
                //マップのようになるように並べる(ローカル座標)
                Vector3 pos = new Vector3(map_x * GetComponent<RectTransform>().sizeDelta.x , map_y * GetComponent<RectTransform>().sizeDelta.y,0.0f);
                //ピース(マップの1つ1つ)を初期化 プレハブを設定
                Piece = Instantiate(block_prefab) as GameObject;
                //ピース(マップの1つ1つ)に親を設定する
                Piece.transform.SetParent(transform, false);
                //ピース(マップの１つ１つ)のサイズを調整する
                Piece.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
                //ピース(マップの１つ１つ)の子の枠のサイズを調整する
                Piece.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
                //ピース(マップの１つ１つ)の子の枠を非表示にする
                Piece.transform.GetChild(0).gameObject.SetActive(false);
                //ピース(マップの１つ１つ)の位置を調整する
                Piece.GetComponent<RectTransform>().localPosition = pos;
                
                //ピース(マップの1つ1つ)の名前を設定
                Piece.name = "Block:" + map_x + map_y;
                //テクスチャを設定
                Piece.GetComponent<Image>().sprite = normal_piece;
                //ピース(マップの1つ1つ)のボタンの設定をする
                Piece.GetComponent<Button>().onClick.AddListener(() => GetComponent<SelectSystemScript>().OnPieceClick(Piece));
                //ピース(マップの1つ1つ)のEventTriggerの設定をする
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { GetComponent<SelectSystemScript>().OnPiecePointerEnter(Piece); });
                Piece.GetComponent<EventTrigger>().triggers.Add(entry);

                //ピース(マップの1つ1つ)に親を設定したらなぜかスケール値が変わるので１に戻す
                Piece.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                //ピース(マップの1つ1つ)をマップに設定
                map[map_x, map_y] = Piece;
                
            }

        //最初のプレイヤーを配置するーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
        //各プレイヤーのピースの画像データを読み込む
        Sprite player1_piece = PlayerManagemaentScript.GetPieceImageDataPlayer1();
        Sprite player2_piece = PlayerManagemaentScript.GetPieceImageDataPlayer2();

        //プレイヤー１用にテクスチャと名前を変更
        ChangeObjctNameAndTexture(map[map_size - 1, 0], player1_piece, "Player1");
        ChangeObjctNameAndTexture(map[0, map_size - 1], player1_piece, "Player1");

        //プレイヤー2用にテクスチャと名前を変更
        ChangeObjctNameAndTexture(map[0, 0], player2_piece, "Player2");
        ChangeObjctNameAndTexture(map[map_size - 1, map_size - 1], player2_piece, "Player2");
        //－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
    }
	

	// Update is called once per frame
	void Update ()
    { 
    }

    //objctのTextureと名前を変更する
    void ChangeObjctNameAndTexture(GameObject objct,Sprite texture,string name)
    {
        objct.GetComponent<Image>().sprite = texture;
        objct.name = name;
    }

    //objctの周囲のマスで置ける場所があるのであれば周囲のオブジェクト情報をout_objctに代入する
    public void WhereYouCanDeploy(GameObject objct,List<GameObject> out_objct)
    {
        //リストの中身をクリア
        out_objct.Clear();

        //objctがマップのどの位置なのかを格納する(mapの要素数)
        int point_x=0;
        int point_y=0;

        //objctがマップのどの位置なのかを取得する
        if (!PieceMapPosition(objct, ref point_x, ref point_y))
            return; //失敗したら終了する

        //方向情報              上       下        右       左
        int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

        //objctの4周囲の移動可能な場所の情報を入れる
        for (int i = 0; i < 4; i++)
        {
            //調べる位置情報
            int findpos_x=point_x+direction[i,0], findpos_y=point_y+direction[i,1];
           
            //findposがmap_sizeを超えていなければ
            if((findpos_x < map_size && findpos_y < map_size)&&
                (findpos_x >= 0 && findpos_y >= 0))
            {
                //設置予定objct
                GameObject rcn_objct = map[findpos_x, findpos_y];
                //名前がBlockで始まるオブジェクトなら置けるマスとしてListにアップする
                if (rcn_objct.name.StartsWith("Block"))
                    out_objct.Add(rcn_objct);
            }
        }
    }

    //SCかCCを使用する
    //引数1 SC_or_CC          :true = SCを使う   false = CCを使う
    //引数2 click_obj         :クリック(選択)したオブジェクト情報
    //引数3 attack_player_obj :現在攻撃中のプレイヤーオブジェクト情報(クリックしたプレイヤー)
    //引数4 partner_player_obj:相手のプレイヤーオブジェクト情報(現在攻撃中でないプレイヤー)
    //引数5 SC_CC_gauge_script:現在攻撃中のプレイヤーのSC_CCゲージスクリプト情報
    public void TheyUseSC_or_CC(bool SC_or_CC,GameObject click_obj,GameObject attack_player_obj,GameObject partner_player_obj)
    {
        //ピースを置き換えるときの名前(プレイヤー名)
        string piece_player_name = click_obj.name;

        //ピースを置き換えるときの画像data(プレイヤーの画像data)
        Sprite player_sprite = click_obj.GetComponent<Image>().sprite;

        //クリックしたオブジェクトが全体マップ配列のどこにあるのかの位置情報(要素数)
        int pos_x=0, pos_y=0;

        //クリックしたオブジェクトが全体マップ配列のどこにあるのかの位置情報(要素数)
        PieceMapPosition(click_obj,ref pos_x,ref pos_y);

        //SCが選択されていたらSCを使用する
        //技：クリックした周囲のピースに色を移す(十字方向に1マスずつ)
        if(SC_or_CC == true)
        {
            //pos_x,pos_yを中心に4方向を調べて、マップ内であれば色を移す
            //方角(上下左右)
            int[,] direction = new int[4, 2] { { -1,0}, {1,0}, { 0,-1}, { 0,1} };
            //移すピースの位置
            int direction_x = 0;
            int direction_y = 0;
            for (int i = 0; i < 4; i++)
            {
                direction_x = pos_x + direction[i, 0];
                direction_y = pos_y + direction[i, 1];
                //マップ内かどうかを調べる
                if (direction_x >= 0 &&
                   direction_x < map_size)
                {
                    if(direction_y >= 0 &&
                       direction_y < map_size)
                    {
                        //マップ内なら周囲の1ピースに色を移す
                        //名前と画像を差し替える
                        map[direction_x, direction_y].name = piece_player_name;
                        map[direction_x, direction_y].GetComponent<Image>().sprite = player_sprite;
                    }
                }
            }

            //現在攻撃中のプレイヤーのSC_CCゲージスクリプト情報を取得する
            SC_CC_GaugeScript SC_CC_gauge_script = attack_player_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>();
            //SCを使用したのでゲージの状態を更新する
            SC_CC_gauge_script.UsedSC();
        }
        //CCが選択されていたらCCを使用する
        else
        {
            //ピースを置いたプレイヤーの名前
            string player_name;
            //ピースを置いたプレイヤーの名前を取得
            if (piece_player_name == "Player1")
                player_name = PlayerManagemaentScript.player1_name;
            else
                player_name = PlayerManagemaentScript.player2_name;

            //固有のスキル発動のための構造体
            UniqueSkill unique_skill = new UniqueSkill();
            //固有のスキル構造体の初期化
            unique_skill.Initialization(player_name, piece_player_name, pos_x, pos_y, player_sprite, map, map_size);
            //固有のスキルを発動する
            unique_skill.SkillInvocation(null,attack_player_obj,partner_player_obj);

        }
    }

    //終了条件に一致しているかどうか
    public bool ExitConditions()
    {
        //マップ内にBlockが存在してれば終了条件はしない
        for (int map_y = 0; map_y < map_size; map_y++)
        {
            for (int map_x = 0; map_x < map_size; map_x++)
            {
                if (map[map_y, map_x].name.StartsWith("Block"))
                    return false;
            }
        }
        return true;
    }

    //現在の各プレイヤーのピースの数を渡す
    public void PlayerPieceNum(ref int player1_piece, ref int player2_piece)
    {
        //ピースの数を初期化
        player1_piece = 0;
        player2_piece = 0;

        for (int map_y = 0; map_y < map_size; map_y++)
        {
            for (int map_x = 0; map_x < map_size; map_x++)
            {
                if (map[map_y, map_x].name.StartsWith("Player1"))
                    player1_piece++;
                if (map[map_y, map_x].name.StartsWith("Player2"))
                    player2_piece++;
            }
        }
    }

    //パス判定を行う
    //引数1　player_name   :パス判定を行うピースの名前(プレイヤーネーム)
    //戻り値 bool  :　true＝パス判定あり   false＝パス判定なし
    //マップ全体からplayer_nameが最初にあるピースを探し、なければパス判定ありでtrueを返す
    //それ以外はfalse
    public bool PathJudgment(string player_name)
    {
        
        for (int map_y = 0; map_y < map_size; map_y++)
        {
            for (int map_x = 0; map_x < map_size; map_x++)
            {
                if (map[map_y, map_x].name.StartsWith(player_name))
                {
                    //周囲におけるブロックがあるかどうかを調べる
                    List<GameObject> circumference_obj = new List<GameObject>();
                    WhereYouCanDeploy(map[map_y, map_x], circumference_obj);
                    //周囲におけるブロックがあるならfalseを返す
                    if (circumference_obj.Count > 0)
                        return false;
                }
            }
        }
        //全て調べてもなければパスをする
        Debug.Log(player_name + "はパスだよ～");
        return true;
    }

    //引数のnameに一致するpieseオブジェクト情報を返す
    public List<GameObject> GetMatchNameObject(string name)
    {
        //ピースオブジェクト情報を格納する用
        List<GameObject> piese_obj_list = new List<GameObject>();
        
        for(int map_y = 0; map_y < map_size; map_y++)
        {
            for(int map_x = 0; map_x < map_size; map_x++)
            {
                //ピースオブジェクトネームと引数のnameが一致したら格納
                if (map[map_x, map_y].name == name)
                    piese_obj_list.Add(map[map_x, map_y]);
            }
        }
        //ピースが格納できたので返す
        return piese_obj_list;
    }

    //引数のpiece_objの周囲のオブジェクト情報を引数のcircumference_objsに格納する
    //引数1 piece_obj             :このオブジェクトの周囲のオブジェクトを返す   
    //引数2 circumference_objs    :piece_objの周囲のオブジェクト情報を格納する用
    //戻り値   bool            ：引数のpiece_objがマップの端かどうか
    public bool GetPieceCircumference(GameObject piece_obj,List<GameObject> circumference_objs)
    {
        //初期化をする
        circumference_objs.Clear();

        for(int map_y = 0; map_y < map_size; map_y++)
        {
            for(int map_x = 0; map_x < map_size; map_x++)
            {
                //オブジェクト情報が一致していたら
                if(map[map_x,map_y] == piece_obj)
                {
                    //方向情報                上 　　下     右       左
                    int[,] direction = { { 0,1 }, { 0,-1 }, { 1,0 }, { -1,0 } };

                    //４方向を調べる
                    for(int i = 0; i < 4; i++)
                    {
                        //調べる方向
                        int direction_x = map_x + direction[i, 0];
                        int direction_y = map_y + direction[i, 1];

                        //調べる場所がマップ内なら
                        if(direction_x >= 0 && direction_x < map_size &&
                           direction_y >= 0 && direction_y < map_size)
                        {
                            //周囲のオブジェクト情報を格納する
                            circumference_objs.Add(map[direction_x, direction_y]);
                        }
                    }

                    //piece_objがマップの端ならtrueを返す
                    if (map_x == map_size - 1 || map_x == 0 ||
                       map_y == map_size - 1 || map_y == 0)
                        return true;
                    //端でないならfalseを返す
                    else
                        return false;
                }
            }
        }

        return false;//処理はされない
    }

    //SCが発動した時の範囲をcandidate_pieceに入れる
    public void SCActivationRange(GameObject piece, List<GameObject> candidate_pieces)
    {
        //candidate_piecesを初期化する
        candidate_pieces.Clear();

        //pieceがマップのどの位置なのか情報
        int point_x = 0, point_y = 0;
        //pieceがマップのどの位置にあるかを調べる
        if (PieceMapPosition(piece, ref point_x, ref point_y))
        {
            //方向情報              上       下        右          左
            int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            //objctの4周囲の移動可能な場所の情報を入れる
            for (int i = 0; i < 4; i++)
            {
                //調べる位置情報
                int findpos_x = point_x + direction[i, 0], findpos_y = point_y + direction[i, 1];

                //findposがmap_sizeを超えていなければ
                if ((findpos_x < map_size && findpos_y < map_size) &&
                    (findpos_x >= 0 && findpos_y >= 0))
                {
                    candidate_pieces.Add(map[findpos_x,findpos_y]);
                }
            }
        }
    }

    //CCが発動した時の範囲をcandidate_pieceに入れる
    public void CCActivationRange(GameObject piece, List<GameObject> candidate_pieces)
    {
        //candidate_piecesを初期化する
        candidate_pieces.Clear();

        //pieceがマップのどの位置なのか情報
        int pos_x = 0, pos_y = 0;

        //固有のスキルの候補を取得するための構造体
        UniqueSkill unique_skill = new UniqueSkill();

        //現在攻撃中のプレイヤーの名前(Player1など)を取得
        string attack_player_name = GetComponent<SelectSystemScript>().turn_system_script.attack_prayer_name;

        //現在攻撃中のプレイヤーのキャラ名
        string player_name;
        //現在攻撃中のプレイヤーのキャラ名
        if (attack_player_name == "Player1")
            player_name = PlayerManagemaentScript.player1_name;
        else
            player_name = PlayerManagemaentScript.player2_name;

       
        //pieceがマップのどの位置にあるかを調べる
        if (PieceMapPosition(piece, ref pos_x, ref pos_y))
        {
            //固有のスキルの候補を取得するための構造体の初期化
            unique_skill.Initialization(player_name, pos_x, pos_y,map,map_size);

            //候補を取得
            unique_skill.SkillInvocation(candidate_pieces);
        }
    }

    //pieceオブジェクトがmapのどの場所にあるのかをmap_x,map_yに代入する
    //戻り値 bool  :成功か失敗か
    private bool PieceMapPosition(GameObject piece,ref int pos_x,ref int pos_y)
    {
        for(int map_y = 0;map_y < map_size; map_y++)
        {
            for(int map_x = 0; map_x < map_size; map_x++)
            {
                //オブジェクト情報が一致していたらtrueを返す
                if(map[map_x,map_y] == piece)
                {
                    pos_x = map_x;
                    pos_y = map_y;
                    return true;
                }
            }
        }

        return false;
    }
    public GameObject[,] GetMapObjs()
    {
        return map;
    }

    //位置情報からピースオブジェクトを取得
    public GameObject GetPiece(int map_x,int map_y)
    {
        return map[map_x, map_y];
    }
}

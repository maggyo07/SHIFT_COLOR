using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//クリックしたオブジェクトをわかりやすく表示する
public class SelectSystemScript : MonoBehaviour
{
    private GameObject hit_player_obj;  //クリックしたプレイヤー(自身の)情報格納用
    private GameObject hit_piece_obj=null;   //クリックしたピースオブジェクト情報(プレイヤーピースの可能性あり)
    private List<GameObject> around_piece;  //クリックしたプレイヤーの周囲のピース情報
    private List<GameObject> candidate_piece;   //スキル(SC/CC)ボタンがONの時、around_pieceのどれかにポインタが乗っている時、スキルを実際に使用した時に移すピースたち
    public TurnSystemScript turn_system_script;    //ターンシステムスクリプト情報
    public GameObject player1_obj;                  //Player1のオブジェクト情報
    public GameObject player2_obj;                  //Player2のオブジェクト情報
    public bool system_flag = false;                //システム(処理)をするかどうかのフラグ
    public GameObject game_end_panel_obj;               //ゲームが終了したときに表示するパネル(Text)
    public GameObject game_progress_obj;                //ゲームの進行状況を表示するオブジェクト
    private bool AI_flag;                                //AIを実装するかどうか
    private bool AI_execution_flag = true;                     //AIを実行するかどうか(コルーチンを使うため用)

    //AI専用構造体-----------------------------------
    public struct GaugeState
    {
        public int SC_accumulated_turn;     //１つのSCがどれくらい溜まっているか
        public int SC_num;                  //使用可能SCの数(現在CCがいくつ溜まっているか)
        public int max_SC_accumulated_tane; //何ターンでSCが1つ溜まるのか
        public int max_SC_num;              //SCゲージの数(SCの溜まる数)
        public bool skill_use_flag;         //スキルを使用できる状態かどうか

        //初期化(scriptを元に初期化をする)
        public void Initialization(SC_CC_GaugeScript script)
        {
            SC_accumulated_turn = script.SC_accumulated_tane;
            SC_num = script.SC_num;
            max_SC_accumulated_tane = script.max_SC_accumulated_tane;
            max_SC_num = script.max_SC_num;
            skill_use_flag = script.skill_use_flag;
        }

        //ゲージの状態を1ターン前に戻す
        public void GaugeStateReturn()
        {
            if (SC_accumulated_turn > 0)
            {
                SC_accumulated_turn--;
            }
            else
            {
                SC_num--;
                SC_accumulated_turn = max_SC_accumulated_tane - 1;
            }
        }

        //ゲージの状態を進める
        public void GaugeStateToProceed()
        {
            if (SC_accumulated_turn < max_SC_accumulated_tane - 1)
            {
                SC_accumulated_turn++;
            }
            else
            {
                if (SC_num != max_SC_num)
                {
                    SC_accumulated_turn = 0;
                    SC_num++;
                }
            }
        }
    }
   
    public struct AIPiece
    {
        public int pos_x, pos_y;   //ピースの位置(マップの要素数)
        public int id;             //ピースのID(player1＝１　player2＝２　その他＝０)
        public int previous_id;     //置き換える前のID

        //位置情報が範囲内かどうかを判定　成功=true 失敗=false;
        public bool RangeDetermination(int map_size)
        {
            if (pos_x < map_size && pos_x >= 0 &&
               pos_y < map_size && pos_y >= 0)
                return true;

            return false;
        }

        //位置がマップの端なのかどうか 端=true 違う=false
        public bool MapEndPosition(int map_size)
        {
            if (pos_x == 0 || pos_x == map_size - 1 ||
                pos_y == 0 || pos_y == map_size - 1)
                return true;

            return false;
        }
    }

    //αβ法
    public struct AIAlphaBeta
    {
        //マップ情報
        public int[,] map;
        //マップのサイズ
        public int map_size;
        //αβ法をするよう
        public int alpha, beta;
        //初期化
        public void Initialization(GameObject[,] map_objs, int map_objs_size)
        {
            //alpha,betaの値を初期化する
            alpha = -999;
            beta = 999;

            //マップサイズを取得
            map_size = map_objs_size;
            //マップ情報初期化
            map = new int[map_objs_size, map_objs_size];

            //Gameobject型のマップ情報をint型のマップ情報に変換する
            for (int map_y = 0; map_y < map_size; map_y++)
            {
                for (int map_x = 0; map_x < map_size; map_x++)
                {
                    //オブジェネームがPlayer1なら１を代入
                    if (map_objs[map_x, map_y].name == "Player1")
                        map[map_x, map_y] = 1;
                    //オブジェネームがPlayer2なら２を代入
                    else if (map_objs[map_x, map_y].name == "Player2")
                        map[map_x, map_y] = 2;
                    //オブジェネームがPlayer1、Player２でもないなら０を代入
                    else
                        map[map_x, map_y] = 0;
                }
            }
        }

        //piece情報を元にMap情報を戻す
        public void MapReturn(List<AIPiece> pieces)
        {
            for(int piece_count = 0; piece_count < pieces.Count; piece_count++)
            {
                map[pieces[piece_count].pos_x, pieces[piece_count].pos_y] = pieces[piece_count].previous_id;
            }
            //マップを元に戻したのでクリアする
            pieces.Clear();
        }

        //局面を変更する関数
        public void ChangeTheAspect(AIPiece put_piece, int id, bool SC_flag, bool CC_flag,List<AIPiece> pieces)
        {
            //piece情報を初期化
            pieces.Clear();

            //まず、置いたとこの情報を変える
            put_piece.id = id;
            put_piece.previous_id = map[put_piece.pos_x, put_piece.pos_y];
            map[put_piece.pos_x, put_piece.pos_y] = id;
            //置いたのでpieceにアップする
            pieces.Add(put_piece);

            //方向情報              上         下        右          左
            int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            //SCを使う際の局面変更
            if (SC_flag)
            {
                for (int i = 0; i < 4; i++)
                {
                    //調べる方向
                    AIPiece find = new AIPiece();
                    find.pos_x = put_piece.pos_x + direction[i, 0];
                    find.pos_y = put_piece.pos_y + direction[i, 1];

                    //調べる方向がマップの範囲内なら
                    if (find.RangeDetermination(map_size))
                    {
                        //局面を変える
                        find.id = id;
                        find.previous_id = map[find.pos_x, find.pos_y];
                        map[find.pos_x, find.pos_y] = id;
                        pieces.Add(find);
                    }
                }
            }

            if (CC_flag)
            {
                for (int i = 0; i < 4; i++)
                {
                    //調べる方向
                    AIPiece find = new AIPiece();
                    find.pos_x = put_piece.pos_x + direction[i, 0];
                    find.pos_y = put_piece.pos_y + direction[i, 1];

                    while (true)
                    {
                        find.pos_x += direction[i, 0];
                        find.pos_y += direction[i, 1];

                        //調べる方向がマップの範囲内なら
                        if (find.RangeDetermination(map_size))
                        {
                            //局面を変える
                            find.id = id;
                            find.previous_id = map[find.pos_x, find.pos_y];
                            map[find.pos_x, find.pos_y] = id;
                            pieces.Add(find);
                        }
                        else
                            break;
                    }
                }
            }
        }

        //put_pieceの周囲のマスで置ける場所(IDが０)があるのであれば周囲のオブジェクト情報をcircumference_piecesに代入する
        public void WhereYouCanDeploy(AIPiece piece, List<AIPiece> circumference_pieces)
        {

            //circumference_piecesを初期化
            circumference_pieces.Clear();
            //方向情報              上         下        右          左
            int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
            for (int i = 0; i < 4; i++)
            {
                //調べる方向
                AIPiece findpos = new AIPiece();
                findpos.pos_x = piece.pos_x + direction[i, 0];
                findpos.pos_y = piece.pos_y + direction[i, 1];
                //範囲内かどうかを調べる
                if (findpos.RangeDetermination(map_size))
                {
                    //idを代入する
                    findpos.id = map[findpos.pos_x, findpos.pos_y];
                    if (findpos.id == 0)
                        //範囲内であれば追加する
                        circumference_pieces.Add(findpos);
                }
            }

            for (int i = 0; i < circumference_pieces.Count; i++)
            {
                //Idが０でないピース情報があれば削除
                if (circumference_pieces[i].id != 0)
                    Debug.Log("周囲のピース情報に変なもんが混入してます");
            }
        }

        //引数のpieceの周囲のオブジェクト情報を引数のcircumference_piecesに格納する
        //引数3 piece                     :このオブジェクトの周囲のオブジェクトを返す   
        //引数4 circumference_pieces      :pieceの周囲のオブジェクト情報を格納する用
        public void GetPieceCircumference(AIPiece piece, List<AIPiece> circumference_pieces)
        {
            //circumference_piecesを初期化
            circumference_pieces.Clear();
            //方向情報              上         下        右          左
            int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
            for (int i = 0; i < 4; i++)
            {
                //調べる方向
                AIPiece findpos = new AIPiece();
                findpos.pos_x = piece.pos_x + direction[i, 0];
                findpos.pos_y = piece.pos_y + direction[i, 1];
                //範囲内かどうかを調べる
                if (findpos.RangeDetermination(map_size))
                {
                    //idを代入する
                    findpos.id = map[findpos.pos_x, findpos.pos_y];
                    //範囲内であれば追加する
                    circumference_pieces.Add(findpos);
                }
            }
        }

        //引数のpieceの位置でCCを発動時に置くピース情報を引数のSC_invocation_piecesに格納する
        //引数3 piece                     :このピースの位置に置いた場合   
        //引数4 CC_invocation_pieces      :pieceの位置でCCを発動時に置くピース情報を格納する用
        public void GetCCInvocationPieces(AIPiece piece, List<AIPiece> CC_invocation_pieces)
        {
            //SC_invocation_piecesの初期化
            CC_invocation_pieces.Clear();
            //方向情報              上         下        右          左
            int[,] direction = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };
            for (int i = 0; i < 4; i++)
            {
                //調べる方向
                AIPiece findpos = new AIPiece();
                findpos.pos_x = piece.pos_x + direction[i, 0];
                findpos.pos_y = piece.pos_y + direction[i, 1];

                while (true)
                {
                    //範囲外であれば終了
                    if (!findpos.RangeDetermination(map_size))
                    {
                        break;
                    }
                    //idを代入する
                    findpos.id = map[findpos.pos_x, findpos.pos_y];
                    //リストに追加する
                    CC_invocation_pieces.Add(findpos);
                    //範囲を進める
                    findpos.pos_x += direction[i, 0];
                    findpos.pos_y += direction[i, 1];
                }
            }
        }

        //特定のピースをIDを元に探しpiecesにアップする
        //引数1 map       :マップ情報
        //引数2 map_size  :マップのサイズ(X,Yともに同じとする)
        //引数3 id        :ピースのID(Player1=1 Player2=2 その他=0)
        //引数4 pieces    :発見したピースたち
        public void FindASpecificPiece(int id, List<AIPiece> pieces)
        {
            //piecesを初期化する
            pieces.Clear();

            for (int map_y = 0; map_y < map_size; map_y++)
            {
                for (int map_x = 0; map_x < map_size; map_x++)
                {
                    if (map[map_x, map_y] == id)
                    {
                        AIPiece piece = new AIPiece();
                        piece.pos_x = map_x;
                        piece.pos_y = map_y;
                        piece.id = id;

                        pieces.Add(piece);
                    }
                }
            }
        }

        //置ける場所があるかどうか判定する　　名前は仮(いい名前が思いついた人はつけてあげてね)
        public bool WhetherThereIsAPlaceToPlace(int id)
        {
            //プレイヤーのピース
            List<AIPiece> player_pieces = new List<AIPiece>();
            //周囲の置けるところのピース情報格納用
            List<AIPiece> circumference_pieces = new List<AIPiece>();

            //idのピースを探し、player_piecesに入れる
            FindASpecificPiece(id, player_pieces);

            for (int player_count = 0; player_count < player_pieces.Count; player_count++)
            {

                //周囲の置けるところのピース情報を取得
                WhereYouCanDeploy(player_pieces[player_count], circumference_pieces);

                if (circumference_pieces.Count != 0)
                    return true;
            }

            return false;
        }

        //mapにあるidの数を返す 評価をする
        //引数1 id        :mapにあるidの数を返す
        //引数2 pieces    :おけないだけでまだ空白が残っているかもなのでそれを置き換えた時の情報を追加するList
        public int Evaluation(int id,List<AIPiece> pieces)
        {
            List<AIPiece> id_pieces = new List<AIPiece>();

            //何も色が移されていないピースをすべて埋める
            if(id == 1)
                EmptyPieceAllFill(2,pieces);
            else
                EmptyPieceAllFill(1,pieces);

            //全てのidピース情報を取得
            FindASpecificPiece(id, id_pieces);

            //idピースがマップに何個あったかを返す
            return id_pieces.Count;
        }

        //何も色が移されていないピースをすべて引数idにする
        public void EmptyPieceAllFill(int id,List<AIPiece> pieces)
        {
            //何も色が移されていないピースを埋める
            for (int map_y = 0; map_y < map_size; map_y++)
            {
                for (int map_x = 0; map_x < map_size; map_x++)
                {
                    if (map[map_x, map_y] == 0)
                    {
                        //置き換えるピースの情報
                        AIPiece replace_piece = new AIPiece();

                        //置き換えるピースの情報を設定
                        replace_piece.id = id;
                        replace_piece.pos_x = map_x;
                        replace_piece.pos_y = map_y;
                        replace_piece.previous_id = map[map_x, map_y];
                        map[map_x, map_y] = replace_piece.id;
                        //情報をセットする
                        pieces.Add(replace_piece);
                    }
                }
            }
        }

        //MinMaxのMinの処理
        //引数1 level         :先読みレベル
        //引数2 player1_gauge :player1のゲージ状態
        //引数3 player2_gauge :player2のゲージ状態
        //引数4 SC_flag       :SCを使うかどうか
        //引数5 CC_flag       :CCを使うかどうか
        //引数6 turn_change_flag  :ターンを進めるかどうか
        //引数7 put_piece     :置くピース(場所)


        public int mmMin(int level, GaugeState player1_gauge, GaugeState player2_gauge, bool SC_flag, bool CC_flag, bool turn_change_flag, AIPiece put_piece, int alpha, int beta)
        {
            //置き換えた場所を記憶するList
            List<AIPiece> place_replaced = new List<AIPiece>();

            //評価値を出す(置いた場所の評価値) Minなので　*-1をする
            int evaluation_value = DecideTheEvaluationValue(put_piece, 1, SC_flag, CC_flag)*-1;

            //α値をセット
            alpha = evaluation_value;

            //局面を変更、保存する
            ChangeTheAspect(put_piece, 1, SC_flag, CC_flag, place_replaced);
            //先読みレベルが最後またわ
            //置ける場所がなければ
            //評価を返す
            if (level == 0 || !WhetherThereIsAPlaceToPlace(2))
            {
                //マップ情報を戻す
                MapReturn(place_replaced);
                return evaluation_value;//評価を返す
            }

            //ターンが変更されたらゲージを進める
            if (turn_change_flag)
            {
                player1_gauge.GaugeStateToProceed();
                player2_gauge.GaugeStateToProceed();
            }

            //プレイヤーのピース
            List<AIPiece> player_pieces = new List<AIPiece>();

            //最も評価が高い値
            int max = -999;
            //評価値格納用
            int value = 0;

            //Player2のピースを探し、player_piecesに入れる
            FindASpecificPiece(2, player_pieces);

            for (int player_count = 0; player_count < player_pieces.Count; player_count++)
            {
                //周囲の置けるところのピース情報格納用
                List<AIPiece> circumference_pieces = new List<AIPiece>();
                //周囲の置けるところのピース情報を取得
                WhereYouCanDeploy(player_pieces[player_count], circumference_pieces);

                for (int circumference_count = 0; circumference_count < circumference_pieces.Count; circumference_count++)
                {
                   
                    //SCが使える状態なら
                    //使う判定で判定
                    if (player2_gauge.SC_num != 0 && player2_gauge.skill_use_flag == true)
                    {
                        value = mmMax(level - 1, player1_gauge, player2_gauge, true, false, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);
                        if (value > max)
                            max = value;
                        //βカット---------------------------
                        if (value > beta)
                        {
                            //評価を得たのでマップ情報を戻す
                            MapReturn(place_replaced);
                            //過去の評価値と現在の評価値の合計を返す
                            return max + evaluation_value;
                        }
                        else if (value < beta)
                        {
                            beta = value;
                        }
                        //-----------------------------------
                    }
                    //CCが使える状態なら
                    //使う判定で判定
                    if (player2_gauge.SC_num == player2_gauge.max_SC_num && player2_gauge.skill_use_flag == true)
                    {
                        value = mmMax(level - 1, player1_gauge, player2_gauge, false, true, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);
                        if (value > max)
                            max = value;
                        //βカット---------------------------
                        if (value > beta)
                        {
                            //評価を得たのでマップ情報を戻す
                            MapReturn(place_replaced);
                            //過去の評価値と現在の評価値の合計を返す
                            return max + evaluation_value;
                        }
                        else if (value < beta)
                        {
                            beta = value;
                        }
                        //-----------------------------------
                    }

                    //SC、CCを使わない判定で判定
                    value = mmMax(level - 1, player1_gauge, player2_gauge, false, false, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);

                    if (value > max)
                        max = value;
                    //βカット---------------------------
                    if (value > beta)
                    {
                        //評価を得たのでマップ情報を戻す
                        MapReturn(place_replaced);
                        //過去の評価値と現在の評価値の合計を返す
                        return max + evaluation_value;
                    }
                    else if (value < beta)
                    {
                        beta = value;
                    }
                    //-----------------------------------
                }
            }
            //評価を得たのでマップ情報を戻す
            MapReturn(place_replaced);

            //過去の評価値と現在の評価値の合計を返す
            return max + evaluation_value;
        }

        //MinMaxのMaxの処理
        //引数3 level         :先読みレベル
        //引数4 player1_gauge :player1のゲージ状態
        //引数5 player2_gauge :player2のゲージ状態
        //引数6 SC_flag       :SCを使うかどうか
        //引数7 CC_flag       :CCを使うかどうか
        //引数8 turn_change_flag  :ターンを進めるかどうか
        //引数9 put_piece     :置くピース(場所)


        public int mmMax(int level, GaugeState player1_gauge, GaugeState player2_gauge, bool SC_flag, bool CC_flag, bool turn_change_flag, AIPiece put_piece, int alpha, int beta)
        {
            //置き換えた場所を記憶するList
            List<AIPiece> place_replaced = new List<AIPiece>();

            //評価値を出す(置いた場所の評価値)
            int evaluation_value = DecideTheEvaluationValue(put_piece, 2, SC_flag, CC_flag);

            //β値をセット
            beta = evaluation_value;

            //局面を変更する
            ChangeTheAspect(put_piece, 2, SC_flag, CC_flag, place_replaced);


            //先読みレベルが最後またわ
            //置ける場所がなければ
            //評価を返す
            if (level == 0 || !WhetherThereIsAPlaceToPlace(1))
            {
                //マップ情報を戻す
                MapReturn(place_replaced);
                return evaluation_value;//評価を返す
            }

            //ターンが変更されたらゲージを進める
            if (turn_change_flag)
            {
                player1_gauge.GaugeStateToProceed();
                player2_gauge.GaugeStateToProceed();
            }

            //プレイヤーのピース
            List<AIPiece> player_pieces = new List<AIPiece>();

            //最も評価が高い値
            int min = 999;
            //評価値格納用
            int value = 0;

            //Player1のピースを探し、player_piecesに入れる
            FindASpecificPiece(1, player_pieces);

            for (int player_count = 0; player_count < player_pieces.Count; player_count++)
            {
                //周囲の置けるところのピース情報格納用
                List<AIPiece> circumference_pieces = new List<AIPiece>();
                //周囲の置けるところのピース情報を取得
                WhereYouCanDeploy(player_pieces[player_count], circumference_pieces);

                for (int circumference_count = 0; circumference_count < circumference_pieces.Count; circumference_count++)
                {
                   
                    //SCが使える状態なら
                    //使う判定で判定
                    if (player1_gauge.SC_num != 0 && player1_gauge.skill_use_flag == true)
                    {
                        value = mmMin(level - 1, player1_gauge, player2_gauge, true, false, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);
                        if (value < min)
                            min = value;
                        //αカット------------------------------------
                        if (value < alpha)
                        {
                            //評価を得たのでマップ情報を戻す
                            MapReturn(place_replaced);
                            //過去の評価値と現在の評価値の合計を返す
                            return min + evaluation_value;
                        }
                        else if (value > alpha)
                        {
                            alpha = value;
                        }
                        //--------------------------------------------
                    }
                    //CCが使える状態なら
                    //使う判定で判定
                    if (player1_gauge.SC_num == player1_gauge.max_SC_num && player1_gauge.skill_use_flag == true)
                    {
                        value = mmMin(level - 1, player1_gauge, player2_gauge, false, true, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);
                        if (value < min)
                            min = value;
                        //αカット------------------------------------
                        if (value < alpha)
                        {
                            //評価を得たのでマップ情報を戻す
                            MapReturn(place_replaced);
                            //過去の評価値と現在の評価値の合計を返す
                            return min + evaluation_value;
                        }
                        else if (value > alpha)
                        {
                            alpha = value;
                        }
                        //--------------------------------------------
                    }

                    //SC、CCを使わない判定で判定
                    value = mmMin(level - 1, player1_gauge, player2_gauge, false, false, !turn_change_flag, circumference_pieces[circumference_count], alpha, beta);

                    if (value < min)
                        min = value;
                    //αカット------------------------------------
                    if (value < alpha)
                    {
                        //評価を得たのでマップ情報を戻す
                        MapReturn(place_replaced);
                        //過去の評価値と現在の評価値の合計を返す
                        return min + evaluation_value;
                    }
                    else if (value > alpha)
                    {
                        alpha = value;
                    }
                    //--------------------------------------------
                }
            }
            //評価を得たのでマップ情報を戻す
            MapReturn(place_replaced);

            //過去の評価値と現在の評価値の合計を返す
            return min + evaluation_value;
        }

        //評価値を計算する
        //引数1 put_piece         :置く場所のピース情報
        //引数2 id                :自身のID(置いたプレイヤーid)
        //引数2 SC_flag           :SCを使うかどうか
        //引数3 CC_flag           :CCを使うかどうか
        //戻り値 int  :最終結果の評価値を返す
        private int DecideTheEvaluationValue(AIPiece put_piece,int id, bool SC_flag, bool CC_flag)
        {
            //最終的な評価値
            int evaluation_value = 0;

            //引数のput_pieceの周囲のピース情報格納用
            List<AIPiece> circumference_pieces = new List<AIPiece>();
            //put_pieceの周囲のピース情報を格納
            GetPieceCircumference(put_piece, circumference_pieces);

            //周囲の自身のピース数
            int circumference_own_piece_num = 0;
            //周囲の相手のピース数
            int circumference_partner_piece_num = 0;
            //周囲の何もないピース数
            int circumference_nothing_piece_num = 0;

            //周囲にあるピース分回す
            //周りにある自身のピース数と相手のピース数を数える
            for (int count = 0; count < circumference_pieces.Count; count++)
            {
                //自身のピース
                if (circumference_pieces[count].id == id)
                    circumference_own_piece_num++;
                //相手のピース
                else if (circumference_pieces[count].id != 0)
                    circumference_partner_piece_num++;
                //何もないピース
                else
                    circumference_nothing_piece_num++;
            }


            //通常通りピースを置く場合の評価値を計算する
            if (!SC_flag && !CC_flag)
            {
                //周囲にあるピース数によって評価を変えるーーーーーーー
                //マップの端の時の評価
                if (put_piece.MapEndPosition(map_size))
                {
                    //マップの端かつ周囲の自身のピース数によって評価を変える
                    switch (circumference_own_piece_num)
                    {
                        case 1:
                            evaluation_value -= 2;
                            break;
                        case 2:
                            evaluation_value -= 25;
                            break;
                        case 3:
                            evaluation_value -= 50;
                            break;
                    }

                    //マップの端かつ周囲の相手のピース数によって評価を変える
                    switch (circumference_partner_piece_num)
                    {
                        case 1:
                            evaluation_value += 5;
                            break;
                        case 2:
                            evaluation_value -= 40;
                            break;
                    }

                    //マップの端かつ周囲の何もないピース数によって評価を変える
                    switch (circumference_nothing_piece_num)
                    {
                        case 1:
                            evaluation_value -= 2;
                            break;
                        case 2:
                            evaluation_value -= 3;
                            break;
                    }
                }
                //マップの端でない場合の評価
                else
                {
                    //置くところの周囲の自身のピース数によって評価を変える
                    switch (circumference_own_piece_num)
                    {
                        case 2:
                            evaluation_value -= 5;
                            break;
                        case 3:
                            evaluation_value -= 25;
                            break;
                        case 4:
                            evaluation_value -= 50;
                            break;
                    }

                    //置くところの周囲の相手のピース数によって評価を変える
                    switch (circumference_partner_piece_num)
                    {
                        case 1:
                            evaluation_value += 8;
                            break;
                        case 2:
                            evaluation_value += 5;
                            break;
                        case 3:
                            evaluation_value -= 40;
                            break;
                    }

                    //置くところの周囲の何もないピース数によって評価を変える
                    switch (circumference_nothing_piece_num)
                    {
                        case 1:
                            evaluation_value -= 2;
                            break;
                        case 2:
                            evaluation_value -= 3;
                            break;
                        case 3:
                            evaluation_value -= 4;
                            break;
                    }
                }


                //－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
            }
            //SCを使う際の評価値を計算する
            else if (SC_flag)
            {
                //マップの端の時の評価
                if (put_piece.MapEndPosition(map_size))
                {
                    evaluation_value -= 5;

                    //置くところがマップの端かつ周囲の相手のピース数によって評価を変える
                    switch (circumference_partner_piece_num)
                    {
                        case 1:
                            evaluation_value += 5;
                            break;
                        case 2:
                            evaluation_value += 10;
                            break;
                    }

                    //置くところがマップの端かつ周囲の自身のピース数によって評価を変える
                    switch (circumference_own_piece_num)
                    {
                        case 2:
                            evaluation_value -= 45;
                            break;
                        case 3:
                            evaluation_value -= 50;
                            break;
                    }

                    //置くところがマップの端かつ周囲の何もないピース数によって評価を変える
                    switch (circumference_nothing_piece_num)
                    {
                        case 1:
                            evaluation_value -= 5;
                            break;
                        case 2:
                            evaluation_value -= 10;
                            break;
                    }
                }
                //マップの端でないときの評価
                else
                {
                    //置くところの周囲の相手のピース数によって評価を変える
                    switch (circumference_partner_piece_num)
                    {
                        case 1:
                            evaluation_value += 2;
                            break;
                        case 2:
                            evaluation_value += 10;
                            break;
                        case 3:
                            evaluation_value += 30;
                            break;
                    }

                    //置くところの周囲の自身のピース数によって評価を変える
                    switch (circumference_own_piece_num)
                    {
                        case 2:
                            evaluation_value -= 10;
                            break;
                        case 3:
                            evaluation_value -= 35;
                            break;
                        case 4:
                            evaluation_value -= 50;
                            break;
                    }

                    //置くところの周囲の何もないピース数によって評価を変える
                    switch (circumference_nothing_piece_num)
                    {
                        case 1:
                            evaluation_value -= 2;
                            break;
                        case 2:
                            evaluation_value -= 5;
                            break;
                        case 3:
                            evaluation_value -= 10;
                            break;
                    }
                }

                //SC発動時に置かれたピース(4つ)の周囲のピース情報取得用
                List<AIPiece> SC_circumference_pieces = new List<AIPiece>();
                //SC発動時に置かれたピース(4つ)の周囲の自身のピース数
                int SC_circumference_own_piece_num = 0;
                //SC発動時に置かれたピース(4つ)の周囲の相手のピース数
                int SC_circumference_partner_piece_num = 0;
                //SC発動時に置かれたピース(4つ)の周囲の何もないピース数
                int SC_circumference_nothing_piece_num = 0;

                //SC発動時に置かれたピース(4つ)にあるピース分回す
                for (int count = 0; count < circumference_pieces.Count; count++)
                {
                    GetPieceCircumference(circumference_pieces[count], SC_circumference_pieces);

                    for (int SC_count = 0; SC_count < SC_circumference_pieces.Count; SC_count++)
                    {
                        //最初に置いたときのピースは省く
                        if (SC_circumference_pieces[SC_count].pos_x == put_piece.pos_x &&
                           SC_circumference_pieces[SC_count].pos_y == put_piece.pos_y)
                        {
                            SC_circumference_pieces.RemoveAt(SC_count);
                        }
                        else
                        {
                            //SC発動時に置かれたピース(4つ)の周囲のピースが自身のピース
                            if (SC_circumference_pieces[SC_count].id == id)
                                SC_circumference_own_piece_num++;
                            //SC発動時に置かれたピース(4つ)の周囲のピースが相手のピース
                            else if (SC_circumference_pieces[SC_count].id != 0)
                                SC_circumference_partner_piece_num++;
                            //SC発動時に置かれたピース(4つ)の周囲のピースが何もないピース
                            else
                                SC_circumference_nothing_piece_num++;
                        }
                    }
                }

                //置くところの周囲に相手のピースがなければ
                if (circumference_partner_piece_num == 0)
                {
                    //置くところの周囲に相手のピースがないかつSCが発動したときのピースの位置が相手のピースと隣接していなかったら
                    if (SC_circumference_partner_piece_num == 0)
                        evaluation_value -= 50;
                    //置くところの周囲に相手のピースがないかつSCが発動したときのピースの位置が相手のピースと隣接していたら
                    else
                        evaluation_value += SC_circumference_partner_piece_num * 2;

                }


            }
            //CCを使う際の評価値を計算する
            else if (CC_flag)
            {
                //CCを発動時に置くピースの情報
                List<AIPiece> CC_invocation_pieces = new List<AIPiece>();
                //CCを発動時に置くピースの情報を取得
                GetCCInvocationPieces(put_piece, CC_invocation_pieces);

                for(int count = 0; count < CC_invocation_pieces.Count; count++)
                {
                    //CC発動時に置かれるピースの位置に自身のピースがあれば
                    if (CC_invocation_pieces[count].id == id)
                        evaluation_value -= 10;
                    //CC発動時に置かれるピースの位置に相手のピースがあれば
                    else if (CC_invocation_pieces[count].id != 0)
                        evaluation_value += 10;
                    //CC発動時に置かれるピースの位置に何もないピースがあれば
                    else
                        evaluation_value += 5;
                }
            }

            //最終的な評価値を返す
            return evaluation_value;
        }

        //デバック用(マップ情報をLogに出す)
        public void MapDebigLog()
        {
            string log = "";
            for(int map_y = 0; map_y < map_size; map_y++)
            {
                for(int map_x = 0; map_x < map_size; map_x++)
                {
                    log += map[map_x, map_y] + ",";
                }
                log += "\n";
            }
            Debug.Log(log);
        }
    }
    //-----------------------------------------------
    // Use this for initialization
    void Start()
    {
        around_piece = new List<GameObject>();
        candidate_piece = new List<GameObject>();
        system_flag = false;
        AI_flag = PlayerManagemaentScript.AI_flag;
    }

    // Update is called once per frame
    void Update()
    {
        //システムをしてもよいなら行う
        if (system_flag)
        {
            //現在攻撃中のプレイヤーがPlayer2(AI)かつAI_flagがONなら
            //AIを実行する
            if (turn_system_script.attack_prayer_name == "Player2" && AI_flag == true && AI_execution_flag == true)
            {
                StartCoroutine(AI());
            }
            //クリックされたピース情報があれば
            if (hit_piece_obj != null)
            {
                //クリックしたオブジェクトの名前が攻撃中のプレイヤー名だったら
                //そのオブジェクトの透明度を0.8にする(仮)
                if (hit_piece_obj.name == turn_system_script.attack_prayer_name)
                {
                    //プレイヤーとその周囲のピースの状態を戻す
                    //戻っていない可能性があるので
                    BackStatePlayerAround();

                    //クリックしたオブジェクト情報を取得
                    hit_player_obj = hit_piece_obj;

                    //プレイヤーの周囲のピース情報を取得
                    GetComponent<EntireMapScript>().WhereYouCanDeploy(hit_player_obj, around_piece);

                    //選択したプレイヤーオブジェクトの子(枠)を表示する
                    hit_player_obj.transform.GetChild(0).gameObject.SetActive(true);

                    //プレイヤーの周囲のピースの透明度を少し薄くする
                    for (int count = 0; count < around_piece.Count; count++)
                    {
                        ChangeTransparency(around_piece[count], 0.6f);
                    }
                }

                //クリックしたオブジェクトがプレイヤーの周囲のピースの時
                //クリックしたオブジェクト(プレイヤーの周囲のピース)のテクスチャと名前をクリックしたプレイヤーのと同じのに変え、
                //ターンを終了する
                for (int count = 0; count < around_piece.Count; count++)
                {
                    if (hit_piece_obj == around_piece[count])
                    {
                        //色を移したので音楽(SE)を流す
                        GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().PutPieceAudio();
                        hit_piece_obj.name = hit_player_obj.gameObject.name;
                        hit_piece_obj.GetComponent<Image>().sprite = hit_player_obj.gameObject.GetComponent<Image>().sprite;

                        //SC、CCを使うかどうかを確認し、使うなら処理を行う
                        UseSC_or_CC(hit_player_obj.name, hit_piece_obj);

                        //ピース数が変更したので更新する
                        game_progress_obj.GetComponent<GameProgressScript>().PlayerPieceNumUpdate();

                        //各プレイヤーピースの数を取得する
                        int player1_piece_num = 0;
                        int player2_piece_num = 0;
                        GetComponent<EntireMapScript>().PlayerPieceNum(ref player1_piece_num, ref player2_piece_num);

                        //局面が変更されたのでiconの表情を変更する-------------------
                        //player1が5ピースリードしていたら
                        if (player1_piece_num - player2_piece_num >= 5)
                        { 
                            //player1の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer1(5);
                            //player2の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer2(6);
                        }
                        //player2が５ピースリードしていたら
                        else if (player2_piece_num - player1_piece_num >= 5)
                        {
                            //player1の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer1(6);
                            //player2の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer2(5);
                        }
                        //どのplayerもリードしていないなら、表情を通常にする
                        else
                        {
                            //player1の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer1(0);
                            //player2の表情を変更する
                            PlayerManagemaentScript.ExpressionChangePlayer2(0);
                        }
                        //-----------------------------------------------------------

                        //クリックされたピース情報をリセットする
                        hit_piece_obj = null;

                        //AIの実行フラグをONにする
                        AI_execution_flag = true;

                        //スキル発動時の範囲内のオブジェクト情報を破棄
                        DeleteCandidateData();

                        //終了条件がたっせいしていたら
                        if (GetComponent<EntireMapScript>().ExitConditions())
                        {
                            //表情を変える
                            //プレイヤー１が勝利していたら
                            if(player1_piece_num > player2_piece_num)
                            {
                                //player1の表情を変更する
                                PlayerManagemaentScript.ExpressionChangePlayer1(3);
                                //player2の表情を変更する
                                PlayerManagemaentScript.ExpressionChangePlayer2(4);
                            }
                            //プレイヤー２が勝利していたら
                            else if(player1_piece_num < player2_piece_num)
                            {
                                //player1の表情を変更する
                                PlayerManagemaentScript.ExpressionChangePlayer1(4);
                                //player2の表情を変更する
                                PlayerManagemaentScript.ExpressionChangePlayer2(3);
                            }
                            //同点の場合何もしない
                            else
                            {

                            }
                            //システムを止める
                            system_flag = false;
                            //ゲーム終了音(SE)を流す
                            GameObject.Find("AudioSystem").GetComponent<AudioSESystemScript>().GameSetAudio();
                            //GameEndPanelを表示する
                            game_end_panel_obj.SetActive(true);
                        }
                        else
                        {
                            //現在攻撃中のプレイヤーのPlayerScript情報
                            PlayerScript attack_player_script = null;
                            //現在攻撃中のプレイヤーのPlayerScript情報を取得
                            if (turn_system_script.attack_prayer_name == "Player1")
                                attack_player_script = player1_obj.GetComponent<PlayerScript>();
                            else
                                attack_player_script = player2_obj.GetComponent<PlayerScript>();

                            //移動数倍化の状態異常にかかっていない　またわ　移動数倍化の効果が切れていたら
                            //出番を交代する
                            if (attack_player_script.state.QuivkMoveStatusCheck())
                                turn_system_script.TuneChange();
                        }
                        break;
                    }
                }

                //前にクリックしたオブジェクト(プレイヤー)と現在クリックしたオブジェクトが同じでないなら
                //前にクリックしたオブジェクト(プレイヤー)とその周囲の状態を元に戻す
                if (hit_player_obj != hit_piece_obj)
                {
                    //プレイヤーとその周囲ののブロックの状態を戻す
                    BackStatePlayerAround();
                    //Listにアップされたプレイヤー周囲のピース情報をクリアする
                    around_piece.Clear();
                        
                }
            }
        }
    }

    //objctの透明度だけを変更する
    void ChangeTransparency(GameObject objct, float Clarity)
    {
        Image image = objct.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, Clarity);
    }

    //プレイヤーとその周囲のピースの状態(透明度やプレイヤーの枠など)を戻す(1.0f)
    void BackStatePlayerAround()
    {
        if(hit_player_obj != null)
        {
            //プレイヤーの子(枠)を非表示にする
            hit_player_obj.transform.GetChild(0).gameObject.SetActive(false);

            //リストにアップされたプレイヤーの周囲のピースの透明度を戻す
            for (int count = 0; count < around_piece.Count; count++)
                ChangeTransparency(around_piece[count], 1.0f);
        }
    }

    //SC、CCを使うかどうかを確認し、使うなら処理を行う
    //引数１　player_name  : 現在攻撃しているプレイヤーの名前
    //引数２  GameObject   : 選択したオブジェクト情報   
    void UseSC_or_CC(string player_name, GameObject click_obj)
    {
        //現在攻撃中のプレイヤーオブジェクト情報(ピースではない)
        GameObject attack_player_obj = null;
        //相手のプレイヤーオブジェクト情報(ピースではない)
        GameObject partner_player_obj = null;
        //現在攻撃中のプレイヤーの子のSC_CC_GaugeObjのSC_CC_GaugeScript情報
        SC_CC_GaugeScript SC_CC_gauge_script;

        //現在攻撃しているプレイヤーの情報を取得する
        if (player_name == "Player1")
        {
            attack_player_obj = player1_obj;
            partner_player_obj = player2_obj;
        }
        else
        {
            attack_player_obj = player2_obj;
            partner_player_obj = player1_obj;
        }

        //もし現在攻撃しているプレイヤーの情報がなければLogを出し処理を終了する
        if (attack_player_obj == null)
        {
            Debug.Log("現在攻撃しているプレイヤーの情報がありませんでした");
            return;
        }
        //現在攻撃中のプレイヤーの子のSC_CC_GaugeObjのSC_CC_GaugeScript情報取得
        SC_CC_gauge_script = attack_player_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>();

        //SCが選択されていたら使用する
        if (SC_CC_gauge_script.SC_button_flag == true)
        {
            //SCを使用するので専用SEを流す
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().StartSCAudio();

            //SCを使用したので周囲のブロックに色を移す
            GetComponent<EntireMapScript>().TheyUseSC_or_CC(true,click_obj,attack_player_obj,partner_player_obj);
        }

        //CCが選択されていたら使用する
        if (SC_CC_gauge_script.CC_button_flag == true)
        {
            //現在攻撃中のプレイヤーのアイコンの表情を変更、更新する
            if (turn_system_script.attack_prayer_name == "Player1")
            {
                //player1の表情を変更する
                PlayerManagemaentScript.ExpressionChangePlayer1(1, 360);
                //player2の表情を変更する
                PlayerManagemaentScript.ExpressionChangePlayer2(2, 360);
            }
            else
            {
                //player1の表情を変更する
                PlayerManagemaentScript.ExpressionChangePlayer1(2, 360);
                //player2の表情を変更する
                PlayerManagemaentScript.ExpressionChangePlayer2(1, 360);
            }
        
            //CCを使用したので専用SEを流す
            GameObject.FindGameObjectWithTag("AudioSystem").GetComponent<AudioSESystemScript>().StartCCAudio();

            //CCを使用したので十字のブロックに色を移す
            GetComponent<EntireMapScript>().TheyUseSC_or_CC(false, click_obj,attack_player_obj,partner_player_obj);

        }
    }

    //ピースオブジェクトをクリックしたときに呼ばれる
    public void OnPieceClick(GameObject piece_obj)
    {
        //システム実行フラグがONかつ
        //AIフラグがOFFなら
        if (system_flag)
        {
            //AIフラグが実行中なら何もしない
            if (AI_execution_flag == false)
                return;
            //クリックされたピースのオブジェクト情報を保管
            hit_piece_obj = piece_obj;
        }
    }

    //ピースオブジェクトにポインターが乗った時
    public void OnPiecePointerEnter(GameObject piece)
    {
        //システム実行フラグがON
        if(system_flag)
        {
            //スキル発動時の範囲内のオブジェクト情報を破棄
            DeleteCandidateData();
            //周囲のピース情報があればその分回す
            for (int i = 0; i < around_piece.Count; i++)
            {
                //周囲のピースと一致したら
                if(around_piece[i] == piece)
                {
                    if (turn_system_script.attack_prayer_name == "Player1")
                    {
                        SC_CC_GaugeScript script = player1_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>();
                        if(script.SC_button_flag)
                        {
                            GetComponent<EntireMapScript>().SCActivationRange(piece, candidate_piece);
                        }
                        else if(script.CC_button_flag)
                        {
                            GetComponent<EntireMapScript>().CCActivationRange(piece, candidate_piece);
                        }

                    }
                    else if(turn_system_script.attack_prayer_name == "Player2")
                    {
                        SC_CC_GaugeScript script = player2_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>();
                        if(script.SC_button_flag)
                        {
                            GetComponent<EntireMapScript>().SCActivationRange(piece, candidate_piece);
                        }
                        else if(script.CC_button_flag)
                        {
                            GetComponent<EntireMapScript>().CCActivationRange(piece, candidate_piece);
                        }
                    }
                }
            }

            //スキル発動時の範囲内のオブジェクト全てを半透明にする
            for (int i = 0; i < candidate_piece.Count; i++)
            {
                //プレイヤー(選択された)でなければ半透明にする
                if (candidate_piece[i].name != turn_system_script.attack_prayer_name)
                    ChangeTransparency(candidate_piece[i], 0.3f);
            }
        }
    }

    //スキル発動時の範囲内のオブジェクト情報を破棄する
    public void DeleteCandidateData()
    {
        //スキル発動時の範囲内のオブジェクト全ての透明度を戻す
        for (int i = 0; i < candidate_piece.Count; i++)
            ChangeTransparency(candidate_piece[i], 1.0f);

        //初期化する
        candidate_piece.Clear();
    }

    //AI専用のメソッドーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーーー
    //AIを行う
    private IEnumerator AI()
    {
        AI_execution_flag = false;
        yield return new WaitForSeconds(1.0f);

        //簡単にどこに移すかを決めるーーーーーーーーーーーーーーーーーーーーーーーー
        if (true)
            AlphaBeta();
        //－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－

    }

    //αβ法
    private void AlphaBeta()
    {
        //MinMaxを行う準備をする
        AIAlphaBeta min_max = new AIAlphaBeta();
        //min_maxの初期化を行う
        min_max.Initialization(GetComponent<EntireMapScript>().GetMapObjs(), GetComponent<EntireMapScript>().map_size);

        //先読みのレベル
        int level = PlayerManagemaentScript.AI_level;

        //一時的に評価値を入れるよう
        int value = 0;
        int max = -999;

        //αβ値を格納用
        int alpha = -999, beta = 999; 
           

        //各プレイヤーのSC/CCゲージ情報
        GaugeState player1_gauge = new GaugeState(), player2_gauge = new GaugeState();
        player1_gauge.Initialization(player1_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>());
        player2_gauge.Initialization(player2_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>());

        //ターンチェンジ(ターンが進むかどうか)フラグ
        bool turn_change_flag = turn_system_script.GetAfterwardsAttack(2);

        //AI(player2)が先攻なら
        //いったんゲージの状態を1ターン前に戻す(mmMaxで進めるので)
        if (turn_change_flag)
        {
            player1_gauge.GaugeStateReturn();
            player2_gauge.GaugeStateReturn();
        }

        //プレイヤーのピース
        List<AIPiece> player_pieces = new List<AIPiece>();

        //Player2のピースを探し、player_piecesに入れる
        min_max.FindASpecificPiece(2, player_pieces);

        for (int player_count = 0; player_count < player_pieces.Count; player_count++)
        {
            //周囲の置けるところのピース情報格納用
            List<AIPiece> circumference_pieces = new List<AIPiece>();
            //周囲の置けるところのピース情報を取得
            min_max.WhereYouCanDeploy(player_pieces[player_count], circumference_pieces);

            for (int circumference_count = 0; circumference_count < circumference_pieces.Count; circumference_count++)
            {
                //SCが使える状態なら
                //使う判定で判定
                if (player2_gauge.SC_num != 0 && player2_gauge.skill_use_flag == true)
                {
                    value = min_max.mmMax(level-1, player1_gauge, player2_gauge, true, false, turn_change_flag, circumference_pieces[circumference_count],alpha,beta);
                    //過去の評価値より大きければ
                    //ルートを決定(仮)する
                    if (value > max)
                    {
                        //置く場所が決まったので設定する
                        hit_player_obj = GetComponent<EntireMapScript>().GetPiece(player_pieces[player_count].pos_x, player_pieces[player_count].pos_y);
                        hit_piece_obj = GetComponent<EntireMapScript>().GetPiece(circumference_pieces[circumference_count].pos_x, circumference_pieces[circumference_count].pos_y);
                        around_piece.Clear();
                        around_piece.Add(hit_piece_obj);

                        //SCを使用するルートなので使用する
                        player2_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>().SC_button_flag = true;

                        //過去の最大の評価値を更新する
                        max = value;
                    }
                }
                //CCが使える状態なら
                //使う判定で判定
                if (player2_gauge.SC_num == player2_gauge.max_SC_num && player2_gauge.skill_use_flag == true)
                {
                    value = min_max.mmMax(level - 1, player1_gauge, player2_gauge, false, true, turn_change_flag,circumference_pieces[circumference_count], alpha, beta);
                    //過去の評価値より大きければ
                    //ルートを決定(仮)する
                    if (value > max)
                    {
                        //置く場所が決まったので設定する
                        hit_player_obj = GetComponent<EntireMapScript>().GetPiece(player_pieces[player_count].pos_x, player_pieces[player_count].pos_y);
                        hit_piece_obj = GetComponent<EntireMapScript>().GetPiece(circumference_pieces[circumference_count].pos_x, circumference_pieces[circumference_count].pos_y);
                        around_piece.Clear();
                        around_piece.Add(hit_piece_obj);

                        //CCを使用するルートなので使用する
                        player2_obj.GetComponent<PlayerScript>().SC_CC_Gauge.GetComponent<SC_CC_GaugeScript>().CC_button_flag = true;

                        //過去の最大の評価値を更新する
                        max = value;
                    }
                }                   

                //SC、CCを使わない判定で判定
                value = min_max.mmMax(level - 1, player1_gauge, player2_gauge, false, false, turn_change_flag, circumference_pieces[circumference_count], alpha, beta);
                //過去の評価値より大きければ
                //ルートを決定(仮)する
                if (value > max)
                {
                    //置く場所が決まったので設定する
                    hit_player_obj = GetComponent<EntireMapScript>().GetPiece(player_pieces[player_count].pos_x, player_pieces[player_count].pos_y);
                    hit_piece_obj = GetComponent<EntireMapScript>().GetPiece(circumference_pieces[circumference_count].pos_x, circumference_pieces[circumference_count].pos_y);
                    around_piece.Clear();
                    around_piece.Add(hit_piece_obj);

                    //過去の最大の評価値を更新する
                    max = value;
                }
            }
        }
    }
    //－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－－
}

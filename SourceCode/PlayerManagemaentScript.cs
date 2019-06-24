using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//プレイヤー情報を管理(シーンを移動しても死なない)
public class PlayerManagemaentScript : MonoBehaviour
{
    //１つのIconデータ
    [System.Serializable]
    public struct IconData
    {
        public string name;                 //Iconの名前
        public CharacterExpressionData expression;   //表情の画像データ
    }

    //1つの台紙とピースのデータ
    [System.Serializable]
    public struct MountPieceData
    {
        public Sprite mount;    //台紙の画像データ
        public Sprite piece;    //ピースの画像データ
    }

    //キャラクターの表情データ
    [System.Serializable]
    public struct CharacterExpressionData
    {
        public Sprite default_sprite;   //通常時の表情画像
        public Sprite CC_sprite;        //CCを使用した時の表情
        public Sprite CC_was_used;      //CCを相手が使用した時の表情
        public Sprite victory_sprite;   //勝負に勝った時の表情
        public Sprite defeat_sprite;    //勝負に負けた時の表情
        public Sprite piecenum_lead_sprite; //ピース数がリードしている時の表情
        public Sprite piecenum_behind_sprite;   //ピース数がビハインドしている時の表情

        public Sprite GetExpressionSprite(int id)
        {
            switch(id)
            {
                    //CC発動時の表情
                case 1:
                    return CC_sprite;
                    //CCを相手が発動した時の表情
                case 2:
                    return CC_was_used;
                    //勝利した時の表情
                case 3:
                    return victory_sprite;
                    //敗北した時の表情
                case 4:
                    return defeat_sprite;
                    //ピース数がリードした時の表情
                case 5:
                    return piecenum_lead_sprite;
                    //ピース数がビハインドした時の表情
                case 6:
                    return piecenum_behind_sprite;
                    //その他は通常の表情
                default:
                    return default_sprite;
            }
        }
    }

    //有無のチェックフラグ
    private static bool created = false;

    //各プレイヤーのicon画像と名前の情報
    public static string player1_name;
    public static string player2_name;

    //プレイヤーのIcon情報
    public static int player1_icon_id=-1;   //どのアイコンなのか(icons_dataの要素数)
    public static int player1_mount_piece_id=-1;   //どの台紙、ピースなのか(mount_piece_datasの要素数)
    public static int player1_expression_id = 0;    //表情をどれにするか(ExpressionID関連)
    public static int player1_icon_expression_change_flame_time = -1; //Icon(キャラクター)の表情を変更するときに変更後のステイ時間(flame時間)(名前が長いので何か良いのないかな？)
    public static int player1_stay_expression_id = -1;            //ステイ中の表情ID(表情がステイ中に変更されたときのID)

    public static int player2_icon_id=-1;
    public static int player2_mount_piece_id=-1;
    public static int player2_expression_id = 0;
    public static int player2_icon_expression_change_flame_time = -1;
    public static int player2_stay_expression_id = -1;

    //AIを実装するかどうか
    public static bool AI_flag;
    //AIレベル
    public static int AI_level;

    //プレイヤーIconScript情報(キャラクターのIcon、名前、台紙を表示しているIconScript情報)
    [System.NonSerialized]
    public static List<PlayerIconScript> player_icon_scripts;

    //Iconデータ集
    public IconData[] inspector_icon_datas;//インスペクタ専用
    private static IconData[] icon_datas;
    //Iconの台紙とピース(メインゲームで使用するピース)の画像データ集
    public MountPieceData[] inspector_mount_piece_datas;//インスペクタ専用
    private static MountPieceData[] mount_piece_datas;

    // Use this for initialization
    void Start () {
        if (!created)
        {
            player_icon_scripts = new List<PlayerIconScript>();
            DontDestroyOnLoad(gameObject);
            //AIレベルを初期化
            AI_level = 1;
            //インスペクタのIcon情報を元にIcon_dataを初期化する
            InitializationIconDatas();
            //インスペクタのIconの台紙、ピース情報を元にmount_piece_datasを作成する
            InitializationIconMountpieceDatas();
            //プレイヤーのIcon情報を初期化する
            PlayerDataInitialization();
            created = true;
        }
        else
            Destroy(gameObject);

    }
	
	// Update is called once per frame
	void Update ()
    {
        //player1のステイ時間が０以上なら
        if(player1_icon_expression_change_flame_time >= 0)
        {
            //ステイ時間を進める
            player1_icon_expression_change_flame_time--;

            //ステイ時間が０なら
            if (player1_icon_expression_change_flame_time == 0)
            {
                //ステイが終了したので現在の表情をステイ中に変更された表情にする
                player1_expression_id = player1_stay_expression_id;

                //ステイが終了したので-1を代入
                player1_icon_expression_change_flame_time = -1;
                player1_stay_expression_id = -1;
                //全てのキャラクターIconを表示している情報を更新する
                ChangeIconDisplayInformation();
            }
        }
        //player2のステイ時間が０以上なら
        if(player2_icon_expression_change_flame_time >= 0)
        {
            //ステイ時間を進める
            player2_icon_expression_change_flame_time--;

            //ステイ時間が０なら
            if (player2_icon_expression_change_flame_time == 0)
            {
                //ステイが終了したので現在の表情をステイ中に変更された表情にする
                player2_expression_id = player2_stay_expression_id;
                
                //ステイが終了したので-1を代入
                player2_icon_expression_change_flame_time = -1;
                player2_stay_expression_id = -1;
                //全てのキャラクターIconを表示している情報を更新する
                ChangeIconDisplayInformation();
            }
        }
	}

    //インスペクタのIcon情報を元にIcon_datasを作成する
    private void InitializationIconDatas()
    {
        icon_datas = new IconData[inspector_icon_datas.Length];

        icon_datas = inspector_icon_datas;
    }
    //インスペクタのIconの台紙、ピース情報を元にmount_piece_datasを作成する
    private void InitializationIconMountpieceDatas()
    {
        mount_piece_datas = new MountPieceData[inspector_mount_piece_datas.Length];
        mount_piece_datas = inspector_mount_piece_datas;
    }

    //プレイヤーのIcon情報を初期化する
    //キャラクター選択シーンに移動した瞬間に呼ばれる
    public static void PlayerDataInitialization()
    {
        player1_icon_id = -1;
        player2_icon_id = -1;
        player1_mount_piece_id = -1;
        player2_mount_piece_id = -1;
        player1_expression_id = 0;
        player2_expression_id = 0;
        player1_name = null;
        player2_name = null;
        player1_icon_expression_change_flame_time = -1;
        player2_icon_expression_change_flame_time = -1;
        player1_stay_expression_id = -1;
        player2_stay_expression_id = -1;
    }
    //IconIDを元に名前とIcon情報を保管する(Player1専用)
    public static void StorageNameName_and_IconInformationPlayer1(int icon_id)
    {
        //iconIDが０未満なら
        //nameをなしにする
        if (icon_id < 0)
        {
            player1_icon_id = icon_id;
            player1_name = null;
            player1_mount_piece_id = icon_id;
            return;
        }

        if (icon_id < icon_datas.Length)
        {
            player1_icon_id = icon_id;
            player1_name = icon_datas[icon_id].name;
            player1_mount_piece_id = icon_id;
            return;
        }
        Debug.Log("Player１の名前とIcon情報を保管できませんでした。");
    }
    //IconIDを元に名前とIcon情報を保管する(Player2専用)
    public static void StorageNameName_and_IconInformationPlayer2(int icon_id)
    {
        //iconIDが０未満なら
        //nameをなしにする
        if(icon_id < 0)
        {
            player2_icon_id = icon_id;
            player2_name = null;
            player2_mount_piece_id = icon_id;
            return;
        }
        if (icon_id < icon_datas.Length)
        {
            player2_icon_id = icon_id;
            player2_name = icon_datas[icon_id].name;
            player2_mount_piece_id = icon_id;
            //台紙が一緒なら違うのにする
            if (player2_mount_piece_id == player1_mount_piece_id)
                IconVerUpPlayer2();
            return;
        }
        Debug.Log("Player2の名前とIcon情報を保管できませんでした。");
    }

    //Icon情報(画像データ)を渡す(Player1専用)
    public static Sprite GetIconImageDataPlayer1()
    {
        //iconIDが０未満なら画像なしとする
        if (player1_icon_id < 0)
            return null;
        return icon_datas[player1_icon_id].expression.GetExpressionSprite(player1_expression_id);
    }
    //Icon情報(画像データ)を渡す(Player2専用)
    public static Sprite GetIconImageDataPlayer2()
    {
        //iconIDが０未満なら画像なしとする
        if (player2_icon_id < 0)
            return null;
        return icon_datas[player2_icon_id].expression.GetExpressionSprite(player2_expression_id);
    }
    //台紙情報(画像データ)を渡す(Player1専用)
    public static Sprite GetMountImageDataPlayer1()
    {
        //台紙、ピースのIDが０未満なら画像なしとする
        if (player1_mount_piece_id < 0)
            return null;
        return mount_piece_datas[player1_mount_piece_id].mount;
    }
    //台紙情報(画像データ)を渡す(Player2専用)
    public static Sprite GetMountImageDataPlayer2()
    {
        //台紙、ピースのIDが０未満なら画像なしとする
        if (player2_mount_piece_id < 0)
            return null;
        return mount_piece_datas[player2_mount_piece_id].mount;
    }
    //ピース情報(画像データ)を渡す(Plauer1専用)
    public static Sprite GetPieceImageDataPlayer1()
    {
        //台紙、ピースのIDが０未満なら画像なしとする
        if (player1_mount_piece_id < 0)
            return null;
        return mount_piece_datas[player1_mount_piece_id].piece;
    }
    //ピース情報(画像データ)を渡す(Plauer2専用)
    public static Sprite GetPieceImageDataPlayer2()
    {
        //台紙、ピースのIDが０未満なら画像なしとする
        if (player2_mount_piece_id < 0)
            return null;
        return mount_piece_datas[player2_mount_piece_id].piece;
    }

    //IconのVerを上げる(Verが最大ならVerを初期に戻す)(Player1専用)
    public static void IconVerUpPlayer1()
    {
        //用意されている画像の最大数
        int image_max_num;
        //IconIDが０未満なら何もしない
        if (player1_mount_piece_id < 0)
            return;

        //用意されている画像の最大数を取得
        image_max_num = mount_piece_datas.Length;

        //現在の画像が用意されている最後の画像だったら
        //用意されている画像の一番最初にする
        if(player1_mount_piece_id + 1 == image_max_num)
        {
            player1_mount_piece_id = 0;
            return;
        }

        //問題がなければ加算する
        player1_mount_piece_id++;
    }
    //IconのVerを上げる(Verが最大ならVerを初期に戻す)(Player2専用)
    public static void IconVerUpPlayer2()
    {
        //用意されている画像の最大数
        int image_max_num;
        //IconIDが０未満なら何もしない
        if (player2_mount_piece_id < 0)
            return;

        //用意されている画像の最大数を取得
        image_max_num = mount_piece_datas.Length;

        //現在の画像が用意されている最後の画像だったら
        //用意されている画像の一番最初にする
        if (player2_mount_piece_id + 1 == image_max_num)
        {
            player2_mount_piece_id = 0;
        }
        else
            //問題がなければ加算する
            player2_mount_piece_id++;

        //加算した際既に使用されていたら、Verをさらに上げる
        if (player1_mount_piece_id == player2_mount_piece_id)
            IconVerUpPlayer2();

    }
    //キャラクターの表情を変更する(Player1専用)
    //引数1 id    :変更する表情ID
    //引数2 time  :Icon(キャラクター)の表情を変更するときに変更後のステイ時間(flame時間)
    public static void ExpressionChangePlayer1(int id,int flame_time = 0)
    {
        //引数のステイ時間が０以下なら設定なしとする
        if (flame_time <= 0)
        {
            //現在、表情を止めているなら、ステイ中の表情Idを変更する
            if (player1_icon_expression_change_flame_time != -1)
                player1_stay_expression_id = id;
            //現在、表情を止めていなければ直接変更する
            else
                player1_expression_id = id;
        }
        //引数のステイ時間が1以上なら、ステイ時間を設定する
        else
        {
            //ステイ時間を設定する
            player1_icon_expression_change_flame_time = flame_time;
            //ステイ中の表情Idを設定する
            player1_stay_expression_id = player1_expression_id;
            //現在の表情を設定する
            player1_expression_id = id;
        }
        //全てのキャラクターIconを表示している情報を更新する
        ChangeIconDisplayInformation();
    }
    //キャラクターの表情を変更する(Player2専用)
    //引数1 id    :変更する表情ID
    //引数2 time  :Icon(キャラクター)の表情を変更するときに変更後のステイ時間(flame時間)
    public static void ExpressionChangePlayer2(int id, int flame_time = 0)
    {
        //引数のステイ時間が０以下なら設定なしとする
        if (flame_time <= 0)
        {
            //現在、表情を止めているなら、ステイ中の表情Idを変更する
            if (player2_icon_expression_change_flame_time != -1)
                player2_stay_expression_id = id;
            //現在、表情を止めていなければ直接変更する
            else
                player2_expression_id = id;
        }
        //引数のステイ時間が1以上なら、ステイ時間を設定する
        else
        {
            //ステイ時間を設定する
            player2_icon_expression_change_flame_time = flame_time;
            //ステイ中の表情Idを設定する
            player2_stay_expression_id = player2_expression_id;
            //現在の表情を設定する
            player2_expression_id = id;
        }
        //全てのキャラクターIconを表示している情報を更新する
        ChangeIconDisplayInformation();
    }

    //全てのキャラクターIconを表示している情報を更新する
    private static void ChangeIconDisplayInformation()
    {
        for(int count = 0; count < player_icon_scripts.Count; count++)
        {
            player_icon_scripts[count].IconDataUpData();
        }
    }
}

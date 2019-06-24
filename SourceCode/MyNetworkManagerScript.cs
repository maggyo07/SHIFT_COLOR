using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

//https://gametukurikata.com/online/unitynetwork/unitynetwork10

/*      メモ
https://docs.unity3d.com/ja/530/ScriptReference/Networking.NetworkManager.html

IsClientConnected  NetworkManager がクライアントを持ち、サーバーと接続しているかチェックします。　ホストだとtrue それ以外だとfalse
[SyncVar]       つけたプロパティはネットワーク越しで共有される
ClientCallback Attribute をつけるとクライアント側だけで実行される（サーバ側は空実装）
Client Attribute をつけると Client のみ実行される（サーバでは空実装になる）
// サーバ側で実行されるコマンド
    // クライアント側からサーバ側へコマンドを送る時はこれが必要
    // Command Attribute と Cmd-prefix な関数をセットで定義
    [Command]

*/
public class MyNetworkManagerScript : NetworkManager
{
    //ルームリストパネル
    [SerializeField]
    private Transform roomListPanel;
    //ネットワーク接続方法を決めるパネル
    [SerializeField]
    private GameObject networkConnectionPanel;
    //ユーザーを待つための部屋のパネル
    [SerializeField]
    private GameObject waitingRoomPanel;
    //部屋に参加するときのパネル
    [SerializeField]
    private GameObject joinTheRoomPanel;
    //部屋の名前を入力するためのパネル
    [SerializeField]
    private GameObject InputRoomNamePanel;
    //キャラクターを選択するためのパネル
    //[SerializeField]
    //private GameObject CharacterCelectPanel;

    //ルームデータプレハブ
    [SerializeField]
    private GameObject roomDataPrefab;
    //ルーム名を表示させるためのText
    [SerializeField]
    private Text displayRoomName;
    //情報表示用テキスト
    [SerializeField]
    private Text informationText;


    //ルームデータインスタンスを入れておく
    [SerializeField]
    private List<GameObject> roomDataLists = new List<GameObject>();

    //NetworkConnectionSceneに入った瞬間かどうか(入った瞬間だけfalse)
    private bool enteredSceneFlag = false;
    //自身がホストかどうか
    public bool is_host;
    //クライアント数(ホストも含む)
    private uint client_count = 0;
    //1つの部屋の最大人数
    public uint math_max_size = 2;
    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        //現在のシーンがネットワーク接続シーンなら
        if (SceneManager.GetActiveScene().name == "NetworkConnectionScene")
        {
            //ネットワーク接続シーンに入った時
            //パネルを表示、マッチングメーカを使用する準備をする
            if (!enteredSceneFlag)
            {
                //マッチングメーカーを使用する
                EnableMatchMaker();
                //ネットワーク接続方法を選択するパネルを表示する
                networkConnectionPanel.SetActive(true);
                //ユーザーを待つための部屋のボタン(キャラクターセレクトシーンに移動する)を押せなくする
                waitingRoomPanel.transform.GetChild(0).GetComponent<Button>().interactable = false;
                //クライアント数を初期化
                client_count = 0;
                enteredSceneFlag = true;
            }
        }
        else
            enteredSceneFlag = false;

        //1つの部屋に最大人数入っていたら、キャラ選択画面を表示してもいいようにする
        if (client_count == math_max_size)
            waitingRoomPanel.transform.GetChild(0).GetComponent<Button>().interactable = true;
        else
            waitingRoomPanel.transform.GetChild(0).GetComponent<Button>().interactable = false;

    }

    //マッチングメーカーを使用
    public void EnableMatchMaker()
    {
        StartMatchMaker();
    }

    //部屋の名前を変更
    public void ChangeRoomName(string roomName)
    {
        //名前が入力されていなかったらデフォルトの名前にする 
        if (roomName == "")
            roomName = "DefaultRoom";
        matchName = roomName;
    }

    //部屋を作成
    public void CreateRoom()
    {
        //https://docs.unity3d.com/jp/current/ScriptReference/Networking.Match.NetworkMatch.CreateMatch.html
        //                      部屋の名前、最大人数、ListMatchesで取得できるかどうか、パスワード、パブリックなクライアントアドレス
        matchMaker.CreateMatch(matchName, math_max_size,                  true,                   "",                 "",
                                //プライベートなクライアントアドレス、Eloスコア、リクエストドキュメント、コールバック
                                "", 0, 0, OnMatchCreate);

        //部屋を作成したので表示するパネルを切り替える
        networkConnectionPanel.SetActive(false);
        waitingRoomPanel.SetActive(true);
        InputRoomNamePanel.SetActive(false);
        //部屋の名前を表示させる
        displayRoomName.text = matchName;
    }

    //部屋が作成されると呼ばれる
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        //部屋を作成しているので自身はホストである
        is_host = true;

        ////キャラクター選択画面パネルを作成
        //CharacterCelectPanel = Instantiate(spawnPrefabs[1]);
        ////パネルを非表示にする
        //CharacterCelectPanel.SetActive(false);
        ////パネルの親をNetworkManagerにする
        //CharacterCelectPanel.transform.parent = transform;
        ////サーバーに作成する
        //NetworkServer.Spawn(CharacterCelectPanel);

        base.OnMatchCreate(success, extendedInfo, matchInfo);
    }

    //部屋を見つける
    public void FindTheRoom()
    {
        //                     始めのページ、マッチの数、マッチの名前のフィルター、プライベートなマッチを含むか、Eloスコア
        matchMaker.ListMatches(0, 10, "", true, 0,
        //リクエストドキュメント、コールバック
                    0, OnMatchList);
    }

    //部屋情報を取得すると呼ばれる
    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        //リストの中身のオブジェクトを消去する
        foreach (var item in roomDataLists)
        {
            Destroy(item);
        }
        //リストをリセットする
        roomDataLists.Clear();

        foreach (var match in matchList)
        {
            Debug.Log("部屋を発見しました。");
            var room = Instantiate(roomDataPrefab) as GameObject;
            room.transform.GetChild(0).GetComponent<Text>().text = match.name;
            room.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => matchMaker.JoinMatch(match.networkId, "", "", "", 0, 0, OnMatchJoined));
            room.transform.SetParent(roomListPanel);
            roomDataLists.Add(room);
        }
        base.OnMatchList(success, extendedInfo, matchList);
    }


    //部屋に入室したら呼ばれる
    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        //入室先の部屋が存在していれば
        if (success)
        {
            Debug.Log("部屋に入室しました。");
            //部屋に入る
            base.OnMatchJoined(success, extendedInfo, matchInfo);
            //表示するパネルを切り替える
            joinTheRoomPanel.SetActive(false);
            waitingRoomPanel.SetActive(true);
            //ルームの名前を表示する
            displayRoomName.text = matchHost;
            //ユーザーを待つための部屋のボタン(キャラクターセレクトシーンに移動する)を押せるようにする
            //サーバーに参加しているので準備ができている前提
            waitingRoomPanel.transform.GetChild(0).GetComponent<Button>().interactable = true;
            //部屋に入っているので、自身はクライアントである
            is_host = false;
            //ルームデータリストをクリアする
            RoomListClear();

        }
        //入室しようとしたが既に存在していなかったら
        else
        {
            informationText.text = "既に部屋が閉じていたようです。再度部屋を検索します。";
            //部屋情報を更新する
            FindTheRoom();
        }
    }

    //マッチメーカーを終了
    public void DisableMatchMaker()
    {
        //マッチメーカーを終了する
        StopMatchMaker();

        //部屋データリストをクリアする
        RoomListClear();
    }

    //サーバーにクライアントのプレイヤーが追加された
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        Debug.Log("プレイヤーが生成されました。");
        //NetworkReaderからプレイヤー番号を取得
        IntegerMessage msg = extraMessageReader.ReadMessage<IntegerMessage>();

        var playerSpawnPos = Vector3.zero;
        var player = GameObject.Instantiate(spawnPrefabs[msg.value], playerSpawnPos, Quaternion.identity) as GameObject;
        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);

        //0番のオブジェクトはプレイヤー管理システムオブジェクトなので
        //それを作成しているのでクライアントが増えたとしてカウントを加算
        if (msg.value == 0)
            client_count++;
    }

    //クライアントのシーンが変更された
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        Debug.Log("クライアントがシーンを変更しました。");
        //現在選択中のプレイヤー番号をIntegerMessageとして作成
        //IntegerMessage msg = new IntegerMessage(selectrdCharaNum);

        //always decome ready
        if (!ClientScene.ready)
            ClientScene.Ready(conn);
        if (!autoCreatePlayer)
            return;

        //bool addPlayer = ((ClientScene.localPlayers.Count == 0));
        bool foundPlayer = false;

        for (int i = 0; i < ClientScene.localPlayers.Count; i++)
        {
            if (ClientScene.localPlayers[i].gameObject != null)
            {
                foundPlayer = true;
                break;
            }
        }
        if (!foundPlayer)
        {
            // there are players, but their game objects have all been deleted
           // addPlayer = true;
        }
        //if (addPlayer)
            //メッセージ付きのAddPlayerを呼び出す
           // ClientScene.AddPlayer(conn, 0, msg);
    }

    //サーバーに接続した時にクライアントで呼び出される
    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("サーバーに接続した時にクライアントで呼び出される");

        if (!clientLoadedScene)
        {
            //クライアントが準備できていなければ
            if (!ClientScene.ready)
            {
                //クライアントの準備をする
                ClientScene.Ready(conn);
            }
            if (NetworkManager.singleton.autoCreatePlayer)
            {
                //プレイヤー情報管理オブジェを生成するために、SpawnablePrefabsに設定した要素数をMassageに変換
                IntegerMessage msg = new IntegerMessage(0);
                //プレイヤー情報管理オブジェクトを作成
                ClientScene.AddPlayer(conn, 0, msg);
            }
        }
    }

    //サーバーが切断した時にクライアントで呼び出される
    public override void OnClientDisconnect(NetworkConnection conn)
    {
        Debug.Log("サーバーが切断しました");
        StopClient();
    }

    //クライアントがサーバーから切断されたときに呼び出される
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        Debug.Log("サーバーから切断されました。");
        NetworkServer.DestroyPlayersForConnection(conn);
    }

    //戻るボタンを押したときの処理
    public void BackButtonClick()
    {
        //切断する
        StopHost();
        //マッチングメーカーを終了する。部屋情報リストをクリアもしている
        DisableMatchMaker();
        
        //取得している全てのパネルの表示をOFFにする
        networkConnectionPanel.SetActive(false);
        waitingRoomPanel.SetActive(false);
        joinTheRoomPanel.SetActive(false);
        //メニューシーンに移動する
        SceneManager.LoadScene("MenuScene");
    }

    //対戦部屋を作成するボタンを押したときの処理
    public void MadeRoomButtonClick()
    {
        //部屋の名前を入力させるためのパネルを表示する
        InputRoomNamePanel.SetActive(true);
    }
    //部屋に参加するボタンを押したときの処理
    public void JoinRoomButtonClick()
    {
        //表示するパネルを切り替える
        networkConnectionPanel.SetActive(false);
        joinTheRoomPanel.SetActive(true);
        FindTheRoom();
    }
    
    //キャラクター選択画面に移動するときに押すボタンが押されたら
    public void CharacterSelectionChangesButtonClick()
    {
        //表示するパネルを切り替える
        waitingRoomPanel.SetActive(false);
        //CharacterCelectPanel.SetActive(true);
    }

    //部屋情報リストにあるオブジェクトをすべて消し、リストをクリアする
    private void RoomListClear()
    {
        for (int i = 0; i < roomDataLists.Count; i++)
            Destroy(roomDataLists[i]);
        roomDataLists.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

/*
 * Credit to example repositories
 * https://github.com/rlabrecque/Steamworks.NET-Test/blob/master/Assets/Scripts/SteamNetworkingTest.cs
 * https://github.com/rlabrecque/Steamworks.NET-Test/blob/master/Assets/Scripts/SteamMatchmakingTest.cs
 */

public class Lobby : MonoBehaviour
{
    ArrayList members = new ArrayList();
    const int MAX_MEMBERS = 8;
    private CSteamID m_Lobby = new CSteamID(0); //Current lobby ID


    enum MsgType : uint
    {
        Ping,
        Ack, //Acknowledge
    }

    //Callback Declarations
    protected Callback<P2PSessionRequest_t> m_P2PSessionRequest;
    protected Callback<P2PSessionConnectFail_t> m_P2PSessionConnectFail;
    protected Callback<SocketStatusCallback_t> m_SocketStatusCallback;
    protected Callback<LobbyEnter_t> m_LobbyEnter;
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    protected Callback<LobbyGameCreated_t> m_LobbyGameCreated;

    //Callresult Declarations
    private CallResult<LobbyEnter_t> OnLobbyEnterCallResult;
    private CallResult<LobbyMatchList_t> OnLobbyMatchListCallResult;
    private CallResult<LobbyCreated_t> OnLobbyCreatedCallResult;

    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
            //Callbacks
            m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
            m_P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);
            m_LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
            m_LobbyGameCreated = Callback<LobbyGameCreated_t>.Create(OnLobbyGameCreated);

            //CallResults
            OnLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
            OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
            OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);


        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SteamManager.Initialized)
        {
            Debug.Log("[STEAM] Initialized");
        }
        else {
            Debug.LogError("[STEAM] Not Initialized");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SteamManager.Initialized && m_Lobby.m_SteamID > 0) { 
            //Send and receive packets
        }
    }

    private void CreateLobby()
    {
        if (SteamManager.Initialized && m_Lobby.m_SteamID > 0)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MAX_MEMBERS);
        }
    }

    void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        Debug.Log("[" + P2PSessionRequest_t.k_iCallback + " - P2PSessionRequest] - " + pCallback.m_steamIDRemote);

        bool ret = SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
        print("SteamNetworking.AcceptP2PSessionWithUser(" + pCallback.m_steamIDRemote + ") - " + ret);

        //m_RemoteSteamId = pCallback.m_steamIDRemote;
    }

    void OnP2PSessionConnectFail(P2PSessionConnectFail_t pCallback)
    {
        Debug.Log("[" + P2PSessionConnectFail_t.k_iCallback + " - P2PSessionConnectFail] - " + pCallback.m_steamIDRemote + " -- " + pCallback.m_eP2PSessionError);
    }

    void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
    {
        Debug.Log("[" + SocketStatusCallback_t.k_iCallback + " - SocketStatusCallback] - " + pCallback.m_hSocket + " -- " + pCallback.m_hListenSocket + " -- " + pCallback.m_steamIDRemote + " -- " + pCallback.m_eSNetSocketState);
    }

    void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);

        m_Lobby = (CSteamID)pCallback.m_ulSteamIDLobby;
    }

    void OnLobbyCreated(LobbyGameCreated_t pCallback)
    {
        Debug.Log("[" + LobbyGameCreated_t.k_iCallback + " - LobbyGameCreated] - " + pCallback.m_ulSteamIDLobby + " -- " + pCallback.m_ulSteamIDGameServer + " -- " + pCallback.m_unIP + " -- " + pCallback.m_usPort);
    }

    void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        Debug.Log("[" + LobbyMatchList_t.k_iCallback + " - LobbyMatchList] - " + pCallback.m_nLobbiesMatching);
    }
}

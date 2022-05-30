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
    private string sessionKey = "EKRX82Z";
    private string gameID = "CFPS";


    enum MsgType : uint
    {
        Ping,
        Ack, //Acknowledge
    }

    //Callback Declarations


    //Callresult Declarations
    private CallResult<LobbyCreated_t> m_OnLobbyCreatedCallResult;
    private CallResult<LobbyMatchList_t> m_OnLobbyMatchListCallResult;
    private CallResult<LobbyEnter_t> m_OnLobbyEnteredCallResult;


    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {
      
            //Callbacks

            //CallResults
            m_OnLobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
            m_OnLobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
            m_OnLobbyEnteredCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEntered);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SteamManager.Initialized)
        {
            Debug.Log("[STEAM] Initialized");
            Debug.Log("[STEAM] Current Username: " + SteamFriends.GetPersonaName());
            //CreateLobby();
            RequestLobby(sessionKey);
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
        if (SteamManager.Initialized && m_Lobby.m_SteamID == 0)
        {
            SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MAX_MEMBERS);
            m_OnLobbyCreatedCallResult.Set(handle);
            Debug.Log("[STEAM] Called: CreateLobby()");
           
        }
    }

    private void RequestLobby(string sessionKey) 
    {
        //Filter Distance (worldwide)
        SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
        //Filter seskey
        SteamMatchmaking.AddRequestLobbyListStringFilter("sessionKey", sessionKey, ELobbyComparison.k_ELobbyComparisonEqual);
        //Filter gameID
        SteamMatchmaking.AddRequestLobbyListStringFilter("game", gameID, ELobbyComparison.k_ELobbyComparisonEqual);
        SteamAPICall_t handle = SteamMatchmaking.RequestLobbyList();
        m_OnLobbyMatchListCallResult.Set(handle);
        Debug.Log("[STEAM] Called: RequestLobbyList()");
    }

    /*
     *******************
     * ON EVENT METHODS
     *******************
     */

    private void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
    {
        Debug.Log("[STEAM] " + "[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);
        m_Lobby = (CSteamID)pCallback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(m_Lobby, "sessionKey", sessionKey);
        SteamMatchmaking.SetLobbyData(m_Lobby, "game", gameID);
    }

    private void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        Debug.Log("[STEAM] " + "[" + LobbyMatchList_t.k_iCallback + " - LobbyMatchList] - " + pCallback.m_nLobbiesMatching);
        CSteamID match = SteamMatchmaking.GetLobbyByIndex(0); //Get lobby ID from first match

        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(match);
        m_OnLobbyMatchListCallResult.Set(handle);
        Debug.Log("[STEAM] Called: JoinLobby()");
    }

    private void onLobbyEntered(LobbyEnter_t pCallback, bool bIOFailure) 
    {
        Debug.Log("[STEAM] Lobby Entered: " + pCallback.m_ulSteamIDLobby);

    }

}

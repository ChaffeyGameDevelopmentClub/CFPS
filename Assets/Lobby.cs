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
    List<CSteamID> members = new List<CSteamID>();
    const int MAX_MEMBERS = 8;
    private CSteamID m_Lobby = new CSteamID(0); //Current lobby ID
    private CSteamID currentUserID;
    private string sessionKey = "EKRX82Z";
    private string gameID = "CFPS";


    enum MsgType : uint
    {
        Ping,
        Ack, //Acknowledge
    }

    //Callback Declarations
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;


    //Callresult Declarations
    private CallResult<LobbyCreated_t> m_OnLobbyCreatedCallResult;
    private CallResult<LobbyMatchList_t> m_OnLobbyMatchListCallResult;
    private CallResult<LobbyEnter_t> m_OnLobbyEnteredCallResult;


    private void OnEnable()
    {
        if (SteamManager.Initialized)
        {

            //Callbacks
            m_LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);

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
            currentUserID = SteamUser.GetSteamID();
            Debug.Log("[STEAM] Initialized");
            Debug.Log("[STEAM] Current Username: " + SteamFriends.GetPersonaName());
            members.Add(currentUserID);
            CreateLobby();
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

    private void UpdateLobbyMembers() {
        Debug.Log("[STEAM] Updating lobby member list for " + m_Lobby.m_SteamID);
        members.Clear();
        members.Add(currentUserID);
        if (m_Lobby.m_SteamID > 0) { 
            int numMembers = SteamMatchmaking.GetNumLobbyMembers(m_Lobby);
            for (int i = 0; i < numMembers; i++) {
                CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(m_Lobby, i);
                Debug.Log(SteamFriends.GetFriendPersonaName(member));
                if (member != currentUserID) {
                    members.Add(member);
                }
            }
        }
    
    }

    /*
     *******************
     * ON EVENT METHODS
     *******************
     */


    /*
     * Callresult
     * The result of SteamMatchmaking.CreateLobby()
     */
    private void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
    {
        Debug.Log("[STEAM] " + "[" + LobbyCreated_t.k_iCallback + " - LobbyCreated] - " + pCallback.m_eResult + " -- " + pCallback.m_ulSteamIDLobby);
        m_Lobby = (CSteamID)pCallback.m_ulSteamIDLobby;
        SteamMatchmaking.SetLobbyData(m_Lobby, "sessionKey", sessionKey);
        SteamMatchmaking.SetLobbyData(m_Lobby, "game", gameID);
        UpdateLobbyMembers();
    }

    /*
     * Callresult
     * The result of SteamMatchmaking.RequestLobbyList()
     */
    private void OnLobbyMatchList(LobbyMatchList_t pCallback, bool bIOFailure)
    {
        Debug.Log("[STEAM] " + "[" + LobbyMatchList_t.k_iCallback + " - LobbyMatchList] - " + pCallback.m_nLobbiesMatching);
        CSteamID match = SteamMatchmaking.GetLobbyByIndex(0); //Get lobby ID from first match
        Debug.Log("[STEAM] Attempting to join "  + match);

        SteamAPICall_t handle = SteamMatchmaking.JoinLobby(match);
        m_OnLobbyEnteredCallResult.Set(handle);
        Debug.Log("[STEAM] Called: JoinLobby()");
    }

    /*
     * Callresult
     * The result of SteamMatchmaking.JoinLobby()
     */
    private void OnLobbyEntered(LobbyEnter_t pCallback, bool bIOFailure) 
    {
        Debug.Log("[STEAM] Lobby Entered: " + pCallback.m_ulSteamIDLobby);
        m_Lobby = new CSteamID(pCallback.m_ulSteamIDLobby);
        UpdateLobbyMembers();
    }

    /*
     * Callback
     * Called when lobby data changes
     */
    private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback) {
        Debug.Log("[STEAM] Lobby data has updated for " + m_Lobby.m_SteamID);
        UpdateLobbyMembers();
    }


}

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
    private CSteamID hostID = new CSteamID(0);
    private string sessionKey = "EKRX82Z";
    private string gameID = "CFPS";
    private bool localIsHost = false;
    private bool connected = false;
    const int READLIMIT = 999;

    enum MsgType : uint
    {
        PING, //Ping recipient
        ACK, //Acknowledge ping
        HGDU, //Game data update from host
        GDUH, //Game data update to host
    }

    //Callback Declarations
    protected Callback<LobbyDataUpdate_t> m_LobbyDataUpdate;
    protected Callback<LobbyChatUpdate_t> m_LobbyChatUpdate;
    protected Callback<P2PSessionRequest_t> m_P2PSessionRequest;
    protected Callback<P2PSessionConnectFail_t> m_P2PSessionConnectFail;
    protected Callback<SocketStatusCallback_t> m_SocketStatusCallback;



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
            m_LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
            m_P2PSessionRequest = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
            m_P2PSessionConnectFail = Callback<P2PSessionConnectFail_t>.Create(OnP2PSessionConnectFail);
            m_SocketStatusCallback = Callback<SocketStatusCallback_t>.Create(OnSocketStatusCallback);

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
        else 
        {
            Debug.LogError("[STEAM] Not Initialized");
            return;
        }
    }

    void FixedUpdate()
    {
        if (SteamManager.Initialized && m_Lobby.m_SteamID > 0) 
        {
            if (localIsHost)
            {
                foreach (CSteamID member in members) //Send data to all connected users if host
                {
                    if (member != currentUserID)
                    {
                        //SendPacket(member, new byte[0])
                    }
                }

            }
            else
            {
                //Send the host local user data
                //SendPacket(hostID, new byte[0]);
            }
            ReadAllPackets();
        }
    }

    private void SendPing(CSteamID recipient) 
    {
        byte[] bytes = new byte[4];
        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
        using (System.IO.BinaryWriter b = new System.IO.BinaryWriter(ms))
        {
            b.Write((uint)MsgType.PING);
        }
        bool ret = SteamNetworking.SendP2PPacket(recipient, bytes, (uint)bytes.Length, EP2PSend.k_EP2PSendReliable);
    }

    private void SendAck(CSteamID recipient) 
    {
        byte[] bytes = new byte[4];
        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
        using (System.IO.BinaryWriter b = new System.IO.BinaryWriter(ms))
        {
            b.Write((uint)MsgType.ACK);
        }
        bool ret = SteamNetworking.SendP2PPacket(recipient, bytes, (uint)bytes.Length, EP2PSend.k_EP2PSendReliable);
    }

    private void SendPacket(CSteamID recipient, byte[] data) 
    {
        EP2PSend sendType = EP2PSend.k_EP2PSendReliable;
        bool ret = SteamNetworking.SendP2PPacket(recipient, data, (uint)data.Length, sendType);
    }

    private void ReadAllPackets(int readCount = 0) {
        uint MsgSize;
        if (SteamNetworking.IsP2PPacketAvailable(out MsgSize) && readCount < READLIMIT) {
            ReadPacket(MsgSize);
            ReadAllPackets(readCount + 1);
        }
    }

    private void ReadPacket(uint MsgSize) 
    {
        byte[] bytes = new byte[MsgSize];
        uint newMsgSize;
        CSteamID SteamIdRemote;
        bool ret = SteamNetworking.ReadP2PPacket(bytes, MsgSize, out newMsgSize, out SteamIdRemote);

        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes))
        using (System.IO.BinaryReader b = new System.IO.BinaryReader(ms))
        {
            MsgType msgtype = (MsgType)b.ReadUInt32();
            switch (msgtype) 
            {
                case MsgType.PING:
                    Debug.Log("[STEAM] Pinged by peer " + SteamIdRemote.m_SteamID);
                    SendAck(SteamIdRemote);
                    break;
                case MsgType.ACK:
                    Debug.Log("[STEAM] Acknowledged by host " + SteamIdRemote.m_SteamID);
                    break;
                case MsgType.HGDU:
                    break;
                case MsgType.GDUH:
                    break;
            }
            print("SteamNetworking.ReadP2PPacket(bytes, " + MsgSize + ", out newMsgSize, out SteamIdRemote) - " + ret + " -- " + newMsgSize + " -- " + SteamIdRemote + " -- " + msgtype);
        }
    }

    private void CreateLobby()
    {
        if (SteamManager.Initialized && m_Lobby.m_SteamID == 0)
        {
            SteamAPICall_t handle = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, MAX_MEMBERS);
            m_OnLobbyCreatedCallResult.Set(handle);
            Debug.Log("[STEAM] Called: CreateLobby()");
            localIsHost = true;
            hostID = currentUserID;
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
        if (m_Lobby.m_SteamID > 0) 
        { 
            int numMembers = SteamMatchmaking.GetNumLobbyMembers(m_Lobby);
            for (int i = 0; i < numMembers; i++) 
            {
                CSteamID member = SteamMatchmaking.GetLobbyMemberByIndex(m_Lobby, i);
                Debug.Log(SteamFriends.GetFriendPersonaName(member));
                if (member != currentUserID) 
                {
                    members.Add(member);
                }
            }

            if (!localIsHost)
            {
                FindHost();
                SendPing(hostID);
                Debug.Log("[STEAM] Pinging Host");
            }
        }
    }

    private void LeaveLobby() {
        SteamMatchmaking.LeaveLobby(m_Lobby);
        m_Lobby = new CSteamID(0);
        if (localIsHost)
        {
            hostID = new CSteamID(0);
        }
        else 
        {
            SteamNetworking.CloseP2PSessionWithUser(hostID);
            hostID = new CSteamID(0);
        }

        localIsHost = false;
        connected = false;
    }

    private void FindHost()
    {
        foreach (CSteamID member in members)
        {
            if (member != currentUserID)
            {
                string hostStatus = SteamMatchmaking.GetLobbyMemberData(m_Lobby, member, "isHost");
                if (hostStatus != null)
                {
                    if (hostStatus == "true")
                    {
                        hostID = member;
                        Debug.Log("[STEAM] Current Host: " + member.m_SteamID);
                        break;
                    }
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
        SteamMatchmaking.SetLobbyMemberLimit(m_Lobby, MAX_MEMBERS);
        connected = true;
        UpdateLobbyMembers();
        SteamMatchmaking.SetLobbyMemberData(m_Lobby, "isHost", "true");
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
        SteamMatchmaking.SetLobbyMemberData(m_Lobby, "isHost", "false");
        Debug.Log("[STEAM] Lobby Entered: " + pCallback.m_ulSteamIDLobby);
        m_Lobby = new CSteamID(pCallback.m_ulSteamIDLobby);
        localIsHost = false;
        connected = true;
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

    /*
     * Callback
     * Called when player leaves
     */
    private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
    {
        Debug.Log("[STEAM] Chat update -- Player left? -- for " + m_Lobby.m_SteamID);
        UpdateLobbyMembers();
    }

    /*
     * Callback
     */
    void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
    {
        Debug.Log("[" + P2PSessionRequest_t.k_iCallback + " - P2PSessionRequest] - " + pCallback.m_steamIDRemote);

        bool ret = SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
        print("SteamNetworking.AcceptP2PSessionWithUser(" + pCallback.m_steamIDRemote + ") - " + ret);
    }

    /*
     * Callback
     */
    void OnP2PSessionConnectFail(P2PSessionConnectFail_t pCallback)
    {
        Debug.Log("[" + P2PSessionConnectFail_t.k_iCallback + " - P2PSessionConnectFail] - " + pCallback.m_steamIDRemote + " -- " + pCallback.m_eP2PSessionError);
    }

    /*
     * Callback
     */
    void OnSocketStatusCallback(SocketStatusCallback_t pCallback)
    {
        Debug.Log("[" + SocketStatusCallback_t.k_iCallback + " - SocketStatusCallback] - " + pCallback.m_hSocket + " -- " + pCallback.m_hListenSocket + " -- " + pCallback.m_steamIDRemote + " -- " + pCallback.m_eSNetSocketState);
    }

}

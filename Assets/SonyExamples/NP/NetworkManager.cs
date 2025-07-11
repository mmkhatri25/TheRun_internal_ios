using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
//using UnityEngine.Networking.NetworkSystem;

// Networking:
// Each member has an IP/Port number. Members on the same console share the same IP/Port address.
// A member belongs to a machine, and multiple members can be one one machine.
// A room contains multiple members.

#if UNITY_PS4
static class NetworkManager
{
/*
    public class TestMessage : MessageBase
    {
        public string onlineName;
        public byte[] data;

        static public TestMessage CreateTestMessage(string onlineName)
        {
            TestMessage msg = new TestMessage();

            msg.onlineName = onlineName;

            Random rand = new Random();
            msg.data = new byte[16];
            rand.NextBytes(msg.data);

            return msg;
        }

        public override string ToString()
        {
            string output = onlineName + " : ";

            for(int i = 0; i < data.Length; i++)
            {
                output += data[i] + ", ";
            }

            return output;
        }
    }

    abstract class Machine
    {
        public Sony.NP.Matching.NatType natType;
        public Sony.NP.NetworkUtils.NetInAddr ipAddress;
        public ushort port;
        public bool isHost;
        public UnityEngine.Networking.NetworkClient client;

        protected void CreateLocalClient()
        {
            client = UnityEngine.Networking.ClientScene.ConnectLocalServer();
            client.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnClientConnected);
            client.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnClientError);
            client.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnClientDisconnected);
            client.RegisterHandler(MyMsgType.SendTestMsg, OnClientTestMsg);
        }

        public void SetupGameClient(Sony.NP.NetworkUtils.NetInAddr serverIPAddr, int serverPortNum, int gamePortNum)
        {
            if (client != null)
            {
                return;
            }

            AddLogMsg("SetupGameClient : IP " + serverIPAddr + " : Port " + serverPortNum + " : Game Port : " + gamePortNum);

            client = new UnityEngine.Networking.NetworkClient();

            client.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnClientConnected);
            client.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnClientError);
            client.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnClientDisconnected);
            client.RegisterHandler(MyMsgType.SendTestMsg, OnClientTestMsg);

            System.Net.EndPoint endpoint = new UnityEngine.PS4.SceEndPoint(serverIPAddr.Addr, serverPortNum, gamePortNum);
            client.Connect(endpoint);
        }

        public abstract bool IsLocal
        {
            get;
        }

        public void OnClientConnected(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnClientConnected : " + ipAddress.ToString() + " : " + port);
        }

        public void OnClientError(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnClientError " );
        }

        public void OnClientDisconnected(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnClientDisconnected : " + ipAddress.ToString() + " : " + port);
        }

        public void OnClientTestMsg(NetworkMessage netMsg)
        {
            if (IsLocal)
            {
                AddLogMsg("OnClientTestMsg : Received from local client");
            }
            else
            {
                AddLogMsg("OnClientTestMsg : Received from remote machine : " + ipAddress.ToString() + " : " + port);
            }

            TestMessage msg = netMsg.ReadMessage<TestMessage>();

            if ( msg != null)
            {
                AddLogMsg("From: " + msg.ToString());
            }
        }

        public void AsyncDisconnect()
        {
            AddAction(() => { Disconnect(); });
        }

        public abstract void Disconnect();
    }

    // A local machine will contain a network server listening for packets, and can send to all connected clients.
    // In client-server topology only the local machine that is also a host needs to create the server.
    // to send data between all machines, each one must have a NetworkServer.
    // This also create a client to it's own local server
    class LocalMachine : Machine
    {
        public bool serverStarted = false;

        public override bool IsLocal
        {
            get { return true; }
        }

        public void AsyncStartServer()
        {
            if (serverStarted == false)
            {
                // Only do this for client-server
                // If this local machine is not a host then it shouldn't create a server
                //if ( isHost == false) return;

                serverStarted = true;
                // SetupGameServer must be executed on the main thread.
                AddAction(() => { SetupGameServer(); });
            }
        }

        // This must be executed on the main thread.
        public void SetupGameServer()
        {
            AddLogMsg("SetupGameServer : Server Port " + serverPort);
            int maxDefaultConnections = 8;

            UnityEngine.Networking.NetworkServer.Reset();

            UnityEngine.Networking.ConnectionConfig sceDefaultConfig = new UnityEngine.Networking.ConnectionConfig();
            sceDefaultConfig.AddChannel(UnityEngine.Networking.QosType.ReliableSequenced);
            sceDefaultConfig.AddChannel(UnityEngine.Networking.QosType.Unreliable);

            sceDefaultConfig.UsePlatformSpecificProtocols = true;       // enable use of UDPP2P protocol
            UnityEngine.Networking.HostTopology sceTopology = new UnityEngine.Networking.HostTopology(sceDefaultConfig, maxDefaultConnections);
            UnityEngine.Networking.NetworkServer.Configure(sceTopology);

            UnityEngine.Networking.NetworkServer.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnServerConnected);
            UnityEngine.Networking.NetworkServer.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnServerError);
            UnityEngine.Networking.NetworkServer.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnServerDisconnected);
            UnityEngine.Networking.NetworkServer.RegisterHandler(MyMsgType.SendTestMsg, OnServerTestMsg);

            UnityEngine.Networking.NetworkServer.Listen(serverPort);

            CreateLocalClient();
        }

        public void OnServerConnected(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnServerConnected " );
        }

        public void OnServerError(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnServerError " );
        }

        public void OnServerDisconnected(UnityEngine.Networking.NetworkMessage netMsg)
        {
            AddLogMsg("OnServerDisconnected " );
        }

        public void OnServerTestMsg(NetworkMessage netMsg)
        {
            AddLogMsg("OnServerTestMsg : Recieved from remote machine : " + netMsg.conn.address);

            TestMessage msg = netMsg.ReadMessage<TestMessage>();

            if (msg != null)
            {
                AddLogMsg("From: " + msg.ToString());
            }
        }

        public override void Disconnect()
        {
            if (serverStarted == true)
            {
                UnityEngine.Networking.NetworkServer.DisconnectAll();
            }

            serverStarted = false;
        }
    }

    // A remote machine contains a client which is used to send data to that machine.
    class RemoteMachine : Machine
    {
        public bool clientStarted = false;

        public override bool IsLocal
        {
            get { return false; }
        }

        public void AsyncStartClient()
        {
            if (clientStarted == false)
            {
                // Only do this for client-server
                // If the remote machine isn't a host don't try to connect to it
                //if ( isHost == false ) return;

                if (ipAddress.Addr == 0) return;

                clientStarted = true;
                AddAction(() => { SetupGameClient(ipAddress, port, 25001); });
            }
        }

        public override void Disconnect()
        {
            if (clientStarted == true && client.isConnected == true)
            {
                client.Disconnect();
            }

            clientStarted = false;
        }
    }

    class Member
    {
        public Machine machine;
        public UInt16 roomMemberId;
        public bool isOwner;
        public bool isLocal;
        public Sony.NP.Core.OnlineUser onlineUser;
        public Sony.NP.Core.UserServiceUserId localUserId;

        public Sony.NP.Matching.NatType natType;
        public Sony.NP.NetworkUtils.NetInAddr ipAddress;
        public ushort port;
    }

    class Room
    {
        public List<Member> members = new List<Member>();
        public List<Machine> machines = new List<Machine>();
        public LocalMachine localMachine;
        public Machine hostMachine;

        public UInt64 roomId;

        public Member FindMember(UInt16 roomMemberId)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].roomMemberId == roomMemberId)
                {
                    return members[i];
                }
            }
            return null;
        }

        public int GetNumLocalUsers()
        {
            int count = 0;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].isLocal == true)
                {
                    count++;
                }
            }
            return count;
        }

        public bool IsLocalOwner(UInt16 roomMemberId)
        {
            Member member = FindMember(roomMemberId);

            if (member == null) return false;

            if(member.isLocal == true && member.isOwner == true)
            {
                return true;
            }

            return false;
        }

        public Member FindMemberLocalId(int localId)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].localUserId.Id == localId)
                {
                    return members[i];
                }
            }
            return null;
        }

        public UInt16[] GetMemberIds(int maxUsers)
        {
            List<UInt16> ids = new List<ushort>();

            for (int i = 0; i < members.Count; i++)
            {
                ids.Add(members[i].roomMemberId);

                if(ids.Count == maxUsers) // Must not exceed the max number of users
                {
                    return ids.ToArray();
                }
            }

            return ids.ToArray();
        }

        public Member RemoveMember(UInt16 roomMemberId)
        {
            Member toRemove = FindMember(roomMemberId);

            if (toRemove != null)
            {
                members.Remove(toRemove);
            }

            return toRemove;
        }

        public Machine FindMachine(Sony.NP.NetworkUtils.NetInAddr ipAddress, ushort port)
        {
            for (int i = 0; i < machines.Count; i++)
            {
                if (machines[i].ipAddress.Addr == ipAddress.Addr &&
                    machines[i].port == port)
                {
                    return machines[i];
                }
            }
            return null;
        }

        public void AddMachine(Machine newMachine)
        {
            machines.Add(newMachine);

            if ( newMachine.IsLocal == true )
            {
                localMachine = newMachine as LocalMachine;
            }

            if ( newMachine.isHost == true )
            {
                hostMachine = newMachine;
            }
        }

        public void RemoveMachine(Machine machine)
        {
            if (machine != null)
            {
                if ( machines.Remove(machine) == true )
                {
                    machine.AsyncDisconnect();
                }
            }
        }

        public int CountMembersOnMachine(Machine machine)
        {
            int count = 0;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].machine == machine)
                {
                    count++;
                }
            }

            return count;
        }
    }

    static bool isHost;
    static Room currentRoom;

    static public bool IsHost
    {
        get { return isHost; }
    }

    static public bool IsClient
    {
        get { return !isHost; }
    }

    static public bool IsLocalOwner(UInt16 roomMemberId)
    {
        if (currentRoom == null) return false;

        return currentRoom.IsLocalOwner(roomMemberId);
    }

    static public bool IsLocalMember(UInt16 roomMemberId)
    {
        if (currentRoom == null) return false;

        Member memeber = currentRoom.FindMember(roomMemberId);

        if (memeber == null) return false;

        return memeber.isLocal;
    }

    static public int CurrentRoomMemberCount
    {
        get
        {
            if (currentRoom == null) return 0;

            return currentRoom.members.Count;
        }
    }

    static public int CurrentRoomNumLocalUsers
    {
        get
        {
            if (currentRoom == null) return 0;

            return currentRoom.GetNumLocalUsers();
        }
    }

    static public UInt16[] GetMemberIds(int maxUsers)
    {
        return currentRoom.GetMemberIds(maxUsers);
    }


    static public void BroadcastToAllMembers()
    {
        if ( currentRoom.localMachine != null && currentRoom.localMachine.serverStarted == true)
        {
            AddLogMsg("Broadcast To All Members");

            Member member = currentRoom.FindMemberLocalId(User.GetActiveUserId);

            string onlineName = "";
            if (member != null && member.onlineUser != null)
            {
                onlineName = member.onlineUser.OnlineID.Name;
            }

            TestMessage msg = TestMessage.CreateTestMessage(onlineName);
            NetworkServer.SendToAll(MyMsgType.SendTestMsg, msg);
        }
    }

    static public void SendToHost()
    {
        if ( currentRoom.hostMachine != null)
        {
            AddLogMsg("Send to Host only");

            Member member = currentRoom.FindMemberLocalId(User.GetActiveUserId);

            string onlineName = "";
            if (member != null && member.onlineUser != null)
            {
                onlineName = member.onlineUser.OnlineID.Name;
            }

            TestMessage msg = TestMessage.CreateTestMessage(onlineName);

            AddLogMsg(msg.ToString());
            currentRoom.hostMachine.client.Send(MyMsgType.SendTestMsg, msg);
        }
    }

    static int serverPort = 25001;
    static Queue<System.Action> actionQueue = new Queue<System.Action>();

    static public void Init()
    {
    }

    static Object _syncObject = new object();

    static void AddAction(System.Action action)
    {
        lock (_syncObject)
        {
            actionQueue.Enqueue(action);
        }
    }

    public class MyMsgType
    {
        public static short SendTestMsg = MsgType.Highest + 1;
    };

    static public void Update()
    {
        int numActions = actionQueue.Count;

        if (numActions > 0)
        {
            System.Action action = null;

            lock (_syncObject)
            {
                action = actionQueue.Dequeue();
            }
            action();
        }
    }

    static private void CreateRoom(Sony.NP.Matching.Room room)
    {
        // Called when either joining or creating a room. Note that the isHost flag will be set correctly,
        // depending on if the room has been created by a local host or this is a client joining an existing room.

        // Create the room class and make it either act as a host or client network.
        // Then add each member. Important to keep track of each member room id as this is primary method of tracking member wen recieving notifications.
        currentRoom = new Room();

        currentRoom.roomId = room.RoomId;

        for(int i = 0; i < room.CurrentMembers.Length; i++)
        {
            AddNewMember(room.CurrentMembers[i]);
        }
    }

    static private void AddNewMember(Sony.NP.Matching.Member member)
    {
        if ( currentRoom.FindMember(member.RoomMemberId) != null)
        {
            return;
        }

        Member newMember = new Member();

        newMember.roomMemberId = member.RoomMemberId;
        newMember.isOwner = member.IsOwner;
        newMember.onlineUser = member.OnlineUser;

        Sony.NP.Core.UserServiceUserId localUserId;

        if ( IsLocalUserAccountId(member.OnlineUser.AccountId, out localUserId) == true )
        {
            newMember.isLocal = true;
            newMember.localUserId = localUserId;
        }
        else
        {
            newMember.isLocal = false;
        }

        if ( member.SignalingInformation.NatType != Sony.NP.Matching.NatType.Invalid )
        {
            newMember.natType = member.SignalingInformation.NatType;
        }

        currentRoom.members.Add(newMember);

        UpdateMachineList(newMember);
    }

    static private void SignalingUpdate(Sony.NP.Matching.Member member)
    {
        if ( currentRoom == null || member == null )
        {
            return;
        }

        Member roomMember = currentRoom.FindMember(member.RoomMemberId);

        if (roomMember == null)
        {
            // May get a signaling update even after the member has left the room.
            return;
        }

        if (member.SignalingInformation.Status == Sony.NP.Matching.SignalingStatus.Established)
        {
            roomMember.ipAddress = member.SignalingInformation.IpAddress;
            roomMember.port = member.SignalingInformation.Port;

            UpdateMachineList(roomMember);
        }
    }

    // This can be called, either then a room is created with whatever members it current has, or
    // when a new member is added to the room or when a signaling update is recieved containing the IP addess/port.

    // Can only really create a machine when the IP address and port are known. This is because mulitple members can be
    // on the same machine. 
    // Can also create a machine if it is local, even if the ip address is unknown. In the case of only one local user in the room
    // the IP address will be 0.0.0.0, however if multiple local users are in a room they will each get updated with an IP address.
    static private void UpdateMachineList(Member roomMember)
    {
        Machine machine = roomMember.machine;
        if (machine == null && roomMember.ipAddress.Addr != 0)
        {
            machine = currentRoom.FindMachine(roomMember.ipAddress, roomMember.port);
        }

        if (machine == null)
        {
            // Create a new machine
            // At this point no machine can be found for this member. If the IP/Port isn't set then can only create a machine
            // if this is a local one.
            // Otherwise must leave and wait for a MemberSignalingUpdate notification to arrive with the IP address.
            if (roomMember.ipAddress.Addr == 0 && roomMember.isLocal == false)
            {
                // This owner is neither local, nor the host, and they don't have a valid ip address. Must wait for another update for this user.
                return;
            }

            // Now check if the member is local, if a local machine already exists.
           if (roomMember.isLocal == true && currentRoom.localMachine != null)
            {
                machine = currentRoom.localMachine;
            }
            else
            {
                // No machine found, so create one.
                if (roomMember.isLocal == true)
                {
                    machine = new LocalMachine();
                }
                else
                {
                    machine = new RemoteMachine();
                }

                if (roomMember.isOwner == true)
                {
                    machine.isHost = true;
                }

                currentRoom.AddMachine(machine);
            }
        }

        // Update the machine state
        if (roomMember.natType != Sony.NP.Matching.NatType.Invalid)
        {
            machine.natType = roomMember.natType;
        }

        if (machine.ipAddress.Addr == 0)
        {
            machine.ipAddress = roomMember.ipAddress;
            machine.port = roomMember.port;
        }

        if (roomMember.isOwner == true)
        {
            if (machine.isHost == false)
            {
                // Update the machine so it becomes a host.
                machine.isHost = true;
                currentRoom.hostMachine = machine;
            }
        }

        if (roomMember.machine == null)
        {
            roomMember.machine = machine;
        }

        if (machine.IsLocal == true)
        {
            // Create a local server on the local machine
            ((LocalMachine)machine).AsyncStartServer();
        }
        else
        {
            // For a remote machine start a client
            ((RemoteMachine)machine).AsyncStartClient();
        }
    }

    static private void RemoveMember(UInt16 roomMemberId)
    {
        Member member = currentRoom.RemoveMember(roomMemberId);

        if ( member != null && member.machine != null)
        {
            if (currentRoom.CountMembersOnMachine(member.machine) == 0)
            {
                currentRoom.RemoveMachine(member.machine);
            }
        }
    }

    static public void OutputInfo()
    {
        AddLogMsg("Room Info");

        if (currentRoom == null)
        {
            AddLogMsg("No room created");
        }
        else
        {
            AddLogMsg("# Members  : " + currentRoom.members.Count);

            for (int i = 0; i < currentRoom.members.Count; i++)
            {
                Member member = currentRoom.members[i];

                AddLogMsg("    Member : [" + i + "]");
                AddLogMsg("        Room Member Id = " + member.roomMemberId);
                AddLogMsg("        Is Owner = " + member.isOwner + " : Is Local = " + member.isLocal);
                if (member.isLocal == true)
                {
                    AddLogMsg("        Local User Id = " + member.localUserId);
                }
                AddLogMsg("        Online User = " + member.onlineUser);
                AddLogMsg("        Nat Type = " + member.natType + " : IP Address = " + member.ipAddress + " : Port = " + member.port);
            }

            OnScreenLog.AddNewLine();

            AddLogMsg("# Machines : " + currentRoom.machines.Count);

            for (int i = 0; i < currentRoom.machines.Count; i++)
            {
                Machine machine = currentRoom.machines[i];

                AddLogMsg("    Machine : [" + i + "]");
                AddLogMsg("        Nat Type        : " + machine.natType);
                AddLogMsg("        IP Address/Port : " + machine.ipAddress + " : " + machine.port);
                AddLogMsg("        Is Local        : " + machine.IsLocal);
                AddLogMsg("        Is Host         : " + machine.isHost);
            }

            OnScreenLog.AddNewLine();
        }
    }
    
    static private void CloseSession()
    {
        AddLogMsg("Close Session");

        // Disconnect all machines.
        while(currentRoom.machines.Count > 0 )
        {
            Machine machine = currentRoom.machines[0];

            currentRoom.RemoveMachine(machine);
        }

        currentRoom = null;
    }

    static public void OnAsyncEvent(Sony.NP.NpCallbackEvent callbackEvent)
    {
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Matching)
        {
            if (callbackEvent.Response.ReturnCode == Sony.NP.Core.ReturnCodes.SUCCESS)
            {
                switch (callbackEvent.ApiCalled)
                {
                    case Sony.NP.FunctionTypes.MatchingCreateRoom:
                        HandleCreateRoom(callbackEvent.Response as Sony.NP.Matching.RoomResponse);
                        break;
                    case Sony.NP.FunctionTypes.MatchingJoinRoom:
                        HandleJoinRoom(callbackEvent.Response as Sony.NP.Matching.RoomResponse);
                        break;
                    case Sony.NP.FunctionTypes.MatchingLeaveRoom:
                        HandleLeaveRoom(callbackEvent.Request as Sony.NP.Matching.LeaveRoomRequest, callbackEvent.Response as Sony.NP.Core.EmptyResponse);
                        break;
                    default:
                        break;
                }
            }
        }

        // Notifications
        if (callbackEvent.Service == Sony.NP.ServiceTypes.Notification)
        {
            switch (callbackEvent.ApiCalled)
            {
                case Sony.NP.FunctionTypes.NotificationRefreshRoom:
                    HandleRefreshRoom(callbackEvent.Response as Sony.NP.Matching.RefreshRoomResponse);
                    break;
                default:
                    break;
            }
        }
    }

    static public void HandleCreateRoom(Sony.NP.Matching.RoomResponse roomResponse)
    {
        if (roomResponse == null) return;

        AddLogMsg("Create Room");

        Sony.NP.Matching.Room room = roomResponse.Room;

        isHost = true;
        CreateRoom(room);
    }

    static public void HandleJoinRoom(Sony.NP.Matching.RoomResponse roomResponse)
    {
        if (roomResponse == null) return;

        AddLogMsg("Join Room");

        Sony.NP.Matching.Room room = roomResponse.Room;

        isHost = false;
        CreateRoom(room);
    }

    static private void HandleLeaveRoom(Sony.NP.Matching.LeaveRoomRequest request, Sony.NP.Core.EmptyResponse emptyResponse)
    {
        // A user has left the room. If there are no local users in the current room then disconnect this consoles from all the others
        Member member = currentRoom.FindMemberLocalId(request.UserId.Id);

        if (NetworkManager.IsLocalOwner(member.roomMemberId) ||
            (NetworkManager.IsLocalMember(member.roomMemberId) == true && NetworkManager.CurrentRoomNumLocalUsers <= 1))
        {
            // Local owner of the room is about to leave and there is only 1 of them left. The room is no longer required.
            // If this is the host then the room will be destoryed, otherwise if this is a remote connection the room is no longer
            // required on this console.
            for (int i = 0; i < currentRoom.machines.Count; i++)
            {
                Machine machine = currentRoom.machines[i];
                machine.AsyncDisconnect();
            }
        }
    }

    static private void HandleRefreshRoom(Sony.NP.Matching.RefreshRoomResponse refreshRoomResponse)
    {
        if (refreshRoomResponse == null) return;

        if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomDestroyed)
        {
            AddLogMsg("Room has been destroyed");

            CloseSession();
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.RoomKickedOut)
        {
            AddLogMsg("Kicked out of room");

            CloseSession();
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberJoined)
        {
            AddLogMsg("Member Joined");

            AddNewMember(refreshRoomResponse.MemberInfo);         
        }
        else if (refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberLeft)
        {
            AddLogMsg("Member Left");

            RemoveMember(refreshRoomResponse.MemberInfo.RoomMemberId);
        }
        else if(refreshRoomResponse.Reason == Sony.NP.Matching.Reasons.MemberSignalingUpdate)
        {
            AddLogMsg("Member Signaling Update");

            SignalingUpdate(refreshRoomResponse.MemberInfo);
        }
            
    }

    static private void AddLogMsg(string msg)
    {
        OnScreenLog.Add("[Networking] " + msg, UnityEngine.Color.green);
    }

    public static bool IsLocalUserAccountId(Sony.NP.Core.NpAccountId accountId, out Sony.NP.Core.UserServiceUserId localUserId)
    {
        localUserId = 0;

        Sony.NP.UserProfiles.LocalUsers users = new Sony.NP.UserProfiles.LocalUsers();
        try
        {
            Sony.NP.UserProfiles.GetLocalUsers(users);
        }
        catch (Sony.NP.NpToolkitException)
        {
            // This means that one or more of the user has an error code associated with them. This might mean they are not signed in or are not signed up to an online account.
        }

        try
        {
            for (int i = 0; i < users.LocalUsersIds.Length; i++)
            {
                if (users.LocalUsersIds[i].AccountId.Id == accountId.Id)
                {
                    localUserId = users.LocalUsersIds[i].UserId;
                    return true;
                }
            }
        }
        catch (Sony.NP.NpToolkitException e)
        {
            OnScreenLog.AddError("Exception : " + e.ExtendedMessage);
        }

        return false;
    }
*/
}
#endif

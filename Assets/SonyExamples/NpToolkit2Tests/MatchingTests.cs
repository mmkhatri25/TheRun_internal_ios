#if NPT2_MATCHING_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;

namespace NpToolkitTests
{
    [TestFixture, Description("Matching tests")]
    public class MatchingTests : BaseTestFramework
    {
        static Sony.NP.Matching.AttributeMetadata[] attributesMetadata;
        static Sony.NP.Matching.Room currentRoom = null;

        public void CreateAttributeMetaData()
        {
            //attributesMetadata = new Sony.NP.Matching.AttributeMetadata[1];
            //attributesMetadata[0] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("NPC_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search, 64);

            attributesMetadata = new Sony.NP.Matching.AttributeMetadata[9];

            // Member attributes
            attributesMetadata[0] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("CURRENT_STATE", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid, 32);
            attributesMetadata[1] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("MAX_BOOSTS", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid);
            attributesMetadata[2] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("MAX_TEAMS", Sony.NP.Matching.AttributeScope.Member, Sony.NP.Matching.RoomAttributeVisibility.Invalid);

            // Room search attributes
            attributesMetadata[3] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("NPC_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search, 64);
            attributesMetadata[4] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("TRACK_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);
            attributesMetadata[5] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("COUNTRY_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);
            attributesMetadata[6] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("SEASON_ID", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Search);

            // Internal Room attributes
            attributesMetadata[7] = Sony.NP.Matching.AttributeMetadata.CreateIntegerAttribute("HOST_NAME", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.Internal);

            // External Room attributes
            attributesMetadata[8] = Sony.NP.Matching.AttributeMetadata.CreateBinaryAttribute("SPECIAL_MESSAGE", Sony.NP.Matching.AttributeScope.Room, Sony.NP.Matching.RoomAttributeVisibility.External, 128);
        }

        public Sony.NP.Matching.Attribute[] CreateDefaultAttributeValues()
        {
            string testBinary = "Some test binary data";
            byte[] data = System.Text.Encoding.ASCII.GetBytes(testBinary);

            //Sony.NP.Matching.Attribute[] attributes = new Sony.NP.Matching.Attribute[1];
            //attributes[0] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("NPC_NAME"), data);     

            Sony.NP.Matching.Attribute[] attributes = new Sony.NP.Matching.Attribute[9];

            // Member attributes
            attributes[0] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("CURRENT_STATE"), data);
            attributes[1] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("MAX_BOOSTS"), 1);
            attributes[2] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("MAX_TEAMS"), 3);

            // Room search attributes
            attributes[3] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("NPC_NAME"), data);
            attributes[4] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("TRACK_ID"), 1);
            attributes[5] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("COUNTRY_ID"), 2);
            attributes[6] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("SEASON_ID"), 3);

            // Internal Room attributes
            attributes[7] = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("HOST_NAME"), 3);

            // External Room attributes
            attributes[8] = Sony.NP.Matching.Attribute.CreateBinaryAttribute(FindAttributeMetaData("SPECIAL_MESSAGE"), data);

            return attributes;
        }

        public Sony.NP.Matching.AttributeMetadata FindAttributeMetaData(string name)
        {
            for (int i = 0; i < attributesMetadata.Length; i++)
            {
                if (attributesMetadata[i].Name == name)
                {
                    return attributesMetadata[i];
                }
            }

            //OnScreenLog.AddError("FindAttributeMetaData : Can't find attribute metadata with name : " + name);
            return new Sony.NP.Matching.AttributeMetadata();
        }

        public Sony.NP.Matching.CreateRoomRequest CreateRoomRequest()
        {
            Sony.NP.Matching.CreateRoomRequest request = new Sony.NP.Matching.CreateRoomRequest();
            request.UserId = GetPrimaryUserId();

            Sony.NP.Matching.Attribute[] attributes = CreateDefaultAttributeValues();

            request.Attributes = attributes;

            request.Name = "Room : Unit Test ";
            request.Status = "Room was created for Unit Test ";

            Sony.NP.Matching.LocalizedSessionInfo[] localisedInfo = new Sony.NP.Matching.LocalizedSessionInfo[2];
            //localisedInfo[0] = new Sony.NP.Matching.LocalizedSessionInfo("German session text", "German text session created on frame " + OnScreenLog.FrameCount, "de");
            //localisedInfo[1] = new Sony.NP.Matching.LocalizedSessionInfo("French session text", "French text session created on frame " + OnScreenLog.FrameCount, "fr");
            localisedInfo[0] = new Sony.NP.Matching.LocalizedSessionInfo("German session text", "German text session created on frame ", "de");
            localisedInfo[1] = new Sony.NP.Matching.LocalizedSessionInfo("French session text", "French text session created on frame ", "fr");

            request.MaxNumMembers = 8;
            request.OwnershipMigration = Sony.NP.Matching.RoomMigrationType.OwnerBind; // OwnerMigration means session will be deleted when joined users reaches 0
            request.Topology = Sony.NP.Matching.TopologyType.Mesh;
            request.Visibility = Sony.NP.Matching.RoomVisibility.PublicRoom;
            request.WorldNumber = 1;
            request.JoinAllLocalUsers = true;

            // Must define an image for the rooms session otherwise it can't be created.
            Sony.NP.Matching.SessionImage sessionImage = new Sony.NP.Matching.SessionImage();
            sessionImage.SessionImgPath = Application.streamingAssetsPath + "/PS4SessionImage.jpg";
            request.Image = sessionImage;

            request.FixedData = System.Text.Encoding.ASCII.GetBytes("Fixed room data test");
            //request.ChangeableData = System.Text.Encoding.ASCII.GetBytes("Changeable room data test setup on frame " + OnScreenLog.FrameCount);
            request.ChangeableData = System.Text.Encoding.ASCII.GetBytes("Changeable room data test setup on frame ");

            return request;
        }

        [UnityTest, Order(1), Description("Set the initial configuration")]
        public IEnumerator SetInitConfiguration()
        {
            yield return new WaitUntil(IsInitialized);

            CreateAttributeMetaData();

            Debug.Log("SetInitConfiguration test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Matching.SetInitConfigurationRequest request = new Sony.NP.Matching.SetInitConfigurationRequest();

                request.UserId = GetPrimaryUserId();

                request.Attributes = attributesMetadata;

                request.Async = true;

                int requestId = Sony.NP.Matching.SetInitConfiguration(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("SetInitConfiguration - test passed");
        }

        [UnityTest, Order(2), Description("Get Worlds")]
        public IEnumerator GetWorlds()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetWorlds test started");

            Sony.NP.Matching.WorldsResponse response = new Sony.NP.Matching.WorldsResponse();

            try
            {
                Sony.NP.Matching.GetWorldsRequest request = new Sony.NP.Matching.GetWorldsRequest();
                request.UserId = GetPrimaryUserId();

                int requestId = Sony.NP.Matching.GetWorlds(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("GetWorlds - test passed");
        }

        [UnityTest, Order(3), Description("Search Rooms")]
        public IEnumerator SearchRooms()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("SearchRooms test started");

            Sony.NP.Matching.RoomsResponse response = new Sony.NP.Matching.RoomsResponse();

            try
            {
                Sony.NP.Matching.SearchRoomsRequest request = new Sony.NP.Matching.SearchRoomsRequest();
                request.UserId = GetPrimaryUserId();

                Sony.NP.Matching.SearchClause[] searchClauses = new Sony.NP.Matching.SearchClause[1];

                searchClauses[0].AttributeToCompare = Sony.NP.Matching.Attribute.CreateIntegerAttribute(FindAttributeMetaData("TRACK_ID"), 1);
                searchClauses[0].OperatorType = Sony.NP.Matching.SearchOperatorTypes.Equals;

                request.SearchClauses = searchClauses;

                int requestId = Sony.NP.Matching.SearchRooms(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("SearchRooms - test passed");
        }

        [UnityTest, Order(4), Description("Create Room")]
        public IEnumerator CreateRoom()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("CreateRoom test started");

            Sony.NP.Matching.RoomResponse response = new Sony.NP.Matching.RoomResponse();

            try
            {
                Sony.NP.Matching.CreateRoomRequest request = CreateRoomRequest();
                request.JoinAllLocalUsers = true;

                int requestId = Sony.NP.Matching.CreateRoom(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            currentRoom = response.Room;

            Debug.Log("CreateRoom - test passed");
        }

        [UnityTest, Order(5), Description("Room Ping Time Room")]
        public IEnumerator GetRoomPingTime()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetRoomPingTime test started");

            Sony.NP.Matching.GetRoomPingTimeResponse response = new Sony.NP.Matching.GetRoomPingTimeResponse();

            try
            {
                Sony.NP.Matching.GetRoomPingTimeRequest request = new Sony.NP.Matching.GetRoomPingTimeRequest();
                request.UserId = GetPrimaryUserId();

                request.RoomId = currentRoom.RoomId;

                int requestId = Sony.NP.Matching.GetRoomPingTime(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("GetRoomPingTime - test passed");
        }

        [UnityTest, Order(15), Description("Leave Room")]
        public IEnumerator LeaveRoom()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("LeaveRoom test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Matching.LeaveRoomRequest request = new Sony.NP.Matching.LeaveRoomRequest();
                request.UserId = GetPrimaryUserId();

                request.RoomId = currentRoom.RoomId;

                Sony.NP.Matching.PresenceOptionData optiondata = new Sony.NP.Matching.PresenceOptionData();
                optiondata.DataAsString = "I'm out of here.";

                request.NotificationDataToMembers = optiondata;

                int requestId = Sony.NP.Matching.LeaveRoom(request, response);

                Assert.GreaterOrEqual(requestId, 0);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Aysnc call failed");
                LogException(e);
            }

            yield return new WaitUntil(() => response.Locked == false);

            OutputAsyncResponseEvent(response);

            // Response object is no longer locked so result should have been returned.
            Assert.AreEqual(Sony.NP.Core.ReturnCodes.SUCCESS, response.ReturnCode);

            Debug.Log("LeaveRoom - test passed");
        }

    }
}
#endif

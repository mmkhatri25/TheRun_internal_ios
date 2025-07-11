#if NPT2_TUS_TESTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System;

namespace NpToolkitTests
{
    [TestFixture, Description("Tus tests")]
    public class TusTests : BaseTestFramework
    {
        [UnityTest, Order(1), Description("Set a Tus Variable")]
        public IEnumerator SetVariables()
        {
            yield return new WaitUntil(IsInitialized);
  
            Debug.Log("SetVariables test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Tus.SetVariablesRequest request = new Sony.NP.Tus.SetVariablesRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);

                request.UserId = GetPrimaryUserId();

                Sony.NP.Tus.VariableInput[] variables = new Sony.NP.Tus.VariableInput[1];
                variables[0].Value = 1;
                variables[0].SlotId = 1;

                request.Vars = variables;

                int requestId = Sony.NP.Tus.SetVariables(request, response);

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

            Debug.Log("SetVariables - test passed");
        }

        [UnityTest, Order(2), Description("Get a Tus Variable")]
        public IEnumerator GetVariables()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetVariables test started");

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            try
            {
                Sony.NP.Tus.GetVariablesRequest request = new Sony.NP.Tus.GetVariablesRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);

                request.UserId = GetPrimaryUserId();

                Int32[] slotIds = new Int32[4];
                slotIds[0] = 1;
                slotIds[1] = 2;
                slotIds[2] = 3;
                slotIds[3] = 4;

                request.SlotIds = slotIds;

                int requestId = Sony.NP.Tus.GetVariables(request, response);

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

            Debug.Log("GetVariables - test passed");
        }

        [UnityTest, Order(3), Description("Set a Tus Virtual user Variable")]
        public IEnumerator SetVirtualVariables()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("SetVirtualVariables test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Tus.SetVariablesRequest request = new Sony.NP.Tus.SetVariablesRequest();

                Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
                id.Name = "_ERGVirtualUser1";
                request.TusUser = new Sony.NP.Tus.UserInput(id);

                request.UserId = GetPrimaryUserId();

                Sony.NP.Tus.VariableInput[] variables = new Sony.NP.Tus.VariableInput[1];
                variables[0].Value = 1;
                variables[0].SlotId = 5; // Slot 5 is configured for the virtual user

                request.Vars = variables;

                int requestId = Sony.NP.Tus.SetVariables(request, response);

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

            Debug.Log("SetVirtualVariables - test passed");
        }

        [UnityTest, Order(4), Description("Get a Tus Virtual User Variable")]
        public IEnumerator GetVirtualVariables()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetVirtualVariables test started");

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            try
            {
                Sony.NP.Tus.GetVariablesRequest request = new Sony.NP.Tus.GetVariablesRequest();

                Sony.NP.Tus.VirtualUserID id = new Sony.NP.Tus.VirtualUserID();
                id.Name = "_ERGVirtualUser1";
                request.TusUser = new Sony.NP.Tus.UserInput(id);

                request.UserId = GetPrimaryUserId();

                Int32[] slotIds = new Int32[1];
                slotIds[0] = 5;

                request.SlotIds = slotIds;

                int requestId = Sony.NP.Tus.GetVariables(request, response);

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

            Debug.Log("GetVirtualVariables - test passed");
        }

        [UnityTest, Order(5), Description("Add to and Get Tus Variable")]
        public IEnumerator AddToAndGetVariable()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("AddToAndGetVariable test started");

            Sony.NP.Tus.VariablesResponse response = new Sony.NP.Tus.VariablesResponse();

            try
            {
                Sony.NP.Tus.AddToAndGetVariableRequest request = new Sony.NP.Tus.AddToAndGetVariableRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);

                request.UserId = GetPrimaryUserId();

                Sony.NP.Tus.VariableInput var = new Sony.NP.Tus.VariableInput();
                var.Value = 100;
                var.SlotId = 1;

                request.Var = var;

                int requestId = Sony.NP.Tus.AddToAndGetVariable(request, response);

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

            Debug.Log("AddToAndGetVariable - test passed");
        }

        [UnityTest, Order(6), Description("Set Tus Data")]
        public IEnumerator SetData()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("SetData test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Tus.SetDataRequest request = new Sony.NP.Tus.SetDataRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);

                request.Data = new byte[] { 1, 2, 3, 4 };

                request.SupplementaryInfo = new byte[] { 5, 6, 7, 8 };

                request.SlotId = 1;

                request.UserId = GetPrimaryUserId();

                int requestId = Sony.NP.Tus.SetData(request, response);

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

            Debug.Log("SetData - test passed");
        }

        [UnityTest, Order(7), Description("Get Tus Data")]
        public IEnumerator GetData()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("GetData test started");

            Sony.NP.Tus.GetDataResponse response = new Sony.NP.Tus.GetDataResponse();

            try
            {
                Sony.NP.Tus.GetDataRequest request = new Sony.NP.Tus.GetDataRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);
                request.SlotId = 1;

                request.UserId = GetPrimaryUserId();

                int requestId = Sony.NP.Tus.GetData(request, response);

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

            Debug.Log("GetData - test passed");
        }

        [UnityTest, Order(8), Description("Delete Tus Data")]
        public IEnumerator DeleteData()
        {
            yield return new WaitUntil(IsInitialized);

            Debug.Log("DeleteData test started");

            Sony.NP.Core.EmptyResponse response = new Sony.NP.Core.EmptyResponse();

            try
            {
                Sony.NP.Tus.DeleteDataRequest request = new Sony.NP.Tus.DeleteDataRequest();

                Sony.NP.Core.NpAccountId accountId = GetLocalAccountId(GetPrimaryUserId().Id);

                request.TusUser = new Sony.NP.Tus.UserInput(accountId);

                Int32[] slotIds = new Int32[1];
                slotIds[0] = 1;

                request.SlotIds = slotIds;

                request.UserId = GetPrimaryUserId();

                int requestId = Sony.NP.Tus.DeleteData(request, response);

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

            Debug.Log("DeleteData - test passed");
        }
    }
}
#endif

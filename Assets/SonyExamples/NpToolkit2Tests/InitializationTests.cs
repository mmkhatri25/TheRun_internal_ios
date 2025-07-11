#if NPT2_INIT_TESTS
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;

namespace NpToolkitTests
{
    [TestFixture, Description("Check all startup, initialization and logged in user tests")]
    public class StartupTests : BaseTestFramework
    {
        //[PrebuildSetup("UnitTestsEditorSetup")]
        [UnityTest, Order(1), Description("Test if the SDK version used by Unity matches the SDK in the native plugin")]
        public IEnumerator InitializeToolkit2()
        {
            yield return new WaitUntil(IsInitialized);

            // ONLy test the major and minor version numbers. Ingorne the lower 3 bytes which contains the patch number.
            uint runtimeSDK = (UnityEngine.PS4.Utility.sdkVersion & 0xFFFFF000);
            uint pluginSDK = (initResult.SceSDKVersionValue & 0xFFFFF000);

            Debug.Log("SDK Versions : " + UnityEngine.PS4.Utility.sdkVersion.ToString("x8") + " = " + initResult.SceSDKVersionValue.ToString("x8"));

            Assert.AreEqual(runtimeSDK, pluginSDK);

            Debug.Log("InitialiseNpToolkit2 finished");
        }

        [UnityTest, Order(2), Description("Check exceptions are correctly thrown from the NpToolkit2 API.")]
        public IEnumerator InitializeExceptionTest()
        {
            yield return new WaitUntil(IsInitialized);

            Sony.NP.InitToolkit init = new Sony.NP.InitToolkit();

            try
            {
                init.SetPushNotificationsFlags(Sony.NP.PushNotificationsFlags.NewGameDataMessage | Sony.NP.PushNotificationsFlags.NewInGameMessage |
                                        Sony.NP.PushNotificationsFlags.NewInvitation | Sony.NP.PushNotificationsFlags.UpdateBlockedUsersList |
                                        Sony.NP.PushNotificationsFlags.UpdateFriendPresence | Sony.NP.PushNotificationsFlags.UpdateFriendsList);

                Sony.NP.Main.Initialize(init);

                Debug.LogAssertion("No exception was thrown even though NpToolkit should have already been initialized.");
            }
            catch (Sony.NP.NpToolkitException)
            {
                Debug.Log("Exception thrown - test past.");
            }
            catch (System.Exception e)
            {
                // Unexcepted expection occured.
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Check there is at least one logged in User with PSN account, otherwise most tests will fail anyway.
        /// </summary>
        [UnityTest, Order(3), Description("Check there is at least one logged in User with PSN account")]
        public IEnumerator LocalUserTest()
        {
            yield return new WaitUntil(IsInitialized);

            if ( GetPrimaryUserId().Id != Sony.NP.Core.UserServiceUserId.UserIdInvalid )
            {
                Debug.Log("User is logged and signed in - test passed.");
            }
            else
            {
                Debug.LogAssertion("The logged in user is not signed into PSN, so many tests will fail.");
            }
        }

    }
    
}
#endif

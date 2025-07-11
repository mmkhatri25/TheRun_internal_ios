#if UNITY_GAMECORE 
// Copyright (c) 2020 Augie R. Maddox, Guavaman Enterprises. All rights reserved.
#pragma warning disable 0219
#pragma warning disable 0618
#pragma warning disable 0649
#pragma warning disable 0067


namespace Rewired.Utils.Platforms.GameCore {

    public static class InitializePlatform {
    
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Initialize() {
            Utils.ExternalTools.getPlatformInitializerDelegate = InitializeGameCore;
           
        }

        private static object InitializeGameCore() {
            return Main.GetPlatformInitializer();

        }
    }
}
#endif
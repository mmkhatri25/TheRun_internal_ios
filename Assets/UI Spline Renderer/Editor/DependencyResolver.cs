// using UnityEditor;
// using UnityEditor.PackageManager;
// using UnityEditor.PackageManager.Requests;
// using UnityEngine;
//
// namespace UISplineRendererEditor
// {
//     internal static class DependencyResolver
//     {
//         static AddRequest splineRequest;
//         static AddRequest collectionsRequest;
//         static AddRequest mathematicsRequest;
//         static AddRequest burstRequest;
//         [InitializeOnLoadMethod]
//         static void Install()
//         {
//             bool requested = false;
// #if !ENABLE_SPLINES
//             splineRequest = Client.Add("com.unity.splines");
//             requested = true;
// #endif
// #if !ENABLE_COLLECTIONS
//             collectionsRequest = Client.Add("com.unity.collections");
//             requested = true;
// #endif
// #if !ENABLE_MATHEMATICS
//             mathematicsRequest = Client.Add("com.unity.mathematics");
//             requested = true;
// #endif
// #if !ENABLE_BURST
//             burstRequest = Client.Add("com.unity.burst");
//             requested = true;
// #endif
//             if(requested) EditorApplication.update += Progress;
//         }
//         
//         static void Progress()
//         {
//             if(splineRequest != null) PrintResult(splineRequest);
//             if(collectionsRequest != null) PrintResult(collectionsRequest);
//             if(mathematicsRequest != null) PrintResult(mathematicsRequest);
//             if(burstRequest != null) PrintResult(burstRequest);
//             
//             EditorApplication.update -= Progress;
//         }
//
//         static void PrintResult(AddRequest request)
//         {
//             if (request.IsCompleted)
//             {
//                 if (request.Status == StatusCode.Success)
//                     Debug.Log("Installed: " + request.Result.packageId);
//                 else if (request.Status >= StatusCode.Failure)
//                     Debug.Log(request.Error.message);
//             }
//         }
//     }
// }

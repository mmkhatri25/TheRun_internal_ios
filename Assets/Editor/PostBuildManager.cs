using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class PostBuildManager
{
    //FileUtil.MoveFileOrDirectory("sourcepath/YourFileOrFolder", "destpath/YourFileOrFolder");

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        //string sourceFolderVideos = GetStreamingAssetsPathInBuild(target, pathToBuiltProject) + "Videos";
        //string sourceFolderSubtitles = GetStreamingAssetsPathInBuild(target, pathToBuiltProject) + "Subtitles";

        //string targetDirectoryVideos = BuildPath(pathToBuiltProject) + "Videos";
        //string targetDirectorySubtiles = BuildPath(pathToBuiltProject) + "Subtiles";

        //Debug.Log(sourceFolderVideos);
        //Debug.Log(targetDirectoryVideos);

        //if (Directory.Exists(targetDirectoryVideos))
        //{
        //    Directory.Delete(targetDirectoryVideos, true);
        //}

        //if (Directory.Exists(targetDirectorySubtiles))
        //{
        //    Directory.Delete(targetDirectorySubtiles, true);
        //}

        //FileUtil.MoveFileOrDirectory(sourceFolderVideos, targetDirectoryVideos);
        //FileUtil.MoveFileOrDirectory(sourceFolderSubtitles, targetDirectorySubtiles);

    }

    static string GetStreamingAssetsPathInBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target.ToString().Contains("OSX"))
        {
            return pathToBuiltProject + "/Contents/Resources/Data/StreamingAssets/";
        }
        if (target.ToString().Contains("Windows"))
        {
            string name = pathToBuiltProject.Substring(pathToBuiltProject.LastIndexOf('/') + 1).Replace(".exe", "");
            // return pathToBuiltProject + "/" + name + "_Data/StreamingAssets/";

            //var targetPath = new DirectoryInfo(pathToBuiltProject + "/../");

            //return Path.GetFullPath(targetPath.FullName) + name + "_Data/StreamingAssets/";
            return BuildPath(pathToBuiltProject) + name + "_Data/StreamingAssets/";

        }

        throw new UnityException("Platform not implemented");
    }

    static string BuildPath(string pathToBuiltProject)
    {
        var targetPath = new DirectoryInfo(pathToBuiltProject + "/../");

        return Path.GetFullPath(targetPath.FullName);
    }
}

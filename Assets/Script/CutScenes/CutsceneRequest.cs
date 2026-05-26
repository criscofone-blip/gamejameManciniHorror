using UnityEngine;
using UnityEngine.Video;

public static class CutsceneRequest
{
    public static VideoClip VideoClip { get; private set; }
    public static string NextSceneName { get; private set; }
    public static bool IncreaseDifficultyAfterCutscene { get; private set; }

    public static void Set(VideoClip videoClip, string nextSceneName, bool increaseDifficultyAfterCutscene)
    {
        VideoClip = videoClip;
        NextSceneName = nextSceneName;
        IncreaseDifficultyAfterCutscene = increaseDifficultyAfterCutscene;
    }

    public static void Clear()
    {
        VideoClip = null;
        NextSceneName = string.Empty;
        IncreaseDifficultyAfterCutscene = false;
    }
}
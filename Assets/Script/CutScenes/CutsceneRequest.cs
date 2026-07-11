using System.Collections.Generic;
using UnityEngine.Video;

public static class CutsceneRequest
{
    public struct Step
    {
        public VideoClip clip;
        public bool skippable;

        public Step(VideoClip clip, bool skippable)
        {
            this.clip = clip;
            this.skippable = skippable;
        }
    }

    public static List<Step> Steps { get; private set; } = new List<Step>();
    public static string NextSceneName { get; private set; }
    public static bool IncreaseDifficultyAfterCutscene { get; private set; }

    // Singola cutscene (skippabile di default).
    public static void Set(VideoClip clip, string nextSceneName, bool increaseDifficultyAfterCutscene, bool skippable = true)
    {
        Steps = new List<Step> { new Step(clip, skippable) };
        NextSceneName = nextSceneName;
        IncreaseDifficultyAfterCutscene = increaseDifficultyAfterCutscene;
    }

    // Sequenza di cutscene riprodotte una dopo l'altra.
    public static void SetSequence(List<Step> steps, string nextSceneName, bool increaseDifficultyAfterCutscene)
    {
        Steps = steps ?? new List<Step>();
        NextSceneName = nextSceneName;
        IncreaseDifficultyAfterCutscene = increaseDifficultyAfterCutscene;
    }

    public static void Clear()
    {
        Steps = new List<Step>();
        NextSceneName = string.Empty;
        IncreaseDifficultyAfterCutscene = false;
    }
}

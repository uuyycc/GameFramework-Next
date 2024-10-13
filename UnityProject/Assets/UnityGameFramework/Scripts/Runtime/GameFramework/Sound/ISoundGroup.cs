namespace GameFramework.Sound
{
    /// <summary>
    /// 声音组接口。
    /// </summary>
    public interface ISoundGroup
    {
        /// <summary>
        /// 获取声音组名称。
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 获取声音代理数。
        /// </summary>
        int SoundAgentCount
        {
            get;
        }

        /// <summary>
        /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
        /// </summary>
        bool AvoidBeingReplacedBySamePriority
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置声音组静音。
        /// </summary>
        bool Mute
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置声音组音量。
        /// </summary>
        float Volume
        {
            get;
            set;
        }

        /// <summary>
        /// 获取声音组辅助器。
        /// </summary>
        ISoundGroupHelper Helper
        {
            get;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        void StopAllLoadedSounds();

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void StopAllLoadedSounds(float fadeOutSeconds);
        
        /// <summary>
        /// 暂停所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void PauseAllLoadedSounds(float fadeOutSeconds);
        
        /// <summary>
        /// 恢复所有已暂停的声音。
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        /// </summary>
        void ResumeAllPausedSounds(float fadeInSeconds);
    }
}

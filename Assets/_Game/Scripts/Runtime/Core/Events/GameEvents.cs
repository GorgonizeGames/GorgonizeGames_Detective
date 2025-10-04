namespace Game.Runtime.Core.Events
{
    using Services;
    
    // ==================== CORE GAME EVENTS ====================
    public struct GameStartedEvent : IGameEvent { }
    public struct GamePausedEvent : IGameEvent { }
    public struct GameResumedEvent : IGameEvent { }
    public struct GameQuitEvent : IGameEvent { }
    
    // ==================== SCENE EVENTS ====================
    public struct SceneLoadStartedEvent : IGameEvent
    {
        public string SceneName;
    }
    
    public struct SceneLoadedEvent : IGameEvent
    {
        public string SceneName;
    }
    
    // ==================== UI EVENTS ====================
    public struct WindowOpenedEvent : IGameEvent
    {
        public string WindowId;
        public int ZOrder;
    }
    
    public struct WindowClosedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowFocusedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowMinimizedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    public struct WindowMaximizedEvent : IGameEvent
    {
        public string WindowId;
    }
    
    // ==================== INVESTIGATION EVENTS ====================
    public struct EvidenceFoundEvent : IGameEvent
    {
        public string EvidenceId;
        public string CaseId;
        public string Description;
    }
    
    public struct ClueDiscoveredEvent : IGameEvent
    {
        public string ClueId;
        public string Description;
    }
    
    public struct CaseProgressEvent : IGameEvent
    {
        public string CaseId;
        public float ProgressPercentage;
    }
    
    public struct CaseCompletedEvent : IGameEvent
    {
        public string CaseId;
        public bool Success;
    }
    
    // ==================== HACKING EVENTS ====================
    public struct HackingStartedEvent : IGameEvent
    {
        public string TargetId;
        public string HackType;
    }
    
    public struct HackingCompletedEvent : IGameEvent
    {
        public string TargetId;
        public bool Success;
    }
    
    public struct HackingFailedEvent : IGameEvent
    {
        public string TargetId;
        public string Reason;
    }
    
    // ==================== AUDIO EVENTS ====================
    public struct MusicStartedEvent : IGameEvent
    {
        public string MusicId;
    }
    
    public struct SoundPlayedEvent : IGameEvent
    {
        public string SoundId;
    }
    
    // ==================== SAVE EVENTS ====================
    public struct GameSavedEvent : IGameEvent
    {
        public string SaveSlot;
    }
    
    public struct GameLoadedEvent : IGameEvent
    {
        public string SaveSlot;
    }
}
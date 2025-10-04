using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Investigation
{
    public class ClueService : MonoBehaviour, IClueService
    {
        [Inject] private IEventService _eventService;
        [Inject] private IAudioService _audioService;
        
        private readonly Dictionary<string, Clue> _clueDatabase = new Dictionary<string, Clue>();
        
        public event Action<Clue> OnClueDiscovered;
        
        private void Start()
        {
            Dependencies.Inject(this);
        }
        
        public void AddClue(Clue clue)
        {
            if (clue == null || string.IsNullOrEmpty(clue.Id))
            {
                Debug.LogError("[ClueService] Invalid clue");
                return;
            }
            
            if (_clueDatabase.ContainsKey(clue.Id))
            {
                Debug.LogWarning($"[ClueService] Clue '{clue.Id}' already exists");
                return;
            }
            
            clue.DiscoveredAt = DateTime.Now;
            clue.IsRead = false;
            _clueDatabase[clue.Id] = clue;
            
            OnClueDiscovered?.Invoke(clue);
            _eventService?.Publish(new ClueDiscoveredEvent 
            { 
                ClueId = clue.Id, 
                Description = clue.Description 
            });
            
            _audioService?.PlayUISound(UISoundType.Notification);
            
            Debug.Log($"[ClueService] New clue discovered: {clue.Title}");
        }
        
        public void RemoveClue(string clueId)
        {
            _clueDatabase.Remove(clueId);
        }
        
        public Clue GetClue(string clueId)
        {
            _clueDatabase.TryGetValue(clueId, out Clue clue);
            return clue;
        }
        
        public List<Clue> GetAllClues()
        {
            return _clueDatabase.Values.ToList();
        }
        
        public List<Clue> GetCluesByCase(string caseId)
        {
            return _clueDatabase.Values.Where(c => c.CaseId == caseId).ToList();
        }
        
        public void MarkClueAsRead(string clueId)
        {
            if (_clueDatabase.TryGetValue(clueId, out Clue clue))
            {
                clue.IsRead = true;
            }
        }
        
        public bool HasUnreadClues()
        {
            return _clueDatabase.Values.Any(c => !c.IsRead);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.Investigation.Services
{
    public class EvidenceService : MonoBehaviour, IEvidenceService
    {
        [Inject] private IEventService _eventService;
        [Inject] private ISaveService _saveService;
        
        private readonly Dictionary<string, Evidence> _evidenceDatabase = new Dictionary<string, Evidence>();
        
        public event Action<Evidence> OnEvidenceAdded;
        public event Action<string> OnEvidenceRemoved;
        
        private void Start()
        {
            Dependencies.Inject(this);
        }
        
        public void AddEvidence(Evidence evidence)
        {
            if (evidence == null || string.IsNullOrEmpty(evidence.Id))
            {
                Debug.LogError("[EvidenceService] Invalid evidence");
                return;
            }
            
            if (_evidenceDatabase.ContainsKey(evidence.Id))
            {
                Debug.LogWarning($"[EvidenceService] Evidence '{evidence.Id}' already exists");
                return;
            }
            
            evidence.DiscoveredAt = DateTime.Now;
            _evidenceDatabase[evidence.Id] = evidence;
            
            OnEvidenceAdded?.Invoke(evidence);
            _eventService?.Publish(new EvidenceFoundEvent 
            { 
                EvidenceId = evidence.Id, 
                CaseId = evidence.CaseId,
                Description = evidence.Description
            });
            
            Debug.Log($"[EvidenceService] Added evidence: {evidence.Name}");
        }
        
        public void RemoveEvidence(string evidenceId)
        {
            if (_evidenceDatabase.Remove(evidenceId))
            {
                OnEvidenceRemoved?.Invoke(evidenceId);
            }
        }
        
        public Evidence GetEvidence(string evidenceId)
        {
            _evidenceDatabase.TryGetValue(evidenceId, out Evidence evidence);
            return evidence;
        }
        
        public List<Evidence> GetAllEvidence()
        {
            return _evidenceDatabase.Values.ToList();
        }
        
        public List<Evidence> GetEvidenceByCase(string caseId)
        {
            return _evidenceDatabase.Values.Where(e => e.CaseId == caseId).ToList();
        }
        
        public bool HasEvidence(string evidenceId)
        {
            return _evidenceDatabase.ContainsKey(evidenceId);
        }
        
        public int GetEvidenceCount()
        {
            return _evidenceDatabase.Count;
        }
    }
}
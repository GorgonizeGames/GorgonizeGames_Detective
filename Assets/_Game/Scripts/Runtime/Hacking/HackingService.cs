using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;
using Game.Runtime.Investigation;

namespace Game.Runtime.Hacking
{
    public class HackingService : MonoBehaviour, IHackingService
    {
        [Inject] private IEventService _eventService;
        [Inject] private IAudioService _audioService;
        [Inject] private IEvidenceService _evidenceService;
        
        private HackingChallenge _currentChallenge;
        private readonly HashSet<string> _completedChallenges = new HashSet<string>();
        
        public event Action<HackingChallenge> OnChallengeStarted;
        public event Action<string, bool> OnChallengeCompleted;
        
        private void Start()
        {
            Dependencies.Inject(this);
        }
        
        public void StartHackingChallenge(HackingChallenge challenge)
        {
            if (challenge == null)
            {
                Debug.LogError("[HackingService] Invalid challenge");
                return;
            }
            
            if (_currentChallenge != null)
            {
                Debug.LogWarning("[HackingService] Already in a challenge");
                return;
            }
            
            _currentChallenge = challenge;
            
            OnChallengeStarted?.Invoke(challenge);
            _eventService?.Publish(new HackingStartedEvent 
            { 
                TargetId = challenge.TargetId, 
                HackType = challenge.Type.ToString() 
            });
            
            Debug.Log($"[HackingService] Started challenge: {challenge.Name}");
        }
        
        public void CompleteChallenge(string challengeId, bool success)
        {
            if (_currentChallenge == null || _currentChallenge.Id != challengeId)
            {
                Debug.LogWarning("[HackingService] No active challenge");
                return;
            }
            
            if (success)
            {
                _completedChallenges.Add(challengeId);
                
                _audioService?.PlayUISound(UISoundType.Success);
                _eventService?.Publish(new HackingCompletedEvent 
                { 
                    TargetId = _currentChallenge.TargetId, 
                    Success = true 
                });
            }
            else
            {
                _audioService?.PlayUISound(UISoundType.Error);
                _eventService?.Publish(new HackingCompletedEvent 
                { 
                    TargetId = _currentChallenge.TargetId, 
                    Success = false 
                });
            }
            
            OnChallengeCompleted?.Invoke(challengeId, success);
            
            _currentChallenge = null;
            
            Debug.Log($"[HackingService] Challenge {challengeId} completed: {success}");
        }
        
        public void FailChallenge(string challengeId, string reason)
        {
            if (_currentChallenge == null || _currentChallenge.Id != challengeId) return;
            
            _audioService?.PlayUISound(UISoundType.Error);
            _eventService?.Publish(new HackingFailedEvent 
            { 
                TargetId = _currentChallenge.TargetId, 
                Reason = reason 
            });
            
            _currentChallenge = null;
            
            Debug.Log($"[HackingService] Challenge {challengeId} failed: {reason}");
        }
        
        public bool IsChallengeCompleted(string challengeId)
        {
            return _completedChallenges.Contains(challengeId);
        }
        
        public List<HackingChallenge> GetCompletedChallenges()
        {
            return new List<HackingChallenge>();
        }
    }
}
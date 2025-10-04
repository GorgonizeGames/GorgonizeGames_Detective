using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Runtime.Core.DI;
using Game.Runtime.Core.Services;
using Game.Runtime.Core.Events;

namespace Game.Runtime.UI.Windows
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class WindowBase : MonoBehaviour, IWindow, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Window Settings")]
        [SerializeField] protected string windowId;
        [SerializeField] protected string windowTitle = "Window";
        [SerializeField] protected Vector2 minSize = new Vector2(400, 300);
        [SerializeField] protected Vector2 maxSize = new Vector2(1920, 1080);
        [SerializeField] protected bool canResize = true;
        [SerializeField] protected bool canMinimize = true;
        [SerializeField] protected bool canMaximize = true;
        [SerializeField] protected bool canClose = true;
        
        [Header("References")]
        [SerializeField] protected RectTransform titleBar;
        [SerializeField] protected Button closeButton;
        [SerializeField] protected Button minimizeButton;
        [SerializeField] protected Button maximizeButton;
        [SerializeField] protected GameObject contentPanel;
        
        [Inject] protected IWindowManager _windowManager;
        [Inject] protected IEventService _eventService;
        [Inject] protected IAudioService _audioService;
        
        protected RectTransform _rectTransform;
        protected CanvasGroup _canvasGroup;
        protected Canvas _canvas;
        
        protected bool _isOpen;
        protected bool _isMinimized;
        protected bool _isMaximized;
        protected bool _isFocused;
        protected int _zOrder;
        
        protected Vector2 _normalizedPosition;
        protected Vector2 _normalizedSize;
        protected Vector2 _dragOffset;
        protected bool _isDragging;
        
        public string WindowId => windowId;
        public string WindowTitle => windowTitle;
        public bool IsOpen => _isOpen;
        public bool IsMinimized => _isMinimized;
        public bool IsMaximized => _isMaximized;
        public bool IsFocused => _isFocused;
        public int ZOrder 
        { 
            get => _zOrder;
            set 
            {
                _zOrder = value;
                if (_canvas != null) _canvas.sortingOrder = _zOrder;
            }
        }
        public RectTransform RectTransform => _rectTransform;
        
        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            
            _canvas = gameObject.AddComponent<Canvas>();
            _canvas.overrideSorting = true;
            
            gameObject.AddComponent<GraphicRaycaster>();
            
            if (string.IsNullOrEmpty(windowId))
            {
                windowId = GetType().Name;
            }
            
            SetupButtons();
            gameObject.SetActive(false);
        }
        
        protected virtual void Start()
        {
            Dependencies.Inject(this);
        }
        
        protected virtual void SetupButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
                closeButton.gameObject.SetActive(canClose);
            }
            
            if (minimizeButton != null)
            {
                minimizeButton.onClick.AddListener(Minimize);
                minimizeButton.gameObject.SetActive(canMinimize);
            }
            
            if (maximizeButton != null)
            {
                maximizeButton.onClick.AddListener(ToggleMaximize);
                maximizeButton.gameObject.SetActive(canMaximize);
            }
        }
        
        public virtual void Open()
        {
            if (_isOpen) return;
            
            gameObject.SetActive(true);
            _isOpen = true;
            _isFocused = true;
            
            _audioService?.PlayUISound(UISoundType.Open);
            _eventService?.Publish(new WindowOpenedEvent { WindowId = windowId, ZOrder = _zOrder });
            
            OnOpen();
        }
        
        public virtual void Close()
        {
            if (!_isOpen || !canClose) return;
            
            _isOpen = false;
            _isFocused = false;
            
            _audioService?.PlayUISound(UISoundType.Close);
            _eventService?.Publish(new WindowClosedEvent { WindowId = windowId });
            
            OnClose();
            
            gameObject.SetActive(false);
        }
        
        public virtual void Minimize()
        {
            if (!_isOpen || _isMinimized || !canMinimize) return;
            
            _isMinimized = true;
            contentPanel?.SetActive(false);
            
            _audioService?.PlayUISound(UISoundType.Click);
            _eventService?.Publish(new WindowMinimizedEvent { WindowId = windowId });
            
            OnMinimize();
        }
        
        public virtual void Maximize()
        {
            if (!_isOpen || _isMaximized || !canMaximize) return;
            
            _normalizedPosition = _rectTransform.anchoredPosition;
            _normalizedSize = _rectTransform.sizeDelta;
            
            _rectTransform.anchoredPosition = Vector2.zero;
            _rectTransform.sizeDelta = ((RectTransform)_rectTransform.parent).rect.size;
            
            _isMaximized = true;
            
            _audioService?.PlayUISound(UISoundType.Click);
            _eventService?.Publish(new WindowMaximizedEvent { WindowId = windowId });
            
            OnMaximize();
        }
        
        public virtual void Restore()
        {
            if (_isMinimized)
            {
                _isMinimized = false;
                contentPanel?.SetActive(true);
                OnRestore();
            }
            else if (_isMaximized)
            {
                _isMaximized = false;
                _rectTransform.anchoredPosition = _normalizedPosition;
                _rectTransform.sizeDelta = _normalizedSize;
                OnRestore();
            }
            
            _audioService?.PlayUISound(UISoundType.Click);
        }
        
        protected virtual void ToggleMaximize()
        {
            if (_isMaximized) Restore();
            else Maximize();
        }
        
        public virtual void Focus()
        {
            if (!_isOpen || _isFocused) return;
            
            _isFocused = true;
            _eventService?.Publish(new WindowFocusedEvent { WindowId = windowId });
            
            OnFocus();
        }
        
        public virtual void Blur()
        {
            if (!_isFocused) return;
            
            _isFocused = false;
            OnBlur();
        }
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            _windowManager?.BringToFront(this);
        }
        
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (_isMaximized || titleBar == null) return;
            
            if (RectTransformUtility.RectangleContainsScreenPoint(titleBar, eventData.position, eventData.pressEventCamera))
            {
                _isDragging = true;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint);
                
                _dragOffset = _rectTransform.anchoredPosition - localPoint;
            }
        }
        
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _isMaximized) return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint);
            
            _rectTransform.anchoredPosition = localPoint + _dragOffset;
        }
        
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
        }
        
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
        protected virtual void OnMinimize() { }
        protected virtual void OnMaximize() { }
        protected virtual void OnRestore() { }
        protected virtual void OnFocus() { }
        protected virtual void OnBlur() { }
        
        protected virtual void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (minimizeButton != null) minimizeButton.onClick.RemoveListener(Minimize);
            if (maximizeButton != null) maximizeButton.onClick.RemoveListener(ToggleMaximize);
        }
    }
}
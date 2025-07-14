using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [System.Serializable]
    public class MusicMode
    {
        public string modeName;
        public AudioClip[] tracks;
        public float delayBetweenTracks = 5f;
        [Range(0.1f, 5f)] public float crossfadeDuration = 1f; // Новый параметр
    }

    [Header("Music Settings")]
    public MusicMode[] musicModes;
    public int currentModeIndex = 0;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("Debug")]
    [SerializeField] private AudioSource _musicSource1; // Первый источник
    [SerializeField] private AudioSource _musicSource2; // Второй источник
    private AudioSource _activeSource; // Текущий активный источник
    private AudioSource _fadeSource;   // Источник для кроссфейда

    private Coroutine _musicCoroutine;
    private Coroutine _crossfadeCoroutine;
    private MusicMode _currentMode;
    private bool _isPaused; // Флаг паузы
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSource();
            InitializeMusicSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeMusicSystem() // Добавляем отсутствующий метод
    {
        if (musicModes.Length > 0 && currentModeIndex < musicModes.Length)
        {
            SetMode(currentModeIndex);
        }
        else
        {
            Debug.LogError("Music modes not configured properly!");
        }
    }

    void InitializeAudioSource()
        {
            // Создаем два источника
            _musicSource1 = CreateAudioSource("MusicSource1");
            _musicSource2 = CreateAudioSource("MusicSource2");
            _activeSource = _musicSource1;
        }
    
        AudioSource CreateAudioSource(string name)
        {
            GameObject child = new GameObject(name);
            child.transform.parent = transform;
            AudioSource source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            return source;
        }

        public void SetMode(int modeIndex)
        {
            if (modeIndex < 0 || modeIndex >= musicModes.Length)
            {
                Debug.LogError($"Invalid mode index: {modeIndex}");
                return;
            }

            _currentMode = musicModes[modeIndex];
            currentModeIndex = modeIndex;

            // Заменяем _musicSource на _activeSource
            if (_musicCoroutine != null)
            {
                StopCoroutine(_musicCoroutine);
                _activeSource.Stop(); // Исправлено здесь
            }

            if (_currentMode.tracks.Length == 0)
            {
                Debug.LogError($"No tracks in mode: {_currentMode.modeName}");
                return;
            }

            Debug.Log($"Switching to mode: {_currentMode.modeName}");
            // Добавляем параметр в вызов корутины
            _musicCoroutine = StartCoroutine(PlayMusicRoutine(_activeSource)); // Исправлено здесь
        
            if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
            _crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(musicModes[modeIndex]));
        
    }

     private IEnumerator CrossfadeRoutine(MusicMode newMode)
        {
            // Определяем источники для кроссфейда
            _fadeSource = (_activeSource == _musicSource1) ? _musicSource2 : _musicSource1;
            _currentMode = newMode;
    
            // Настраиваем новый источник
            _fadeSource.volume = 0f;
            StartCoroutine(PlayMusicRoutine(_fadeSource));
    
            float timer = 0f;
            float fadeDuration = _currentMode.crossfadeDuration;
    
            while (timer < fadeDuration)
            {
                float progress = timer / fadeDuration;
                _activeSource.volume = musicVolume * (1 - progress);
                _fadeSource.volume = musicVolume * progress;
                timer += Time.deltaTime;
                yield return null;
            }
    
            // Переключаем активный источник
            _activeSource.Stop();
            _activeSource = _fadeSource;
            _activeSource.volume = musicVolume;
        }
    
        // Новая корутина с параметром
        private IEnumerator PlayMusicRoutine(AudioSource source)
        {
            int trackIndex = 0;
            while (true)
            {
                if (_isPaused) // Проверка паузы
                {
                    yield return new WaitWhile(() => _isPaused);
                    source.Play();
                }
    
                source.clip = _currentMode.tracks[trackIndex];
                source.Play();
    
                yield return new WaitForSeconds(source.clip.length + _currentMode.delayBetweenTracks);
                trackIndex = (trackIndex + 1) % _currentMode.tracks.Length;
            }
        }
    
        // Новые методы управления паузой
        public void PauseMusic()
        {
            _isPaused = true;
            _activeSource.Pause();
            if (_musicCoroutine != null) StopCoroutine(_musicCoroutine);
        }
    
        public void ResumeMusic()
        {
            _isPaused = false;
            _activeSource.UnPause();
            _musicCoroutine = StartCoroutine(PlayMusicRoutine(_activeSource));
        }
    
        // Обновленный метод смены громкости
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            _activeSource.volume = musicVolume;
            _fadeSource.volume = musicVolume;
        }
    
        // Новый метод для параллельного воспроизведения
        public void PlayParallelClip(AudioClip clip, float volumeScale = 1f)
        {
            GameObject tempObject = new GameObject("TempAudioSource");
            AudioSource tempSource = tempObject.AddComponent<AudioSource>();
            tempSource.spatialBlend = 0f;
            tempSource.volume = musicVolume * volumeScale;
            tempSource.clip = clip;
            tempSource.Play();
            Destroy(tempObject, clip.length + 0.1f);
        }
    }
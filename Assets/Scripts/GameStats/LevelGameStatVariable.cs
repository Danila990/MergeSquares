using System;

namespace GameStats
{
    public class LevelGameStatVariableParams
    {
        public int currentLevel;
        public int maxLevel = -1;
        public int statMax;
        public EGameStatType type;
        public GameStatService gameStatService;
    }
    
    public class LevelGameStatVariable
    {
        public int Value => _gameStatService.Get(_type);
        public int MaxValue => _statMax;
        public int Level => _currentLevel;
        public EGameStatType Type => _type;
        
        public bool ReadyLevelUp => _stat.GetValue() >= _statMax;
        
        public event Action<EGameStatType, int> LevelChanged = (type, level) => { };
        public event Action<EGameStatType, int> StatChanged = (type, statValue) => { };
        
        private int _currentLevel;
        private int _maxLevel = -1;
        private int _statMax;
        private EGameStatType _type;
        private GameStatService _gameStatService;
        private GameStatVariable _stat;

        public void Init(LevelGameStatVariableParams initParams)
        {
            _currentLevel = initParams.currentLevel;
            _maxLevel = initParams.maxLevel;
            _statMax = initParams.statMax;
            _type = initParams.type;
            _gameStatService = initParams.gameStatService;
            
            _stat = _gameStatService.GetStat(_type);
            _gameStatService.StatChanged += OnStatChanged;
        }

        public void UpdateMaxLevel(int value)
        {
            _maxLevel = value;
            ReportState();
        }

        public void UpdateStatMax(int value)
        {
            _statMax = value;
            ReportState();
        }

        public void ReportState()
        {
            OnStatChanged(Type, Value);
        }
        
        private void OnStatChanged(EGameStatType stat, int value)
        {
            if (stat != _type) return;
            
            if (value < _statMax)
            {
                StatChanged.Invoke(_type, value);
            }
            else
            {
                var remainder = _stat.GetValue() - _statMax;

                if (_currentLevel < _maxLevel || _maxLevel == -1)
                {
                    _currentLevel++;
                    LevelChanged.Invoke(_type, _currentLevel);
                    _gameStatService.TryResetLocalDelta(_type);
                    _gameStatService.TrySet(_type, 0);
                    _gameStatService.TryIncWithAnim(_type, remainder);
                }
            }
        }
    }
}
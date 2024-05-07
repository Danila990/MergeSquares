using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Core.DeviceSimulatorPlugins
{
    public class TouchInfoPlugin : DeviceSimulatorPlugin
    {
        public override string title => "Touch Info";
        private Label m_TouchCountLabel;
        private Label m_LastTouchEvent;
        private Button m_ResetCountButton;

        [SerializeField]
        private int m_TouchCount = 0;

        public override void OnCreate()
        {
            deviceSimulator.touchScreenInput += touchEvent =>
            {
                m_TouchCount += 1;
                UpdateTouchCounterText();
                m_LastTouchEvent.text = $"Last touch event: {touchEvent.phase.ToString()}";
            };
        }

        public override VisualElement OnCreateUI()
        {
            VisualElement root = new VisualElement();
        
            m_LastTouchEvent = new Label("Last touch event: None");
        
            m_TouchCountLabel = new Label();
            UpdateTouchCounterText();

            m_ResetCountButton = new Button {text = "Reset Count" };
            m_ResetCountButton.clicked += () =>
            {
                m_TouchCount = 0;
                UpdateTouchCounterText();
            };

            root.Add(m_LastTouchEvent);
            root.Add(m_TouchCountLabel);
            root.Add(m_ResetCountButton);
            
            return root;
        }

        private void UpdateTouchCounterText()
        {
            if (m_TouchCount > 0)
                m_TouchCountLabel.text = $"Touches recorded: {m_TouchCount}";
            else
                m_TouchCountLabel.text = "No taps recorded";
        }
    }
}
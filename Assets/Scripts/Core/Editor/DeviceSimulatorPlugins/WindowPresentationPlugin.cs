using System;
using Core.Windows;
using Plugins.WindowsManager;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Core.DeviceSimulatorPlugins
{
    public class WindowPresentationPlugin : DeviceSimulatorPlugin
    {
        public override string title => "Window Presentation";
        private Label m_WindowNameLabel;
        private Label m_WindowIndexLabel;
        private Button m_NextWindowButton;
        private Button m_CloseAllWindowsButton;
        private Button m_LoadWindowManagerButton;

        [SerializeField] private int m_CurrentWindow = 0;
        [SerializeField] private WindowManager windowManager = null;

        public override VisualElement OnCreateUI()
        {
            VisualElement root = new VisualElement();

            m_WindowNameLabel = new Label();
            m_WindowIndexLabel = new Label();
            UpdateLabels();

            m_NextWindowButton = new Button {text = "Next window"};
            m_NextWindowButton.clicked += () =>
            {
                if (windowManager != null)
                {
                    windowManager.CloseAll();
                    try
                    {
                        var window = windowManager.ShowWindowByIndex(m_CurrentWindow);
                        window.Canvas.sortingLayerName = "Default";
                        m_CurrentWindow++;
                        m_CurrentWindow %= windowManager.GetWindowsCount();
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning(
                            $"[WindowPresentationPlugin][OnNextWindow] can't open window {windowManager.GetWindowIdByIndex(m_CurrentWindow)} with exception {e.Message}");
                    }
                }

                UpdateLabels();
            };

            m_CloseAllWindowsButton = new Button {text = "Close all windows"};
            m_CloseAllWindowsButton.clicked += CloseAll;

            m_LoadWindowManagerButton = new Button {text = "Load window manager"};
            m_LoadWindowManagerButton.clicked += () =>
            {
                windowManager = Object.FindObjectOfType<WindowManager>();
                CloseAll();
            };

            root.Add(m_WindowNameLabel);
            root.Add(m_WindowIndexLabel);
            root.Add(m_NextWindowButton);
            root.Add(m_CloseAllWindowsButton);
            root.Add(m_LoadWindowManagerButton);

            return root;
        }

        private void UpdateLabels()
        {
            m_WindowIndexLabel.text = $"Current window index: {m_CurrentWindow}";
            if (windowManager != null)
            {
                var window = windowManager.GetLastOpenedWindow();
                m_WindowNameLabel.text =
                    window != null ? $"Window {windowManager.GetWindowIdByIndex(m_CurrentWindow)} opened" : "There is no open windows";
            }
            else
            {
                m_WindowNameLabel.text = "Can't find window manager";
            }
        }

        private void CloseAll()
        {
            if (windowManager != null)
            {
                windowManager.CloseAll();
                m_CurrentWindow = 0;
                UpdateLabels();
            }
        }
    }
}
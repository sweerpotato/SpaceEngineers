using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;

namespace Scripting.SolarPanel
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        private float _lastOutput = 0.0f;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            IMyMotorStator rotor = GridTerminalSystem.GetBlockWithName("Solar Panel Rotor") as IMyMotorStator;
            IMySolarPanel solarPanel = GridTerminalSystem.GetBlockWithName("Solar Panel Reference") as IMySolarPanel;

            if (solarPanel.MaxOutput < _lastOutput)
            {
                rotor.TargetVelocityRPM *= -1.0f;
            }

            _PanelTextSurface.WriteText(String.Format(
                "Last: {0}\nOutput: {1}\nRotor: {2}",
                _lastOutput,
                solarPanel.MaxOutput,
                rotor.TargetVelocityRPM
                ));

            _lastOutput = solarPanel.MaxOutput;
        }

        #endregion
    }
}

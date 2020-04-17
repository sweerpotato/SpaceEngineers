using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceEngineers.SolarPanelRegulator
{
    public class Program : MyGridProgram
    {
        #region Properties

        /// <summary>
        /// Reference to the debug screen
        /// </summary>
        private IMyTextSurface _DebugLCD = null;

        /// <summary>
        /// The rotor rotating araound the Y axis
        /// </summary>
        private IMyMotorStator _RotorY = null;

        /// <summary>
        /// The rotor rotating around the X axes
        /// </summary>
        private IMyMotorStator _RotorX = null;

        /// <summary>
        /// All solar panels connected to the rotors
        /// </summary>
        private List<IMySolarPanel> _SolarPanels = null;

        /// <summary>
        /// Last average value of the solar panels
        /// </summary>
        private float _LastAverage = 0f;

        private float _LastXAngle = 0f;

        /// <summary>
        /// Maximum degrees in radians (+45 degrees)
        /// </summary>
        private const float MAX_DEGREES = (float)(Math.PI / 4);

        /// <summary>
        /// Minimum degrees in radians (-45 degrees)
        /// </summary>
        private const float MIN_DEGREES = -MAX_DEGREES;

        #endregion

        #region Constructor

        public Program()
        {
            _DebugLCD = (IMyTextSurface)GridTerminalSystem.GetBlockWithName("Debug LCD");
            _DebugLCD.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _DebugLCD.FontSize = 2;
            _DebugLCD.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;

            _RotorY = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("RotorY");
            _RotorX = (IMyMotorStator)GridTerminalSystem.GetBlockWithName("RotorX");

            _RotorY.TargetVelocityRPM = 3;
            _RotorX.TargetVelocityRPM = 3;
            _RotorY.Torque = 1500000f;
            _RotorX.Torque = 1500000f;

            List<IMyTerminalBlock> tempList = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlockGroupWithName("Solar Panels").GetBlocksOfType<IMySolarPanel>(tempList);

            _SolarPanels = tempList.Cast<IMySolarPanel>().ToList();

            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }

        #endregion

        #region Main

        public void Main(string argument, UpdateType updateSource)
        {
            ControlSolarPanels();
        }

        #endregion

        #region Methods

        private void ControlSolarPanels()
        {
            float averagePower = 0f;

            foreach (IMySolarPanel solarPanel in _SolarPanels)
            {
                averagePower += solarPanel.MaxOutput;
            }

            //Calculate average and convert to kilowatts
            averagePower = 1000 * (averagePower /= _SolarPanels.Count);

            LogValues(
                "Last average power: ", _LastAverage.ToString(), "kW\n",
                "Current average power:", averagePower.ToString(), "kW\n",
                "Last X angle:", (_LastXAngle * (180 / Math.PI)).ToString(), "\n",
                "Current X angle:", (_RotorX.Angle * (180 / Math.PI)).ToString());

            RotateY(averagePower);
            RotateX(averagePower);

            _LastAverage = averagePower;
        }

        /// <summary>
        /// Rotates the solar panel tower around the Y axis
        /// </summary>
        /// <param name="averagePower">The current average power in kilowatts</param>
        private void RotateY(float averagePower)
        {
            if (_RotorY == null)
            {
                throw new ArgumentException("RotorY is null");
            }

            if (_LastAverage < averagePower)
            {
                if (_RotorY.TargetVelocityRPM < 1)
                {
                    _RotorY.TargetVelocityRPM = 0.1f;
                }
            }
            else
            {
                if (_RotorY.TargetVelocityRPM == 1)
                {
                    _RotorY.TargetVelocityRPM = 0f;
                }

                _RotorY.TargetVelocityRPM *= -1.02f;
            }
        }

        /// <summary>
        /// Rotates the solar panel tower around the X axis
        /// </summary>
        /// <param name="averagePower">The current average power in kilowatts</param>
        private void RotateX(float averagePower)
        {
            if (_RotorX == null)
            {
                throw new ArgumentException("RotorX is null");
            }

            if (_RotorX.Angle > MAX_DEGREES && _RotorX.Angle < MIN_DEGREES)
            {
                _DebugLCD.WriteText("Rotor X Angle is above max degrees", true);
                _DebugLCD.WriteText("\nRotor X Angle is below min degrees", true);

                if (_RotorX.TargetVelocityRPM < 0)
                {
                    _RotorX.TargetVelocityRPM = -2f;
                }
                
                _RotorX.UpperLimitRad = MAX_DEGREES;
                _RotorX.LowerLimitRad = MIN_DEGREES;
            }
            else
            {
                if (_LastAverage < averagePower)
                {
                    _RotorX.TargetVelocityRPM = 3;
                }
                else
                {
                    _RotorX.TargetVelocityRPM *= -1f;
                }
            }

            _LastXAngle = _RotorX.Angle;
        }

        private void LogValues(params string[] messages)
        {
            StringBuilder sb = new StringBuilder();

            foreach (string message in messages)
            {
                sb.Append(message + " ");
            }

            _DebugLCD.WriteText(sb.ToString());
        }

        #endregion
    }
}

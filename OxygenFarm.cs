using System;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace Scripting.OxygenFarm
{
    public class Program : MyGridProgram
    {
        #region Fields

        private IMyTextSurface _PanelTextSurface = null;
        private bool _on = false;

        #endregion

        #region Constructor

        public Program()
        {
            _PanelTextSurface = Me.GetSurface(0);
            _PanelTextSurface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _PanelTextSurface.FontSize = 2;
            _PanelTextSurface.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        #endregion

        #region Methods

        public void Main(string argument, UpdateType updateSource)
        {
            IMyGasTank oxygenTank = GridTerminalSystem.GetBlockWithName("Oxygen Tank") as IMyGasTank;

            IMyBlockGroup oxygenFarmGroup = GridTerminalSystem.GetBlockGroupWithName("Oxygen Farms");
            List<IMyOxygenFarm> oxygenFarms = new List<IMyOxygenFarm>();
            oxygenFarmGroup.GetBlocksOfType(oxygenFarms);

            float generating = 0.0f;

            if (oxygenTank.FilledRatio < 0.9f)
            {
                oxygenFarms.ForEach((IMyOxygenFarm farm) => {
                    if (_on == false)
                    {
                        farm.ApplyAction("OnOff_On");
                    }
                    generating += farm.GetOutput();
                });
                _on = true;
            }
            else
            {
                if (_on == true)
                {
                    oxygenFarms.ForEach((IMyOxygenFarm farm) => { farm.ApplyAction("OnOff_Off"); });
                    _on = false;
                }
            }

            _PanelTextSurface.WriteText(String.Format(
                "Oxygen: {0:0.000}\nFarms {1}\nGenerating: {2}",
                oxygenTank.FilledRatio,
                _on ? "Enabled" : "Disabled",
                generating
                ));
        }

        #endregion
    }
}

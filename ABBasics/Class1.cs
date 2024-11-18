using ABB.Robotics.Math;
using ABB.Robotics.RobotStudio;
using ABB.Robotics.RobotStudio.Environment;
using ABB.Robotics.RobotStudio.Stations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABBasics
{
    public class Class1
    {
        // This is the entry point which will be called when the Add-in is loaded
        public static void AddinMain()
        {
            Logger.AddMessage(new LogMessage("Hello, this is Lane Durst's Robot Studio Addin!"));
            Logger.AddMessage(new LogMessage("Here is a link to some basic ABB tutorials:\nhttps://new.abb.com/products/robotics/robotstudio/tutorials\n"));
        }
    }
}
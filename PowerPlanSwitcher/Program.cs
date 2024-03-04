using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace PowerPlanSwitcher
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        private static string currentPowerPlanGUID = string.Empty;

        static void Main(string[] args)
        {
            //Change to your GUIDs [powercfg /list]
            var ultimatePerformanceGUID = "0124ced5-20d4-4c97-8f56-1ddde9b00a37";
            var powerSaverGUID = "0368cc85-5c1b-4f76-8966-f12f6aeec7d7";
            var balancedGUID = "381b4222-f694-41f0-9685-ff5bb260df2e";

            Console.WriteLine("Power Plan Switcher started...");

            while (true)
            {
                Thread.Sleep(111);

                uint idleTime = GetIdleTime();

                if (idleTime >= 60 && currentPowerPlanGUID != powerSaverGUID) // 1 minute of inactivity
                {
                    Console.WriteLine("Switching to Power Saver");
                    SwitchPowerPlan(powerSaverGUID);
                    currentPowerPlanGUID = powerSaverGUID;
                }
                else if (idleTime >= 30 && idleTime < 60 && currentPowerPlanGUID != balancedGUID) // 30 seconds of inactivity
                {
                    Console.WriteLine("Switching to Balanced");
                    SwitchPowerPlan(balancedGUID);
                    currentPowerPlanGUID = balancedGUID;
                }
                else if (idleTime < 30 && currentPowerPlanGUID != ultimatePerformanceGUID)
                {
                    Console.WriteLine("Switching to Ultimate Performance");
                    SwitchPowerPlan(ultimatePerformanceGUID);
                    currentPowerPlanGUID = ultimatePerformanceGUID;
                }
            }
        }

        static uint GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);

            uint lastInputTick = lastInputInfo.dwTime;

            return ((uint)Environment.TickCount - lastInputTick) / 1000;
        }

        static void SwitchPowerPlan(string powerPlanGUID)
        {
            var startInfo = new ProcessStartInfo("powercfg", $"/s {powerPlanGUID}")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(startInfo);
        }
    }
}

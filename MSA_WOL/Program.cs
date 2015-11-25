using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Globalization;
using System.Net;
using System.Collections;
using System.IO;
using MSA_WOL;
using System.Diagnostics;
using System.Threading;

namespace MSA_WOL
{
    public class Program
    {
        private static void Main(string[] args)
        {
            List<Machine> machines = new List<Machine>();
            string outputFilename = string.Format("Wake on Lan output-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now);

            foreach(string subnetFile in Settings.SubnetFiles())
            {
                foreach (string subnet in Settings.GetSubnetsFromFile(subnetFile))
                {
                    //Add found machines to list
                    machines.AddRange(DHCP.FindClientsOnSubnet(Settings.GetDHCPServer(), subnet, subnetFile));
                }
            }

            foreach (Machine machine in machines)
            {
                Console.WriteLine(WakeUp(machine.mac, machine.broadcastAddress));
                System.Threading.Thread.Sleep(100);
            }


            Console.WriteLine("Magic packets sent to {0} machines", machines.Count);
            Thread.Sleep(10000); //Wait 5 minutes
            Console.WriteLine("Initiating pinging...");

            
            foreach (Machine machine in machines)
            {
                if (machine.hostname != null)
                    machine.Online = WOLClass.PingHost(machine.hostname);
                else
                    machine.Online = false;
            }

            using (StreamWriter sw = new StreamWriter(outputFilename))
            {
                sw.WriteLine("Overall statistics:");
                sw.WriteLine(string.Format("Total computers: {0}", machines.Count));
                sw.WriteLine(string.Format("Total computers successfully turned on: {0} ({1:0.##%})", machines.Count(f => f.Online), ((decimal)machines.Count(f => f.Online) / (decimal)machines.Count)));
                sw.WriteLine(string.Format("Total computers failed: {0} ({1:0.##%})", machines.Count(f => !f.Online), ((decimal)machines.Count(f => !f.Online) / (decimal)machines.Count)));
                sw.WriteLine();
                sw.WriteLine("Statistics per location:");
                sw.WriteLine();

                foreach (string subnetFile in Settings.SubnetFiles())
                {
                    string subnetFilename = Path.GetFileNameWithoutExtension(subnetFile);

                    List<Machine> FailedList = machines.Where(f => !f.Online && f.SubnetFile == subnetFilename).ToList();
                    List<Machine> SuccessList = machines.Where(f => f.Online && f.SubnetFile == subnetFilename).ToList();

                    sw.WriteLine(string.Format("Location: {0}   Failed: {1} ({3:0.##%})   Successful: {2} ({4:0.##%})", Path.GetFileNameWithoutExtension(subnetFile), FailedList.Count, SuccessList.Count, ((decimal)FailedList.Count / (decimal)machines.Count), ((decimal)SuccessList.Count / (decimal)machines.Count)));
                    sw.WriteLine();
                    sw.WriteLine("{0} computers that failed:", subnetFilename);
                    foreach (Machine m in FailedList)
                    {
                        sw.WriteLine(string.Format("{0} [{1}]", m.hostname == null ? "Unknown" : m.hostname, m.mac));

                    }
                    sw.WriteLine();
                    sw.WriteLine();
                }
            }
        }
        private static string WakeUp(string mac, string ip, int port = 9)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "wolcmd.exe",
                    Arguments = string.Format("{0} {1} {2} {3}", mac, ip, "255.255.255.0", port),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            return proc.StandardOutput.ReadToEnd();
        }
    }
}

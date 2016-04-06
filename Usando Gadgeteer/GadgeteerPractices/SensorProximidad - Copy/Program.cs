using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.SocketInterfaces;
using System.Threading;

namespace SensorProximidad
{
    public partial class Program
    {
        AnalogInput entrada = null;
        DigitalOutput salida = null;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            entrada = extender.CreateAnalogInput(GT.Socket.Pin.Three); 
            salida = extender.CreateDigitalOutput(GT.Socket.Pin.Five, false);            
            var timer = new GT.Timer(20000);
            timer.Tick += Timer_Tick;

            ethernetJ11D.NetworkDown += EthernetJ11D_NetworkDown;
            ethernetJ11D.NetworkUp += EthernetJ11D_NetworkUp;
            ethernetJ11D.UseThisNetworkInterface(); //necesario para habilitar el uso de la interface

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
            timer.Start();            
            SetupEthernet();            
        }

        private void SetupEthernet()
        {
            //ethernetJ11D.UseDHCP();
            ethernetJ11D.UseStaticIP(
                "192.168.65.4",
                "255.255.255.0",
                "192.168.65.8");
            string[] dns = { "200.10.150.20","200.10.150.16" };
            ethernetJ11D.NetworkSettings.EnableStaticDns(dns);
            //netif.Open();
            //netif.EnableDhcp();
            while (!ethernetJ11D.IsNetworkUp)
            {
                Debug.Print("Waiting for DHCP");
                Thread.Sleep(250);
            }
        }

        private void EthernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network up.");
            ListNetworkInterfaces();
        }

        private void ListNetworkInterfaces()
        {
            var settings = ethernetJ11D.NetworkSettings;

            Debug.Print("------------------------------------------------");
            Debug.Print("IP Address:   " + settings.IPAddress);
            Debug.Print("DHCP Enabled: " + settings.IsDhcpEnabled);
            Debug.Print("Subnet Mask:  " + settings.SubnetMask);
            Debug.Print("Gateway:      " + settings.GatewayAddress);
            Debug.Print("DNS:      " + settings.DnsAddresses.GetValue(0));
            Debug.Print("------------------------------------------------");
        }

        private void EthernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network down.");
        }

        private void Timer_Tick(GT.Timer timer)
        {
            salida.Write(true);
            Debug.Print("proximidad: " + entrada.ReadProportion() + " " + entrada.ReadVoltage());
            //SetupEthernet();
        }
    }
}

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using GTM = Gadgeteer.Modules;

namespace MQTTDevice
{
    public partial class Program
    {
        private const string DOMAIN = "Gadgeteer";
        private const string CLIENTID = "GadgeteerDevice"; //this is the device id for the broker to use

        private const string LIGHTSTATUS = "status/light";
        private const string LIGHTCONTROL = "cmd/light";
        private const string DEVICESTATUS = "status";

        private const string DEVICEID = "device1";
        //name of this perticular device in the topic, in case I wanted to add more similar devices

        //using HiveMQ demo at http://www.hivemq.com/demos/websocket-client/
        //private const string BROKER = "broker.mqttdashboard.com";
        //private const string USERNAME = "";
        //private const string PASSWORD = "";

        private const string BROKER = "m10.cloudmqtt.com";
        private const int PORT = 11001;
        private const string USERNAME = "exdvilmf";
        private const string PASSWORD = "ijE6kV-yyvhv";

        //private const string BROKER = "dev.rabbitmq.com"; // RabbitMQ
        //private const string USERNAME = "guest";
        //private const string PASSWORD = "guest";

        //Message to control lights
        // Gadgeteer/device1/light/cmd/light_number color

        //Message for light status
        // Gadgeteer/device1/light/status/light_number color

        //Message for device status
        // Gadgeteer/device1/status status

        //only want control messages for this device (subscription)
        private const string mqttDeviceCommand = DOMAIN + "/" + DEVICEID + "/" + LIGHTCONTROL + "/+";

        // used to send out status messages for the lights (publish)
        private const string mqttLightStatus = DOMAIN + "/" + DEVICEID + "/" + LIGHTSTATUS + "/";
        //just needs which light appended to the topic

        // used to send out status messages for the device (publish)
        private const string mqttDeviceStatus = DOMAIN + "/" + DEVICEID + "/" + DEVICESTATUS;
        //only a message needed to send

        private MqttClient _mqttclient;

        private readonly string[] DeviceSubscriptions = { mqttDeviceCommand };

        private readonly byte[] QOSServiceLevels = { 2 };
        //requested QoS level, the client receives PUBLISH messages at less than or equal to this level

        private static bool _cleanSession = true;
        
        private readonly char[] _delimiters = { '/' }; //used to parse topic strings

        private void ProgramStarted()
        {
            Debug.Print("Program Started");

            ethernetJ11D.NetworkDown += EthernetJ11D_NetworkDown;
            ethernetJ11D.NetworkUp += EthernetJ11D_NetworkUp;
            ethernetJ11D.UseThisNetworkInterface(); //necesario para habilitar el uso de la interface

            Mainboard.SetDebugLED(true);            
            
            SetupEthernet();
            inicio();
        }

        private void inicio()
        {
            if (ethernetJ11D.IsNetworkConnected)
            {
                Debug.Print("IP Address " + ethernetJ11D.NetworkInterface.IPAddress);

                if (ethernetJ11D.IsNetworkConnected && (ethernetJ11D.NetworkInterface.IPAddress != "0.0.0.0"))
                {
                    //NTPTime("time.windows.com", -360);

                    _mqttclient = new MqttClient(BROKER, PORT, false, null,null,MqttSslProtocols.None);

                    _mqttclient.MqttMsgPublishReceived += Mqttclient_MqttMsgPublishReceived;
                    //where we get messages from our subscriptions

                    _mqttclient.MqttMsgSubscribed += Mqttclient_MqttMsgSubscribed;
                    //a message from the broker that we have subscribed
                    //_mqttclient.MqttMsgDisconnected += Mqttclient_MqttMsgDisconnected; //a message when we disconnect
                    _mqttclient.MqttMsgPublished += Mqttclient_MqttMsgPublished;
                    //a message from the broker that we have sent a message
                    _mqttclient.MqttMsgUnsubscribed += Mqttclient_MqttMsgUnsubscribed;
                    //a message from the broker that we have unsubscribed


                    Mainboard.SetDebugLED(false);

                    try
                    {
                        if (_mqttclient.IsConnected)
                        {
                            var disconnect = new Thread(ThreadedDisconnect);
                            disconnect.Start();
                            Debug.Print("Disconnecting");

                            int loopcounter = 0;

                            while (_mqttclient.IsConnected && loopcounter++ < 5)
                            {
                                Thread.Sleep(100);
                            }
                            Debug.Print("disconnect loops " + loopcounter);

                            if (loopcounter < 5)
                            {
                                //don't need to send a message here as the last will message will be sent on disconnect or discover of disconnect (ie failed heartbeat)                    
                                Debug.Print("Disconnected");
                            }
                        }
                        else
                        {
                            byte response = _mqttclient.Connect(CLIENTID, USERNAME, PASSWORD, true, 2, true,
                                mqttDeviceStatus, "offline", _cleanSession, 60);
                            /*
                             * 
                             * When a client connects to a broker, it may inform the broker that it has a will. This is a message that it wishes the broker to send when the client 
                             * disconnects unexpectedly or from a disconnect request.  The will message has a topic, QoS and retain status just the same as any other message.
                             * 
                             */

                            if (response == 0)
                            {
                                _mqttclient.Publish(mqttDeviceStatus, Encoding.UTF8.GetBytes("online"), 2, true);
                                Debug.Print(mqttDeviceStatus + " online : 2 true");
                            }
                            Debug.Print("Connect " + response);

                            /*
                             * 
                             * Clean session / Durable connections
                             * 
                             * When a client sends a connect frame with the clean session flag set to cleared (false), any previously used 
                             * session with the same client_id will be re-used.  This means that when the client disconnects, any subscriptions 
                             * it has will remain and any subsequent QoS 1 or 2 messages will be stored until it connects again in the 
                             * future.  If clean session is true, then all subscriptions will be removed for the client when it disconnects. 
                             * 
                             * If you change modes from using cleanSession=false to cleanSession=true, all previous subscriptions for the client, 
                             * and any publications that have not been received, are discarded.
                             * 
                             */
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
            }

        }

        private void EthernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            if (state == GTM.Module.NetworkModule.NetworkState.Up)
                Debug.Print("Network Up event; state = Up");
            else
                Debug.Print("Network Up event; state = Down");
        }

        private void EthernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            if (state == GTM.Module.NetworkModule.NetworkState.Down)
                Debug.Print("Network Up event; state = Down");
            else
                Debug.Print("Network Up event; state = Up");
        }

        private void SetupEthernet()
        {
            //ethernetJ11D.UseDHCP();
            ethernetJ11D.UseStaticIP(
                "192.168.65.4",
                "255.255.255.0",
                "192.168.65.8");
            string[] dns = { "200.10.150.20", "200.10.150.16" };
            ethernetJ11D.NetworkSettings.EnableStaticDns(dns);
            
            while (!ethernetJ11D.IsNetworkUp)
            {
                Debug.Print("Waiting for DHCP");
                Thread.Sleep(250);
            }
        }

        private void subscribe_TapEvent(object sender)
        {
            ushort x = _mqttclient.Subscribe(DeviceSubscriptions, QOSServiceLevels);
            Debug.Print("Subscribe " + x.ToString());
        }

        private void connect_TapEvent(object sender)
        {
            
        }

        private void ThreadedDisconnect()
        {
            _mqttclient.Disconnect();
        }

        private void Mqttclient_MqttMsgDisconnected(object sender, EventArgs e)
        {           
            Debug.Print("Disconnected");
        }

        private void Mqttclient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            //parse the topic
            string[] topicparts = e.Topic.Split(_delimiters);
            Debug.Print(topicparts.Length.ToString());

            //get the message
            char[] chars = Encoding.UTF8.GetChars(e.Message);
            var message = new string(chars);

            Debug.Print(e.Topic + " " + message + " : " + e.QosLevel + " " + e.Retain + " " + e.DupFlag);

            if (topicparts.Length == 5 && topicparts[1] == DEVICEID && topicparts[2] == "cmd" &&
                topicparts[3] == "light")
            {
                try
                {
                    //MessageRecieved(e.Topic + " " + message + " : " + e.QosLevel + " " + e.Retain + " " + e.DupFlag);
                    //int light = int.Parse(topicparts[4]);
                    //SetLight(light, message);
                }
                catch
                {
                    //MessageRecieved("Invalid Message " + e.Topic + " " + message + " : " + e.QosLevel + " " + e.Retain +
                    //                " " +
                    //                e.DupFlag);
                    Debug.Print("Invalid Message " + message);
                }
            }
        }

        //private void SetLight(int light, string color, bool updateStatus = true)
        //{
        //    if (light > -1 && light < 4) //only change lights we have
        //    {
        //        Debug.Print("setting light " + light + " to " + color);

        //        LightOnOFf(light, color);
        //        string topic = mqttLightStatus + light;

        //        if (updateStatus)
        //        {
        //            _mqttclient.Publish(topic, Encoding.UTF8.GetBytes(color), 2, true);
        //            MessageSent(topic + " " + color + " : 2 true");
        //        }
        //    }
        //}

        //void LightOnOFf(int _Light, string _Color)
        //{
        //    if (_Light == 0)
        //    {
        //        if (_Color == "off")
        //            multicolorLED.TurnOff();
        //        else
        //            multicolorLED.TurnWhite();
        //    }
        //    else if (_Light == 1)
        //    {
        //        if (_Color == "off")
        //            multicolorLED2.TurnOff();
        //        else
        //            multicolorLED2.TurnRed();
        //    }
        //    else if (_Light == 2)
        //    {
        //        if (_Color == "off")
        //            multicolorLED3.TurnOff();
        //        else
        //            multicolorLED3.TurnGreen();
        //    }
        //    else if (_Light == 3)
        //    {
        //        if (_Color == "off")
        //            multicolorLED4.TurnOff();
        //        else
        //            multicolorLED4.TurnBlue();
        //    }
        //}

        //private void ResetLights()
        //{
        //    multicolorLED.TurnOff();
        //    multicolorLED2.TurnOff();
        //    multicolorLED3.TurnOff();
        //    multicolorLED4.TurnOff();
        //}

        private void Mqttclient_MqttMsgUnsubscribed(object sender, MqttMsgUnsubscribedEventArgs e)
        {            
            Debug.Print("Msg Unsubscribed " + e.MessageId);
        }

        private void Mqttclient_MqttMsgPublished(object sender, MqttMsgPublishedEventArgs e)
        {
            Debug.Print("Msg Published " + e.MessageId);
        }

        private void Mqttclient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {            
            Debug.Print("Message Subscribed " + e.MessageId);
        }

        public bool NTPTime(string TimeServer, int GmtOffset = 0)
        {
            Socket s = null;
            try
            {
                EndPoint rep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 123);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var ntpData = new byte[48];
                Array.Clear(ntpData, 0, 48);
                ntpData[0] = 0x1B; // Set protocol version
                s.SendTo(ntpData, rep); // Send Request   
                if (s.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) // Waiting an answer for 30s, if nothing: timeout
                {
                    s.ReceiveFrom(ntpData, ref rep); // Receive Time
                    byte offsetTransmitTime = 40;
                    ulong intpart = 0;
                    ulong fractpart = 0;
                    for (int i = 0; i <= 3; i++) intpart = (intpart << 8) | ntpData[offsetTransmitTime + i];
                    for (int i = 4; i <= 7; i++) fractpart = (fractpart << 8) | ntpData[offsetTransmitTime + i];
                    ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
                    s.Close();
                    DateTime dateTime = new DateTime(1900, 1, 1) +
                                        TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
                    Utility.SetLocalTime(dateTime.AddMinutes(GmtOffset));
                    Debug.Print("Current Date and time " + DateTime.Now);
                    return true;
                }
                s.Close();
            }
            catch (Exception exception)
            {
                try
                {
                    s.Close();
                }
                catch
                {
                }
                Debug.Print(exception.Message);
            }
            return false;
        }

       
    }
}

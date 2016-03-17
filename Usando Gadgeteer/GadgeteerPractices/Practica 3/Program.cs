using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.SocketInterfaces;

//Estas referencias son necesarias para usar GLIDE
using GHI.Glide;
using GHI.Glide.Display;
using GHI.Glide.UI;
using System.Diagnostics;
//using Microsoft.SPOT.Hardware;


namespace Practica3DSCC
{
    
    public partial class Program
    {
        //Objetos de interface gráfica GLIDE        
        //private GHI.Glide.Display.Window controlWindow;
        //private GHI.Glide.Display.Window camaraWindow;

        //private Button btn_start;
        //private Button btn_stop;

        //SensorProximidad Sensor;
        AnalogInput entrada = null;

        
        //enum Estados
        //{
        //    SensorOff,
        //    SensorOn,
        //    Monitoreo
        //}

        //Estados estado;


        
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
            //Sensor = new SensorProximidad(extender);

            //camera.BitmapStreamed += Camera_BitmapStreamed;
            //Sensor.ObjectOn += Sensor_ObjectOn;
            //Sensor.ObjectOff += Sensor_ObjectOff;

            entrada = extender.CreateAnalogInput(GT.Socket.Pin.Three);


            ////Carga las ventanas
            //controlWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.controlWindow));
            //camaraWindow = GlideLoader.LoadWindow(Resources.GetString(Resources.StringResources.camaraWindow));
            //GlideTouch.Initialize();

            ////Inicializa los botones en la interface
            //btn_start = (Button)controlWindow.GetChildByName("start");
            //btn_stop = (Button)controlWindow.GetChildByName("stop");
            //btn_start.TapEvent += btn_start_TapEvent;
            //btn_stop.TapEvent += btn_stop_TapEvent;

            ////Selecciona mainWindow como la ventana de inicio
            //Glide.MainWindow = controlWindow;

            //// Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            //estado = Estados.Monitoreo;

            while (true)
            {
                Debug.Print("voltaje: " + entrada.ReadVoltage());
            }
        }

        //private void Sensor_ObjectOff()
        //{
        //    Debug.Print("Sensor Apagado");
        //    cambiarEstados(Estados.SensorOn);
        //}

        //private void Sensor_ObjectOn()
        //{
        //    Debug.Print("Sensor Encendido");
        //    cambiarEstados(Estados.Monitoreo);
        //}

        //private void Camera_BitmapStreamed(Camera sender, Bitmap e)
        //{
        //    displayT35.SimpleGraphics.DisplayImage(e, 0, 0);
        //}

        //void btn_stop_TapEvent(object sender)
        //{
        //    Debug.Print("Stop");
        //    Sensor.StopSampling();
        //    cambiarEstados(Estados.SensorOff);
        //}

        //void btn_start_TapEvent(object sender)
        //{
        //    Debug.Print("Start");
        //    Sensor.StartSampling();
        //    cambiarEstados(Estados.SensorOn);
        //}

        //void cambiarEstados(Estados estado)
        //{
        //    TextBlock text = (TextBlock)controlWindow.GetChildByName("status");
        //    switch (estado)
        //    {
        //        case Estados.SensorOff:
        //            text.Text = "No monitoreando";
        //            break;
        //        case Estados.SensorOn:
        //            text.Text = "Monitoreando";
        //            camera.StopStreaming();
        //            Glide.MainWindow = controlWindow;
        //            break;
        //        case Estados.Monitoreo:
        //            Glide.MainWindow = camaraWindow;
        //            camera.StartStreaming();
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}

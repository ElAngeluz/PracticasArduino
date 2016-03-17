using System;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules;
using GT = Gadgeteer;
using Gadgeteer.SocketInterfaces;

namespace Practica3DSCC
{
    // Referencia tipo "delegate" para función callback ObjectOn
    public delegate void ObjectOnEventHandler();

    // Referencia tipo "delegate" para función callback ObjectOff
    public delegate void ObjectOffEventHandler();

    /*
     * Clase SensorProximidad, encapsula el funcionanmiento del sensor de proximidad infrarrojo.
     * Esta clase gestiona los dos componentes del sensor: el LED infrarrojo y el foto-transistor.
     * Además, dispara dos eventos: ObjectOn y ObjectOff cuando el sensor detecta la presencia o
     * ausencia de un objeto.
     */
    class SensorProximidad
    {
        //EVENTO ObjectOff: Disparar este evento cuando el sensor detecte la ausencia del objeto
        public event ObjectOffEventHandler ObjectOff;

        //EVENTO ObjectOn: Disparar este evento cuando el sensor detecte la presencia de un objeto
        public event ObjectOnEventHandler ObjectOn;

        enum Estado
        {
            Ninguno,
            Presente,
            Ausente
        }

        Estado estado;
        private GT.Timer timer;

        AnalogInput entrada = null;
        DigitalOutput salida = null;

        public SensorProximidad(GTM.GHIElectronics.Extender extender)
        {
            entrada = extender.CreateAnalogInput(GT.Socket.Pin.Three);
            salida = extender.CreateDigitalOutput(GT.Socket.Pin.Five, false);

            timer = new GT.Timer(1000);
            timer.Tick += Timer_Tick;
            estado = Estado.Ninguno;
        }

        private void Timer_Tick(GT.Timer timer)
        {
            double voltaje = entrada.ReadVoltage();
            Debug.Print("voltaje: "+ entrada.ReadVoltage());
            if (voltaje<3)
            {
                if (estado == Estado.Ninguno || estado == Estado.Ausente)
                {
                    ObjectOn();
                    estado = Estado.Presente;
                }
            }
            else
            {
                if (estado == Estado.Presente)
                {
                    ObjectOff();
                    estado = Estado.Ausente;
                }
            }
        }

        public void StartSampling()
        {
            Debug.Print("voltaje: " + entrada.ReadVoltage());
            salida.Write(true);
            Debug.Print("encendido");
            timer.Start();
        }

        public void StopSampling()
        {
            salida.Write(false);
            Debug.Print("apagado");
            timer.Stop();
        }
    }
}

using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using GT = Gadgeteer;

namespace practica_2
{
    public partial class Program
    {
        GT.Timer timer = new GT.Timer(1500, GT.Timer.BehaviorType.RunOnce);
        int count = 1;

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
            displayT35.WPFWindow.TouchDown += (a, b) => { };            
            displayT35.WPFWindow.TouchUp += (a, b) => { };
            displayT35.WPFWindow.TouchMove += (a, b) => { }; 

            //camera.PictureCaptured += Camera_PictureCaptured;
            camera.CameraConnected += Camera_CameraConnected;
            camera.BitmapStreamed += Camera_BitmapStreamed;
            camera.PictureCaptured += Camera_PictureCaptured;
            button.ButtonPressed += Button_ButtonPressed;
            timer.Tick += Timer_Tick;
            sdCard.Mounted += SdCard_Mounted;

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");

            
        }

        private void Camera_PictureCaptured(Camera sender, GT.Picture e)
        {
            if (sdCard.IsCardMounted)
                {
                    button.TurnLedOn();
                    sdCard.StorageDevice.WriteFile("foto"+ count.ToString() + ".bipmap", e.PictureData);                    
                    count++;
                    Debug.Print("Save picture");
                    button.TurnLedOff();
            }
            else 
               Debug.Print("Not CardInserted");

           timer.Start();
        }        

        private void SdCard_Mounted(SDCard sender, GT.StorageDevice device)
        {
            Debug.Print("Memoria Lista");
        }

        private void Timer_Tick(GT.Timer timer)
        {
            camera.StartStreaming();
        }

        private void Camera_CameraConnected(Camera sender, EventArgs e)
        {
            camera.StartStreaming();
        }

        /// <summary>
        /// prcedimiento que captura la foto y la presenta
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Camera_BitmapStreamed(Camera sender, Bitmap e)
        {
            displayT35.SimpleGraphics.DisplayImage(e, 0, 0);
        }

        private void Button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            camera.StopStreaming();
            camera.TakePicture();
        }       
    }
}

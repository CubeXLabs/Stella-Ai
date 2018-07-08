using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Speech.Tts;
using System.Collections.Generic;
using System.Linq;
//FlashLight Plugin
using Camera = Android.Hardware.Camera;
//Background Audio Plugin
using Android.Media;
using Android.Util;
//MySQL Plugin
using MySql.Data.MySqlClient;
using System.Data;
using Android.Views;
//TCP Plugins
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
//BackgroundStreamingAudio Plugin
using StellaAI.Services;
//Location Plugin
using Android.Locations;
//MessagingPlugin
using Android.Telephony;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
//Timer Plugin
//using System.Timers;

namespace StellaAI
{

    //[Activity(Label = "Stella AI", MainLauncher = true, Icon = "@drawable/icon")]
    [Activity(Label = "Stella Ai")]
    public class MainActivity : Activity, IRecognitionListener, TextToSpeech.IOnInitListener, ILocationListener
    {

        //SpeechRecognition
        public const string Tag = "VoiceRec";

        SpeechRecognizer Recognizer { get; set; }
        Intent SpeechIntent { get; set; }
        TextView Label { get; set; }


        private TextView restext;
        private Camera camera;
        MediaPlayer _player;


        TextToSpeech textToSpeech;
        Context context;
        private readonly int MyCheckCode = 101, NeedLang = 103;
        Java.Util.Locale lang;

        //TCP
        private TcpClient client;
        private StreamReader STR;
        private StreamWriter STW;
        private string received;
        private String text_to_send;

        //Location Manager
        private LocationManager locMgr;
        string tag = "MainActivity";

        //Timer Switch
        bool SyncTimer;

        //Weather
        String temp;
        String condition;

        //Bot_Response
        String bot_responsed;


        /*private async Task TimerAsync(int interval, CancellationToken token)
        {
            while (token.IsCancellationRequested)
            {
                // do your stuff here...
                //
                string message = string.Empty;
                message = "CountDownTimer End";
                restext.Text = "Message Sended To +923009474752.";
                //
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();

                await Task.Delay(interval, token);
            }
        }*/

        public void PlayAudioFile(string fileName)
        {
            var player = new MediaPlayer();
            var fd = global::Android.App.Application.Context.Assets.OpenFd(fileName);
            player.Prepared += (s, e) =>
            {
                player.Start();
            };
            player.SetDataSource(fd.FileDescriptor, fd.StartOffset, fd.Length);
            player.Prepare();
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            this.Window.AddFlags(WindowManagerFlags.Fullscreen); // hide the status bar
            
            SetContentView(Resource.Layout.Main);
            //StartBackground Service
            //StartService(intent);
            OnStart();

            //CountDown Timer


            Log.Debug(tag, "OnCreate called"); //Location Refresher

            //SpeechRecognitionRefresher();

            Recognizer = SpeechRecognizer.CreateSpeechRecognizer(this);
            Recognizer.SetRecognitionListener(this);

            SpeechIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            SpeechIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            SpeechIntent.PutExtra(RecognizerIntent.ExtraCallingPackage, PackageName);

            // if there is more then 1.5s of silence, consider the speech over
            SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            SpeechIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            SpeechIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);https://www.cubexlabs.com//Projects/VS-Plugins/SQL-Plugins/MySql.Data.CF.dll


            var button = FindViewById<Button>(Resource.Id.button1);
            button.Click += ButtonClick;

            Label = FindViewById<TextView>(Resource.Id.textYourText);

            // get the resources from the layout
            //recButton = FindViewById<Button>(Resource.Id.btnRecord);
            restext = FindViewById<TextView>(Resource.Id.textView1);
            //textBox = FindViewById<TextView>(Resource.Id.textYourText);
            


            //TextToSpeech
            // set up the TextToSpeech object
            // third parameter is the speech engine to use
            textToSpeech = new TextToSpeech(this, this, "com.google.android.tts");

            // set up the langauge spinner
            // set the top option to be default
            var langAvailable = new List<string> { "Default" };

            // our spinner only wants to contain the languages supported by the tts and ignore the rest
            var localesAvailable = Java.Util.Locale.GetAvailableLocales().ToList();
            foreach (var locale in localesAvailable)
            {
                LanguageAvailableResult res = textToSpeech.IsLanguageAvailable(locale);
                switch (res)
                {
                    case LanguageAvailableResult.Available:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                    case LanguageAvailableResult.CountryVarAvailable:
                        langAvailable.Add(locale.DisplayLanguage);
                        break;
                }

            }
            langAvailable = langAvailable.OrderBy(t => t).Distinct().ToList();

            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, langAvailable);
            //spinLanguages.Adapter = adapter;

            // set up the speech to use the default langauge
            // if a language is not available, then the default language is used.
            lang = Java.Util.Locale.Default;
            textToSpeech.SetLanguage(lang);

            // set the speed and pitch
            textToSpeech.SetPitch(.0f);
            textToSpeech.SetSpeechRate(.0f);
        }
        // Interface method required for IOnInitListener
        void TextToSpeech.IOnInitListener.OnInit(OperationResult status)
        {
            // if we get an error, default to the default language
            if (status == OperationResult.Error)
                textToSpeech.SetLanguage(Java.Util.Locale.Default);
            // if the listener is ok, set the lang
            if (status == OperationResult.Success)
                textToSpeech.SetLanguage(lang);
        }



        //Speech Recognition
        private void ButtonClick(object sender, EventArgs e)
        {
            Recognizer.StartListening(SpeechIntent);
        }

        //Speech Recognition Refresh
        /*public bool SpeechRecognitionRefresherTMR()
        {

            /*if (SpeechRecognizer.ResultsRecognition < )
            {

            }
            var matchesTMR = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            //int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (SpeechRecognizer.ResultsRecognition.toString().trim().length() == 0)
            {
                
                if ()
                    //msgText.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    //msgText.Text = "Sorry, this device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                //msgText.Text = "Google Play Services is available.";
                return true;
            }

        }*/





        //Location Refresher

        // OnResume gets called every time the activity starts, so we'll put our RequestLocationUpdates
        // code here, so that 
        protected override void OnResume()
        {
            base.OnResume();
            Log.Debug(tag, "OnResume called");

            // initialize location manager
            locMgr = GetSystemService(Context.LocationService) as LocationManager;
            
                //button.Text = "Location Service Running";
                

                if (locMgr.AllProviders.Contains(LocationManager.NetworkProvider)
                    && locMgr.IsProviderEnabled(LocationManager.NetworkProvider))
                {
                    locMgr.RequestLocationUpdates(LocationManager.NetworkProvider, 2000, 1, this);
                }
                else
                {
                    Toast.MakeText(this, "The Network Provider does not exist or is not enabled!", ToastLength.Long).Show();
                


                // Comment the line above, and uncomment the following, to test 
                // the GetBestProvider option. This will determine the best provider
                // at application launch. Note that once the provide has been set
                // it will stay the same until the next time this method is called

                /*var locationCriteria = new Criteria();

				locationCriteria.Accuracy = Accuracy.Coarse;
				locationCriteria.PowerRequirement = Power.Medium;

				string locationProvider = locMgr.GetBestProvider(locationCriteria, true);

				Log.Debug(tag, "Starting location updates with " + locationProvider.ToString());
				locMgr.RequestLocationUpdates (locationProvider, 2000, 1, this);*/
            };
        }
        protected override void OnStart()
        {
            base.OnStart();
            Log.Debug(tag, "OnStart called");
        }

        protected override void OnPause()
        {
            base.OnPause();

            // stop sending location updates when the application goes into the background
            // to learn about updating location in the background, refer to the Backgrounding guide
            // http://docs.xamarin.com/guides/cross-platform/application_fundamentals/backgrounding/


            // RemoveUpdates takes a pending intent - here, we pass the current Activity
            //locMgr.RemoveUpdates(this);
            Log.Debug(tag, "Location updates paused because application is entering the background");
        }

        protected override void OnStop()
        {
            base.OnStop();
            Log.Debug(tag, "OnStop called");
        }

        public void OnLocationChanged(Android.Locations.Location location)
        {
            Log.Debug(tag, "Location changed");
            //restext.Text = "Latitude: " + location.Latitude.ToString();
            //longitude.Text = "Longitude: " + location.Longitude.ToString();
            //provider.Text = "Provider: " + location.Provider.ToString();
        }
        public void OnProviderDisabled(string provider)
        {
            Log.Debug(tag, provider + " disabled by user");
        }
        public void OnProviderEnabled(string provider)
        {
            Log.Debug(tag, provider + " enabled by user");
        }
        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            Log.Debug(tag, provider + " availability has changed to " + status.ToString());
        }


        //BackgroundStreamingServices
        private void SendAudioCommand(string action)
        {
            var intent = new Intent(action);
            StartService(intent);
        }


        //CountDownTimer
        private void CountDown()
        {

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;
            restext.Text = "Timer timer.Enabled = true!";

        }

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            //string message = string.Empty;
            //message = "CountDownTimer End";
            restext.Text = "Timer Finished!";
            CountDown();
            //Recognizer.StartListening(SpeechIntent);
            //Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            if (SpeechRecognizer.ResultsRecognition != null)
            {
                Recognizer.StartListening(SpeechIntent);
            }
        }


        private async void RunUpdateLoop()
        {
            int count = 1;
            while (SyncTimer)
            {
                await Task.Delay(1000);
                //restext.Text = string.Format("{0} ticks!", count++);

                if (SpeechRecognizer.ResultsRecognition != null)
                {
                    Recognizer.StartListening(SpeechIntent);
                }
            }
        }

        private async void RunSQLUpdateLoop()
        {
            int SQLcount = 1;
            while (SyncTimer)
            {
                await Task.Delay(1000);

                
                //////////////////

                if (!string.IsNullOrEmpty(bot_responsed))
                {

                    //If bot_responsed contain texts

                    restext.Text = bot_responsed;
                    //Readed
                    //MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8");

                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update Aiml_Server set is_readed=@Readed where user_id=1", con);
                            cmd.Parameters.AddWithValue("@Readed", "Readed");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //txtSysLog.Text = ex.ToString();
                    }
                    finally
                    {
                        //CountDown();
                        SyncTimer = false;
                        RunSQLUpdateLoop();
                        con.Close();
                    }//End First Rule

                }else {
                    //If bot_responsed not comtain text

                    //Display Data From MySQL
                    string myConnection = "Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8";
                    MySqlConnection myConn = new MySqlConnection(myConnection);
                    MySqlCommand command = myConn.CreateCommand();
                    command.CommandText = "select * from Aiml_Server where user_id=1";
                    MySqlDataReader myReader;

                    try
                    {
                        myConn.Open();
                        myReader = command.ExecuteReader();

                        while (myReader.Read())
                        {
                            bot_responsed = myReader[3].ToString();
                            restext.Text = bot_responsed;

                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                    myConn.Close();
                    textToSpeech.Speak(bot_responsed, QueueMode.Flush, null);
                    /*//Readed
                    //MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=admin_ta;Password=TAta12345;charset=utf8");

                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update Aiml_Server set is_readed=@Readed where user_id=1", con);
                            cmd.Parameters.AddWithValue("@Readed", "Readed");
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //txtSysLog.Text = ex.ToString();
                    }
                    finally
                    {
                        //CountDown();
                        SyncTimer = true;
                        RunSQLUpdateLoop();
                        con.Close();
                    }//End First Rule*/


                }//END !string.IsNullOrEmpty(bot_responsed


            }
        }


        //Yahoo Weather API
        //Get Weather
        public String GetWeather(String input)
        {
            String query = String.Format("https://query.yahooapis.com/v1/public/yql?q=select * from weather.forecast where woeid in (select woeid from geo.places(1) where text='Lahore, Punjab, PK')&format=xml&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys");
            XmlDocument wData = new XmlDocument();
            try
            {
                wData.Load(query);
            }
            catch
            {
                //MessageBox.Show("No internet connection");
                return "No internet";
            }

            XmlNamespaceManager manager = new XmlNamespaceManager(wData.NameTable);
            manager.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");

            XmlNode channel = wData.SelectSingleNode("query").SelectSingleNode("results").SelectSingleNode("channel");
            XmlNodeList nodes = wData.SelectNodes("query/results/channel");
            try
            {
                int rawtemp = int.Parse(channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["temp"].Value);
                temp = (rawtemp - 32) * 5 / 9 + "";
                condition = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["text"].Value;
                //high = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["high"].Value;
                //low = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["low"].Value;
                if (input == "temp")
                {
                    return temp;
                }
                //if (input == "high")
                //{
                //return high;
                //}
                //if (input == "low")
                //{
                //return low;
                //}
                if (input == "cond")
                {
                    return condition;
                }
            }
            catch
            {
                return "Error Reciving data";
            }
            return "error";
        }



        public void OnResults(Bundle results)
        {
            var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches != null && matches.Count > 0)
            {
                Label.Text = matches[0];

                /////////////////////////////////
                ////////Switches Control////////
                ///////////////////////////////

                //Your Responses
                if (Label.Text == "Google turn light on")
                {
                    textToSpeech.Speak("Okay i'm turning light on.", QueueMode.Flush, null);
                    restext.Text = "Okay i'm turning light on.";

                    //TCP Connect
                    client = new TcpClient();
                    IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse("192.168.1.140"), int.Parse("65535"));

                    try
                    {
                        client.Connect(IP_End);
                        if (client.Connected)
                        {
                            //textviewConversation.Text += "Connected to server" + "\n";
                            STR = new StreamReader(client.GetStream());
                            STW = new StreamWriter(client.GetStream());
                            STW.AutoFlush = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                    text_to_send = "1";
                    STW.WriteLine(text_to_send);

                    /*//MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Smart_Switch_db;User Id=admin_ta;Password=TAta12345;charset=utf8");

                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            string offstatus = "1";
                            //string Enabledqwerty = "";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update App_v1 set off=@off where switch=1", con);
                            cmd.Parameters.AddWithValue("@off", offstatus);
                            //cmd.Parameters.AddWithValue("@mode", Enabledqwerty);
                            cmd.ExecuteNonQuery();
                            //txtSysLog.Text = "Your Speech is " + Speechqwerty + " Successfully inserted!";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //txtSysLog.Text = ex.ToString();
                    }
                    finally
                    {
                        con.Close();
                    }*/

                }

                else if (Label.Text == "Google turn light off")
                {
                    textToSpeech.Speak("i'm turning light off.", QueueMode.Flush, null);
                    restext.Text = "i'm turning light off.";


                    //TCP Connect
                    client = new TcpClient();
                    IPEndPoint IP_End = new IPEndPoint(IPAddress.Parse("192.168.1.140"), int.Parse("65535"));

                    try
                    {
                        client.Connect(IP_End);
                        if (client.Connected)
                        {
                            //textviewConversation.Text += "Connected to server" + "\n";
                            STR = new StreamReader(client.GetStream());
                            STW = new StreamWriter(client.GetStream());
                            STW.AutoFlush = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    text_to_send = "0";
                    STW.WriteLine(text_to_send);

                    /*//MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Smart_Switch_db;User Id=MYSQLUSERNAME;Password=MYSQLUSERNAME;charset=utf8");

                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            string offstatus = "0";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update App_v1 set off=@off where switch=1", con);
                            cmd.Parameters.AddWithValue("@off", offstatus);
                            cmd.ExecuteNonQuery();
                            //txtSysLog.Text = "Your Speech is " + Speechqwerty + " Successfully inserted!";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //txtSysLog.Text = ex.ToString();
                    }
                    finally
                    {
                        con.Close();
                    }*/

                }

                /////////////////////////////////
                //////////App Control///////////
                ///////////////////////////////

                else if (Label.Text == "speech app")
                {
                    textToSpeech.Speak("Your speech command sended to app!", QueueMode.Flush, null);
                    //restext.Text = "Did you know that if you wake up between 2 and 3am for no reason, there’s an 80% chance that someone was staring at you.";

                    //MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Smart_Switch_db;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8");


                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            string Speechqwerty = "Hello From Stella!";
                            string Enabledqwerty = "1";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update App_v2 set speech=@speech, enabled=@enabled where aid=1", con);
                            cmd.Parameters.AddWithValue("@speech", Speechqwerty);
                            cmd.Parameters.AddWithValue("@enabled", Enabledqwerty);
                            cmd.ExecuteNonQuery();
                            restext.Text = "Your Speech is " + Speechqwerty + " Successfully updated!";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        restext.Text = ex.ToString();
                    }
                    finally
                    {
                        con.Close();
                    }

                }

                /////////////////////////////////
                //////////News/Weather//////////
                ///////////////////////////////

                else if (Label.Text == "what's the weather like")
                {
                    if (GetWeather("cond") == "No internet")
                    {
                        restext.Text = "Internet not responding! Please Try Again!";
                        textToSpeech.Speak("Internet not responding! Please Try Again!", QueueMode.Flush, null);
                    }
                    else
                    {
                        restext.Text = "The sky is " + GetWeather("cond") + ".";
                        textToSpeech.Speak("The sky is " + GetWeather("cond") + ".", QueueMode.Flush, null);
                    }

                }

                else if (Label.Text == "what's the temperature")
                {
                    if (GetWeather("temp") == "No internet")
                    {
                        restext.Text = "Internet not responding! Please Try Again!";
                        textToSpeech.Speak("Internet not responding! Please Try Again!", QueueMode.Flush, null);

                    }
                    else
                    {
                        restext.Text = "it is " + GetWeather("temp") + "degrees.";
                        textToSpeech.Speak("it is " + GetWeather("temp") + "degrees.", QueueMode.Flush, null);

                    }

                }

                /////////////////////////////////
                ///////Multimedia Controls///////
                ///////////////////////////////

                else if (Label.Text == "play")
                {
                    //textToSpeech.Speak("Playing Somebody To You - The Vamps!", QueueMode.Flush, null);
                    restext.Text = "Playing: Music";
                    SendAudioCommand(StellaAiBackgroundService.ActionPlay);
                }
                else if (Label.Text == "pause")
                {
                    //textToSpeech.Speak("Your Music Is Paused: Somebody To You - The Vamps", QueueMode.Flush, null);
                    restext.Text = "Pause: Music";
                    SendAudioCommand(StellaAiBackgroundService.ActionPause);
                }
                else if (Label.Text == "stop")
                {
                    //textToSpeech.Speak("Stop: Somebody To You - The Vamps", QueueMode.Flush, null);
                    restext.Text = "Stop: Music";
                    SendAudioCommand(StellaAiBackgroundService.ActionStop);
                }

                else if (Label.Text == "play music")
                {
                    restext.Text = "Playing Music!";
                    //_player = MediaPlayer.Create(this, Resource.Raw.music);
                    //_player.Start();
                }


                /////////////////////////////////
                //////////OS Controls///////////
                ///////////////////////////////

                else if (Label.Text == "flashlight on")
                {
                    textToSpeech.Speak("Okay, turning flashlight on.", QueueMode.Flush, null);
                    restext.Text = "Okay, turning flashlight on.";
                    // Turn on the flashlight
                    if (camera == null)
                        camera = Camera.Open();

                    if (camera == null)
                    {
                        // Debug.WriteLine("Camera failed to initialize");
                        return;
                    }

                    var p = camera.GetParameters();
                    var supportedFlashModes = p.SupportedFlashModes;

                    if (supportedFlashModes == null)
                        supportedFlashModes = new List<string>();

                    var flashMode = string.Empty;

                    if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeTorch))
                        flashMode = Android.Hardware.Camera.Parameters.FlashModeTorch;

                    if (!string.IsNullOrEmpty(flashMode))
                    {
                        p.FlashMode = flashMode;
                        camera.SetParameters(p);
                    }

                    camera.StartPreview();

                }

                else if (Label.Text == "flashlight off")
                {
                    textToSpeech.Speak("I'm turning flashlight off.", QueueMode.Flush, null);
                    restext.Text = "I'm turning flashlight off.";
                    // Turn off the flashlight  
                    if (camera == null)
                        camera = Camera.Open();

                    if (camera == null)
                    {
                        //Debug.WriteLine("Camera failed to initialize");
                        return;
                    }

                    var p = camera.GetParameters();
                    var supportedFlashModes = p.SupportedFlashModes;

                    if (supportedFlashModes == null)
                        supportedFlashModes = new List<string>();

                    var flashMode = string.Empty;

                    if (supportedFlashModes.Contains(Android.Hardware.Camera.Parameters.FlashModeTorch))
                        flashMode = Android.Hardware.Camera.Parameters.FlashModeOff;

                    if (!string.IsNullOrEmpty(flashMode))
                    {
                        p.FlashMode = flashMode;
                        camera.SetParameters(p);
                    }
                    //END Flashlight Switch

                }

                //My GeoLocation
                else if (Label.Text == "my location")
                {
                    // restext.Text = "Latitude:" + location.Latitude.ToString() + "Longitude: " + location.Longitude.ToString();


                }

                else if (Label.Text == "send SMS")
                {

                    SmsManager.Default.SendTextMessage("+923009474752", null, "Hello from Android.Xamarin.", null, null);
                    var smsUri = Android.Net.Uri.Parse("smsto:+923009474752");
                    var smsIntent = new Intent(Intent.ActionSendto, smsUri);
                    smsIntent.PutExtra("sms_body", "Hello Xamarin This is my test SMS");
                    StartActivity(smsIntent);
                    restext.Text = "Message Sended To +923009474752.";

                }


                //
                else if (Label.Text == "keep listening me")
                {
                    textToSpeech.Speak("I'm listening you!", QueueMode.Flush, null);
                    restext.Text = "Keep Listening is activated";
                    //CountDown();
                    SyncTimer = true;
                    RunUpdateLoop();
                }
                else if (Label.Text == "stop listening me")
                {
                    textToSpeech.Speak("Deactivating Keep Listening!", QueueMode.Flush, null);
                    restext.Text = "Deactivating Keep Listening";
                    //CountDown();
                    SyncTimer = false;
                    RunUpdateLoop();
                }
                else if (Label.Text == "testing timer")
                {
                    restext.Text = "CountDownTimer Started";
                    CountDown();
                    //RunUpdateLoop();
                }
                else if (Label.Text == "Stella bye bye")
                {
                    restext.Text = "Application being terminating!";
                    System.Environment.Exit(0);
                }



                /////////////////////////////////
                //////////Web Control///////////
                ///////////////////////////////

                else if (Label.Text == "open website")
                {
                    textToSpeech.Speak("Please choose your browser!", QueueMode.Flush, null);
                    restext.Text = "Please choose your browser!";
                    //Open URL
                    var uri = Android.Net.Uri.Parse("http://www.xamarin.com");
                    var intent = new Intent(Intent.ActionView, uri);
                    StartActivity(intent);
                }

                else if (Label.Text == "airplane")
                {
                    textToSpeech.Speak("Airplane mode is ON!", QueueMode.Flush, null);
                    restext.Text = "Airplane mode is ON!!";
                    /*  //Open URL
                      // TODO Auto-generated method stub
                      // read the airplane mode setting
                      boolean isEnabled = android.provider.Settings.System.getInt(
                            getContentResolver(),
                            android.provider.Settings.System.AIRPLANE_MODE_ON, 0) == 1;

                      // toggle airplane mode
                      android.provider.Settings.System.putInt(
                            getContentResolver(),
                            android.provider.Settings.System.AIRPLANE_MODE_ON, isEnabled ? 0 : 1);

                      // Post an intent to reload
                      Intent intent = new Intent(Intent.ACTION_AIRPLANE_MODE_CHANGED);
                      intent.putExtra("state", !isEnabled);
                      sendBroadcast(intent); */
                }

                /////////////////////////////////
                //////////About stella//////////
                ///////////////////////////////

                ////////////////////////////////
                ///////Stella Greetings////////
                //////////////////////////////



                /////////////////////////////////
                ///////////Launch Apps//////////
                ///////////////////////////////
                else if (Label.Text == "launch Twitter")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.twiiter.android");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch snapchat")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.twiiter.android");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch Instagram")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.instagram.android");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch Facebook")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.facebook.katana");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch Clash of Clans")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.supercell.clashofclans");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch my note")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.socialnmobile.dictapps.notepad.color.note");
                    StartActivity(intent);
                }
                else if (Label.Text == "launch YouTube")
                {
                    Intent intent = PackageManager.GetLaunchIntentForPackage("com.google.android.youtube");
                    StartActivity(intent);


                    /////////////////////////////////
                    //////////////Help//////////////
                    ///////////////////////////////

                    ////////////////////////////////
                    ///////Stella Wikipedia////////
                    //////////////////////////////
                }
                else if (Label.Text.StartsWith("Wikipedia"))
                {

                    string wikiword = Label.Text;
                    if (wikiword.Length > 0)
                    {
                        int i = wikiword.IndexOf(" ") + 1;
                        string wikiwordstr = wikiword.Substring(i);
                        //Response.Write(str);

                        //Search from Wikipedia
                        var webclient = new WebClient();
                        var pageSourceCode = webclient.DownloadString("http://en.wikipedia.org/w/api.php?format=xml&action=query&prop=extracts&titles=" + wikiwordstr + "&redirects=true");
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(pageSourceCode);

                        var fnode = doc.GetElementsByTagName("extract")[0];

                        try
                        {
                            string ss = fnode.InnerText;
                            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("\\<[^\\>]*\\>");
                            string.Format("Befoore:(0)", ss);
                            ss = regex.Replace(ss, string.Empty);
                            string wikiresult = String.Format(ss);

                            //restext.Text = (e.Result.Text.ToString());

                            //speakText(wikiresult);
                            //Spilt Sentences from Paragraph
                            //string input = "First sentence. Second sentence! Third sentence? Yes.";
                            string[] sentences = System.Text.RegularExpressions.Regex.Split(wikiresult, @"(?<=[\.!\?])\s+");

                            foreach (string sentence in sentences)
                            {
                                Console.WriteLine(sentence);
                                restext.Text = sentence;
                                //speakText(sentence);
                                textToSpeech.Speak(sentence, QueueMode.Flush, null);
                                break;
                            }


                        }
                        catch (Exception)
                        {

                            //restext.Text = "Internet not responding! Please Try Again!";
                            //textToSpeech.Speak("Internet not responding! Please Try Again!", QueueMode.Flush, null);
                        }
                    }//Separate word


                    /////////////////////////////////
                    /////////////Search/////////////
                    ///////////////////////////////

                }
                else if (Label.Text.StartsWith("search"))
                {
                    string searchword = Label.Text;
                    if (searchword.Length > 0)
                    {
                        int i = searchword.IndexOf(" ") + 1;
                        string wikiwordstr = searchword.Substring(i);
                        var uri = Android.Net.Uri.Parse("https://www.google.ae/search?site=&source=hp&q=" + searchword);
                        var intent = new Intent(Intent.ActionView, uri);
                        StartActivity(intent);
                    }
                    ////////////////////////////////
                    //////////Stella Jokes/////////
                    //////////////////////////////
                }
                else if (Label.Text == "tell me a joke")
                {
                    //string joke;
                    //WebClient w = new WebClient();
                    //bool worrking = true;
                    /*while (worrking)
                    {
                        try
                        {*/
                    //API http://api.icndb.com/jokes/random?firstName=John&amp;lastName=Doe
                    //API http://api.yomomma.info/
                    // string joke = (w.DownloadString("http://192.168.1.140/data2/Jokes%20API").Replace("\"", "").Replace("{", "").Replace("}", "").Replace(":", "").Replace("joke", ""));


                    string[] jokes = new string[] {
                        "The past, present and future walk into a bar. It was tense.",
                        "A woman gets on a bus with her baby. The bus driver says: Ugh thats the ugliest baby Ive ever seen! The woman walks to the rear of the bus and sits down fuming. She says to a man next to her: The driver just insulted me! The man says: You go up there and tell him off. Go on Ill hold your monkey for you.",
                        "I went to the docters the other day and said Have you got anything for wind? So he gave me a kite.",
                        "Your love life ha ha ha ha ha ha!"
                    };

                    restext.Text = jokes[new Random().Next(0, jokes.Length)];
                    textToSpeech.Speak(jokes[new Random().Next(0, jokes.Length)], QueueMode.Flush, null);
                    /*  }
                      catch { worrking = false; break; }

                  }*/

                    ////////////////////////////////
                    //////////////Q&A//////////////
                    //////////////////////////////

                } else if (Label.Text == "update") {


                    restext.Text = "Updating MySQL ...";

                    //
                    //Display Data From MySQL
                        string myConnection = "Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8";
                        MySqlConnection myConn = new MySqlConnection(myConnection);
                        MySqlCommand command = myConn.CreateCommand();
                        command.CommandText = "select * from Aiml_Server where user_id=1";
                        MySqlDataReader myReader;

                    try
                    {
                        myConn.Open();
                        myReader = command.ExecuteReader();

                        while (myReader.Read())
                        {
                            restext.Text = myReader[3].ToString();

                        }
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                    myConn.Close();


                    

                    /*
                    //Send To Readed
                    //MySQL Update Database
                    MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8");

                    try
                    {
                        if (con.State == ConnectionState.Closed)
                        {
                            string Readstatus = "Readed";
                            con.Open();
                            MySqlCommand cmd = new MySqlCommand("update Aiml_Server set is_readed=@Readed where user_id=1", con);
                            cmd.Parameters.AddWithValue("@Readed", Readstatus);
                            cmd.ExecuteNonQuery();
                            //txtSysLog.Text = "Your Speech is " + Speechqwerty + " Successfully inserted!";
                        }
                    }
                    catch (MySqlException ex)
                    {
                        //txtSysLog.Text = ex.ToString();
                    }
                    finally
                    {
                        con.Close();
                    }*/






                }
                else
                {
                    if (matches[0] != null)
                    {
                        //restext.Text = "Mic ON";
                        //Recognizer.StartListening(SpeechIntent);

                        //Sent Question To AIML Bot Server
                        //MySQL Update Database
                        MySqlConnection con = new MySqlConnection("Server=192.168.43.1;Port=3306;database=Stella_Ai;User Id=MYSQLUSERNAME;Password=PASS;charset=utf8");

                        try
                        {
                            if (con.State == ConnectionState.Closed)
                            {
                                con.Open();
                                MySqlCommand cmd = new MySqlCommand("update Aiml_Server set question=@question where user_id=1", con);
                                cmd.Parameters.AddWithValue("@question", Label.Text);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (MySqlException ex)
                        {
                            //txtSysLog.Text = ex.ToString();
                        }
                        finally
                        {
                            //CountDown();
                            SyncTimer = true;
                            RunSQLUpdateLoop();
                            bot_responsed = "";
                            con.Close();
                        }//End First Rule
                        



                    }
                    //Recognizer.StartListening(SpeechIntent);
                }
                    //End Responses
                }
            }

        public void OnReadyForSpeech(Bundle @params)
        {
            Log.Debug(Tag, "OnReadyForSpeech");
        }

        public void OnBeginningOfSpeech()
        {
            Log.Debug(Tag, "OnBeginningOfSpeech");
        }

        public void OnEndOfSpeech()
        {
            Log.Debug(Tag, "OnEndOfSpeech");
        }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            Log.Debug("OnError", error.ToString());
        }

        public void OnBufferReceived(byte[] buffer) { }

        public void OnEvent(int eventType, Bundle @params) { }

        public void OnPartialResults(Bundle partialResults) { }

        public void OnRmsChanged(float rmsdB) { }


    }
}



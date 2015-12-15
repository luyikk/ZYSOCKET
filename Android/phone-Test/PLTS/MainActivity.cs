using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using P2PCLIENT;
using System.Text;
using System.Collections.Generic;

namespace PLTS
{
    [Activity(Label = "PLTS", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        public ClientInfo client { get; set; }
        public Handler MessHander { get; set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.button1);
            button.Click += button_Click;

            Button button2 = FindViewById<Button>(Resource.Id.button2);
            button2.Click += button2_Click;

            LogOut.Action += LogOut_Action;

            MessHander = new Handler(Msg =>
            {
                FindViewById<TextView>(Resource.Id.textView1).Append(Msg.Obj.ToString() + "\n");

            });


            client = new ClientInfo(GetString(Resource.String.ServerIP), int.Parse(GetString(Resource.String.ServerPort)), int.Parse(GetString(Resource.String.ServerRegPort)), int.Parse(GetString(Resource.String.MinPort)), int.Parse(GetString(Resource.String.MaxPort)), int.Parse(GetString(Resource.String.ResCount)), GetString(Resource.String.MAC));
            client.ConToServer();
            client.ClientDataIn += client_ClientDataIn;
            client.ClientConnToMe += client_ClientConnToMe;
            client.ClientDiscon += client_ClientDiscon;

          
        }

        void button2_Click(object sender, EventArgs e)
        {
            FindViewById<TextView>(Resource.Id.textView1).Text = "";
        }

        void client_ClientDiscon(ConClient client, string message)
        {
            SetViewText(client.Host + ":" + client.Port + "-" + client.Key + " ->" + message);
        }

        void client_ClientConnToMe(ConClient client)
        {
            SetViewText(client.Host + ":" + client.Port + "-" + client.Key + " 连接");
        }

        void client_ClientDataIn(string key,ConClient client, byte[] data)
        {
            SetViewText("Revc:"+ Encoding.UTF8.GetString(data));
        }

        void LogOut_Action(string message, ActionType type)
        {
           // this.SetViewText(type.ToString()+":"+message);

            if (type == ActionType.ServerConn)
            {
                this.SetViewText(type.ToString() + ":" + message);
            }
            else if (type == ActionType.ServerNotConn)
            {
                this.SetViewText(type.ToString() + ":" + message);
            }
            else if (type == ActionType.ServerDiscon)
            {
                this.SetViewText(type.ToString() + ":" + message);
            }

        }

        void button_Click(object sender, EventArgs e)
        {
            EditText textbox = FindViewById<EditText>(Resource.Id.editText1);
           
            string text = textbox.Text;

            RunCmd(text);

            textbox.Text = "";
        }

        public void RunCmd(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);

            List<string> userlist = client.GetAllUser();


            foreach (var item in userlist)
            {
                client.SendData(item, data);
            }
        }



        public void SetViewText(string text)
        {
            Message  obj=new Message();
            obj.Obj=text;
            MessHander.SendMessage(obj);
        }
    }
}


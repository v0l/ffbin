using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.IO;
using Microsoft.Win32;

namespace ffbin
{
    public static class Notification
    {
        //http://stackoverflow.com/questions/31885302/how-can-i-detect-if-my-app-is-running-on-windows-10
        public static bool IsWin10
        {
            get
            {
                var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                string productName = (string)reg.GetValue("ProductName");
                return productName.StartsWith("Windows 10");
            }
        }

        public static void Send(string msg)
        {
            if (IsWin10)
            {
                SendToast(msg);
            }
            else
            {
                //who cares about anything other than win10?
            }
        }

        private static void SendToast(string msg)
        {
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText02);

            XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode("FFBin"));
            stringElements[1].AppendChild(toastXml.CreateTextNode(msg));

            String imagePath = "file:///" + Environment.CurrentDirectory +  "\\ff.gif"; ;
            XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            ToastNotification toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("FFBIN").Show(toast);
        }
    }
}

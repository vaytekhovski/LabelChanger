
using eTagTech.SDK.Core;
using eTagTech.SDK.Core.Entity;
using eTagTech.SDK.Core.Enum;
using eTagTech.SDK.Core.Event;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace LabelChanger
{
    class Program
    {
        static string TAG_ID = "";
        static string SHOP_CODE = "05CF";
        static string STATION_ID = "12";

        static string itemName;
        static string itemPrice;


        static void Main(string[] args)
        {
            Server.Instance.StationEventHandler += Instance_StationEventHandler;
            Server.Instance.ResultEventHandler += Instance_ResultEventHandler;

            Server.Instance.Start();


            Console.WriteLine("\nIf station is ONLINE press any button\n");
            Console.ReadKey();


            string answ = "";
            do
            {
                ChangeValueOnTag();

                Console.WriteLine("Try again? Y/N");

                answ = Console.ReadLine();
            } while (answ.ToLower() == "y");


        }

        private static void ChangeValueOnTag()
        {
            Console.Write("Label ID: ");
            TAG_ID = Console.ReadLine();

            Console.Write("Item name: ");
            itemName = Console.ReadLine();

            Console.Write("Item price: ");
            itemPrice = Console.ReadLine();

            string desc = "test";

            var img = CreateBMP(itemName, itemPrice, desc, desc, desc);
            Send(img, STATION_ID, TAG_ID, SHOP_CODE);

        }


        private static Bitmap CreateBMP(string ItemName, string ItemPrice, string Desc1, string Desc2, string Desc3)
        {
            Bitmap image = new Bitmap(300, 300);
            var font = new Font("Arial Black", 24, FontStyle.Regular, GraphicsUnit.Pixel);
            var graphics = Graphics.FromImage(image);

            graphics.FillRectangle(Brushes.Red, 0, 0, 300, 35);

            graphics.DrawString(ItemName, font, Brushes.White, new Point(50, 0));

            font = new Font("Arial", 28, FontStyle.Regular, GraphicsUnit.Pixel);
            graphics.DrawString(ItemPrice, font, Brushes.Black, new Point(100, 34));

            font = new Font("Calibri", 13, FontStyle.Regular, GraphicsUnit.Pixel);
            graphics.DrawString(Desc1, font, Brushes.Black, new Point(0, 50));
            graphics.DrawString(Desc2, font, Brushes.Black, new Point(0, 65));
            graphics.DrawString(Desc3, font, Brushes.Black, new Point(0, 80));

            return image;

        }

        private static void Send(Bitmap image, string STATION_ID, string TAG_ID, string SHOP_CODE, string BARCODE = "0123456798")
        {
            TagEntity t0 = new TagEntity
            {
                R = false,                          // LED red turn off
                B = false,                          // LED blue turn off
                G = false,                           // LED green turn on
                Times = 0,                         // LED light flashing 50 times
                Before = false,                     // LED light flashing after screen refresh
                TagType = ESLType.ESL213,          // The tag type is ESL290R
                PageIndex = PageIndex.P0,           // Refresh the 1st page
                Pattern = Pattern.UpdateDisplay,    // Update data cache and refresh screen
                StationID = STATION_ID,                   // Station ID is 01
                Status = TagStatus.Unknow,          // Default tag status is Unknow
                TagID = TAG_ID,                 // Tag ID
                ServiceCode = new Random(DateTime.Now.Millisecond).Next(65536),   // Token, between 0~65535
                DataList = new List<DataEntity>     // Data List
                {
                    new ImageEntity
                    {
                        ImageType = ImageType.Image,
                        Data = image,
                        W = 1,
                        H = 1,
                        ID = 1,
                        Color = FontColor.Red
                    },

                    new BarcodeEntity
                    {
                        BarcodeType = BarcodeType.Code128,
                        Color = FontColor.Black,
                        Data = BARCODE,
                        Height = 5,
                        ID = 3,
                        InvertColor = false,
                        W = 70,
                        H = 70,
                    },
                }
            };

            Result r0 = Server.Instance.Send(SHOP_CODE, STATION_ID, t0, true, true);
            Console.WriteLine("\r\nDemo #0 Send Result:" + r0);
            Console.WriteLine("Status is " + t0.Status);
        }




        /// <summary>
        /// Instance of result event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Instance_ResultEventHandler(object sender, ResultEventArgs e)
        {
            Console.WriteLine("Shop Code:{0}, AP:{1}, Result Type:{2}, Count:{3}", e.ShopCode, e.StationID, e.ResultList, e.ResultList.Count);
            foreach (var item in e.ResultList)
            {
                Console.WriteLine(" >> Tag ID:{0}, Status:{1}, Temperature:{2}, Power:{3}, Signal:{4}, Key: {5},Token:{6}",
                    item.TagID, item.TagStatus, item.Temperature, item.PowerValue, item.Signal, item.ResultType, item.Token);
            }
        }

        /// <summary>
        /// Instance of station event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Instance_StationEventHandler(object sender, StationEventArgs e)
        {
            Console.WriteLine("Shop Code:{0} AP: {1} IP:{2} Online:{3}", e.Shop, e.ID, e.IP, e.Online);
        }
    }
}

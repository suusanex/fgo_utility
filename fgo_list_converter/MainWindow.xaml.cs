using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace fgo_list_converter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public enum LineItemName
        {
            No = 0,
            Rare,
            Name,
            Class,
            Sairin1,
            Sairin2,
            Sairin3,
            Sairin4,
        }

        public class SairinItem
        {
            public string Name;
            public int Count;
        }

        public class SairinOnceItems
        {
            public int QP;
            public List<SairinItem> Items = new List<SairinItem>();
        }

        public class SairinServant
        {
            public int ID;
            public string Name;


            public SairinOnceItems[] SairinItems = new SairinOnceItems[4];
        }


        private void StartConvert(object sender, RoutedEventArgs e)
        {
            Convert(@"Input.txt", @"Output.csv");
        }

        void Convert(string InputFilePath, string OutputFilePath)
        {

            //var InputLine = "|004|4|[[アルトリア・ペンドラゴン〔リリィ〕]]|剣|50,000QP&br()剣ピースx4|150,000QP&br()英雄の証x18&br()剣ピースx10|500,000QP&br()世界樹の種x5&br()竜の牙x20&br()剣モニュメントx4|1,500,000QP&br()世界樹の種x10&br()竜の逆鱗x4&br()剣モニュメントx10|";

            var svs = new List<SairinServant>();

            var InputLines = File.ReadAllLines(InputFilePath);
            foreach (var InputLine in InputLines)
            {                
                var sv = InputLineToSairinServant(InputLine);
                svs.Add(sv);
            }


            var OutputBuf = new StringBuilder();

            OutputBuf.AppendLine(string.Join(",", "ID", "キャラクター名", "再臨", "必要数", "アイテム名"));

            foreach(var sv in svs)
            {
                for (int i = 0; i < 4; i++)
                {
                    //QPは現時点で使わないので処理なし

                    foreach (var SairinItem in sv.SairinItems[i].Items)
                    {
                        OutputBuf.AppendLine(string.Join(",",
                            sv.ID.ToString(),
                            sv.Name,
                            i.ToString(),
                            SairinItem.Count.ToString(),
                            SairinItem.Name));
                    }
                }
            }

            File.WriteAllText(OutputFilePath, OutputBuf.ToString());
        }

        private SairinServant InputLineToSairinServant(string InputLine)
        {
            SairinServant sv;
            var LineItems = InputLine.Split(new char[]{'|'}, StringSplitOptions.RemoveEmptyEntries);

            sv = new SairinServant();

            sv.ID = int.Parse(LineItems[(int)LineItemName.No]);
            sv.Name = LineItems[(int)LineItemName.Name].TrimStart('[').TrimEnd(']');

            for (int i = 0; i < 4; i++)
            {

                sv.SairinItems[i] = SairinOnceStringToItems(LineItems[(int)LineItemName.Sairin1 + i]);
            }

            return sv;
        }

        SairinOnceItems SairinOnceStringToItems(string str)
        {
            SairinOnceItems result = new SairinOnceItems();

            var strs = str.Replace("&br()", "|").Split('|');

            foreach (var SairinItemStr in strs)
            {
                if (0 <= SairinItemStr.IndexOf("QP"))
                {
                    result.QP = int.Parse(SairinItemStr.Replace("QP", null).Replace(",", null));

                }
                else if (0 <= SairinItemStr.IndexOf("x"))
                {
                    var SairinItemSplit = SairinItemStr.Split('x');
                    if(SairinItemSplit.Length != 2)
                    {
                        throw new Exception("SairinItemStr ItemCount Invalid, " + SairinItemStr);
                    }
                    var item = new SairinItem();
                    item.Name = SairinItemSplit[0];
                    //なぜかたまに個数の末尾にPを付けているデータがあるため、数値限定処理を入れる
                    string CountStr = new string(SairinItemSplit[1].TakeWhile(c => char.IsNumber(c)).ToArray());
                    item.Count = int.Parse(CountStr);

                    result.Items.Add(item);
                }
                else
                {
                    var item = new SairinItem();
                    item.Name = SairinItemStr;
                    item.Count = 1;

                    result.Items.Add(item);
                }
            }

            return result;
        }

    }
}

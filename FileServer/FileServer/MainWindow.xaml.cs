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

namespace FileServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UITest();
        }

        private void FileList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10; // take into account vertical scrollbar
            var col1 = 0.70;
            var col2 = 0.30;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
        }

        public void UITest()
        {
            List<ExampleFile> list = new List<ExampleFile>() {
                new ExampleFile(){Name = "AssassinCreed.exe", Size = "27kb"},
                new ExampleFile(){Name = "GodOfWar.txt", Size = "89kb"},
                new ExampleFile(){Name = "Terraria.pdf", Size = "88kb"},
                new ExampleFile(){Name = "LeagueOfLegend.doc", Size = "6kb"},
                new ExampleFile(){Name = "ShadowOfTheTombRaider.exe", Size = "14kb"},
                new ExampleFile(){Name = "MonsterHunterWorld.docx", Size = "112kb"},
                new ExampleFile(){Name = "Minecraft.html", Size = "25kb"},
                new ExampleFile(){Name = "WorldOfWarcraft.css", Size = "97kb"},
                new ExampleFile(){Name = "HeartStone.txt", Size = "16kb"},
                new ExampleFile(){Name = "GTA5.docx", Size = "21kb"},
                new ExampleFile(){Name = "DeadOrAlive.docx", Size = "44kb"},
                new ExampleFile(){Name = "SkyGarden.mp3", Size = "61kb"},
                new ExampleFile(){Name = "Gunny.dll", Size = "74kb"},
                new ExampleFile(){Name = "FarCry.pdf", Size = "48kb"},
                new ExampleFile(){Name = "DevilMayCry.xaml", Size = "52kb"},
                new ExampleFile(){Name = "ResidentEvil.cs", Size = "8kb"},
                new ExampleFile(){Name = "LifeIsStrange.c", Size = "11kb"},

            };
            FileList.ItemsSource = list;
        }
    }

    class ExampleFile
    {
        public string Name { get; set; }
        public string Size { get; set; }
    }
}

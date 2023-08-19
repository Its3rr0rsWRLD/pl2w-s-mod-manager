using SimpleJSON;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace pl2w_s_mod_manager
{
    public partial class MainForm : Form
    {
        public string gorillaTagPath = @"C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag";
        string githubModLink = "https://raw.githubusercontent.com/pl2w/pl2w-s-mod-manager/master/mods.json";
        GorillaMod[] allMods;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = gorillaTagPath;
            listView1.CheckBoxes = true;
            LoadMods();
        }

        void LoadMods()
        {
            WebClient client = new WebClient();
            // this breaks the form randomly
            string modsJson = client.DownloadString("https://raw.githubusercontent.com/pl2w/pl2w-s-mod-manager/master/mods.json");

            var allMods = JSON.Parse(modsJson).AsArray;
            List<GorillaMod> mods = new List<GorillaMod>();
            for (int i = 0; i < allMods.Count; i++)
            {
                JSONNode current = allMods[i];
                GorillaMod mod = new GorillaMod()
                {
                    modName = current["modName"],
                    modAuthor = current["modAuthor"],
                    modLink = current["modLink"],
                    isZipped = current["isZipped"]
                };
                mods.Add(mod);
            }
            foreach (GorillaMod mod in mods)
            {
                ListViewItem item = new ListViewItem(mod.modName);
                item.Tag = mod;
                item.SubItems.Add(mod.modAuthor);
                listView1.Items.Add(item);
            }
            client.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.FileName = "Gorilla Tag.exe";
                fileDialog.Filter = "Exe Files (.exe)|*.exe|All Files (*.*)|*.*";
                fileDialog.FilterIndex = 1;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    string path = fileDialog.FileName;
                    if (Path.GetFileName(path).Equals("Gorilla Tag.exe"))
                    {
                        gorillaTagPath = Path.GetDirectoryName(path);
                        textBox1.Text = gorillaTagPath;
                    }
                    else
                    {
                        MessageBox.Show("That's not Gorilla Tag.exe! please try again!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            using(var client = new WebClient())
            {
                foreach (ListViewItem item in listView1.CheckedItems)
                {
                    GorillaMod mod = (GorillaMod)item.Tag;
                    if (!mod.isZipped) InstallDll(mod, client);
                    else InstallZip(mod, client);
                }
            }
            button2.Enabled = true;
        }

        private void InstallZip(GorillaMod mod, WebClient client)
        {
            if (mod.modName == "BepInEx")
            {
                if (!File.Exists(Path.Combine(gorillaTagPath, "winhttp.dll")))
                {
                    client.DownloadFile(mod.modLink, "BepInEx.zip");
                    MessageBox.Show(gorillaTagPath);
                    ZipFile.ExtractToDirectory("BepInEx.zip", gorillaTagPath);
                    File.Delete("BepInEx.zip");
                }
                return;
            }

            client.DownloadFile(mod.modLink, mod.modName + ".zip");
            ZipFile.ExtractToDirectory(mod.modName + ".zip", Path.Combine(gorillaTagPath, "BepInEx", "Plugins"));
            File.Delete(mod.modName + ".zip");
        }

        private void InstallDll(GorillaMod mod, WebClient client)
        {
            string cleansedFileName = CleanFileName(mod.modName) + ".dll";
            if (File.Exists(Path.Combine(gorillaTagPath, cleansedFileName)))
            {
                button2.Enabled = true;
                return;
            }
            byte[] file = client.DownloadData(mod.modLink);
            File.WriteAllBytes(Path.Combine(gorillaTagPath, "BepInEx", "Plugins", cleansedFileName), file);
        }

        private string CleanFileName(string modName)
        {
            return new string(modName.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
    }

    [System.Serializable]
    public class GorillaMod
    {
        public string modLink { get; set; }

        public string modName { get; set; }
        public string modAuthor { get; set; }
        public bool isZipped { get; set; }  
    }
}

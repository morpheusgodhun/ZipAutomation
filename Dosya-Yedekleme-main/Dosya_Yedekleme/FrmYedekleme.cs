using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Dosya_Yedekleme
{
    public partial class FrmYedekleme : Form
    {
        private static Icon ExtractFromPath(string path)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(
                path,
                0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
                SHGFI_ICON | SHGFI_LARGEICON);
            return System.Drawing.Icon.FromHandle(shinfo.hIcon);
        }

        //Struct used by SHGetFileInfo function
        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        [DllImport("shell32.dll")]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x000000001;
        public FrmYedekleme()
        {
            InitializeComponent();
            lswYedeklenecekler.View = View.List;
            lswYedeklenecekler.GridLines = true;
        }

        private void FrmYedekleme_Load(object sender, EventArgs e)
        {
            textBoxKaynak.Enabled = false;
            textBoxHedef.Enabled = false;
            for (int i = 1; i <= 23; i++)
            {
                if (i <= 9)
                {
                    comboBoxSaat.Items.Add("0" + i);
                }
                else
                {
                    comboBoxSaat.Items.Add(i);
                }
            }
            comboBoxSaat.Items.Add("00");
            for (int i = 1; i <= 59; i++)
            {
                if (i <= 9)
                {
                    comboBoxDakika.Items.Add("0" + i);
                }
                else
                {
                    comboBoxDakika.Items.Add(i);
                }
            }
            comboBoxDakika.Items.Add("00");
            /*System.Windows.Forms.ImageList myImageList1 = new ImageList();
            myImageList1.ImageSize = new Size(64, 64);
            myImageList1.Images.Add(Image.FromFile(@"C:\Users\zeynep\Desktop\txtt.png"));
            lswYedeklenecekler.LargeImageList = myImageList1;
            lswYedeklenecekler.View = View.SmallIcon;
            //Set the view to show details.
            lswYedeklenecekler.View = View.Details;
            //Allow the user to edit item text.
            lswYedeklenecekler.LabelEdit = true;
            // Allow the user to rearrange columns.
            lswYedeklenecekler.AllowColumnReorder = true;
            // Display check boxes.
            lswYedeklenecekler.CheckBoxes = true;
            // Select the item and subitems when selection is made.
            lswYedeklenecekler.FullRowSelect = true;
            // Display grid lines.
            lswYedeklenecekler.GridLines = true;
            // Sort the items in the list in ascending order.
            lswYedeklenecekler.Sorting = SortOrder.Ascending;
            lswYedeklenecekler.Columns.Add("Dosya Adı", -2, HorizontalAlignment.Left);
            lswYedeklenecekler.Columns.Add("Dosya Boyutu", -2, HorizontalAlignment.Left);
            lswYedeklenecekler.SmallImageList = new ImageList();*/
        }

        private void btnKaynak_Click(object sender, EventArgs e)
        {
            // Kopyalanacak klasör seçilir. Dosya türlerine göre imageIndex kullanılarak ilgili icon eklenir.
            openFileDialog1.Title = "Yedeklenecek klasörü seçiniz.";
            openFileDialog1.FileName = "";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                kaynakKlasor = folderBrowserDialog1.SelectedPath.ToString();
                textBoxKaynak.Text = kaynakKlasor;
                var dosyalar = new DirectoryInfo(kaynakKlasor).GetFiles("*.*");
                foreach (FileInfo dosya in dosyalar)
                {                    
                    Icon ico = Icon.ExtractAssociatedIcon(kaynakKlasor + "\\" + dosya.Name);
                    Image img = ico.ToBitmap();
                    lswYedeklenecekler.SmallImageList.Images.Add(img);
                    ListViewItem item1 = new ListViewItem(dosya.Name, dosya.Length.ToString());
           
                    if (dosya.Name.EndsWith(".zip"))
                    {
                        lswYedeklenecekler.Items.Add(dosya.Name, imageIndex:0);
                    }
                    else if (dosya.Name.EndsWith(".txt"))
                    {
                        lswYedeklenecekler.Items.Add(dosya.Name, imageIndex:1);
                    }                                      
                }
                DirectoryInfo DrInf = new DirectoryInfo(kaynakKlasor);
                DirectoryInfo[] DrInfLst = DrInf.GetDirectories();
                foreach (DirectoryInfo klasor in DrInfLst)
                {
                    if (Directory.Exists(klasor.FullName))
                    {
                        Icon ico = ExtractFromPath(klasor.FullName);
                        Image img = ico.ToBitmap();
                        lswYedeklenecekler.SmallImageList.Images.Add(img);
                        ListViewItem item1 = new ListViewItem(klasor.Name, klasor.ToString());
                        lswYedeklenecekler.Items.Add(klasor.Name, imageIndex: 2);
                    }
                }
            }
            else
            {
                MessageBox.Show("Klasör Seçmediniz.");
            }
        }

        private void btnHedef_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                hedefKlasor = folderBrowserDialog1.SelectedPath.ToString();
                textBoxHedef.Text = hedefKlasor;
            }
            else
            {
                MessageBox.Show("Hedef klasörü seçmediniz.");
            }
        }

        private void btnYedekle_Click(object sender, EventArgs e)
        {
            // lswYedeklenecekler dönülür. Dosya adında "." varsa File.Copy fonk çalıştırıp dosyayı kopyalar, yoksa Kopyala fonk çalıştırıp klasörü kopyalar.
            if (hedefKlasor != "" && kaynakKlasor != "")
            {
                int sayac = 0;
                foreach (ListViewItem item in lswYedeklenecekler.Items)
                {
                    var dosyaAdi = item.Text.Substring(0, item.Text.LastIndexOf(".") + 1);

                    if (!File.Exists(hedefKlasor + "\\" + item.Text) && dosyaAdi != "")
                    {
                        File.Copy(kaynakKlasor + "\\" + item.Text, hedefKlasor + "\\" + item.Text);
                        sayac++;
                    }

                    else if (dosyaAdi == "")
                    {
                        Kopyala(textBoxKaynak.Text, textBoxHedef.Text, true);
                        sayac++;
                    }
                }
                MessageBox.Show($"{sayac} Adet Dosya Yedekleme İşlemi Başarılı");
            }
        }

        private void Zip_SaveProgress(object sender, Ionic.Zip.SaveProgressEventArgs e)
        {
            if (e.EventType == Ionic.Zip.ZipProgressEventType.Saving_BeforeWriteEntry)
            {
                progressBar1.Invoke(new MethodInvoker(delegate
                {
                    progressBar1.Maximum = e.EntriesTotal;
                    progressBar1.Value = e.EntriesSaved + 1;
                    progressBar1.Update();
                }));
            }
        }

        string kaynakKlasor = "", hedefKlasor = "";

        private void btnEkle_Click(object sender, EventArgs e)
        {
            // lswYedeklenecekler dönülür. Eğer silinecek dosya lswYedeklenecekler içerisinde varsa dosya silinir.
            // Eğer silinecek dosya zip veya txt ile bitiyorsa ve ilk harfi * ise, dosya türü ile eşleşen tüm dosyalar silinir.
            string metin = textBoxSil.Text;
            var dosyalar = new DirectoryInfo(kaynakKlasor).GetFiles("*.*");
            DirectoryInfo DrInf = new DirectoryInfo(kaynakKlasor);
            DirectoryInfo[] DrInfLst = DrInf.GetDirectories();
            if (metin != string.Empty)
            {
                foreach (ListViewItem item in lswYedeklenecekler.Items)
                {
                    string ilkHarf = metin.Substring(0, 1);  
                    string sonMetin = item.Text.Substring(item.Text.Length - 3, 3);
                    if (metin == item.Text)
                    {
                        listBoxSilinenler.Items.Add(metin);
                        textBoxSil.Text = "";
                        lblSilinen.Text = metin;   
                        lswYedeklenecekler.Items.Remove(item);                       
                    }
                    else if (metin.Substring(metin.Length - 3, 3) == sonMetin && ilkHarf == "*")
                    {
                        listBoxSilinenler.Items.Add(item.Text);
                        lswYedeklenecekler.Items.Remove(item);
                    }
                }
            }
        }

        private void btnZiple_Click(object sender, EventArgs e)
        {
            // Yedekleme miktarı belirtildiyse, yedeklenen dosyalar belirtilen miktar kadar ziplenir.
            var ziplenecekKlasor = textBoxHedef.Text;
            var ziplenecekKlasor2 = textBoxHedef.Text + ("\\");
            var ziplemeKonumu = ziplenecekKlasor2.Substring(0, ziplenecekKlasor2.LastIndexOf("\\") + 1);
            var tarihBilgi = DateTime.Now.ToShortDateString().Replace(".", "-") + " " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString();
            var klasorAdi = tarihBilgi + " " + ziplenecekKlasor.Substring(ziplenecekKlasor.LastIndexOf("\\") + 1);
            var dosyalar = new DirectoryInfo(hedefKlasor).GetFiles("*.*");
            if (textBoxYedekMiktar.Text != "")
            {
                int miktar = int.Parse(textBoxYedekMiktar.Text);
                int sayac = 0;
                if (comboBoxSaat.Text == "" && comboBoxDakika.Text == "" && textBoxYedekMiktar.Text != "")
                {
                    for (var i = 1; i <= miktar; i++)
                    {
                        sayac++;
                        KlasorZipleme(ziplenecekKlasor, ziplemeKonumu + klasorAdi + sayac + ".zip", klasorAdi, tarihBilgi);
                    }
                }
                else
                {
                    tmrZamanKontrol.Start();
                }               
            }                  
            else if(comboBoxSaat.Text != "" && comboBoxDakika.Text != "")
            {  
                tmrZamanKontrol.Start();
            }
            else
            {
                KlasorZipleme(ziplenecekKlasor, ziplemeKonumu + klasorAdi + ".zip", klasorAdi, tarihBilgi);
            }
        }

        private void KlasorZipleme(string kaynakKlasor, string ziplemeKonumu, string klasorAdi, string tarihBilgi)
        {
            if (string.IsNullOrEmpty(textBoxHedef.Text))
            {
                MessageBox.Show("Please select your folder", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBoxHedef.Focus();
                return;
            }
            string path = textBoxHedef.Text;
            Thread thread = new Thread(t =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile())
                {
                    zip.AddDirectory(kaynakKlasor);
                    zip.SaveProgress += Zip_SaveProgress;
                    zip.Save(ziplemeKonumu);
                }

                var dosyalar = new DirectoryInfo(hedefKlasor).GetFiles("*.*");
                foreach (FileInfo dosya in dosyalar)
                {
                    var varolanZip = dosya.Name.Substring(1, dosya.Name.IndexOf("-") + 1);
                    if (dosya.Name != klasorAdi + ".zip" && varolanZip == "")
                    {
                        dosya.Delete();
                    }
                }

                DirectoryInfo DrInf = new DirectoryInfo(kaynakKlasor);
                DirectoryInfo[] DrInfLst = DrInf.GetDirectories();
                foreach (DirectoryInfo klasor in DrInfLst)
                {
                    if (klasor.Name != klasorAdi + ".zip")
                    {
                        klasor.Delete();
                    }
                }

                // Dosya sayısı 5 olursa ilk oluşturulan dosyayı siler.
                string[] fileList = Directory.GetFiles(hedefKlasor, "*.*", SearchOption.AllDirectories);
                string minDate = fileList.Min();
                var silinecekDosya = minDate.Substring(minDate.LastIndexOf("\\") + 1);
                if (fileList.Count() > 4)
                {
                    foreach (FileInfo dosyaa in dosyalar)
                    {
                        if (silinecekDosya == dosyaa.Name)
                        {
                            dosyaa.Delete();
                        }
                    }
                }
            })

            { IsBackground = true };
            thread.Start();
        }

        private void tmrZamanKontrol_Tick(object sender, EventArgs e)
        {            
            int sayac = 0;
            var saat = DateTime.Now.Hour.ToString();
            var dakika = DateTime.Now.Minute.ToString();
            var ziplenecekKlasor = textBoxHedef.Text;
            var ziplenecekKlasor2 = textBoxHedef.Text + ("\\");
            var ziplemeKonumu = ziplenecekKlasor2.Substring(0, ziplenecekKlasor2.LastIndexOf("\\") + 1);
            var tarihBilgi = DateTime.Now.ToShortDateString().Replace(".", "-") + " " + DateTime.Now.Hour.ToString() + "." + DateTime.Now.Minute.ToString();
            var klasorAdi = tarihBilgi + " " + ziplenecekKlasor.Substring(ziplenecekKlasor.LastIndexOf("\\") + 1);

            if(saat.Length == 1)
            {
                saat = "0" + saat;
            }

            if(dakika.Length == 1)
            {
                dakika = "0" + dakika;
            }

            if (saat == comboBoxSaat.Text && dakika == comboBoxDakika.Text && textBoxYedekMiktar.Text != "")
            {
                int miktar = int.Parse(textBoxYedekMiktar.Text);
                for (var i = 1; i <= miktar; i++)
                {
                    sayac++;
                    KlasorZipleme(ziplenecekKlasor, ziplemeKonumu + klasorAdi + sayac + ".zip", klasorAdi, tarihBilgi);
                }
            }
            else
            {
                KlasorZipleme(ziplenecekKlasor, ziplemeKonumu + klasorAdi + ".zip", klasorAdi, tarihBilgi);
            }
        }

        private void geriAlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string geriAl = lblSilinen.Text;
            listBoxSilinenler.Items.Remove(geriAl);
            lblSilinen.Text = string.Empty;
            if(geriAl.EndsWith(".zip"))
            {
                lswYedeklenecekler.Items.Add(geriAl, imageIndex:0);
            }
            else if (geriAl.EndsWith(".txt"))
            {
                lswYedeklenecekler.Items.Add(geriAl, imageIndex: 1);
            }
            else
            {
                lswYedeklenecekler.Items.Add(geriAl, imageIndex: 2);
            }
        }

        private void lswYedeklenecekler_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lswYedeklenecekler.SelectedItems)
            {
                textBoxSil.Text = item.Text;
            }
        }

        protected void Kopyala(string Prmt1, string prmt2, bool prmt3)
        {
            DirectoryInfo DrInf = new DirectoryInfo(Prmt1);
            DirectoryInfo[] DrInfLst = DrInf.GetDirectories();
            if (!Directory.Exists(prmt2))
            {
                Directory.CreateDirectory(prmt2);
            }

            string path1 = "";

            if (true)
            {
                foreach (DirectoryInfo klasor in DrInfLst)
                {
                    path1 = Path.Combine(prmt2, klasor.Name);
                    Kopyala(klasor.FullName, path1, true);
                }
            }
        }
    }
 }

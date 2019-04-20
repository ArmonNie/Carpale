using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

namespace Carpale
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window,Interface1
    {
        private Bitmap m_Bitmap;
        private Bitmap c_Bitmap; //车牌图像
        private BitmapSource bs;
        int[] gray = new int[256]; //灰度化
        int[] rr = new int[256];
        int[] gg = new int[256];
        int[] bb = new int[256];
        private float count;
        int tt = 0; 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_open_Click(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;
            switch(button.Name)
            {
                case "btn_open":
                    getImage();
                    img_show.Source = bs;           
                    break;
                case "btn_save":
                    saveImage();
                    MessageBox.Show(" " + button.Name);
                    break;
                case "btn_imggray"://图片灰度化     
                    Stopwatch sp = new Stopwatch();
                    sp.Start();
                    imageToGray(m_Bitmap);//54ms
                    //imageToGray_memory(m_Bitmap);//54ms
                    //imageToGray_pixel();//3097ms
                    sp.Stop();
                    MessageBox.Show("" + sp.ElapsedMilliseconds);
                    break;
                case "btn_imggraybalance"://灰度均衡化
                    break;
                case "btn_imgmiddle"://中值滤波
                    break;

            }
        }


        //读取文件
        public void getImage()
        {    
            String file_name = "";
            String str_path = "Jpeg文件(*.jpg)|*.jpg|Bitmap文件(*.bmp)|*.bmp|所有合适文件(*.bmp/*jpg)|*.bmp/*.jpg";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = str_path;
            openFileDialog.RestoreDirectory = true;
            if(openFileDialog.ShowDialog() == true)
            {
                file_name = openFileDialog.FileName;
                MessageBox.Show("" + file_name);
                m_Bitmap = (Bitmap)Bitmap.FromFile(file_name,false);        
                //this.m_Bitmap1 = m_Bitmap.Clone(new System.Drawing.Rectangle(0,0,m_Bitmap.Width,m_Bitmap.Height));
                IntPtr ip = m_Bitmap.GetHbitmap();
                BitmapSource bis = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,IntPtr.Zero,Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                bs = bis;
            }
        
        }

        //保存文件
        public void saveImage()
        {
            SaveFileDialog saveFiledialog = new SaveFileDialog();
            saveFiledialog.Filter = "Jpeg文件(*.jpg)|*.jpg|Bitmap文件(*.bmp)|*.bmp|所有合适文件(*.bmp/*jpg)|*.bmp/*.jpg";
            saveFiledialog.FilterIndex = 1;
            saveFiledialog.RestoreDirectory = true;
            if(saveFiledialog.ShowDialog() == true)
            {
                c_Bitmap = m_Bitmap;
                IntPtr ip = c_Bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                c_Bitmap.Save(saveFiledialog.FileName);
            }
        }

        //图片灰度化,老师
        public void imageToGray(Bitmap bitmap)
        {
            if (bitmap != null)
            {

                for (int i = 0; i < 256; i++)
                {
                    gray[i] = 0;
                }
                for (int i = 0; i < 256; i++)
                {
                    rr[i] = 0;

                }
                for (int i = 0; i < 256; i++)
                {
                    gg[i] = 0;
                }
                for (int i = 0; i < 256; i++)
                {
                    bb[i] = 0;
                }
                BitmapData bmData = bitmap.LockBits
                (new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;
                IntPtr Scan0 = bmData.Scan0;
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = bitmap.Width;
                    int nHeight = bitmap.Height;
                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];
                            tt = p[0] = p[1] = p[2] = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);
                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++;
                            p += 3;
                        }
                        p += nOffset;
                    }
                }
                bitmap.UnlockBits(bmData);
                count = bitmap.Width * bitmap.Height;
                IntPtr ip = bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                img_show.Source = bitmapSource;
            }
        }

        //图片灰度化，简化1
        public void imageToGray_memory(Bitmap bitmap)
        {
            //内存法
            if (m_Bitmap != null)
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height);
                BitmapData bmData = m_Bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, m_Bitmap.PixelFormat);
                IntPtr ptr = bmData.Scan0;
                int bytes = bmData.Stride * bmData.Height;
                byte[] rgbValue = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValue, 0, bytes);
                double colorTemp = 0;
                for (int i = 0; i < bmData.Height; i++)
                {
                    for (int j = 0; j < bmData.Width * 3; j += 3)
                    {
                        colorTemp = rgbValue[i * bmData.Stride + j + 2] * 0.299 + rgbValue[i * bmData.Stride + j + 1] * 0.587
                            + rgbValue[i * bmData.Stride + j] * 0.114;
                        rgbValue[i * bmData.Stride + j] = rgbValue[i * bmData.Stride + j + 1]
                            = rgbValue[i * bmData.Stride + j + 2] = (byte)colorTemp;
                    }
                }
                System.Runtime.InteropServices.Marshal.Copy(rgbValue, 0, ptr, bytes);
                m_Bitmap.UnlockBits(bmData);
                count = bitmap.Width * bitmap.Height;
                IntPtr ip = bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                img_show.Source = bitmapSource;
            }
        }


        //图片灰度化，简化2
        public void imageToGray_pixel()
        {
            //提取像素法
            if (m_Bitmap != null)
            {
                System.Drawing.Color curcolor;
                int ret;

                for (int i = 0; i < m_Bitmap.Width; i++)
                {
                    for (int j = 0; j < m_Bitmap.Height; j++)
                    {
                        curcolor = m_Bitmap.GetPixel(i, j);
                        ret = (int)(curcolor.R * 0.299 + curcolor.G * 0.587 + curcolor.B * 0.114);

                        m_Bitmap.SetPixel(i, j, System.Drawing.Color.FromArgb(ret, ret, ret));
                    }
                }
                IntPtr ip = m_Bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                    IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                img_show.Source = bitmapSource;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Ots.draw
{
    public class Canvas : Tile2Image, IDisposable
    {
        public String Filename { get; set; }
        public String PngFile { get; set; }
        public String DrawFile { get; set; }

        private Image _image;
        private Bitmap _bitmap;
        public Graphics Graphics { get; protected set; }

        public Size MaxSize
        {
            get { return _bitmap != null ? _image.Size : new Size(0,0); }
        }

        public bool IsOpen { get { return _image != null && Graphics != null; } }

        public Canvas(String filename)
        {
            Filename = filename;
            PngFile = Path.ChangeExtension(filename, ".png");
            DrawFile = Path.ChangeExtension(filename, ".png");
        }

        public Canvas(String filename, String drawFilename, String drawExtension)
        {
            Filename = filename;
            PngFile = Path.ChangeExtension(filename, ".png");
            DrawFile = (string.IsNullOrEmpty(drawFilename)
                ? Path.ChangeExtension(filename, drawExtension)
                : drawFilename);
            Open();
        }

        public bool Open()
        {
            if (IsOpen) return IsOpen;

            try
            {
                var fs = new FileStream(PngFile, FileMode.Open, FileAccess.Read);
                _image = Image.FromStream(fs);
                fs.Close();
                _bitmap = new Bitmap(_image);
                Graphics = Graphics.FromImage(_bitmap);
                Console.Out.WriteLine("- Read:" + Path.GetFileName(PngFile) + " = OK.");
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine("- Read:" + Path.GetFileName(PngFile) + " = " + ex.Message);
            }
            return IsOpen;
        }

        public static Canvas Clone(Canvas source, String filename, RectangleF rect)
        {
            var canvas = new Canvas(filename);
            try
            {
                canvas._bitmap = source._bitmap.Clone(rect, source._bitmap.PixelFormat);
                Console.Out.WriteLine("- Clone:" + Path.GetFileName(filename) + " = OK.");
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("- Clone:" + Path.GetFileName(filename) + " = " + ex.Message);
            }
            return canvas;
        }

        public bool Save()
        {
            if (_bitmap != null)
            {
                try
                {
                    _bitmap.Save(DrawFile, GetFormat());
                    Console.Out.WriteLine("- Write:" + Path.GetFileName(DrawFile) + " = OK.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("- Write:" + Path.GetFileName(DrawFile) + " = " + ex.Message);
                }
            }
            return false;
        }

        private ImageFormat GetFormat()
        {
            var ext = Path.GetExtension(DrawFile.ToLowerInvariant());
            if (String.Compare(ext, ".jpg") == 0) return ImageFormat.Jpeg;
            if (String.Compare(ext, ".jpeg") == 0) return ImageFormat.Jpeg;
            if (String.Compare(ext, ".bmp") == 0) return ImageFormat.Bmp;
            if (String.Compare(ext, ".gif") == 0) return ImageFormat.Gif;
            if (String.Compare(ext, ".png") == 0) return ImageFormat.Png;
            return _image.RawFormat;
        }

        ~Canvas()
        {
            Dispose(false);
        }
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed == false)
            {
                _disposed = true;
                if (_image != null)
                {
                    _image.Dispose();
                    _image = null;
                }
                if (_bitmap != null)
                {
                    _bitmap.Dispose();
                    _bitmap = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

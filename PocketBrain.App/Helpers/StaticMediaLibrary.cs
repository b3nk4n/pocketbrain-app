﻿using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using PhoneKit.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PocketBrain.App.Helpers
{
    /// <summary>
    /// Global access to the media library.
    /// </summary>
    public static class StaticMediaLibrary
    {
        private static MediaLibrary _mediaLibrary;

        /// <summary>
        /// Gets the media library
        /// </summary>
        public static MediaLibrary Instance
        {
            get
            {
                if (_mediaLibrary == null || _mediaLibrary.IsDisposed)
                    _mediaLibrary = new MediaLibrary();
                return _mediaLibrary;
            }
        }

        /// <summary>
        /// Updates the library and its items by reinstantiating the object.
        /// Remark: New photos are available not before the MediaLibrary is reinitialized.
        /// </summary>
        public static void Update()
        {
            _mediaLibrary = new MediaLibrary();
        }

        public static RobustPicture GetImageFromToken(string token)
        {
            Picture image = null;

            try
            {
                image = StaticMediaLibrary.Instance.GetPictureFromToken(token);
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            return (image == null) ? null : new RobustPicture(image);
        }

        public static RobustPicture GetImageFromFileName(string fileName)
        {
            try
            {
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    if (pic.Name == fileName)
                    {
                        return new RobustPicture(pic);
                    }
                }

                // second try without extension.
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    var picNameWithoutExtension = Path.GetFileNameWithoutExtension(pic.GetPath());

                    if (picNameWithoutExtension == fileNameWithoutExtension)
                    {
                        return new RobustPicture(pic);
                    }
                }

                // third try, because sometime the file extenstion was not applied, but is very unrealiable, but should hit a file.
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    var nameWithoutCounter = RemoveImageCopyCounter(fileName);
                    if (pic.Name.Contains(fileName) || fileName.Contains(pic.Name) ||
                        pic.Name.Contains(nameWithoutCounter) || pic.Name.Contains(nameWithoutCounter))
                    {
                        return new RobustPicture(pic);
                    }
                }
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }
            return null;
        }

        public static int GetMediaLibIndexFromFileName(string fileName)
        {
            try
            {
                int i = 0;
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    if (pic.Name == fileName)
                    {
                        return i;
                    }
                    i++;
                }

                // second trywithout extension.
                i = 0;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    var picFullNameWithoutExtension = Path.GetFileNameWithoutExtension(pic.GetPath());

                    if (picFullNameWithoutExtension == fileNameWithoutExtension)
                    {
                        return i;
                    }
                    i++;
                }

                // second try, because sometime the file extenstion was not applied.
                i = 0;
                foreach (var pic in StaticMediaLibrary.Instance.Pictures)
                {
                    var nameWithoutCounter = RemoveImageCopyCounter(fileName);
                    if (pic.Name.Contains(fileName) || fileName.Contains(pic.Name) ||
                        pic.Name.Contains(nameWithoutCounter) || pic.Name.Contains(nameWithoutCounter))
                    {
                        return i;
                    }
                    i++;
                }
            }
            catch (InvalidOperationException ioex)
            {
                Debug.WriteLine("Could not retrieve photo from library with error: " + ioex.Message);
            }

            return -1;
        }

        private static string RemoveImageCopyCounter(string fileName)
        {
            if (fileName.Length <= 3)
                return fileName;

            var bracketsStart = fileName.IndexOf('(');
            var bracketsEnd = fileName.IndexOf(')');

            if (bracketsStart != -1 && bracketsEnd != -1 && bracketsStart < bracketsEnd)
            {
                try
                {
                    return fileName.Substring(0, bracketsStart);
                }
                catch (Exception) { }
            }

            return fileName;
        }
    }
}

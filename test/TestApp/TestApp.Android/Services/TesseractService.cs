using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tesseract.Xamarin.Droid;
using TesseractDotNet;
using TestApp.Droid.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Rectangle = TesseractDotNet.Rectangle;
using Result = TesseractDotNet.Result;

[assembly: Dependency(typeof(TesseractService))]

namespace TestApp.Droid.Services
{
    public class TesseractService : ITesseractApi
    {
        private readonly TesseractApi _ocr;

        public TesseractService()
        {
            _ocr = new TesseractApi(Platform.AppContext, AssetsDeployment.OncePerVersion);
        }

        public event EventHandler<ProgressEventArgs> Progress
        {
            add
            {
                ((ITesseractApi)_ocr).Progress += value;
            }

            remove
            {
                ((ITesseractApi)_ocr).Progress -= value;
            }
        }

        public bool Initialized => ((ITesseractApi)_ocr).Initialized;
        public bool IsInitialized => _ocr.Initialized;

        public string Text => _ocr.Text;

        public void Clear()
        {
            ((ITesseractApi)_ocr).Clear();
        }

        public void Dispose()
        {
            ((IDisposable)_ocr).Dispose();
        }

        public Task<bool> Init(string lang) => _ocr.Init(lang);

        public Task<bool> Init(string lang, OcrEngineMode? mode = null)
        {
            return ((ITesseractApi)_ocr).Init(lang, mode);
        }

        public void MaximumRecognitionTime(double value)
        {
            // Not supported
        }

        public IEnumerable<Result> Results()
        {
            var retVal = new List<Result>();
            var results = _ocr.Results(PageIteratorLevel.Word);
            foreach (var item in results)
            {
                retVal.Add(new Result()
                {
                    Box = new Rectangle()
                    {
                        X = item.Box.X,
                        Y = item.Box.Y,
                        Height = item.Box.Height,
                        Width = item.Box.Width
                    },
                    Text = item.Text
                });
            }
            return retVal;
        }

        public IEnumerable<Result> Results(PageIteratorLevel level)
        {
            return ((ITesseractApi)_ocr).Results(level);
        }

        public void SetBlacklist(string blacklist)
        {
            ((ITesseractApi)_ocr).SetBlacklist(blacklist);
        }

        public Task SetImage(MemoryStream stream) => _ocr.SetImage(stream);

        public Task<bool> SetImage(string path)
        {
            return ((ITesseractApi)_ocr).SetImage(path);
        }

        public Task<bool> SetImage(byte[] data)
        {
            return ((ITesseractApi)_ocr).SetImage(data);
        }

        public Task<bool> SetImage(Stream stream)
        {
            return ((ITesseractApi)_ocr).SetImage(stream);
        }

        public void SetPageSegmentationMode() => _ocr.SetPageSegmentationMode(PageSegmentationMode.Auto);

        public void SetPageSegmentationMode(PageSegmentationMode mode)
        {
            ((ITesseractApi)_ocr).SetPageSegmentationMode(mode);
        }

        public void SetRectangle(Rectangle? rect)
        {
            ((ITesseractApi)_ocr).SetRectangle(rect);
        }

        public void SetVariable(string key, string value)
        {
            ((ITesseractApi)_ocr).SetVariable(key, value);
        }

        public void SetWhitelist(string value) => _ocr.SetWhitelist(value);
    }
}
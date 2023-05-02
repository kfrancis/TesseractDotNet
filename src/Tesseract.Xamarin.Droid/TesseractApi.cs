using Android.Content;
using Android.Graphics;
using Android.Util;
using Com.Googlecode.Tesseract.Android;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TesseractDotNet;
using Exception = System.Exception;
using File = Java.IO.File;
using Object = Java.Lang.Object;
using Rectangle = TesseractDotNet.Rectangle;

namespace Tesseract.Xamarin.Droid;

public class TesseractApi : ITesseractApi
{
    /// <summary>
    ///     Blacklist of characters to not recognize.
    /// </summary>
    public const string VAR_CHAR_BLACKLIST = "tessedit_char_blacklist";

    /// <summary>
    ///     Whitelist of characters to recognize.
    /// </summary>
    public const string VAR_CHAR_WHITELIST = "tessedit_char_whitelist";

    private readonly AssetsDeployment _assetsDeployment;
    private readonly Context _context;
    private readonly ProgressHandler _progressHandler = new();
    private TessBaseAPI _api;
    private volatile bool _busy;
    private Rectangle? _rect;

    public TesseractApi(Context context, AssetsDeployment assetsDeployment)
    {
        _assetsDeployment = assetsDeployment;
        _context = context;
        _progressHandler.Progress += (sender, e) =>
        {
            OnProgress(e.Progress);
        };
        _api = new TessBaseAPI(_progressHandler);
    }

    public event EventHandler<ProgressEventArgs> Progress;

    public bool Initialized { get; private set; }
    public BitmapFactory.Options Options { get; set; } = new BitmapFactory.Options { InSampleSize = 1 };

    public string Text { get; private set; }

    public void AddPageToDocument(Com.Googlecode.Leptonica.Android.Pix imageToProcess, string imageToWrite, TessPdfRenderer tessPdfRenderer)
    {
        _api.AddPageToDocument(imageToProcess, imageToWrite, tessPdfRenderer);
    }

    public void BeginDocument(TessPdfRenderer tessPdfRenderer, string title = null)
    {
        if (title == null)
            _api.BeginDocument(tessPdfRenderer);
        else
            _api.BeginDocument(tessPdfRenderer, title);
    }

    public void Clear()
    {
        _rect = null;
        _api.Clear();
    }

    public void Dispose()
    {
        if (_api != null)
        {
            _api.Dispose();
            _api = null;
        }
    }

    public void End()
    {
        _api.Recycle();
    }

    public void EndDocument(TessPdfRenderer tessPdfRenderer)
    {
        _api.EndDocument(tessPdfRenderer);
    }

    public string GetHOCRText(int page)
    {
        return _api.GetHOCRText(page);
    }

    public async Task<bool> Init(string language, OcrEngineMode? mode = null)
    {
        if (string.IsNullOrEmpty(language))
            return false;
        try
        {
            var path = await CopyAssets();
            var result = mode.HasValue
                ? _api.Init(path, language, GetOcrEngineMode(mode.Value))
                : _api.Init(path, language);
            Initialized = result;
            return result;
        }
        catch (IllegalArgumentException ex)
        {
            Log.Debug("TesseractApi", ex, ex.Message);
            Initialized = false;
            return false;
        }
    }

    public async Task<bool> Init(string tessDataPath, string language)
    {
        var result = _api.Init(tessDataPath, language);
        Initialized = result;
        return result;
    }

    public void ReadConfigFile(string fileName)
    {
        _api.ReadConfigFile(fileName);
    }

    public async Task<bool> Recognise(Bitmap bitmap)
    {
        CheckIfInitialized();
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));
        if (_busy)
            return false;
        _busy = true;
        try
        {
            await Task.Run(() =>
            {
                _api.SetImage(bitmap);
                if (_rect.HasValue)
                {
                    _api.SetRectangle((int)_rect.Value.Left, (int)_rect.Value.Top, (int)_rect.Value.Width,
                        (int)_rect.Value.Height);
                }
                Text = _api.UTF8Text;
            });
            return true;
        }
        finally
        {
            _busy = false;
        }
    }

    public IEnumerable<Result> Results(PageIteratorLevel level)
    {
        CheckIfInitialized();
        var pageIteratorLevel = GetPageIteratorLevel(level);
        var iterator = _api.ResultIterator;
        if (iterator == null)
            yield break;
        // ReSharper disable once TooWideLocalVariableScope
        int[] boundingBox;
        iterator.Begin();
        do
        {
            boundingBox = iterator.GetBoundingBox(pageIteratorLevel);
            var result = new Result
            {
                Confidence = iterator.Confidence(pageIteratorLevel),
                Text = iterator.GetUTF8Text(pageIteratorLevel),
                Box = new Rectangle(boundingBox[0], boundingBox[1], boundingBox[2] - boundingBox[0], boundingBox[3] - boundingBox[1])
            };
            yield return result;
        } while (iterator.Next(pageIteratorLevel));
    }

    public void SetBlacklist(string blacklist)
    {
        CheckIfInitialized();
        _api.SetVariable(VAR_CHAR_BLACKLIST, blacklist);
    }

    public async Task<bool> SetImage(byte[] data)
    {
        CheckIfInitialized();
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        using var bitmap = await BitmapFactory.DecodeByteArrayAsync(data, 0, data.Length, Options);
        return await Recognise(bitmap);
    }

    public async Task<bool> SetImage(string path)
    {
        CheckIfInitialized();
        if (path == null)
            throw new ArgumentNullException(nameof(path));
        using var bitmap = await BitmapFactory.DecodeFileAsync(path, Options);
        return await Recognise(bitmap);
    }

    public async Task<bool> SetImage(Stream stream)
    {
        CheckIfInitialized();
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        using var bitmap = await BitmapFactory.DecodeStreamAsync(stream, null, Options);
        return await Recognise(bitmap);
    }

    public void SetPageSegmentationMode(PageSegmentationMode mode)
    {
        CheckIfInitialized();
        _api.SetPageSegMode((int)mode);
    }

    public void SetRectangle(Rectangle? rect)
    {
        CheckIfInitialized();
        _rect = rect;
    }

    public void SetVariable(string key, string value)
    {
        CheckIfInitialized();
        _api.SetVariable(key, value);
    }

    public void SetWhitelist(string whitelist)
    {
        CheckIfInitialized();
        _api.SetVariable(VAR_CHAR_WHITELIST, whitelist);
    }

    public void Stop()
    {
        _api.Stop();
    }

    private void CheckIfInitialized()
    {
        if (!Initialized)
            throw new InvalidOperationException("Call Init first");
    }

    private async Task<string> CopyAssets()
    {
        try
        {
            var assetManager = _context.Assets;
            var files = assetManager.List("tessdata");
            var file = _context.GetExternalFilesDir(null);
            var tessdata = new File(_context.GetExternalFilesDir(null), "tessdata");
            if (!tessdata.Exists())
            {
                tessdata.Mkdir();
            }
            else if (_assetsDeployment == AssetsDeployment.OncePerVersion)
            {
                var packageInfo = _context.PackageManager.GetPackageInfo(_context.PackageName, 0);
                var version = packageInfo.VersionName;
                var versionFile = new File(tessdata, "version");
                if (versionFile.Exists())
                {
                    var fileVersion = System.IO.File.ReadAllText(versionFile.AbsolutePath);
                    if (version == fileVersion)
                    {
                        Log.Debug("TesseractApi", "Application version didn't change, skipping copying assets");
                        return file.AbsolutePath;
                    }
                    versionFile.Delete();
                }
                System.IO.File.WriteAllText(versionFile.AbsolutePath, version);
            }

            Log.Debug("TesseractApi", "Copy assets to " + file.AbsolutePath);

            foreach (var filename in files)
            {
                using var inStream = assetManager.Open("tessdata/" + filename);
                var outFile = new File(tessdata, filename);
                if (outFile.Exists())
                {
                    outFile.Delete();
                }
                using var outStream = new FileStream(outFile.AbsolutePath, FileMode.Create);
                await inStream.CopyToAsync(outStream);
                await outStream.FlushAsync();
            }
            return file.AbsolutePath;
        }
        catch (Exception ex)
        {
            Log.Error("TesseractApi", ex.Message);
        }
        return null;
    }

    private int GetOcrEngineMode(OcrEngineMode mode)
    {
        return (int)mode;
    }

    private int GetPageIteratorLevel(PageIteratorLevel level)
    {
        return (int)level;
    }

    private void OnProgress(int progress)
    {
        var handler = Progress;
        handler?.Invoke(this, new ProgressEventArgs(progress));
    }

    private class ProgressHandler : Object, TessBaseAPI.IProgressNotifier
    {
        internal event EventHandler<ProgressEventArgs> Progress;

        public void OnProgressValues(TessBaseAPI.ProgressValues progress)
        {
            OnProgress(progress.Percent);
        }

        private void OnProgress(int progress)
        {
            var handler = Progress;
            handler?.Invoke(this, new ProgressEventArgs(progress));
        }
    }
}
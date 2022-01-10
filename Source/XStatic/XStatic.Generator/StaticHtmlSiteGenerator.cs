using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Generator;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors.ValueConverters;
using Umbraco.ModelsBuilder.Embedded;
using Umbraco.Web;
using Umbraco.Web.Models.ContentEditing;
using XStatic.Generator.Ssl;
using XStatic.Generator.Storage;
using XStatic.Generator.Transformers;
using Umbraco.Core.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Diagnostics;

namespace XStatic.Generator
{

    public class StaticHtmlSiteGenerator : GeneratorBase
    {
        public const string ImagesRegex = "=['\"(](/media/\\S+)['\")]";

        public StaticHtmlSiteGenerator(IUmbracoContextFactory umbracoContextFactory, IStaticSiteStorer storer, IImageCropNameGenerator imageCropNameGenerator, IMediaFileSystem mediaFileSystem)
            : base(umbracoContextFactory, storer, imageCropNameGenerator, mediaFileSystem) {
        }

        private string GetHost() {
            var ctx = _umbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
            var port = ctx.HttpContext.Request.Url.Port;
            var portStr = port == 80 ? "" : $":{port}";
            return $"{ctx.HttpContext.Request.Url.Scheme}://{ctx.HttpContext.Request.Url.Host}{portStr}";
        }

        public override async Task<string> GeneratePage(int id, int staticSiteId, IFileNameGenerator fileNamer, IEnumerable<ITransformer> transformers = null) {
            SslTruster.TrustSslIfAppSettingConfigured();

            var node = GetNode(id);

            if (node == null) {
                return null;
            }

            var url = node.Url(mode: UrlMode.Relative);
            string absoluteUrl = node.Url(mode: UrlMode.Absolute);

            var fileData = await GetFileDataFromWebClient(absoluteUrl);

            if (fileData == null) {
                return null;
            }

            var imgs = await ProcessImages(fileData, GetHost(), staticSiteId);
            foreach (var img in imgs) {
                fileData = fileData.Replace(img.OldUrl, img.NewUrl);
            }
            var transformedData = RunTransformers(fileData, transformers);

            var filePath = fileNamer.GetFilePartialPath(url);

            var generatedFileLocation = await Store(staticSiteId, filePath, transformedData);

            return generatedFileLocation;
        }

        private async Task<List<ReplacedImage>> ProcessImages(string html, string host, int siteId) {
            var result = new List<ReplacedImage>();
            var images = new Regex(ImagesRegex, RegexOptions.CultureInvariant).Matches(html);
            foreach (Match image in images) {
                result.Add(new ReplacedImage { OldUrl = image.Groups[1].Value });
            }
            var wc = new WebClient();
            foreach (var ri in result) {
                var uri = new Uri($"{host}{ri.OldUrl}");
                var path = $"{_storer.GetStorageLocationOfSite(siteId)}{ri.FilePath}";
                try {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    await wc.DownloadFileTaskAsync(uri, path);
                } catch (Exception e) {
                    Debugger.Break();
                }
            }
            return result;
        }
    }

    public class ReplacedImage
    {
        public string OldUrl { get; set; }
        public string FilePath {
            get {
                var urlPart = OldUrl.Split('?')[0];
                var folder = Path.GetDirectoryName(urlPart);
                var fn = Path.GetFileNameWithoutExtension(urlPart);
                var ext = Path.GetExtension(urlPart);
                var parms = HttpUtility.ParseQueryString(OldUrl);
                var mode = parms.Get("mode");
                var width = parms.Get("width");
                var height = parms.Get("height");
                return $@"{folder}\{fn}-{mode}-{width}x{height}{ext}";
            }
        }

        public string NewUrl { get { return FilePath.Replace('\\', '/'); } }

    }

}

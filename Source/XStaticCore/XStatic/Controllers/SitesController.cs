﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.BackOffice.Controllers;
//using System.Web.Http;
//using System.Web.UI;
using Umbraco.Cms.Web.Common.Attributes;
using XStatic.Plugin;
//using Umbraco.Core.Composing;
//using Umbraco.Web.Editors;
//using Umbraco.Web.Mvc;
//using XStatic.Generator.Storage;
//using XStatic.Library;
//using XStatic.Plugin.Repositories;

namespace XStatic.Plugin.Controllers
{
    [PluginController("xstatic")]
    public class SitesController : UmbracoAuthorizedJsonController
    {
        private readonly IUmbracoContextFactory _context;

        //private readonly IStaticSiteStorer _storer;
        //private SitesRepository _sitesRepo;

        //public SitesController(IStaticSiteStorer storer)
        //{
        //    _sitesRepo = new SitesRepository();
        //    _storer = storer;
        //}

        public SitesController(IUmbracoContextFactory context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<ExtendedGeneratedSite> GetAll()
        {
            var sites = new[]
            {
                new ExtendedGeneratedSite
                {
                    AssetPaths = "/css",
                    AutoPublish = false,
                    ExportFormat = "html",
                    FolderSize = "100Tb",
                    Id = 1,
                    ImageCrops = "200x200",
                    LastBuildDurationInSeconds = 10,
                    LastDeployDurationInSeconds = 10,
                    LastDeployed = DateTime.Now,
                    LastRun = DateTime.Now,
                    MediaRootNodes = "",
                    Name = "Mock Data",
                    RootNode = 1063,
                    TargetHostname = "demo.com"
                }
            };

            using (var cref = _context.EnsureUmbracoContext())
            {
                foreach (var site in sites)
                {
                    var node = cref.UmbracoContext.Content.GetById(site.RootNode);

                    if(node == null)
                    {
                        site.RootPath = "Item Not Found";
                    }
                    else
                    {
                        site.RootPath = node.Parent == null ? node.Name : node.Parent.Name + "/" + node.Name;

                        //var folder = _storer.GetStorageLocationOfSite(site.Id);
                        //var size = FileHelpers.GetDirectorySize(new DirectoryInfo(folder));

                        //site.FolderSize = FileHelpers.BytesToString(size);
                    }
                }

                return sites;
            }
            
        }

        //[HttpDelete]
        //public IEnumerable<ExtendedGeneratedSite> ClearStoredSite(int staticSiteId)
        //{
        //    var folder = _storer.GetStorageLocationOfSite(staticSiteId);

        //    var doNotDeletePaths = FileHelpers.DefaultNonDeletePaths;

        //    var doNotDeletePathsRaw = ConfigurationManager.AppSettings["xStatic.DoNotDeletePaths"];

        //    if(doNotDeletePathsRaw != null)
        //    {
        //        var split = doNotDeletePathsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        //        if(split.Any())
        //        {
        //            doNotDeletePaths = split;
        //        }
        //    }

        //    FileHelpers.DeleteFolderContents(folder, doNotDeletePaths);
            
        //    return GetAll();
        //}
    }

    public class ExtendedGeneratedSite : SiteConfig
    {
        public string RootPath { get; set; }

        public string FolderSize { get; set; }
    }
}
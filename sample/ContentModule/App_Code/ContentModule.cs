using Microsoft.Ajax.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

public class ContentModule : IHttpModule
{
    private bool IsLive { get { return !HttpContext.Current.Request.ServerVariables["HTTP_HOST"].Contains("localhost"); } }

    public void Init(HttpApplication context)
    {
        context.BeginRequest += context_BeginRequest;
    }

    string botName = "ContentModule";
    void context_BeginRequest(object sender, EventArgs e)
    {
        var context = HttpContext.Current;
        var path = context.Request.Url.PathAndQuery;
        if (path.ToLower().Contains("google") || path.ToLower().Contains("minified"))
            return;
        if (context.Request.UserAgent != botName)
        {
            var cache = Convert.ToString(context.Cache[path]);
            var cacheType = Convert.ToString(context.Cache["Type" + path]);
            if (!string.IsNullOrWhiteSpace(cache) && !string.IsNullOrWhiteSpace(cacheType))
            {
                context.Response.ContentType = cacheType;
                context.Response.Write(cache);
                SetOutputCache(context);
            }
            var remote = new Uri(context.Request.Url.OriginalString);
            using (WebClient client = new WebClient())
            {
                string content = string.Empty;
                string minified = string.Empty;
                client.Headers.Add("User-Agent", botName);
                if (path.ToLower().EndsWith(".js") && File.Exists(context.Server.MapPath(context.Request.FilePath)))
                {
                    content = File.ReadAllText(context.Server.MapPath(context.Request.FilePath));
                    var mf = JsMinifier(content, out minified);
                    if (mf.Errors.Count > 0) //js compile errors
                    {
                        minified = ParseCss(content);
                        context.Response.ContentType = "text/javascript";
                    }
                }
                else if (path.ToLower().EndsWith(".css") && File.Exists(context.Server.MapPath(context.Request.FilePath)))
                {
                    content = File.ReadAllText(context.Server.MapPath(context.Request.FilePath));
                    minified = ParseCss(content);
                    context.Response.ContentType = "text/css";
                }
                else if (path.ToLower().Contains("webresource.axd")) //scriptresource is already minified, we just have to compress it
                {
                    content = client.DownloadString(remote);
                    var mf = JsMinifier(content, out minified);
                    if (mf.Errors.Count > 0) //js compile errors or not a js file
                    {
                        minified = ParseCss(content);
                        context.Response.ContentType = "text/css";
                    }
                    else
                        context.Response.ContentType = "text/javascript";
                }

                if (!string.IsNullOrWhiteSpace(minified))
                {
                    context.Response.Write(content = minified);

                    if (IsLive)
                    {
                        context.Cache.Insert("Type" + path, context.Response.ContentType, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(365));
                        context.Cache.Insert(path, content, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(365));
                    }

                    SetOutputCache(context);
                }
            }
        }
    }

    void HttpCompress(HttpContext context)
    {
        if (context.Request.UserAgent != null && context.Request.UserAgent.Contains("MSIE 6"))
            return;
        //compress each request
        if (IsLive)
        {
            var headers = context.Request.Headers;
            if (headers["Accept-encoding"] != null && headers["Accept-encoding"].Contains("gzip"))
            {
                context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AppendHeader("Content-encoding", "gzip");
            }
            else if (headers["Accept-encoding"] != null && headers["Accept-encoding"].Contains("deflate"))
            {
                context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress);
                context.Response.AppendHeader("Content-encoding", "deflate");
            }
        }
    }

    public static string ParseCss(string body)
    {
        body = Regex.Replace(body, @"[a-zA-Z]+#", "#");
        body = Regex.Replace(body, @"[\n\r]+\s*", string.Empty);
        body = Regex.Replace(body, @"\s+", " ");
        body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
        body = body.Replace(";}", "}");
        body = Regex.Replace(body, @"([\s:]0)(px|pt|%|em)", "$1");

        // Remove comments from CSS
        body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);
        return body;
    }

    Minifier JsMinifier(string content, out string minified)
    {
        var mf = new Minifier();
        var settings = new CodeSettings();
        settings.MinifyCode = true;
        settings.LocalRenaming = LocalRenaming.CrunchAll;
        settings.RemoveFunctionExpressionNames = true;
        settings.EvalTreatment = EvalTreatment.MakeAllSafe;
        minified = mf.MinifyJavaScript(content, settings);
        return mf;
    }

    void SetOutputCache(HttpContext context)
    {
        if (IsLive)
        {
            context.Response.Cache.SetValidUntilExpires(true);
            context.Response.Cache.SetExpires(DateTime.Now.AddMonths(1));
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetLastModified(new DateTime(2012, 12, 12));
            context.Response.Cache.SetOmitVaryStar(true);
        }
        context.Response.Flush(); //send back the response
        context.Response.End(); //don't let .net framework know about my changes
    }

    public void Dispose()
    {
    }
}

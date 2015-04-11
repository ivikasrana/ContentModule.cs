# ContentModule
Minify and compress content files like .axd, .js and .css on runtime with caching

The content files are not minimized by default. This is a sample website with a HttpModule that handle all the content files like .axd, .css, .js file requests and render it after minify and caching with http compression.

<b>Useage:</b>
<br/>
&lt;system.webServer&gt;
<br/>
&lt;modules runAllManagedModulesForAllRequests=&quot;true&quot;&gt;
<br/>
&lt;add name=&quot;ContentModule&quot; type=&quot;ContentModule&quot;/&gt;
<br/>
&lt;/modules&gt;
<br/>
&lt;/system.webServer&gt;
